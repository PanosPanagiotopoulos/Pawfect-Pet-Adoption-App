import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { AuthService } from 'src/app/services/auth.service';
import { takeUntil } from 'rxjs';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { AuthButtonComponent } from '../shared/auth-button/auth-button.component';
import { NgIconsModule } from '@ng-icons/core';
import { TranslationService } from 'src/app/common/services/translation.service';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-reset-password-request',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormInputComponent,
    AuthButtonComponent,
    NgIconsModule,
    TranslatePipe
  ],
  template: `
    <div class="min-h-screen flex items-center justify-center bg-gray-900 pt-6">
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
        <!-- Logo -->
        <div class="text-center mb-8">
          <h2 class="text-3xl font-bold">
            <span
              class="bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 bg-clip-text text-transparent animate-gradient"
            >
              {{ 'APP.AUTH.RESET_PASSWORD_REQUEST.TITLE' | translate }}
            </span>
          </h2>
          <p class="mt-2 text-gray-400">
            {{ 'APP.AUTH.RESET_PASSWORD_REQUEST.INSTRUCTIONS' | translate }}
          </p>
        </div>

        <div
          class="bg-white/5 backdrop-blur-lg rounded-2xl shadow-xl p-8 space-y-8 border border-white/10"
        >
          <!-- Success message -->
          <div
            *ngIf="emailSent"
            class="bg-green-500/10 border border-green-500/30 rounded-lg p-4 text-green-400 text-sm animate-fadeIn"
          >
            <div class="flex items-center">
              <ng-icon name="lucideCheck" class="mr-2" [size]="'20'"></ng-icon>
              <p>{{ 'APP.AUTH.RESET_PASSWORD_REQUEST.SUCCESS' | translate }}</p>
            </div>
          </div>

          <!-- Error message -->
          <div
            *ngIf="errorMessage"
            class="bg-red-500/10 border border-red-500/30 rounded-lg p-4 text-red-400 text-sm animate-fadeIn"
          >
            <div class="flex items-start">
              <ng-icon
                name="lucideX"
                class="mr-2 mt-0.5"
                [size]="'20'"
              ></ng-icon>
              <p>{{ 'APP.AUTH.RESET_PASSWORD_REQUEST.ERROR' | translate }}</p>
            </div>
          </div>

          <form
            [formGroup]="resetForm"
            (ngSubmit)="onSubmit()"
            class="space-y-6"
          >
            <app-form-input
              [form]="resetForm"
              controlName="email"
              type="email"
              [placeholder]="'APP.AUTH.RESET_PASSWORD_REQUEST.EMAIL_PLACEHOLDER' | translate"
            ></app-form-input>

            <div class="space-y-4">
              <app-auth-button
                type="submit"
                [isLoading]="isLoading"
                [disabled]="resetForm.invalid"
                icon="lucideMail"
              >
                {{ 'APP.AUTH.RESET_PASSWORD_REQUEST.SUBMIT' | translate }}
              </app-auth-button>

              <button
                type="button"
                (click)="navigateToLogin()"
                class="w-full px-4 py-3 border border-white/20 text-white rounded-xl hover:bg-white/10 transition-all duration-300"
              >
                {{ 'APP.AUTH.RESET_PASSWORD_REQUEST.BACK_TO_LOGIN' | translate }}
              </button>
            </div>
          </form>
        </div>
      </div>
    </div>
  `,
})
export class ResetPasswordRequestComponent extends BaseComponent {
  resetForm: FormGroup;
  isLoading = false;
  emailSent = false;
  errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private translationService: TranslationService
  ) {
    super();
    this.resetForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  onSubmit(): void {
    if (this.resetForm.valid) {
      this.isLoading = true;
      this.errorMessage = null;
      const email = this.resetForm.get('email')?.value;

      this.authService
        .requestPasswordReset(email)
        .pipe(takeUntil(this._destroyed))
        .subscribe({
          next: () => {
            this.isLoading = false;
            this.emailSent = true;
          },
          error: (error) => {
            this.isLoading = false;
            this.errorMessage = this.translationService.translate('APP.AUTH.RESET_PASSWORD_REQUEST.ERROR');
            console.error('Reset password request error:', error);
          },
        });
    }
  }

  navigateToLogin(): void {
    this.router.navigate(['/auth/login']);
  }
}
