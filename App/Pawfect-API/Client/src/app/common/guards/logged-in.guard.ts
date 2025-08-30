import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable, map, of } from 'rxjs';
import { AuthService } from '../../services/auth.service';

@Injectable({
  providedIn: 'root',
})
export class LoggedInGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    const targetUrl: string = state.url.split('?')[0] || '/';
    const authRoutes = ['/auth/login', '/auth/sign-up', '/auth/reset-password-request', '/auth/reset-password'];
    
    if (!authRoutes.includes(targetUrl)) return of(true);

    return this.authService.isLoggedIn().pipe(
      map((isLoggedIn) => {
        if (isLoggedIn) {
          // User is already logged in, redirect to home
          this.router.navigate(['/home']);
          return false;
        }
        // User is not logged in, allow access to auth pages
        return true;
      })
    );
  }
}
