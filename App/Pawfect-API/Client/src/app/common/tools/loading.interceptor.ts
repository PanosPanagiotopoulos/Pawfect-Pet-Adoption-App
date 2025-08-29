import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpResponse,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap, finalize } from 'rxjs/operators';
import { LoadingService } from '../services/loading.service';

@Injectable()
export class LoadingInterceptor implements HttpInterceptor {
  constructor(private loadingService: LoadingService) {}

  intercept(
    request: HttpRequest<any>,
    next: HttpHandler
  ): Observable<HttpEvent<any>> {
    // Generate unique request ID
    const requestId = this.generateRequestId(request);
    
    // Check if this URL should trigger loading
    if (this.loadingService.shouldShowLoading(request.url)) {
      this.loadingService.startLoading(requestId);
    }

    return next.handle(request).pipe(
      tap({
        next: (event: HttpEvent<any>) => {},
        error: (error: HttpErrorResponse) => {}
      }),
      finalize(() => {
        if (this.loadingService.shouldShowLoading(request.url)) {
          this.loadingService.stopLoading(requestId);
        }
      })
    );
  }

  private generateRequestId(request: HttpRequest<any>): string {
    return `${request.method}-${request.url}-${Date.now()}-${Math.random()}`;
  }
}