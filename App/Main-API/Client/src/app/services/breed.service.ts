import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { BreedLookup } from '../lookup/breed-lookup';
import { Breed, BreedPersist } from '../models/breed/breed.model';
import { HttpParams } from '@angular/common/http';
import { QueryResult } from '../common/models/query-result';

@Injectable({
  providedIn: 'root',
})
export class BreedService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}api/breeds`;
  }

  query(q: BreedLookup): Observable<QueryResult<Breed>> {
    const url = `${this.apiBase}/query`;
    return this.http
      .post<QueryResult<Breed>>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, reqFields: string[] = []): Observable<Breed> {
    const url = `${this.apiBase}/${id}`;
    let params = new HttpParams();
        reqFields.forEach(field => {
          params = params.append('fields', field);
        });
        const options = { params };
    return this.http
      .get<Breed>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(item: BreedPersist, reqFields: string[] = []): Observable<Breed> {
    const url = `${this.apiBase}/persist`;
    let params = new HttpParams();
    reqFields.forEach(field => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .post<Breed>(url, item, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  delete(id: string): Observable<void> {
    const url = `${this.apiBase}/delete/${id}`;
    return this.http
      .post<void>(url)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
