import {
    Component,
    Input,
    ViewChild,
    ElementRef,
    OnInit,
    OnDestroy,
    ChangeDetectionStrategy,
    Output,
    EventEmitter,
    DestroyRef,
    inject,
    signal,
    computed,
  } from '@angular/core';
  import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
  import { CommonModule } from '@angular/common';
  import {
    GifService,
    GifInstance,
    GifConfig,
    GifLoadState,
  } from '../services/gif.service';
  
  export interface GifComponentConfig extends Partial<GifConfig> {
    readonly width?: number;
    readonly height?: number;
    readonly fallbackSrc?: string;
    readonly cssClasses?: string;
    readonly loadingText?: string;
  }
  
  @Component({
    selector: 'app-gif',
    standalone: true,
    imports: [CommonModule],
    template: `
      <div
        class="relative inline-block"
        [style.width.px]="width()"
        [style.height.px]="height()"
      >
        <!-- Canvas for GIF rendering -->
        <canvas
          #gifCanvas
          [width]="width()"
          [height]="height()"
          [class]="canvasClasses()"
          [style.display]="loadState().isLoaded ? 'block' : 'none'"
          [attr.aria-label]="alt() || 'Animated GIF'"
          role="img"
        ></canvas>
  
        <!-- Loading state -->
        <div
          *ngIf="loadState().isLoading"
          class="absolute inset-0 flex items-center justify-center bg-transparent rounded"
          [attr.aria-live]="'polite'"
        >
          <div class="text-sm text-gray-400">
            {{ config().loadingText || 'Loading...' }}
          </div>
        </div>
  
        <!-- Error state with fallback -->
        <div
          *ngIf="loadState().hasError"
          class="absolute inset-0 flex items-center justify-center"
        >
          <img
            *ngIf="config().fallbackSrc; else errorMessage"
            [src]="config().fallbackSrc"
            [alt]="alt() || 'Loading animation'"
            [width]="width()"
            [height]="height()"
            [class]="fallbackClasses()"
            (error)="onFallbackError($event)"
          />
          <ng-template #errorMessage>
            <div class="text-xs text-red-500 text-center p-2">
              Failed to load animation
            </div>
          </ng-template>
        </div>
      </div>
    `,
    changeDetection: ChangeDetectionStrategy.OnPush,
  })
  export class GifComponent implements OnInit, OnDestroy {
    private readonly gifService = inject(GifService);
    private readonly destroyRef = inject(DestroyRef);
    private readonly componentId = `gif-${Math.random()
      .toString(36)
      .substr(2, 9)}`;
  
    @ViewChild('gifCanvas', { static: true })
    private readonly canvasRef!: ElementRef<HTMLCanvasElement>;
  
    private gifInstance: GifInstance | null = null;
  
    // Inputs as signals
    readonly src = signal<string>('');
    readonly alt = signal<string>('');
    readonly width = signal<number>(160);
    readonly height = signal<number>(160);
    readonly config = signal<GifComponentConfig>({});
  
    // Internal state
    readonly loadState = signal<GifLoadState>({
      isLoading: false,
      isLoaded: false,
      hasError: false,
    });
  
    // Computed values
    readonly canvasClasses = computed(() => {
      const base = 'w-full h-full object-contain transition-all duration-300';
      const custom = this.config().cssClasses || '';
      const hover = 'hover:scale-105';
      return `${base} ${custom} ${hover}`.trim();
    });
  
    readonly fallbackClasses = computed(() => {
      const base = 'w-full h-full object-contain opacity-75';
      const custom = this.config().cssClasses || '';
      return `${base} ${custom}`.trim();
    });
  
    // Input setters
    @Input() set gifSrc(value: string) {
      this.src.set(value || '');
    }
  
    @Input() set gifAlt(value: string) {
      this.alt.set(value || '');
    }
  
    @Input() set gifWidth(value: number) {
      this.width.set(Math.max(1, value || 160));
    }
  
    @Input() set gifHeight(value: number) {
      this.height.set(Math.max(1, value || 160));
    }
  
    @Input() set gifConfig(value: GifComponentConfig) {
      this.config.set(value || {});
    }
  
    // Outputs
    @Output() readonly loadStart = new EventEmitter<void>();
    @Output() readonly loadComplete = new EventEmitter<void>();
    @Output() readonly loadError = new EventEmitter<string>();
  
    async ngOnInit(): Promise<void> {
      if (!this.src()) {
        this.setError('No GIF source provided');
        return;
      }
  
      await this.initializeGif();
    }
  
    private async initializeGif(): Promise<void> {
      try {
        // Create GIF instance
        this.gifInstance = this.gifService.createInstance(this.componentId);
  
        // Subscribe to load state changes
        this.gifInstance.loadState$
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe((state) => {
            this.loadState.set(state);
  
            if (state.isLoading) {
              this.loadStart.emit();
            } else if (state.isLoaded) {
              this.loadComplete.emit();
            } else if (state.hasError) {
              this.loadError.emit(state.errorMessage || 'Unknown error');
            }
          });
  
        // Load the GIF with faster speed (4x faster)
        const gifConfig: Partial<GifConfig> = {
          speedMultiplier: this.config().speedMultiplier || 4, // Increased speed
          quality: this.config().quality || 'high',
          smoothing: this.config().smoothing ?? true,
          autoPlay: this.config().autoPlay ?? true,
          loop: this.config().loop ?? true,
        };
  
        await this.gifInstance.load(
          this.src(),
          this.canvasRef.nativeElement,
          gifConfig
        );
      } catch (error) {
        this.setError(
          error instanceof Error ? error.message : 'Failed to initialize GIF'
        );
      }
    }
  
    // Public API methods
    play(): void {
      this.gifInstance?.play();
    }
  
    pause(): void {
      this.gifInstance?.pause();
    }
  
    reset(): void {
      this.gifInstance?.reset();
    }
  
    updateSpeed(multiplier: number): void {
      this.gifInstance?.updateConfig({
        speedMultiplier: Math.max(0.1, multiplier),
      });
    }
  
    // Event handlers
    onFallbackError(event: Event): void {
      console.warn('Fallback image failed to load', event);
    }
  
    private setError(message: string): void {
      this.loadState.set({
        isLoading: false,
        isLoaded: false,
        hasError: true,
        errorMessage: message,
      });
    }
  
    ngOnDestroy(): void {
      if (this.gifInstance) {
        this.gifService.destroyInstance(this.componentId);
        this.gifInstance = null;
      }
    }
  }