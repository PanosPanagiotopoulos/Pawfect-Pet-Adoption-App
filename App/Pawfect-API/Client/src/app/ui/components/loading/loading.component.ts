import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoadingService } from 'src/app/common/services/loading.service';

import { Observable, Subscription } from 'rxjs';

@Component({
  selector: 'app-loading',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './loading.component.html',
  styleUrls: ['./loading.component.css'],
})
export class LoadingComponent implements OnInit, OnDestroy {
  isLoading$: Observable<boolean>;
  private loadingSubscription?: Subscription;

  // Multiple GIF sources to try
  private gifSources = [
    '/assets/loader.gif',
    'https://i.gifer.com/ZKZg.gif', // Fast loading spinner
    'https://cdnjs.cloudflare.com/ajax/libs/semantic-ui/0.16.1/images/loader-large.gif',
    'https://i.gifer.com/4V0b.gif', // Another backup
  ];

  currentGifSrc = this.gifSources[0];
  private currentGifIndex = 0;
  gifError = false;

  constructor(private loadingService: LoadingService) {
    this.isLoading$ = this.loadingService.isLoading$;
  }

  ngOnInit(): void {
    // Disable scrolling when loading is active
    this.loadingSubscription = this.isLoading$.subscribe((isLoading) => {
      if (isLoading) {
        document.body.style.overflow = 'hidden';
        document.body.style.position = 'fixed';
        document.body.style.width = '100%';
      } else {
        document.body.style.overflow = 'auto';
        document.body.style.position = 'static';
        document.body.style.width = 'auto';
      }
    });

    // Force browser to reload GIF by adding cache buster
    this.currentGifSrc = this.addCacheBuster(this.currentGifSrc);
  }

  ngOnDestroy(): void {
    // Ensure scrolling is re-enabled when component is destroyed
    document.body.style.overflow = 'auto';
    document.body.style.position = 'static';
    document.body.style.width = 'auto';

    // Clean up subscription
    this.loadingSubscription?.unsubscribe();
  }

  onGifLoad(): void {
    this.gifError = false;
  }

  onGifError(event: Event): void {

    // Try next GIF source
    this.currentGifIndex++;

    if (this.currentGifIndex < this.gifSources.length) {
      this.currentGifSrc = this.addCacheBuster(
        this.gifSources[this.currentGifIndex]
      );
      this.gifError = false;
    } else {
      this.gifError = true;
    }
  }

  private addCacheBuster(url: string): string {
    const separator = url.includes('?') ? '&' : '?';
    return `${url}${separator}t=${Date.now()}`;
  }

  // Method to manually retry GIF loading (for debugging)
  retryGif(): void {
    this.currentGifIndex = 0;
    this.currentGifSrc = this.addCacheBuster(this.gifSources[0]);
    this.gifError = false;
  }
}
