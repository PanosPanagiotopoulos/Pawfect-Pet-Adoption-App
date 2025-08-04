import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslationService } from 'src/app/common/services/translation.service';
import { Location } from '@angular/common';

@Component({
  selector: 'app-unauthorized',
  standalone: false,
  template: `
    <div
      class="min-h-screen bg-gray-900 flex items-center justify-center px-4 sm:px-6 lg:px-8"
    >
      <div class="fixed inset-0 z-0">
        <div
          class="absolute inset-0 bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900"
        ></div>
        <div
          class="absolute inset-0 bg-gradient-to-br from-red-900/20 via-orange-900/20 to-red-900/20 animate-gradient"
        ></div>

        <!-- Floating background icons -->
        <div class="absolute inset-0 overflow-hidden pointer-events-none">
          <div
            class="absolute top-1/4 left-1/4 w-32 h-32 text-red-500/5 transform rotate-45 animate-float"
          >
            <ng-icon name="lucideShieldAlert" [size]="'256'"></ng-icon>
          </div>
          <div
            class="absolute bottom-1/3 right-1/3 w-24 h-24 text-orange-500/5 transform -rotate-12 animate-float"
          >
            <ng-icon name="lucideLock" [size]="'96'"></ng-icon>
          </div>
          <div
            class="absolute top-1/3 right-1/4 w-20 h-20 text-red-500/5 transform rotate-12 animate-float"
          >
            <ng-icon name="lucideAlertTriangle" [size]="'80'"></ng-icon>
          </div>
        </div>

        <!-- Gradient orbs -->
        <div
          class="absolute top-0 left-0 w-96 h-96 bg-red-500/10 rounded-full filter blur-3xl animate-pulse"
        ></div>
        <div
          class="absolute bottom-0 right-0 w-96 h-96 bg-orange-500/10 rounded-full filter blur-3xl animate-pulse delay-1000"
        ></div>
      </div>

      <div class="relative z-10 max-w-md w-full">
        <div
          class="bg-white/5 backdrop-blur-lg rounded-2xl p-8 border border-white/10 shadow-2xl text-center"
        >
          <!-- Error Icon -->
          <div
            class="mx-auto flex items-center justify-center w-20 h-20 rounded-full bg-red-500/20 mb-6"
          >
            <ng-icon
              name="lucideShieldAlert"
              class="w-10 h-10 text-red-400"
              [size]="'40'"
            ></ng-icon>
          </div>

          <!-- Error Title -->
          <h1 class="text-2xl font-bold text-white mb-4">
            {{ 'APP.UNAUTHORIZED.TITLE' | translate }}
          </h1>

          <!-- Error Message -->
          <p class="text-gray-300 mb-6 leading-relaxed">
            {{ errorMessage! | translate }}
          </p>

          <!-- Action Buttons -->
          <div class="space-y-3">
            <button
              *ngIf="returnUrl"
              (click)="goToReturnUrl()"
              class="w-full px-6 py-3 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg font-medium shadow-md hover:shadow-lg hover:-translate-y-0.5 transition-all duration-300 flex items-center justify-center gap-2"
            >
              <ng-icon name="lucideEye" class="w-4 h-4"></ng-icon>
              {{ 'APP.UNAUTHORIZED.VIEW_INSTEAD' | translate }}
            </button>

            <button
              (click)="goBack()"
              class="w-full px-6 py-3 bg-white/10 text-white rounded-lg font-medium border border-white/20 hover:bg-white/20 transition-all duration-300 flex items-center justify-center gap-2"
            >
              <ng-icon name="lucideArrowLeft" class="w-4 h-4"></ng-icon>
              {{ 'APP.UNAUTHORIZED.GO_BACK' | translate }}
            </button>

            <button
              (click)="goHome()"
              class="w-full px-6 py-3 text-gray-400 hover:text-white transition-colors duration-300 flex items-center justify-center gap-2"
            >
              <ng-icon name="lucideHome" class="w-4 h-4"></ng-icon>
              {{ 'APP.UNAUTHORIZED.GO_HOME' | translate }}
            </button>
          </div>
        </div>

        <!-- Additional Info -->
        <div class="mt-6 text-center">
          <p class="text-sm text-gray-500">
            {{ 'APP.UNAUTHORIZED.CONTACT_SUPPORT' | translate }}
          </p>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      @keyframes float {
        0%,
        100% {
          transform: translateY(0px) rotate(0deg);
        }
        50% {
          transform: translateY(-20px) rotate(5deg);
        }
      }

      .animate-float {
        animation: float 6s ease-in-out infinite;
      }

      .animate-gradient {
        animation: gradient 15s ease infinite;
      }

      @keyframes gradient {
        0%,
        100% {
          opacity: 1;
        }
        50% {
          opacity: 0.8;
        }
      }
    `,
  ],
})
export class UnauthorizedComponent implements OnInit {
  errorMessage: string | null = null;
  returnUrl: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    public translationService: TranslationService,
    private location: Location
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.errorMessage = params['message'] || null;
      this.returnUrl = params['returnUrl'] || null;
    });
  }

  goToReturnUrl(): void {
    if (this.returnUrl) {
      this.router.navigateByUrl(this.returnUrl);
    }
  }

  goBack(): void {
    // Go back 2 steps to avoid looping back to the page that redirected here
    window.history.go(-2);
  }

  goHome(): void {
    this.router.navigate(['/home']);
  }
}
