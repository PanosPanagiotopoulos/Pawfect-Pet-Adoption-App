import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgIconsModule } from '@ng-icons/core';

export interface ErrorDetails {
  title?: string;
  message: string;
  type?: 'error' | 'warning' | 'info';
}

@Component({
  selector: 'app-error-message-banner',
  standalone: true,
  imports: [CommonModule, NgIconsModule],
  template: `
    <div
      *ngIf="error"
      class="relative rounded-xl p-5 mb-6 animate-fadeIn shadow-lg"
      [class]="getBackgroundClass()"
      role="alert"
    >
      <div class="flex items-start gap-4">
        <!-- Icon -->
        <div class="flex-shrink-0 mt-0.5">
          <div class="p-2 rounded-lg" [class]="getIconBackgroundClass()">
            <ng-icon
              [name]="getIcon()"
              [class]="getIconClass()"
              [size]="'20'"
            ></ng-icon>
          </div>
        </div>

        <!-- Content -->
        <div class="flex-1 min-w-0">
          <h3
            *ngIf="error.title"
            [class]="getTitleClass()"
            class="text-base font-semibold leading-6 mb-1"
          >
            {{ error.title }}
          </h3>
          <div [class]="getMessageClass()" class="text-sm leading-5">
            {{ error.message }}
          </div>
        </div>

        <!-- Close Button -->
        <div class="flex-shrink-0">
          <button
            type="button"
            [class]="getCloseButtonClass()"
            (click)="clearError()"
            class="p-1.5 rounded-lg transition-all duration-200"
          >
            <span class="sr-only">Dismiss</span>
            <ng-icon name="lucideX" [size]="'16'" class="opacity-70 hover:opacity-100"></ng-icon>
          </button>
        </div>
      </div>
    </div>
  `,
})
export class ErrorMessageBannerComponent {
  @Input() error?: ErrorDetails;

  getBackgroundClass(): string {
    switch (this.error?.type) {
      case 'warning':
        return 'bg-yellow-500/5 border border-yellow-500/20 backdrop-blur-sm';
      case 'info':
        return 'bg-blue-500/5 border border-blue-500/20 backdrop-blur-sm';
      default:
        return 'bg-red-500/5 border border-red-500/20 backdrop-blur-sm';
    }
  }

  getIconBackgroundClass(): string {
    switch (this.error?.type) {
      case 'warning':
        return 'bg-yellow-500/10';
      case 'info':
        return 'bg-blue-500/10';
      default:
        return 'bg-red-500/10';
    }
  }

  getIcon(): string {
    switch (this.error?.type) {
      case 'warning':
        return 'lucideAlertTriangle';
      case 'info':
        return 'lucideInfo';
      default:
        return 'lucideAlertCircle';
    }
  }

  getIconClass(): string {
    switch (this.error?.type) {
      case 'warning':
        return 'text-yellow-500';
      case 'info':
        return 'text-blue-500';
      default:
        return 'text-red-500';
    }
  }

  getTitleClass(): string {
    switch (this.error?.type) {
      case 'warning':
        return 'text-yellow-700 dark:text-yellow-400';
      case 'info':
        return 'text-blue-700 dark:text-blue-400';
      default:
        return 'text-red-700 dark:text-red-400';
    }
  }

  getMessageClass(): string {
    switch (this.error?.type) {
      case 'warning':
        return 'text-yellow-600/90 dark:text-yellow-300/90';
      case 'info':
        return 'text-blue-600/90 dark:text-blue-300/90';
      default:
        return 'text-red-600/90 dark:text-red-300/90';
    }
  }

  getCloseButtonClass(): string {
    switch (this.error?.type) {
      case 'warning':
        return 'text-yellow-500 hover:bg-yellow-500/10 focus:outline-none focus:ring-2 focus:ring-yellow-500/20';
      case 'info':
        return 'text-blue-500 hover:bg-blue-500/10 focus:outline-none focus:ring-2 focus:ring-blue-500/20';
      default:
        return 'text-red-500 hover:bg-red-500/10 focus:outline-none focus:ring-2 focus:ring-red-500/20';
    }
  }

  clearError(): void {
    this.error = undefined;
  }
}
