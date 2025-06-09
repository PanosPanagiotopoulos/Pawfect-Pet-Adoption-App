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
      class="relative rounded-lg p-4 mb-6 animate-fadeIn"
      [class]="getBackgroundClass()"
      role="alert"
    >
      <div class="flex items-start">
        <!-- Icon -->
        <div class="flex-shrink-0">
          <ng-icon
            [name]="getIcon()"
            [class]="getIconClass()"
            [size]="'24'"
          ></ng-icon>
        </div>

        <!-- Content -->
        <div class="ml-3">
          <h3
            *ngIf="error.title"
            [class]="getTitleClass()"
            class="text-lg font-medium"
          >
            {{ error.title }}
          </h3>
          <div [class]="getMessageClass()" class="text-sm">
            {{ error.message }}
          </div>
        </div>

        <!-- Close Button -->
        <div class="ml-auto pl-3">
          <div class="-mx-1.5 -my-1.5">
            <button
              type="button"
              [class]="getCloseButtonClass()"
              (click)="clearError()"
            >
              <span class="sr-only">Dismiss</span>
              <ng-icon name="lucideX" [size]="'16'"></ng-icon>
            </button>
          </div>
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
        return 'bg-yellow-500/10 border border-yellow-500/30';
      case 'info':
        return 'bg-blue-500/10 border border-blue-500/30';
      default:
        return 'bg-red-500/10 border border-red-500/30';
    }
  }

  getIcon(): string {
    switch (this.error?.type) {
      case 'warning':
        return 'lucideTriangle';
      case 'info':
        return 'lucideInfo';
      default:
        return 'lucideX';
    }
  }

  getIconClass(): string {
    switch (this.error?.type) {
      case 'warning':
        return 'text-yellow-400';
      case 'info':
        return 'text-blue-400';
      default:
        return 'text-red-400';
    }
  }

  getTitleClass(): string {
    switch (this.error?.type) {
      case 'warning':
        return 'text-yellow-400';
      case 'info':
        return 'text-blue-400';
      default:
        return 'text-red-400';
    }
  }

  getMessageClass(): string {
    switch (this.error?.type) {
      case 'warning':
        return 'text-yellow-300';
      case 'info':
        return 'text-blue-300';
      default:
        return 'text-red-300';
    }
  }

  getCloseButtonClass(): string {
    switch (this.error?.type) {
      case 'warning':
        return 'rounded-md p-1.5 hover:bg-yellow-500/20 text-yellow-400 hover:text-yellow-300 focus:outline-none focus:ring-2 focus:ring-yellow-500/30 transition-colors';
      case 'info':
        return 'rounded-md p-1.5 hover:bg-blue-500/20 text-blue-400 hover:text-blue-300 focus:outline-none focus:ring-2 focus:ring-blue-500/30 transition-colors';
      default:
        return 'rounded-md p-1.5 hover:bg-red-500/20 text-red-400 hover:text-red-300 focus:outline-none focus:ring-2 focus:ring-red-500/30 transition-colors';
    }
  }

  clearError(): void {
    this.error = undefined;
  }
}
