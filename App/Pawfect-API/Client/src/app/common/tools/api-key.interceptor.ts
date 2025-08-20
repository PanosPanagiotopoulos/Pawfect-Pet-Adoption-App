import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
} from '@angular/common/http';
import { Observable } from 'rxjs';
import { InstallationConfigurationService } from '../services/installation-configuration.service';

@Injectable()
export class ApiKeyInterceptor implements HttpInterceptor {
  constructor(private readonly configService: InstallationConfigurationService) {}

  intercept(
    request: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    const apiKey = this.configService.mainAppApiKey;
    const apiKeyHeader = apiKey ? `ApiKey: ${apiKey}` : '';
    const clonedRequest = apiKey
      ? request.clone({
          setHeaders: {
            'ApiKey': apiKeyHeader,
          },
        })
      : request;
    return next.handle(clonedRequest);
  }
}