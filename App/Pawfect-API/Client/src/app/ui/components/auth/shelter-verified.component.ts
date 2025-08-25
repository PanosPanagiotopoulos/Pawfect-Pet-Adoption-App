import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { TranslationService } from 'src/app/common/services/translation.service';
import { AdminVerifyPayload } from 'src/app/models/auth/auth.model';

@Component({
  selector: 'app-shelter-verified',
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

      <div class="max-w-md w-full mx-4 z-10">
        <div
          class="bg-white/5 backdrop-blur-lg rounded-2xl shadow-xl p-8 space-y-8 border border-white/10 text-center"
        >
          <!-- Loading State -->
          <div *ngIf="isLoading" class="space-y-4">
            <div
              class="w-16 h-16 mx-auto rounded-full border-4 border-primary-500 border-t-transparent animate-spin"
            ></div>
            <p class="text-gray-400">{{ 'APP.AUTH.SHELTER_VERIFIED.PROCESSING' | translate }}</p>
          </div>

          <!-- Success State - Approved -->
          <div *ngIf="isVerified && action === 'approve'" class="space-y-4">
            <div
              class="w-16 h-16 mx-auto bg-gradient-to-r from-green-500 to-emerald-500 rounded-full flex items-center justify-center"
            >
              <ng-icon
                name="lucideCheck"
                [size]="'32'"
                class="text-white"
              ></ng-icon>
            </div>
            <h2 class="text-2xl font-bold text-white">
              {{ 'APP.AUTH.SHELTER_VERIFIED.APPROVED_TITLE' | translate }}
            </h2>
            <p class="text-gray-400">
              {{ 'APP.AUTH.SHELTER_VERIFIED.APPROVED_MESSAGE' | translate }}
            </p>
          </div>

          <!-- Success State - Rejected -->
          <div *ngIf="isVerified && action === 'reject'" class="space-y-4">
            <div
              class="w-16 h-16 mx-auto bg-gradient-to-r from-orange-500 to-red-500 rounded-full flex items-center justify-center"
            >
              <ng-icon
                name="lucideX"
                [size]="'32'"
                class="text-white"
              ></ng-icon>
            </div>
            <h2 class="text-2xl font-bold text-white">
              {{ 'APP.AUTH.SHELTER_VERIFIED.REJECTED_TITLE' | translate }}
            </h2>
            <p class="text-gray-400">
              {{ 'APP.AUTH.SHELTER_VERIFIED.REJECTED_MESSAGE' | translate }}
            </p>
          </div>

          <!-- Error State -->
          <div *ngIf="!isVerified && !isLoading" class="space-y-4">
            <div
              class="w-16 h-16 mx-auto bg-red-500/20 rounded-full flex items-center justify-center"
            >
              <ng-icon
                name="lucideTriangle"
                [size]="'32'"
                class="text-red-500"
              ></ng-icon>
            </div>
            <h2 class="text-2xl font-bold text-white">
              {{ 'APP.AUTH.SHELTER_VERIFIED.ERROR_TITLE' | translate }}
            </h2>
            <p class="text-red-400">{{ error }}</p>
          </div>

          <!-- Action Button -->
          <div class="pt-4">
            <button
              (click)="navigateToAdminDashboard()"
              class="w-full px-4 py-3 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-xl hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 transform hover:-translate-y-1"
            >
              {{ 'APP.AUTH.SHELTER_VERIFIED.BACK_TO_ADMIN' | translate }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class ShelterVerifiedComponent implements OnInit {
  isLoading = true;
  isVerified = false;
  error: string | null = null;
  action: 'approve' | 'reject' | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly authService: AuthService,
    private readonly translationService: TranslationService
  ) {}

  ngOnInit(): void {
    const token: string | null = this.route.snapshot.queryParamMap.get('token');
    const action: string | null = this.route.snapshot.queryParamMap.get('action');

    // Validate required parameters
    if (!token) {
      this.error = this.translationService.translate('APP.AUTH.SHELTER_VERIFIED.NO_TOKEN');
      this.isLoading = false;
      return;
    }

    if (!action || (action !== 'approve' && action !== 'reject')) {
      this.error = this.translationService.translate('APP.AUTH.SHELTER_VERIFIED.INVALID_ACTION');
      this.isLoading = false;
      return;
    }

    this.action = action as 'approve' | 'reject';
    
    // Prepare payload for admin verification
    const payload: AdminVerifyPayload = {
      adminToken: token,
      accept: action === 'approve'
    };

    // Call the verification service
    this.authService.verifyShelter(payload).subscribe(
      (result: boolean) => {
        this.isVerified = result;
        this.isLoading = false;
        this.error = null;
        
        if (!result) {
          this.error = this.translationService.translate('APP.AUTH.SHELTER_VERIFIED.VERIFICATION_FAILED');
        }
      },
      (error) => {
        this.isVerified = false;
        this.isLoading = false;
        console.error('Shelter verification error:', error);
        
        // Handle specific error cases
        if (error.status === 400) {
          this.error = this.translationService.translate('APP.AUTH.SHELTER_VERIFIED.INVALID_TOKEN_OR_ACTION');
        } else if (error.status === 404) {
          this.error = this.translationService.translate('APP.AUTH.SHELTER_VERIFIED.SHELTER_NOT_FOUND');
        } else if (error.status === 403) {
          this.error = this.translationService.translate('APP.AUTH.SHELTER_VERIFIED.UNAUTHORIZED');
        } else {
          this.error = this.translationService.translate('APP.AUTH.SHELTER_VERIFIED.UNEXPECTED_ERROR');
        }
      }
    );
  }

  navigateToAdminDashboard(): void {
    // Navigate to admin dashboard or login
    this.router.navigate(['/auth/login']);
  }
}