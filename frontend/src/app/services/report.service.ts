import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { ReportLookup } from '../lookup/report-lookup';
import { Report, ReportPersist } from '../models/report/report.model';

@Injectable({
  providedIn: 'root',
})
export class ReportService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}api/reports`;
  }

  query(q: ReportLookup): Observable<Report[]> {
    const url = `${this.apiBase}/query`;
    return this.http
      .post<Report[]>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, reqFields: string[] = []): Observable<Report> {
    const url = `${this.apiBase}/${id}`;
    const options = { params: { f: reqFields } };
    return this.http
      .get<Report>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  persist(item: ReportPersist): Observable<Report> {
    const url = `${this.apiBase}/persist`;
    return this.http
      .post<Report>(url, item)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
