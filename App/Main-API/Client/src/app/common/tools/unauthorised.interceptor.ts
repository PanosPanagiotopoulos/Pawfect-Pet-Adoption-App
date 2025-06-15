import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse,
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Router } from '@angular/router';
import { SnackbarService } from '../services/snackbar.service';

@Injectable()
export class UnauthorizedInterceptor implements HttpInterceptor {
  private readonly excludedRoutes = ['/auth/login', '/auth/sign-up', '/search', '/home', '/', '/404', ''];

  constructor(
    private router: Router,
    private snackbarService: SnackbarService
  ) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && !this.excludedRoutes.includes(this.router.url)) {
          // Show unauthorized error snackbar with custom message
          this.snackbarService.showError({
            message: 'Ωχ! Φαίνεται ότι χρειάζεται να συνδεθείτε πρώτα',
            subMessage: 'Θα σας μεταφέρουμε στη σελίδα σύνδεσης'
          });
          
          // Navigate to login page after a short delay to allow the snackbar to be seen
          setTimeout(() => {
            this.router.navigate(['/auth/login'], {
              queryParams: { returnUrl: this.router.url }
            });
          }, 1000);
        }

        return throwError(() => error);
      })
    );
  }
}