import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ShelterLookup } from '../lookup/shelter-lookup';
import { Shelter, ShelterPersist } from '../models/shelter/shelter.model';

@Injectable({
  providedIn: 'root',
})
export class ShelterService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}api/shelters`;
  }

  query(q: ShelterLookup): Observable<Shelter[]> {
    const url = `${this.apiBase}/query`;
    return this.http
      .get<Shelter[]>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, reqFields: string[] = []): Observable<Shelter> {
    const url = `${this.apiBase}/${id}`;
    const options = { params: { f: reqFields } };
    return this.http
      .get<Shelter>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(item: ShelterPersist): Observable<Shelter> {
    const url = `${this.apiBase}/persist`;
    return this.http
      .post<Shelter>(url, item)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
