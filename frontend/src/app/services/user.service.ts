import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { UserLookup } from '../lookup/user-lookup';
import { User, UserPersist } from '../models/user/user.model';
import { AuthProvider } from '../common/enum/auth-provider.enum';
import { HttpParams } from '@angular/common/http';

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
    let params = new HttpParams();
    reqFields.forEach(field => {
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
    reqFields.forEach(field => {
      params = params.append('fields', field);
    });
    const options = { params };
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

  isExternalProvider(user: User): boolean {
    return user.authProvider !== AuthProvider.Local;
  }

  delete(id: string): Observable<void> {
    const url = `${this.apiBase}/delete`;
    return this.http
      .post<void>(url, { id })
      .pipe(catchError((error: any) => throwError(error)));
  }

  deleteMany(ids: string[]): Observable<void> {
    const url = `${this.apiBase}/delete/many`;
    return this.http
      .post<void>(url, ids)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
