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
import { UnauthorizedSnackbarComponent } from '../ui/unauthorized-snackbar.component';

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
          // Show the cute unauthorized snackbar
          this.snackbarService.showCustom(UnauthorizedSnackbarComponent, 3000);
          
          // Navigate to login page after a short delay to allow the snackbar to be seen
          setTimeout(() => {
            this.router.navigate(['/auth/login']);
          }, 1000);
        }

        return throwError(() => error);
      })
    );
  }
}