import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { AdoptionApplicationLookup } from '../lookup/adoption-application-lookup';
import {
  AdoptionApplication,
  AdoptionApplicationPersist,
} from '../models/adoption-application/adoption-application.model';
import { catchError, Observable, throwError } from 'rxjs';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class AdoptionApplicationService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}api/adoption-applications`;
  }

  query(q: AdoptionApplicationLookup): Observable<AdoptionApplication[]> {
    const url = `${this.apiBase}/query`;
    return this.http
      .post<AdoptionApplication[]>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(
    id: string,
    reqFields: string[] = []
  ): Observable<AdoptionApplication> {
    const url = `${this.apiBase}/${id}`;
    let params = new HttpParams();
        reqFields.forEach(field => {
          params = params.append('fields', field);
        });
    const options = { params };

    return this.http
      .get<AdoptionApplication>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(item: AdoptionApplicationPersist, reqFields: string[] = []): Observable<AdoptionApplication> {
    const url = `${this.apiBase}/persist`;
    let params = new HttpParams();
    reqFields.forEach(field => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .post<AdoptionApplication>(url, item, options)
      .pipe(catchError((error: any) => throwError(error)));
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