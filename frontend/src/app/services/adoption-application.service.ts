import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { AdoptionApplicationLookup } from '../lookup/adoption-application-lookup';
import {
  AdoptionApplication,
  AdoptionApplicationPersist,
} from '../models/adoption-application/adoption-application.model';
import { catchError, Observable, throwError } from 'rxjs';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';

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
      .get<AdoptionApplication[]>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(
    id: string,
    reqFields: string[] = []
  ): Observable<AdoptionApplication> {
    const url = `${this.apiBase}/${id}`;
    const options = { params: { fields: reqFields } };

    return this.http
      .get<AdoptionApplication>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(item: AdoptionApplicationPersist): Observable<AdoptionApplication> {
    const url = `${this.apiBase}/persist`;
    return this.http
      .post<AdoptionApplication>(url, item)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
