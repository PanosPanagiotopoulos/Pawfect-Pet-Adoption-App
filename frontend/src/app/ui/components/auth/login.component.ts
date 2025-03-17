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
import { ErrorDetails } from 'src/app/common/ui/error-message-banner.component';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent extends BaseComponent implements OnInit {
  loginForm: FormGroup;
  isLoading = false;
  error?: ErrorDetails;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly errorHandler: ErrorHandlerService
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
        this.router.navigate(['/']);
      }
    });

    this.route.queryParams.subscribe((params: any) => {
      if (params['mode'] === 'google') {
        const googleAuthCode: string | null =
          sessionStorage.getItem('googleAuthCode');

        if (googleAuthCode) {
          sessionStorage.removeItem('googleAuthCode');
          sessionStorage.removeItem('googleAuthOrigin');
          this.isLoading = true;
          this.processGoogleLogin(googleAuthCode);
        }
      }
    });
  }

  private processGoogleLogin(authCode: string): void {
    this.authService.loginWithGoogle(authCode).subscribe({
      next: (response: LoggedAccount) => {
        if (response && !response.isPhoneVerified) {
          sessionStorage.setItem('unverifiedPhone', response.phone);
          this.navigateToPhoneVerification();
        }

        if (response && !response.isEmailVerified) {
          sessionStorage.setItem('unverifiedEmail', response.email);
          this.navigateToEmailVerification();
        } else {
          this.router.navigate(['/']);
        }
        this.isLoading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.isLoading = false;
        this.error = this.errorHandler.handleAuthError(error);
      },
    });
  }

  onSubmit(): void {
    this.error = undefined;
    this.markFormGroupTouched(this.loginForm);

    if (this.loginForm.valid) {
      this.isLoading = true;
      const { email, password } = this.loginForm.value;

      this.authService.login(email, password).subscribe({
        next: (response) => {
          if (response && !response.isPhoneVerified) {
            sessionStorage.setItem('unverifiedPhone', response.phone);
            this.navigateToPhoneVerification();
          }

          if (response && !response.isEmailVerified) {
            sessionStorage.setItem('unverifiedEmail', email);
            this.navigateToEmailVerification();
          } else {
            this.router.navigate(['/']);
          }
          this.isLoading = false;
        },
        error: (error: HttpErrorResponse) => {
          this.isLoading = false;
          this.error = this.errorHandler.handleAuthError(error);
        },
      });
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
    this.error = undefined;
    this.isLoading = true;

    this.authService
      .loginWithGoogle('')
      .pipe(takeUntil(this._destroyed))
      .subscribe({
        next: (response) => {
          if (response && !response.isEmailVerified) {
            sessionStorage.setItem('unverifiedEmail', response.email || '');
            this.navigateToEmailVerification();
          } else {
            this.router.navigate(['/']);
          }
          this.isLoading = false;
        },
        error: (error) => {
          this.isLoading = false;
          this.error = this.errorHandler.handleAuthError(error);
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
