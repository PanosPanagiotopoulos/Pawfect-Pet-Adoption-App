import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { AuthService } from 'src/app/services/auth.service';
import { takeUntil } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { SignupStep } from './signup.component';
import { LoggedAccount } from 'src/app/models/auth/auth.model';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { SnackbarService } from 'src/app/common/services/snackbar.service';
import { SecureStorageService } from 'src/app/common/services/secure-storage.service';
import { TranslationService } from 'src/app/common/services/translation.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent extends BaseComponent implements OnInit {
  loginForm: FormGroup;
  isLoading = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly errorHandler: ErrorHandlerService,
    private readonly snackbarService: SnackbarService,
    private readonly secureStorageService: SecureStorageService,
    private readonly translationService: TranslationService
  ) {
    super();
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.authService.isLoggedIn().subscribe((isLoggedIn) => {
      if (isLoggedIn) {
        const returnUrl =
          this.route.snapshot.queryParams['returnUrl'] || '/home';
        this.router.navigateByUrl(returnUrl);
      }
    });

    this.route.queryParams.subscribe((params: any) => {
      if (params['mode'] === 'google') {
        const googleAuthCode: string | null =
          this.secureStorageService.getItem<string>('googleAuthCode');

        if (googleAuthCode) {
          this.secureStorageService.removeItem('googleAuthCode');
          this.secureStorageService.removeItem('googleAuthOrigin');
          this.isLoading = true;
          this.processGoogleLogin(googleAuthCode);
        }
      }
    });
  }

  private processGoogleLogin(authCode: string): void {
    this.authService.loginWithGoogle(authCode).subscribe({
      next: (response: LoggedAccount) => {
        this.isLoading = false;

        // Check verification status and route accordingly
        if (
          response &&
          (!response.isPhoneVerified || !response.isEmailVerified)
        ) {
          const userEmail = this.authService.getUserEmail() || response.email;
          this.handleUnverifiedUser(response, userEmail);
        } else {
          const returnUrl =
            this.route.snapshot.queryParams['returnUrl'] || '/home';
          this.router.navigateByUrl(returnUrl);
        }
      },
      error: (error: HttpErrorResponse) => {
        this.isLoading = false;
        const errorDetails = this.errorHandler.handleAuthError(error);
        this.snackbarService.showError({
          message: errorDetails.message,
          subMessage: errorDetails.title,
        });
      },
    });
  }

  onSubmit(): void {
    this.markFormGroupTouched(this.loginForm);

    if (this.loginForm.valid) {
      this.isLoading = true;
      const { email, password } = this.loginForm.value;

      this.authService.login(email, password).subscribe({
        next: (response) => {
          this.isLoading = false;

          // Check verification status and route accordingly
          if (
            response &&
            (!response.isPhoneVerified || !response.isEmailVerified)
          ) {
            this.handleUnverifiedUser(response, email);
          } else {
            // User is fully verified, proceed to home
            this.snackbarService.showSuccess({
              message: this.translationService.translate(
                'APP.AUTH.LOGIN.SUCCESS'
              ),
              subMessage: this.translationService.translate(
                'APP.AUTH.LOGIN.WELCOME_BACK'
              ),
            });
            const returnUrl =
              this.route.snapshot.queryParams['returnUrl'] || '/home';
            this.router.navigateByUrl(returnUrl);
          }
        },
        error: (error: HttpErrorResponse) => {
          this.isLoading = false;
          const errorDetails = this.errorHandler.handleAuthError(error);
          this.snackbarService.showError({
            message: errorDetails.message,
            subMessage: errorDetails.title,
          });
        },
      });
    }
  }

  private handleUnverifiedUser(response: LoggedAccount, email: string): void {
    // Priority: Phone verification first, then email verification
    if (!response.isPhoneVerified) {
      // Store phone for verification
      this.secureStorageService.setItem(
        'unverifiedPhone',
        response.phone || ''
      );
      this.secureStorageService.setItem('unverifiedEmail', email);
      this.navigateToPhoneVerification();
    } else if (!response.isEmailVerified) {
      // Only email verification needed
      this.secureStorageService.setItem('unverifiedEmail', email);
      this.navigateToEmailVerification();
    }
  }

  private navigateToPhoneVerification(): void {
    this.router.navigate(['/auth/sign-up'], {
      state: {
        step: SignupStep.OtpVerification,
        fromLogin: true,
      },
    });
  }

  private navigateToEmailVerification(): void {
    this.router.navigate(['/auth/sign-up'], {
      state: {
        step: SignupStep.EmailConfirmation,
        fromLogin: true,
      },
    });
  }

  loginWithGoogle(): void {
    this.isLoading = true;

    this.authService
      .loginWithGoogle('')
      .pipe(takeUntil(this._destroyed))
      .subscribe({
        next: (response) => {
          this.isLoading = false;

          // Check verification status and route accordingly
          if (
            response &&
            (!response.isPhoneVerified || !response.isEmailVerified)
          ) {
            const userEmail = this.authService.getUserEmail() || response.email;
            this.handleUnverifiedUser(response, userEmail);
          } else {
            const returnUrl =
              this.route.snapshot.queryParams['returnUrl'] || '/home';
            this.router.navigateByUrl(returnUrl);
          }
        },
        error: (error) => {
          this.isLoading = false;
          const errorDetails = this.errorHandler.handleAuthError(error);
          this.snackbarService.showError({
            message: errorDetails.message,
          });
        },
      });
  }

  private markFormGroupTouched(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach((key) => {
      const control = formGroup.get(key);
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      } else if (control) {
        control.markAsTouched();
        control.markAsDirty();
        control.updateValueAndValidity();
      }
    });
  }
}
