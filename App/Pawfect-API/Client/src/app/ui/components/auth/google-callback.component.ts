import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { GoogleAuthService } from '../../../services/google-auth.service';
import { CommonModule } from '@angular/common';
import { NgIconsModule } from '@ng-icons/core';
import { SecureStorageService } from 'src/app/common/services/secure-storage.service';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { TranslationService } from 'src/app/common/services/translation.service';

@Component({
  selector: 'app-google-callback',
  standalone: true,
  imports: [CommonModule, NgIconsModule, TranslatePipe],
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
            <p class="text-gray-400">{{ 'APP.AUTH.GOOGLE.PROCESSING' | translate }}</p>
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
                {{ 'APP.AUTH.GOOGLE.ERROR_TITLE' | translate }}
              </h2>
              <p class="text-red-400">{{ error }}</p>
            </div>

            <div class="pt-4">
              <button
                (click)="returnToPrevious()"
                class="w-full px-4 py-3 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-xl hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 transform hover:-translate-y-1"
              >
                {{ 'APP.AUTH.GOOGLE.BACK_TO_LOGIN' | translate }}
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
    private readonly googleAuthService: GoogleAuthService,
    private readonly secureStorageService: SecureStorageService,
    private readonly translationService: TranslationService
  ) {}

  ngOnInit() {
    // Add a small delay to ensure page is fully loaded
    setTimeout(() => {
      const queryParams = new URLSearchParams(window.location.search);
  
      // Fix for TypeScript error - convert URLSearchParams to plain object
      const paramsObject: { [key: string]: string } = {};
      queryParams.forEach((value, key) => {
        paramsObject[key] = value;
      });
      
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
        console.error('Missing required parameters:', { code: !!code, state: !!state });
        this.error = this.translationService.translate('APP.AUTH.GOOGLE.INVALID_RESPONSE');
        return;
      }
  
      try {
        // Attempt to decode state
        const decodedState = JSON.parse(atob(state));
  
        // Store the auth code with a timestamp to prevent reuse
        const authData = {
          code: code,
          timestamp: Date.now(),
          state: decodedState
        };
        
        this.secureStorageService.setItem('googleAuthData', authData);
  
        // Store original path for potential return
        if (decodedState.origin) {
          this.secureStorageService.setItem('googleAuthOrigin', decodedState.origin);
        }
  
        // Clear any existing auth data to prevent conflicts
        this.secureStorageService.removeItem('googleAuthCode');
  
        // Handle the callback with proper error handling
        try {
          // Only call handleAuthCallback if it exists and is needed
          if (this.googleAuthService.handleAuthCallback) {
            this.googleAuthService.handleAuthCallback(queryParams);
          }
          
          // Use replace instead of navigate to avoid back button issues
          this.router.navigateByUrl('/auth/sign-up?mode=google&t=' + Date.now(), { 
            replaceUrl: true 
          }).then(
            (success) => {
              if (!success) {
                console.error('Navigation to signup failed');
                this.error = 'Navigation failed. Please try again.';
              }
            }
          ).catch((error) => {
            console.error('Navigation error:', error);
            this.error = 'Navigation error. Please try again.';
          });
            
        } catch (callbackError) {
          console.error('Error in handleAuthCallback:', callbackError);
          this.error = this.translationService.translate('APP.AUTH.GOOGLE.PROCESSING_ERROR');
        }
          
      } catch (e) {
        console.error('Error processing callback:', e);
        this.error = this.translationService.translate('APP.AUTH.GOOGLE.PROCESSING_ERROR');
      }
    }, 200); // Increased delay to ensure everything is ready
  }

  private handleError(errorCode: string): void {
    switch (errorCode) {
      case 'access_denied':
        this.error = this.translationService.translate('APP.AUTH.GOOGLE.ACCESS_DENIED');
        break;
      case 'invalid_request':
        this.error = this.translationService.translate('APP.AUTH.GOOGLE.INVALID_REQUEST');
        break;
      default:
        this.error = this.translationService.translate('APP.AUTH.GOOGLE.GENERIC_ERROR');
    }
  }

  returnToPrevious(): void {
    const origin = this.secureStorageService.getItem<string>('googleAuthOrigin');
    if (origin) {
      this.secureStorageService.removeItem('googleAuthOrigin');
      this.router.navigate([origin]);
    } else {
      this.router.navigate(['/auth/login']);
    }
  }
}