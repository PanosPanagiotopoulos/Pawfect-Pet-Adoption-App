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
  providedIn: 'root'
})
export class UnauthorizedInterceptor implements HttpInterceptor {
  private readonly excludedRoutes = ['/auth/login', '/auth/sign-up', '/auth/google/callback', '/search', '/home', '/', '/404', ''];
  private readonly LANG_STORAGE_KEY = 'pawfect-language';

  private readonly fallbackMessages = {
    loginRequired: {
      en: 'Oops! It looks like you need to log in first',
      gr: 'Ωχ! Φαίνεται ότι χρειάζεται να συνδεθείτε πρώτα'
    },
    redirectMessage: {
      en: 'We\'ll redirect you to the login page',
      gr: 'Θα σας μεταφέρουμε στη σελίδα σύνδεσης'
    }
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
        if (
          error.status !== 401 ||
          this.excludedRoutes.some(route => this.router.url.startsWith(route))
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
              subMessage: this.getFallbackMessage('redirectMessage')
            });

            const attemptedUrl = window.location.pathname + window.location.search + window.location.hash;

            this.router.navigate(['/auth/login'], {
              queryParams: { returnUrl: attemptedUrl }
            });

            return throwError(() => refreshError);
          })
        );
      })
    );
  }

  private getFallbackMessage(messageType: 'loginRequired' | 'redirectMessage'): string {
    const currentLang = this.getCurrentLanguage();
    return this.fallbackMessages[messageType][currentLang] || this.fallbackMessages[messageType].gr;
  }

  private getCurrentLanguage(): 'en' | 'gr' {
    const stored = this.secureStorageService.getItem<string>(this.LANG_STORAGE_KEY);
    return (stored === 'en' || stored === 'gr') ? stored : 'en';
  }
}
