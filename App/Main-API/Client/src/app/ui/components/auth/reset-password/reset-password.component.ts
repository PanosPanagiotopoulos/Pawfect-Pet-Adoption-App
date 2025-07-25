import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { AuthButtonComponent } from '../shared/auth-button/auth-button.component';
import { NgIconsModule } from '@ng-icons/core';
import { CustomValidators } from '../validators/custom.validators';
import { User } from 'src/app/models/user/user.model';
import { LogService } from 'src/app/common/services/log.service';
import { TranslationService } from 'src/app/common/services/translation.service';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-reset-password',
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
              {{ 'APP.AUTH.RESET_PASSWORD.TITLE' | translate }}
            </span>
          </h2>
          <p class="mt-2 text-gray-400">
            {{ 'APP.AUTH.RESET_PASSWORD.INSTRUCTIONS' | translate }}
          </p>
        </div>

        <div
          class="bg-white/5 backdrop-blur-lg rounded-2xl shadow-xl p-8 space-y-8 border border-white/10"
        >
          <!-- Success message -->
          <div
            *ngIf="isSuccess"
            class="bg-green-500/10 border border-green-500/30 rounded-lg p-4 text-green-400 text-sm animate-fadeIn"
          >
            <div class="flex items-center">
              <ng-icon name="lucideCheck" class="mr-2" [size]="'20'"></ng-icon>
              <p>{{ 'APP.AUTH.RESET_PASSWORD.SUCCESS' | translate }}</p>
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
              <p>{{ 'APP.AUTH.RESET_PASSWORD.ERROR' | translate }}</p>
            </div>
          </div>

          <!-- Only show form when there's no error and no success -->
          <form
            *ngIf="!isSuccess && !errorMessage"
            [formGroup]="resetForm"
            (ngSubmit)="onSubmit()"
            class="space-y-6"
          >
            <app-form-input
              [form]="resetForm"
              controlName="password"
              type="password"
              [placeholder]="'APP.AUTH.RESET_PASSWORD.PASSWORD_PLACEHOLDER' | translate"
            ></app-form-input>

            <app-form-input
              [form]="resetForm"
              controlName="confirmPassword"
              type="password"
              [placeholder]="'APP.AUTH.RESET_PASSWORD.CONFIRM_PASSWORD_PLACEHOLDER' | translate"
            ></app-form-input>

            <div class="text-sm text-gray-400 space-y-1">
              <p>{{ 'APP.AUTH.RESET_PASSWORD.REQUIREMENTS_TITLE' | translate }}</p>
              <ul class="list-disc list-inside pl-4">
                <li>{{ 'APP.AUTH.RESET_PASSWORD.REQUIREMENT_MIN' | translate }}</li>
                <li>{{ 'APP.AUTH.RESET_PASSWORD.REQUIREMENT_UPPER' | translate }}</li>
                <li>{{ 'APP.AUTH.RESET_PASSWORD.REQUIREMENT_LOWER' | translate }}</li>
                <li>{{ 'APP.AUTH.RESET_PASSWORD.REQUIREMENT_NUMBER' | translate }}</li>
                <li>{{ 'APP.AUTH.RESET_PASSWORD.REQUIREMENT_SPECIAL' | translate }}</li>
              </ul>
            </div>

            <div class="space-y-4">
              <app-auth-button
                type="submit"
                [isLoading]="isLoading"
                [disabled]="resetForm.invalid"
                icon="lucideCheck"
              >
                {{ 'APP.AUTH.RESET_PASSWORD.SUBMIT' | translate }}
              </app-auth-button>
            </div>
          </form>

          <!-- Add a button to go back when there's an error -->
          <div *ngIf="errorMessage || isSuccess" class="text-center">
            <button
              type="button"
              (click)="navigateToLogin()"
              class="w-full px-4 py-3 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-xl hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300"
            >
              {{ 'APP.AUTH.RESET_PASSWORD.BACK_TO_LOGIN' | translate }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class ResetPasswordComponent implements OnInit {
  resetForm: FormGroup;
  isLoading = false;
  isSuccess = false;
  errorMessage: string | null = null;
  private token: string | null = null;
  private email: string = '';

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly logService: LogService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly translationService: TranslationService
  ) {
    this.resetForm = this.fb.group({
      password: [
        '',
        [Validators.required, CustomValidators.passwordValidator(translationService)],
      ],
      confirmPassword: ['', [Validators.required]],
    });

    this.resetForm
      .get('confirmPassword')
      ?.setValidators([
        Validators.required,
        CustomValidators.matchValidator('password', translationService),
      ]);
  }

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParamMap.get('token');

    if (!this.token) {
      this.errorMessage = this.translationService.translate('APP.AUTH.RESET_PASSWORD.ERROR');
    }

    this.authService.verifyResetPasswordToken(this.token!).subscribe(
      (model: User) => {
        this.email = (model as any).email;
        this.isLoading = false;
        this.errorMessage = null;
      },
      (error) => {
        this.isLoading = false;
        console.error('Email verification error:', error);
        this.errorMessage = this.translationService.translate('APP.AUTH.RESET_PASSWORD.ERROR');
      }
    );
  }

  onSubmit(): void {
    if (this.resetForm.valid) {
      this.isLoading = true;
      this.errorMessage = null;

      const { password } = this.resetForm.value;

      this.authService.resetPassword(this.email, password).subscribe({
        next: () => {
          this.isLoading = false;
          this.isSuccess = true;
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = this.translationService.translate('APP.AUTH.RESET_PASSWORD.ERROR');
          console.error('Reset password error:', error);
        },
      });
    }
  }

  navigateToLogin(): void {
    this.router.navigate(['/auth/login']);
  }
}
