import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { UserRoundCogIcon } from 'lucide-angular';
import { UserRole } from 'src/app/common/enum/user-role.enum';
import { User } from 'src/app/models/user/user.model';
import { TranslationService } from 'src/app/common/services/translation.service';

@Component({
  selector: 'app-verified',
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
            <p class="text-gray-400">{{ 'APP.AUTH.VERIFIED.LOADING' | translate }}</p>
          </div>

          <!-- Success State -->
          <div *ngIf="isVerified" class="space-y-4">
            <div
              class="w-16 h-16 mx-auto bg-gradient-to-r from-primary-500 to-accent-500 rounded-full flex items-center justify-center"
            >
              <ng-icon
                name="lucideCheck"
                [size]="'32'"
                class="text-white"
              ></ng-icon>
            </div>
            <h2 class="text-2xl font-bold text-white">
              {{ 'APP.AUTH.VERIFIED.SUCCESS_TITLE' | translate }}
            </h2>
            <p class="text-gray-400">
              {{ getVerificationMessage() }}
            </p>
          </div>

          <!-- Error State -->
          <div *ngIf="!isVerified && !isLoading" class="space-y-4">
            <div
              class="w-16 h-16 mx-auto bg-red-500/20 rounded-full flex items-center justify-center"
            >
              <ng-icon
                name="lucideX"
                [size]="'32'"
                class="text-red-500"
              ></ng-icon>
            </div>
            <h2 class="text-2xl font-bold text-white">
              {{ 'APP.AUTH.VERIFIED.ERROR_TITLE' | translate }}
            </h2>
            <p class="text-red-400">{{ error }}</p>
          </div>

          <!-- Action Button -->
          <div class="pt-4">
            <button
              (click)="navigateToLogin()"
              class="w-full px-4 py-3 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-xl hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 transform hover:-translate-y-1"
            >
              {{ 'APP.AUTH.VERIFIED.BACK_TO_LOGIN' | translate }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class VerifiedComponent implements OnInit {
  isLoading = true;
  isVerified = false;
  error: string | null = null;
  userRoles: UserRole[] | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly authService: AuthService,
    private readonly translationService: TranslationService
  ) {}

  ngOnInit(): void {
    const token: string | null = this.route.snapshot.queryParamMap.get(
      'token'
    ) as string;

    const complete: string | null = this.route.snapshot.queryParamMap.get(
      'complete'
    ) as string;

    const userId: string | null = this.route.snapshot.queryParamMap.get(
      'identification'
    ) as string;

    if (complete && userId) {
      this.handleAlreadyVerified(userId);
      return;
    }

    if (!token) {
      this.error = this.translationService.translate('APP.AUTH.VERIFIED.NO_TOKEN');
      this.isLoading = false;
      return;
    }

    this.authService.verifyEmail(token).subscribe(
      (model: User) => {
        this.isVerified = true;
        this.isLoading = false;
        this.error = null;
        this.userRoles = model.roles!;
      },
      (error) => {
        this.isVerified = false;
        this.isLoading = false;
        console.error('Email verification error:', error);
        this.error = this.translationService.translate('APP.AUTH.VERIFIED.INVALID_TOKEN');
      }
    );
  }

  getVerificationMessage(): string {
    if (this.userRoles?.includes(UserRole.Shelter)) {
      return this.translationService.translate('APP.AUTH.VERIFIED.SHELTER_MESSAGE');
    }
    return this.translationService.translate('APP.AUTH.VERIFIED.USER_MESSAGE');
  }

  navigateToLogin(): void {
    this.router.navigate(['/auth/login']);
  }

  handleAlreadyVerified(userId: string): void {
    this.authService.verifyUser(userId).subscribe(
      () => {
        this.isVerified = true;
        this.isLoading = false;
        this.error = null;
      },
      (error) => {
        this.isVerified = false;
        this.isLoading = false;
        console.error('Email verification error:', error);
        this.error = this.translationService.translate('APP.AUTH.VERIFIED.USER_VERIFY_ERROR');
      }
    );
  }

  isRole(complete: string): UserRole | null {
    let role: UserRole | null = null;
    const numericRole = Number(complete);
    if (!isNaN(numericRole)) {
      role = numericRole as UserRole;
    } else {
      // Fallback: Map string values
      switch (complete.toLowerCase()) {
        case 'user':
          role = UserRole.User;
          break;
        case 'shelter':
          role = UserRole.Shelter;
          break;
        case 'admin':
          role = UserRole.Admin;
          break;
        default:
          throw new Error('Invalid role');
      }
    }

    return role;
  }
}
