import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { GoogleAuthService } from '../../../services/google-auth.service';
import { CommonModule } from '@angular/common';
import { NgIconsModule } from '@ng-icons/core';

@Component({
  selector: 'app-google-callback',
  standalone: true,
  imports: [CommonModule, NgIconsModule],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-900">
      <!-- Background elements -->
      <div class="fixed inset-0 z-0">
        <div
          class="absolute inset-0 bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900"
        ></div>
        <div
          class="absolute inset-0 bg-gradient-to-br from-primary-900/20 via-secondary-900/20 to-accent-900/20 animate-gradient"
        ></div>
        <div
          class="absolute inset-0 bg-gradient-radial from-transparent via-primary-900/10 to-transparent"
        ></div>
      </div>

      <!-- Content -->
      <div class="relative z-10 max-w-md w-full mx-4">
        <div
          class="bg-white/5 backdrop-blur-lg rounded-2xl shadow-xl p-8 space-y-6 border border-white/10 text-center"
        >
          <!-- Loading State -->
          <div *ngIf="!error" class="space-y-4">
            <div
              class="w-16 h-16 mx-auto border-4 border-primary-500 border-t-transparent rounded-full animate-spin"
            ></div>
            <p class="text-gray-400">Επεξεργασία σύνδεσης Google...</p>
          </div>

          <!-- Error State -->
          <div *ngIf="error" class="space-y-6">
            <div
              class="w-16 h-16 mx-auto bg-red-500/20 rounded-full flex items-center justify-center"
            >
              <ng-icon
                name="lucideX"
                class="text-red-500"
                [size]="'32'"
              ></ng-icon>
            </div>

            <div>
              <h2 class="text-2xl font-bold text-white mb-2">
                Σφάλμα Σύνδεσης
              </h2>
              <p class="text-red-400">{{ error }}</p>
            </div>

            <div class="pt-4">
              <button
                (click)="returnToPrevious()"
                class="w-full px-4 py-3 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-xl hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 transform hover:-translate-y-1"
              >
                Επιστροφή στη Σύνδεση
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class GoogleCallbackComponent implements OnInit {
  error: string | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly googleAuthService: GoogleAuthService
  ) {}

  ngOnInit() {
    const queryParams = new URLSearchParams(window.location.search);

    // Check for error parameter from Google
    const errorCode = queryParams.get('error');
    if (errorCode) {
      this.handleError(errorCode);
      return;
    }

    // Check for required parameters
    const code = queryParams.get('code');
    const state = queryParams.get('state');

    if (!code || !state) {
      this.error = 'Μη έγκυρη απάντηση από το Google. Παρακαλώ δοκιμάστε ξανά.';
      return;
    }

    try {
      // Attempt to decode state
      const decodedState = JSON.parse(atob(state));

      // Store the auth code temporarily
      sessionStorage.setItem('googleAuthCode', code);

      // Store original path for potential return
      if (decodedState.origin) {
        sessionStorage.setItem('googleAuthOrigin', decodedState.origin);
      }

      // Handle the callback
      this.googleAuthService.handleAuthCallback(queryParams);
    } catch (e) {
      console.error('Error processing callback:', e);
      this.error =
        'Σφάλμα κατά την επεξεργασία της απάντησης. Παρακαλώ δοκιμάστε ξανά.';
    }
  }

  private handleError(errorCode: string): void {
    switch (errorCode) {
      case 'access_denied':
        this.error =
          'Η πρόσβαση δεν επιτράπηκε. Παρακαλώ επιτρέψτε την πρόσβαση για να συνεχίσετε.';
        break;
      case 'invalid_request':
        this.error = 'Μη έγκυρο αίτημα. Παρακαλώ δοκιμάστε ξανά.';
        break;
      default:
        this.error =
          'Παρουσιάστηκε σφάλμα κατά τη σύνδεση. Παρακαλώ δοκιμάστε ξανά.';
    }
  }

  returnToPrevious(): void {
    const origin = sessionStorage.getItem('googleAuthOrigin');
    if (origin) {
      sessionStorage.removeItem('googleAuthOrigin');
      this.router.navigate([origin]);
    } else {
      this.router.navigate(['/auth/login']);
    }
  }
}
