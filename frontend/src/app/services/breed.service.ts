import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { BreedLookup } from '../lookup/breed-lookup';
import { Breed, BreedPersist } from '../models/breed/breed.model';

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

  query(q: BreedLookup): Observable<Breed[]> {
    const url = `${this.apiBase}/query`;
    return this.http
      .get<Breed[]>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, reqFields: string[] = []): Observable<Breed> {
    const url = `${this.apiBase}/${id}`;
    const options = { params: { fields: reqFields } };
    return this.http
      .get<Breed>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(item: BreedPersist): Observable<Breed> {
    const url = `${this.apiBase}/persist`;
    return this.http
      .post<Breed>(url, item)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
