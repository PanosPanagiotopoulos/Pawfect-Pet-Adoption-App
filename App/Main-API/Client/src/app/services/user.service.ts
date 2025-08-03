import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { UserLookup } from '../lookup/user-lookup';
import { User, UserUpdate } from '../models/user/user.model';
import { AuthProvider } from '../common/enum/auth-provider.enum';
import { HttpParams } from '@angular/common/http';
import { QueryResult } from '../common/models/query-result';

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

  query(q: UserLookup): Observable<QueryResult<User>> {
    const url = `${this.apiBase}/query`;
    return this.http
      .post<QueryResult<User>>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, reqFields: string[] = []): Observable<User> {
    const url = `${this.apiBase}/${id}`;
    let params = new HttpParams();
    reqFields.forEach((field) => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .get<User>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getMe(reqFields: string[] = []): Observable<User> {
    const url = `${this.apiBase}/me`;

    let params = new HttpParams();
    reqFields.forEach((field) => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .get<User>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  update(item: UserUpdate, reqFields: string[] = []): Observable<User> {
    const url = `${this.apiBase}/update`;
    let params = new HttpParams();
    reqFields.forEach((field) => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .post<User>(url, item, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getCurrentUser(): Observable<User> {
    const url = `${this.apiBase}/current`;
    return this.http
      .get<User>(url)
      .pipe(catchError((error: any) => throwError(error)));
  }

  isExternalProvider(user: User): boolean {
    return user.authProvider !== AuthProvider.Local;
  }

  delete(id: string): Observable<void> {
    const url = `${this.apiBase}/delete/${id}`;
    return this.http
      .post<void>(url)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
