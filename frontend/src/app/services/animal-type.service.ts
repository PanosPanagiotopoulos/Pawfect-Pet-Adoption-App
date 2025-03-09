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

  query(q: AnimalTypeLookup): Observable<AnimalType[]> {
    const url = `${this.apiBase}/query`;
    return this.http
      .get<AnimalType[]>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, reqFields: string[] = []): Observable<AnimalType> {
    const url = `${this.apiBase}/${id}`;
    const options = { params: { fields: reqFields } };
    return this.http
      .get<AnimalType>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(item: AnimalTypePersist): Observable<AnimalType> {
    const url = `${this.apiBase}/persist`;
    return this.http
      .post<AnimalType>(url, item)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
