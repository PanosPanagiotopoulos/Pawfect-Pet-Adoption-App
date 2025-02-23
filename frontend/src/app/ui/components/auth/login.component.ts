import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { AuthService } from 'src/app/services/auth.service';
import { takeUntil } from 'rxjs';
import { FormInputComponent } from './shared/form-input/form-input.component';
import { AuthButtonComponent } from './shared/auth-button/auth-button.component';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent extends BaseComponent implements OnInit {
  loginForm: FormGroup;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    super();
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required],
    });
  }

  ngOnInit(): void {
    this.authService.isLoggedIn()
      .pipe(takeUntil(this._destroyed))
      .subscribe((isLoggedIn) => {
        if (isLoggedIn) {
          this.router.navigate(['/']);
        }
      });
  }

  onSubmit(): void {
    if (this.loginForm.valid) {
      this.isLoading = true;
      const { email, password } = this.loginForm.value;

      this.authService
        .login(email, password)
        .pipe(takeUntil(this._destroyed))
        .subscribe({
          next: () => {
            this.router.navigate(['/']);
          },
          error: (error) => {
            console.error('Login error:', error);
            this.isLoading = false;
          },
        });
    } else {
      Object.keys(this.loginForm.controls).forEach(key => {
        const control = this.loginForm.get(key);
        if (control?.invalid) {
          control.markAsTouched();
        }
      });
    }
  }

  loginWithGoogle(): void {
    // Implement Google login
    console.log('Google login not implemented yet');
  }
}