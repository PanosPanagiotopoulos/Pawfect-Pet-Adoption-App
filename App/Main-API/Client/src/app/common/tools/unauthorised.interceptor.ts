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

@Injectable()
export class UnauthorizedInterceptor implements HttpInterceptor {
  private readonly excludedRoutes = ['/auth/login', '/auth/sign-up', '/search', '/home', '/', '/404', ''];

  constructor(
    private router: Router,
    private snackbarService: SnackbarService,
    private authService: AuthService
  ) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status !== 401 || this.excludedRoutes.includes(this.router.url)) {
          return throwError(() => error);
        }

        return this.authService.refresh().pipe(
          switchMap(() => {
            return next.handle(request.clone());
          }),
          catchError((refreshError) => {
            this.snackbarService.showError({
              message: 'Ωχ! Φαίνεται ότι χρειάζεται να συνδεθείτε πρώτα',
              subMessage: 'Θα σας μεταφέρουμε στη σελίδα σύνδεσης'
            });
            
            setTimeout(() => {
              const currentUrl = this.router.url;
              const urlParams = new URLSearchParams(window.location.search);
              const returnUrl = urlParams.get('returnUrl') || currentUrl;
              
              this.router.navigate(['/auth/login'], {
                queryParams: { returnUrl: returnUrl }
              });
            }, 500);

            return throwError(() => refreshError);
          })
        );
      })
    );
  }
}