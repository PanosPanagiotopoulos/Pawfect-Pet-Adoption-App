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

@Injectable()
export class UnauthorizedInterceptor implements HttpInterceptor {
  constructor(private router: Router) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    return next.handle(request).pipe(
      catchError((error: HttpErrorResponse) => {
        // Check specifically for 403 Forbidden responses
        if (error.status === 403) {
          console.log('Access forbidden. Redirecting to login page.');

          // Navigate to login page
          this.router.navigate(['/auth/login']);
        }

        // Re-throw the error so other error handlers can process it
        return throwError(() => error);
      })
    );
  }
}
