import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { UserLookup } from '../lookup/user-lookup';
import { User, UserPersist } from '../models/user/user.model';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}api/users`;
  }

  query(q: UserLookup): Observable<User[]> {
    const url = `${this.apiBase}/query`;
    return this.http
      .post<User[]>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, reqFields: string[] = []): Observable<User> {
    const url = `${this.apiBase}/${id}`;
    const options = { params: { fields: reqFields } };
    return this.http
      .get<User>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(item: UserPersist): Observable<User> {
    const url = `${this.apiBase}/persist`;
    return this.http
      .post<User>(url, item)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getCurrentUser(): Observable<User> {
    const url = `${this.apiBase}/current`;
    return this.http
      .get<User>(url)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
