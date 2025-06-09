import { Component } from '@angular/core';
import { MatSnackBarRef } from '@angular/material/snack-bar';
import { NgIconsModule } from '@ng-icons/core';
import { lucideLock, lucidePawPrint } from '@ng-icons/lucide';

@Component({
  selector: 'app-unauthorized-snackbar',
  standalone: true,
  imports: [NgIconsModule],
  template: `
    <div class="flex items-center gap-3 p-2">
      <ng-icon
        name="lucideLock"
        [size]="'24'"
        class="text-red-400 animate-bounce"
      ></ng-icon>
      <div class="flex-1">
        <p class="text-sm font-medium text-gray-200">
          Ωχ! Φαίνεται ότι χρειάζεται να συνδεθείτε πρώτα
        </p>
        <p class="text-xs text-gray-400">
          Θα σας μεταφέρουμε στη σελίδα σύνδεσης
        </p>
      </div>
      <ng-icon
        name="lucidePawPrint"
        [size]="'20'"
        class="text-primary-400 animate-pulse"
      ></ng-icon>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      background: rgba(31, 41, 55, 0.95);
      border-left: 4px solid #ef4444;
      border-radius: 0.5rem;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
    }
  `]
})
export class UnauthorizedSnackbarComponent {
  constructor(
    public snackBarRef: MatSnackBarRef<UnauthorizedSnackbarComponent>
  ) {}
} 