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

@Injectable()
export class NotificationService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}api/notifications`;
  }

  query(q: NotificationLookup): Observable<Notification[]> {
    const url = `${this.apiBase}/query`;
    return this.http
      .get<Notification[]>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, reqFields: string[] = []): Observable<Notification> {
    const url = `${this.apiBase}/${id}`;
    const options = { params: { f: reqFields } };
    return this.http
      .get<Notification>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(item: NotificationPersist): Observable<Notification> {
    const url = `${this.apiBase}/persist`;
    return this.http
      .post<Notification>(url, item)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
