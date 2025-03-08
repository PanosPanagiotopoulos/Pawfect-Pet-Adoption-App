import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { AuthService } from 'src/app/services/auth.service';
import { takeUntil } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { SignupStep } from './signup.component';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent extends BaseComponent implements OnInit {
  loginForm: FormGroup;
  isLoading = false;
  errorMessage: string | null = null;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
    super();
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.authService
      .isLoggedIn()
      .pipe(takeUntil(this._destroyed))
      .subscribe((isLoggedIn) => {
        if (isLoggedIn) {
          this.router.navigate(['/']);
        }
      });
  }

  onSubmit(): void {
    this.errorMessage = null;

    this.markFormGroupTouched(this.loginForm);

    if (this.loginForm.valid) {
      this.isLoading = true;
      const { email, password } = this.loginForm.value;

      this.authService
        .login(email, password)
        .pipe(takeUntil(this._destroyed))
        .subscribe({
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

            if (
              error.status === 200 &&
              error.error?.isEmailVerified === false
            ) {
              // This is a special case where the backend returns 200 but with isEmailVerified = false
              sessionStorage.setItem('unverifiedEmail', email);
              this.navigateToEmailVerification();
            } else {
              // Handle other errors
              this.handleLoginError(error);
            }
          },
        });
    }
  }

  private navigateToPhoneVerification(): void {
    // Navigate to signup page with email verification step
    this.router.navigate(['/auth/sign-up'], {
      state: {
        step: SignupStep.OtpVerification,
        fromLogin: true,
      },
    });
  }

  private navigateToEmailVerification(): void {
    // Navigate to signup page with email verification step
    this.router.navigate(['/auth/sign-up'], {
      state: {
        step: SignupStep.EmailConfirmation,
        fromLogin: true,
      },
    });
  }

  private handleLoginError(error: any): void {
    console.error('Login error:', error);

    if (error.status === 401) {
      this.errorMessage = 'Λάθος email ή κωδικός πρόσβασης';
    } else if (error.status === 403) {
      this.errorMessage = 'Ο λογαριασμός σας έχει απενεργοποιηθεί';
    } else {
      this.errorMessage =
        'Παρουσιάστηκε σφάλμα κατά τη σύνδεση. Παρακαλώ δοκιμάστε ξανά αργότερα.';
    }
  }

  loginWithGoogle(): void {
    this.errorMessage = null;
    this.isLoading = true;

    // For testing without backend
    // setTimeout(() => {
    //   this.isLoading = false;
    //   this.router.navigate(['/']);
    // }, 1500);

    // Uncomment this when ready to connect to backend
    this.authService
      .loginWithGoogle('')
      .pipe(takeUntil(this._destroyed))
      .subscribe({
        next: (response) => {
          // Check if user is verified
          if (response && !response.isEmailVerified) {
            // User exists but email is not verified
            sessionStorage.setItem('unverifiedEmail', response.email || '');
            this.navigateToEmailVerification();
          } else {
            // User is verified, proceed with normal login flow
            this.router.navigate(['/']);
          }
          this.isLoading = false;
        },
        error: (error) => {
          this.isLoading = false;
          this.handleLoginError(error);
        },
      });
  }

  // Helper method to mark all controls in a form group as touched
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
