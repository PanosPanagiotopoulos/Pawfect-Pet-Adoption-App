import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';
import { Permission } from '../enum/permission.enum';
import { SnackbarService } from '../services/snackbar.service';
import { UnauthorizedSnackbarComponent } from '../ui/unauthorized-snackbar.component';

@Injectable({
  providedIn: 'root'
})
export class AuthGuard implements CanActivate {
  constructor(
    private authService: AuthService,
    private router: Router,
    private snackbarService: SnackbarService
  ) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    // Get required permissions from route data
    const requiredPermissions = route.data['permissions'] as Permission[];
    
    // If no permissions are required, allow access
    if (!requiredPermissions || requiredPermissions.length === 0) {
      return true;
    }

    // Check if user has any of the required permissions
    const hasPermission = this.authService.hasAnyPermission(requiredPermissions);

    if (!hasPermission) {
      // Show the cute unauthorized snackbar
      this.snackbarService.showCustom(UnauthorizedSnackbarComponent, 3000);

      // Navigate to 404 page after a short delay to allow the snackbar to be seen
      setTimeout(() => {
        this.router.navigate(['/404']);
      }, 1000);

      return false;
    }

    return true;
  }
} 