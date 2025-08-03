import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AnimalTypeLookup } from '../lookup/animal-type-lookup';
import {
  AnimalType,
  AnimalTypePersist,
} from '../models/animal-type/animal-type.model';
import { HttpParams } from '@angular/common/http';
import { QueryResult } from '../common/models/query-result';

@Injectable({
  providedIn: 'root',
})
export class AnimalTypeService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}api/animal-types`;
  }

  query(q: AnimalTypeLookup): Observable<QueryResult<AnimalType>> {
    const url = `${this.apiBase}/query`;
    return this.http
      .post<QueryResult<AnimalType>>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, reqFields: string[] = []): Observable<AnimalType> {
    const url = `${this.apiBase}/${id}`;
    let params = new HttpParams();
    reqFields.forEach((field) => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .get<AnimalType>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(
    item: AnimalTypePersist,
    reqFields: string[] = []
  ): Observable<AnimalType> {
    const url = `${this.apiBase}/persist`;
    let params = new HttpParams();
    reqFields.forEach((field) => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .post<AnimalType>(url, item, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  delete(id: string): Observable<void> {
    const url = `${this.apiBase}/delete/${id}`;
    return this.http
      .post<void>(url)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
