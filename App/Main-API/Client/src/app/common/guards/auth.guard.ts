import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { Permission } from '../enum/permission.enum';
import { SnackbarService } from '../services/snackbar.service';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router,
    private snackbarService: SnackbarService
  ) {}

  canActivate(route: ActivatedRouteSnapshot): Observable<boolean> {
    // Get required permissions from route data
    const requiredPermissions = route.data['permissions'] as Permission[];
    // If no permissions are required, allow access
    if (!requiredPermissions || requiredPermissions.length === 0) {
      return of(true);
    }

    // First check if we have a token
    if (!this.authService.isLoggedInSync()) {
      // If we have no token, try to get user data
      return this.authService.me().pipe(
        map(() => {
          // After me() completes, check permissions
          if (!this.authService.hasAnyPermission(requiredPermissions)) {
            this.showUnauthorizedMessage(true);
            return false;
          }
          return true;
        }),
        catchError(() => {
          this.showUnauthorizedMessage();
          return of(false);
        })
      );
    }

    // If we already have user data, check permissions
    if (!this.authService.hasAnyPermission(requiredPermissions)) {
      this.showUnauthorizedMessage(true);
      return of(false);
    }

    return of(true);
  }

  private showUnauthorizedMessage(isLoggedIn: boolean = false): void {
    this.snackbarService.showError({
      message: isLoggedIn ? 'Δεν έχετε τα απαραίτητα δικαιώματα για αυτή τη σελίδα' : 'Ωχ! Φαίνεται ότι χρειάζεται να συνδεθείτε πρώτα',
      subMessage: isLoggedIn ? 'Θα σας μεταφέρουμε στη σελίδα 404' : 'Θα σας μεταφέρουμε στη σελίδα σύνδεσης'
    });

    // Navigate to appropriate page after a short delay
    setTimeout(() => {
      const redirectUrl = isLoggedIn ? '/404' : '/auth/login';
      
      // Extract the returnUrl query parameter from the current URL
      const currentUrl = this.router.url;
      const urlParams = new URLSearchParams(window.location.search);
      const returnUrl = urlParams.get('returnUrl') || currentUrl;
      
      this.router.navigate([redirectUrl], {
        queryParams: { returnUrl: returnUrl }
      });
      debugger;
    }, 1000);
  }
} 