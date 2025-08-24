import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { SnackbarService } from '../services/snackbar.service';
import { AuthService } from '../../services/auth.service';
import { SecureStorageService } from '../services/secure-storage.service';

@Injectable({
  providedIn: 'root',
})
export class UnauthorizedInterceptor implements HttpInterceptor {
  private readonly excludedRoutes = [
    '/',
    '/home',
    '/404',
    '',
    '/auth/login',
    '/auth/sign-up',
    '/auth/google/callback',
    '/auth/google/callback-page',
    '/auth/verified',
    '/auth/reset-password-request',
    '/auth/reset-password',
  ];
  private readonly LANG_STORAGE_KEY = 'pawfect-language';

  private readonly fallbackMessages = {
    loginRequired: {
      en: 'Oops! It looks like you need to log in first',
      gr: 'Ωχ! Φαίνεται ότι χρειάζεται να συνδεθείτε πρώτα',
    },
    redirectMessage: {
      en: "We'll redirect you to the login page",
      gr: 'Θα σας μεταφέρουμε στη σελίδα σύνδεσης',
    },
  };

  constructor(
    private router: Router,
    private snackbarService: SnackbarService,
    private authService: AuthService,
    private secureStorageService: SecureStorageService
  ) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        const failedRequestUrl = request.url.split('?')[0];
        const currentRoute = window.location.pathname;
        // Handle 403 Forbidden errors - redirect to unauthorized page
        if (error.status === 403 && !this.excludedRoutes.includes(currentRoute)) {
          const attemptedUrl = window.location.pathname + window.location.search + window.location.hash;
          this.router.navigate(['/unauthorized'], {
            queryParams: {
              message: 'You do not have permission to access this resource.',
              returnUrl: attemptedUrl
            }
          });
          return throwError(() => error);
        }

        // Handle 401 Unauthorized errors
        if (
          error.status !== 401 ||
          failedRequestUrl.includes('/auth/refresh') ||
          this.excludedRoutes.includes(currentRoute)
        ) {
          return throwError(() => error);
        }

        return this.authService.refresh().pipe(
          switchMap(() => {
            return next.handle(request.clone());
          }),
          catchError((refreshError) => {
            this.snackbarService.showError({
              message: this.getFallbackMessage('loginRequired'),
              subMessage: this.getFallbackMessage('redirectMessage'),
            });

            const attemptedUrl =
              window.location.pathname +
              window.location.search +
              window.location.hash;

            this.router.navigate(['/auth/login'], {
              queryParams: { returnUrl: attemptedUrl },
            });

            return throwError(() => refreshError);
          })
        );
      })
    );
  }

  private getFallbackMessage(
    messageType: 'loginRequired' | 'redirectMessage'
  ): string {
    const currentLang = this.getCurrentLanguage();
    return (
      this.fallbackMessages[messageType][currentLang] ||
      this.fallbackMessages[messageType].gr
    );
  }

  private getCurrentLanguage(): 'en' | 'gr' {
    const stored = this.secureStorageService.getItem<string>(
      this.LANG_STORAGE_KEY
    );
    return stored === 'en' || stored === 'gr' ? stored : 'en';
  }
}
