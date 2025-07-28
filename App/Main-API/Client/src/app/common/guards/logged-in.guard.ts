import { Injectable } from '@angular/core';
import { CanActivate, Router } from '@angular/router';
import { Observable, map, of } from 'rxjs';
import { AuthService } from '../../services/auth.service';

@Injectable({
  providedIn: 'root',
})
export class LoggedInGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(): Observable<boolean> {
    const route: string = this.router.url.split('?')[0] || '/';
    if (route !== '/auth/login' && route !== '/auth/sign-up') return of(true);

    return this.authService.isLoggedIn().pipe(
      map((isLoggedIn) => {
        if (isLoggedIn) {
          // User is already logged in, redirect to home
          this.router.navigate(['/']);
          return false;
        }
        // User is not logged in, allow access to login page
        return true;
      })
    );
  }
}
