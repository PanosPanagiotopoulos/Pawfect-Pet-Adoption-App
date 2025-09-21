import { Injectable } from '@angular/core';
import {
  CanActivate,
  Router,
  ActivatedRouteSnapshot,
  RouterStateSnapshot,
} from '@angular/router';
import { Observable, of } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { AdoptionApplicationService } from 'src/app/services/adoption-application.service';
import { AuthService } from 'src/app/services/auth.service';
import { SnackbarService } from '../services/snackbar.service';
import { TranslationService } from '../services/translation.service';
import { Permission } from '../enum/permission.enum';

@Injectable({
  providedIn: 'root',
})
export class AdoptionApplicationGuard implements CanActivate {
  constructor(
    private adoptionApplicationService: AdoptionApplicationService,
    private authService: AuthService,
    private router: Router,
    private snackbarService: SnackbarService,
    private translationService: TranslationService
  ) {}

  canActivate(
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
): Observable<boolean> {
  const animalId = route.params['id'];

  // Skip check for edit route or no animalId
  if (!animalId || route.url[0]?.path === 'edit') {
    return of(true);
  }

  // Check if user is a shelter - block immediately
  const userShelterId = this.authService.getUserShelterId();
  if (userShelterId) {
    this.router.navigate(['/unauthorized'], {
      queryParams: {
        message: 'APP.PROFILE-PAGE.ADOPTION_APPLICATIONS.FORBIDDEN',
        returnUrl: '/profile?tab=received-applications',
      },
    });
    return of(false);
  }

  // ✅ Return pipeline directly, let Angular decide
  return this.adoptionApplicationService.adoptionRequestExists(animalId).pipe(
    map((existingApplicationId) => {
      if (existingApplicationId?.trim()) {
        this.snackbarService.showWarning({
          message: this.translationService.translate(
            'APP.ADOPT.DUPLICATE_APPLICATION_WARNING'
          ),
          subMessage: this.translationService.translate(
            'APP.ADOPT.REDIRECTING_TO_EXISTING_APPLICATION'
          ),
        });

        const hasViewPermission = this.authService.hasPermission(
          Permission.CanViewAdoptionApplications
        );

        if (hasViewPermission) {
          this.router.navigate(['/adopt/edit', existingApplicationId]);
        } else {
          this.snackbarService.showWarning({
            message: this.translationService.translate(
              'APP.ADOPT.REDIRECTING_TO_PROFILE'
            ),
            subMessage: this.translationService.translate(
              'APP.ADOPT.VIEW_APPLICATION_IN_PROFILE'
            ),
          });
          this.router.navigate(['/profile'], {
            queryParams: { tab: 'adoption-applications' },
          });
        }

        return false; // block navigation
      }

      return true; // allow navigation
    }),
    catchError((error) => {
      console.warn('Failed to check for existing application:', error);
      return of(true); // allow fallback
    })
  );
}
}