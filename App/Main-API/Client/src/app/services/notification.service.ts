import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { NotificationLookup } from '../lookup/notification-lookup';
import {
  Notification,
  NotificationPersist,
} from '../models/notification/notification.model';
import { HttpParams } from '@angular/common/http';
import { QueryResult } from '../common/models/query-result';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}api/notifications`;
  }

  query(q: NotificationLookup): Observable<QueryResult<Notification>> {
    const url = `${this.apiBase}/query`;
    return this.http
      .post<QueryResult<Notification>>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, reqFields: string[] = []): Observable<Notification> {
    const url = `${this.apiBase}/${id}`;
    let params = new HttpParams();
        reqFields.forEach(field => {
          params = params.append('fields', field);
        });
        const options = { params };
    return this.http
      .get<Notification>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(item: NotificationPersist, reqFields: string[] = []): Observable<Notification> {
    const url = `${this.apiBase}/persist`;
    let params = new HttpParams();
    reqFields.forEach(field => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .post<Notification>(url, item, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  delete(id: string): Observable<void> {
    const url = `${this.apiBase}/delete/${id}`;
    return this.http
      .post<void>(url)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
