import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { AuthService } from 'src/app/services/auth.service';
import { takeUntil } from 'rxjs';

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
    // Mark all fields as touched to trigger validation messages immediately
    this.markFormGroupTouched(this.loginForm);
    
    if (this.loginForm.valid) {
      this.isLoading = true;
      const { email, password } = this.loginForm.value;

      // For testing without backend
      setTimeout(() => {
        this.isLoading = false;
        this.router.navigate(['/']);
      }, 1500);

      // Uncomment this when ready to connect to backend
      // this.authService
      //   .login(email, password)
      //   .pipe(takeUntil(this._destroyed))
      //   .subscribe({
      //     next: () => {
      //       this.router.navigate(['/']);
      //     },
      //     error: (error) => {
      //       console.error('Login error:', error);
      //       this.isLoading = false;
      //     },
      //   });
    }
  }

  loginWithGoogle(): void {
    // For testing without backend
    this.isLoading = true;
    setTimeout(() => {
      this.isLoading = false;
      this.router.navigate(['/']);
    }, 1500);

    // Uncomment this when ready to connect to backend
    // this.authService
    //   .loginWithGoogle('')
    //   .pipe(takeUntil(this._destroyed))
    //   .subscribe({
    //     next: () => {
    //       this.router.navigate(['/']);
    //     },
    //     error: (error) => {
    //       console.error('Google login error:', error);
    //     },
    //   });
  }
  
  // Helper method to mark all controls in a form group as touched
  private markFormGroupTouched(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach(key => {
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