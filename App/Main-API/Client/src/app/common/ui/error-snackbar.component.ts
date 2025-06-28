import { Component, Inject } from '@angular/core';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';
import { NgIconsModule } from '@ng-icons/core';
import { lucidePawPrint, lucideX } from '@ng-icons/lucide';
import { CommonModule } from '@angular/common';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

export interface ErrorSnackbarData {
  message: string;
  subMessage?: string;
  icon?: string;
}

@Component({
  selector: 'app-error-snackbar',
  standalone: true,
  imports: [CommonModule, NgIconsModule, TranslatePipe],
  template: `
    <div class="flex items-center gap-3 p-3 sm:p-4 bg-gray-800/95 border-l-4 border-red-500 rounded-lg shadow-lg max-w-[90vw] sm:max-w-md">
      <!-- Icon -->
      <ng-icon
        [name]="data.icon ?? 'lucidePawPrint'"
        [size]="'20'"
        class="text-red-500 animate-pulse flex-shrink-0"
        aria-hidden="true"
      ></ng-icon>

      <!-- Message -->
      <div class="flex-1 min-w-0">
        <p class="text-sm font-medium text-white truncate">
          {{ data.message || ('APP.UI_COMPONENTS.PET_DETAILS.CLOSE' | translate) }}
        </p>
        <p *ngIf="data.subMessage" class="text-xs text-gray-400 mt-1 truncate">
          {{ data.subMessage }}
        </p>
      </div>

      <!-- Close Button -->
      <button
        (click)="snackBarRef.dismiss()"
        class="p-1 rounded-full hover:bg-white/10 transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-white/20 flex-shrink-0"
        [attr.aria-label]="'APP.UI_COMPONENTS.PET_DETAILS.CLOSE' | translate"
      >
        <ng-icon
          name="lucideX"
          [size]="'18'"
          class="text-white/70 hover:text-white"
          aria-hidden="true"
        ></ng-icon>
      </button>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      border-radius: 0.5rem;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.2), 0 2px 4px -1px rgba(0, 0, 0, 0.1);
      backdrop-filter: blur(8px);
      z-index: 9999 !important;
    }

    ::ng-deep .mdc-snackbar__surface {
      background-color: transparent !important;
      box-shadow: none !important;
      padding: 0 !important;
    }

    ::ng-deep .mdc-snackbar__label {
      color: white !important;
      padding: 0 !important;
    }

    ::ng-deep .mat-mdc-snack-bar-container {
      --mdc-snackbar-container-color: transparent;
      --mat-mdc-snack-bar-button-color: transparent;
      --mdc-snackbar-supporting-text-color: transparent;
    }

    @media (max-width: 640px) {
      ::ng-deep .mat-mdc-snack-bar-container {
        margin: 0 auto !important;
        max-width: 90vw !important;
      }
    }
  `]
})
export class ErrorSnackbarComponent {
  constructor(
    public snackBarRef: MatSnackBarRef<ErrorSnackbarComponent>,
    @Inject(MAT_SNACK_BAR_DATA) public data: ErrorSnackbarData
  ) {}
} 