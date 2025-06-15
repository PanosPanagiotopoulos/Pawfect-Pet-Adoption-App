import { Component, Inject } from '@angular/core';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';
import { NgIconsModule } from '@ng-icons/core';
import { lucideCheck, lucideCircle, lucideTriangle, lucideX } from '@ng-icons/lucide';
import { CommonModule } from '@angular/common';

export interface CustomSnackbarData {
  message: string;
  type: 'success' | 'error' | 'warning' | 'info';
  action?: string;
  icon?: string;
}

@Component({
  selector: 'app-custom-snackbar',
  standalone: true,
  imports: [CommonModule, NgIconsModule],
  template: `
    <div class="flex items-center gap-3 p-4" [ngClass]="getContainerClass()">
      <!-- Icon -->
      <ng-icon
        [name]="getIcon()"
        [size]="'24'"
        [ngClass]="getIconClass()"
      ></ng-icon>

      <!-- Message -->
      <div class="flex-1">
        <p class="text-sm font-medium text-white">
          {{ data.message }}
        </p>
      </div>

      <!-- Close Button -->
      <button
        (click)="snackBarRef.dismiss()"
        class="p-1 rounded-full hover:bg-white/10 transition-colors duration-200"
        aria-label="Close"
      >
        <ng-icon
          name="lucideX"
          [size]="'20'"
          class="text-white/70 hover:text-white"
        ></ng-icon>
      </button>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      border-radius: 0.5rem;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
      backdrop-filter: blur(8px);
    }
  `]
})
export class CustomSnackbarComponent {
  constructor(
    public snackBarRef: MatSnackBarRef<CustomSnackbarComponent>,
    @Inject(MAT_SNACK_BAR_DATA) public data: CustomSnackbarData
  ) {}

  getContainerClass(): string {
    const baseClasses = 'rounded-lg border-l-4';
    switch (this.data.type) {
      case 'success':
        return `${baseClasses} bg-green-600/95 border-green-700`;
      case 'error':
        return `${baseClasses} bg-red-600/95 border-red-700`;
      case 'warning':
        return `${baseClasses} bg-amber-600/95 border-amber-700`;
      default:
        return `${baseClasses} bg-gray-800/95 border-gray-700`;
    }
  }

  getIcon(): string {
    if (this.data.icon) return this.data.icon;
    
    switch (this.data.type) {
      case 'success':
        return 'lucideCheck';
      case 'error':
        return 'lucideCircle';
      case 'warning':
        return 'lucideTriangle';
      default:
        return 'lucideCircle';
    }
  }

  getIconClass(): string {
    switch (this.data.type) {
      case 'success':
        return 'text-green-200 animate-bounce';
      case 'error':
        return 'text-red-200 animate-pulse';
      case 'warning':
        return 'text-amber-200 animate-pulse';
      default:
        return 'text-gray-200';
    }
  }
} 