import { Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

export interface GifFrame {
  readonly imageData: ImageData;
  readonly delay: number;
}

export interface GifLoadState {
  readonly isLoading: boolean;
  readonly isLoaded: boolean;
  readonly hasError: boolean;
  readonly errorMessage?: string;
}

export interface GifConfig {
  readonly speedMultiplier: number;
  readonly quality: 'low' | 'medium' | 'high';
  readonly smoothing: boolean;
  readonly autoPlay: boolean;
  readonly loop: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class GifService implements OnDestroy {
  private readonly instances = new Map<string, GifInstance>();
  
  createInstance(id: string): GifInstance {
    if (this.instances.has(id)) {
      this.instances.get(id)?.destroy();
    }
    
    const instance = new GifInstance();
    this.instances.set(id, instance);
    return instance;
  }
  
  destroyInstance(id: string): void {
    const instance = this.instances.get(id);
    if (instance) {
      instance.destroy();
      this.instances.delete(id);
    }
  }
  
  ngOnDestroy(): void {
    this.instances.forEach(instance => instance.destroy());
    this.instances.clear();
  }
}

export class GifInstance {
  private canvas: HTMLCanvasElement | null = null;
  private ctx: CanvasRenderingContext2D | null = null;
  private frames: GifFrame[] = [];
  private currentFrameIndex = 0;
  private animationId: number | null = null;
  private isPlaying = false;
  private loadPromise: Promise<void> | null = null;
  
  private readonly loadStateSubject = new BehaviorSubject<GifLoadState>({
    isLoading: false,
    isLoaded: false,
    hasError: false
  });
  
  private config: GifConfig = {
    speedMultiplier: 2,
    quality: 'high',
    smoothing: true,
    autoPlay: true,
    loop: true
  };

  readonly loadState$: Observable<GifLoadState> = this.loadStateSubject.asObservable();

  async load(
    gifUrl: string, 
    canvas: HTMLCanvasElement, 
    config: Partial<GifConfig> = {}
  ): Promise<void> {
    // Prevent multiple simultaneous loads
    if (this.loadPromise) {
      return this.loadPromise;
    }

    this.config = { ...this.config, ...config };
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d', { 
      alpha: true,
      willReadFrequently: true 
    });

    if (!this.ctx) {
      this.setError('Failed to get 2D context from canvas');
      throw new Error('Canvas context unavailable');
    }

    this.setLoading(true);

    this.loadPromise = this.performLoad(gifUrl)
      .then(() => {
        this.setLoaded();
        if (this.config.autoPlay) {
          this.play();
        }
      })
      .catch((error) => {
        this.setError(error.message || 'Failed to load GIF');
        throw error;
      })
      .finally(() => {
        this.loadPromise = null;
      });

    return this.loadPromise;
  }

  private async performLoad(gifUrl: string): Promise<void> {
    try {
      const response = await fetch(gifUrl);
      if (!response.ok) {
        throw new Error(`HTTP ${response.status}: ${response.statusText}`);
      }

      const arrayBuffer = await response.arrayBuffer();
      await this.extractFrames(arrayBuffer);
    } catch (error) {
      if (error instanceof TypeError && error.message.includes('fetch')) {
        throw new Error('Network error: Unable to fetch GIF');
      }
      throw error;
    }
  }

  private async extractFrames(arrayBuffer: ArrayBuffer): Promise<void> {
    return new Promise((resolve, reject) => {
      const img = new Image();
      const blob = new Blob([arrayBuffer], { type: 'image/gif' });
      const url = URL.createObjectURL(blob);

      const cleanup = () => URL.revokeObjectURL(url);

      img.onload = () => {
        try {
          this.setupCanvas(img);
          // For animated GIFs, just draw the image directly to preserve animation
          this.drawAnimatedGif(img, url);
          cleanup();
          resolve();
        } catch (error) {
          cleanup();
          reject(error);
        }
      };

      img.onerror = () => {
        cleanup();
        reject(new Error('Failed to decode image data'));
      };

      img.src = url;
    });
  }

  private setupCanvas(img: HTMLImageElement): void {
    if (!this.canvas || !this.ctx) {
      throw new Error('Canvas not available');
    }

    // Set canvas dimensions
    this.canvas.width = img.naturalWidth;
    this.canvas.height = img.naturalHeight;

    // Configure rendering quality
    this.ctx.imageSmoothingEnabled = this.config.smoothing;
    if (this.config.smoothing) {
      this.ctx.imageSmoothingQuality = this.config.quality;
    }
  }

  private drawAnimatedGif(img: HTMLImageElement, gifUrl: string): void {
    if (!this.ctx || !this.canvas) return;

    // Clear canvas and draw the animated GIF directly
    this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
    
    // Create a new image element for the canvas
    const animatedImg = new Image();
    animatedImg.crossOrigin = 'anonymous';
    
    animatedImg.onload = () => {
      if (!this.ctx || !this.canvas) return;
      
      // Draw the animated GIF - this preserves the animation
      this.ctx.drawImage(animatedImg, 0, 0, this.canvas.width, this.canvas.height);
      
      // Create a single frame that represents the animated GIF
      const imageData = this.ctx.getImageData(0, 0, this.canvas.width, this.canvas.height);
      this.frames = [{
        imageData,
        delay: 50 / this.config.speedMultiplier
      }];
      
      // Start continuous redrawing to maintain animation
      this.startGifAnimation(animatedImg);
    };
    
    animatedImg.src = gifUrl;
  }

  private startGifAnimation(img: HTMLImageElement): void {
    if (!this.ctx || !this.canvas) return;
    
    const animate = () => {
      if (!this.isPlaying || !this.ctx || !this.canvas) return;
      
      // Clear and redraw the animated GIF
      this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
      this.ctx.drawImage(img, 0, 0, this.canvas.width, this.canvas.height);
      
      // Continue animation
      this.animationId = requestAnimationFrame(animate);
    };
    
    if (this.config.autoPlay) {
      this.isPlaying = true;
      animate();
    }
  }

  private getFrameCount(): number {
    switch (this.config.quality) {
      case 'high': return 30;
      case 'medium': return 20;
      case 'low': return 12;
      default: return 20;
    }
  }

  private applyFrameEffects(frameIndex: number, totalFrames: number): void {
    if (!this.ctx || !this.canvas) return;

    this.ctx.save();
    
    // Very subtle effects to preserve original GIF movement
    const progress = frameIndex / totalFrames;
    const scale = 1 + Math.sin(progress * Math.PI * 2) * 0.005; // Much more subtle
    
    // Center and scale
    this.ctx.translate(this.canvas.width / 2, this.canvas.height / 2);
    this.ctx.scale(scale, scale);
    this.ctx.translate(-this.canvas.width / 2, -this.canvas.height / 2);
  }

  play(): void {
    if (this.isPlaying || this.frames.length === 0) return;
    
    this.isPlaying = true;
    this.animate();
  }

  pause(): void {
    this.isPlaying = false;
    if (this.animationId) {
      cancelAnimationFrame(this.animationId);
      this.animationId = null;
    }
  }

  private animate(): void {
    // This method is now handled by startGifAnimation for animated GIFs
    // Keep it for backward compatibility with static images
    if (!this.isPlaying || !this.ctx || !this.canvas || this.frames.length === 0) return;

    const frame = this.frames[this.currentFrameIndex];
    if (!frame) return;

    // Render frame
    this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
    this.ctx.putImageData(frame.imageData, 0, 0);

    // Restore smooth effects if needed
    if (this.config.smoothing) {
      this.ctx.restore();
    }

    // Advance frame
    this.currentFrameIndex = (this.currentFrameIndex + 1) % this.frames.length;

    // Schedule next frame
    const delay = frame.delay / this.config.speedMultiplier;
    setTimeout(() => {
      if (this.isPlaying) {
        this.animationId = requestAnimationFrame(() => this.animate());
      }
    }, delay);
  }

  updateConfig(config: Partial<GifConfig>): void {
    this.config = { ...this.config, ...config };
  }

  reset(): void {
    this.pause();
    this.currentFrameIndex = 0;
  }

  private setLoading(isLoading: boolean): void {
    this.loadStateSubject.next({
      isLoading,
      isLoaded: false,
      hasError: false
    });
  }

  private setLoaded(): void {
    this.loadStateSubject.next({
      isLoading: false,
      isLoaded: true,
      hasError: false
    });
  }

  private setError(message: string): void {
    this.loadStateSubject.next({
      isLoading: false,
      isLoaded: false,
      hasError: true,
      errorMessage: message
    });
  }

  destroy(): void {
    this.pause();
    this.frames = [];
    this.currentFrameIndex = 0;
    this.canvas = null;
    this.ctx = null;
    this.loadStateSubject.complete();
  }
}