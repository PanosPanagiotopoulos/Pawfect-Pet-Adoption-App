import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { Permission } from '../enum/permission.enum';
import { SnackbarService } from '../services/snackbar.service';
import { TranslationService } from '../services/translation.service';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router,
    private snackbarService: SnackbarService,
    private translate: TranslationService
  ) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    const requiredPermissions = route.data['permissions'] as Permission[];
    const attemptedUrl = state.url; // ✅ Accurate route user tried to access

    if (!requiredPermissions || requiredPermissions.length === 0) {
      return of(true);
    }

    if (!this.authService.isLoggedInSync()) {
      return this.authService.me().pipe(
        map(() => {
          if (!this.authService.hasAnyPermission(requiredPermissions)) {
            this.redirectUnauthorized(true, attemptedUrl);
            return false;
          }
          return true;
        }),
        catchError(() => {
          this.redirectUnauthorized(false, attemptedUrl);
          return of(false);
        })
      );
    }

    if (!this.authService.hasAnyPermission(requiredPermissions)) {
      this.redirectUnauthorized(true, attemptedUrl);
      return of(false);
    }

    return of(true);
  }

  private redirectUnauthorized(isLoggedIn: boolean, attemptedUrl: string): void {
    const messageKey = isLoggedIn
      ? 'APP.SERVICES.AUTH_GUARD.INSUFFICIENT_PERMISSIONS'
      : 'APP.SERVICES.AUTH_GUARD.LOGIN_REQUIRED';

    const subMessageKey = isLoggedIn
      ? 'APP.SERVICES.AUTH_GUARD.REDIRECT_TO_404'
      : 'APP.SERVICES.AUTH_GUARD.REDIRECT_TO_LOGIN';

    this.snackbarService.showError({
      message: this.translate.translate(messageKey),
      subMessage: this.translate.translate(subMessageKey),
    });

    const redirectUrl = isLoggedIn ? '/404' : '/auth/login';

    this.router.navigate([redirectUrl], {
      queryParams: { returnUrl: attemptedUrl }
    });
  }
}
