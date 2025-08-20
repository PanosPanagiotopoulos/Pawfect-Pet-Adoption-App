import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, from } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { InstallationConfigurationService } from './installation-configuration.service';

@Injectable()
export class BaseHttpService {
  constructor(
    protected http: HttpClient,
    private readonly configService: InstallationConfigurationService
  ) {}

  private waitForConfig<T>(requestFn: () => Observable<T>): Observable<T> {
    return from(this.configService.waitForConfig()).pipe(
      switchMap(() => requestFn())
    );
  }

  get<T>(url: string, options?: Object): Observable<T> {
    return this.waitForConfig(() => this.http.get<T>(url, options));
  }

  post<T>(url: string, body?: any, options?: Object): Observable<T> {
    return this.waitForConfig(() => this.http.post<T>(url, body, options));
  }

  put<T>(url: string, body: any, options?: Object): Observable<T> {
    return this.waitForConfig(() => this.http.put<T>(url, body, options));
  }

  delete<T>(url: string, options?: Object): Observable<T> {
    return this.waitForConfig(() => this.http.post<T>(url, options));
  }

  patch<T>(url: string, body: any, options?: Object): Observable<T> {
    return this.waitForConfig(() => this.http.patch<T>(url, body, options));
  }

  head<T>(url: string, options?: Object): Observable<T> {
    return this.waitForConfig(() => this.http.head<T>(url, options));
  }

  options<T>(url: string, options?: Object): Observable<T> {
    return this.waitForConfig(() => this.http.options<T>(url, options));
  }
}
