import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AnimalLookup } from '../lookup/animal-lookup';
import { Animal, AnimalPersist } from '../models/animal/animal.model';

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

  getSingle(id: string, reqFields: string[] = []): Observable<Animal> {
    const url = `${this.apiBase}/${id}`;
    const options = { params: { f: reqFields } };
    return this.http
      .get<Animal>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(item: AnimalPersist): Observable<Animal> {
    const url = `${this.apiBase}/persist`;
    return this.http
      .post<Animal>(url, item)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
