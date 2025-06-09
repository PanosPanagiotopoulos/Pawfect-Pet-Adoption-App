import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AnimalLookup } from '../lookup/animal-lookup';
import { Animal, AnimalPersist } from '../models/animal/animal.model';
import { HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root',
})
export class AnimalService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}api/animals`;
  }

  query(q: AnimalLookup): Observable<Animal[]> {
    const url = `${this.apiBase}/query`;
    return this.http
      .post<Animal[]>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  queryFreeView(q: AnimalLookup): Observable<Animal[]> {
    const url = `${this.apiBase}/query/free-view`;
    return this.http
      .post<Animal[]>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, reqFields: string[] = []): Observable<Animal> {
    const url = `${this.apiBase}/${id}`;
    let params = new HttpParams();
        reqFields.forEach(field => {
          params = params.append('fields', field);
        });
        const options = { params };
    return this.http
      .get<Animal>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(item: AnimalPersist, reqFields: string[] = []): Observable<Animal> {
    const url = `${this.apiBase}/persist`;
    let params = new HttpParams();
    reqFields.forEach(field => {
      params = params.append('fields', field);
    });
    const options = { params };
    return this.http
      .post<Animal>(url, item, options)
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
