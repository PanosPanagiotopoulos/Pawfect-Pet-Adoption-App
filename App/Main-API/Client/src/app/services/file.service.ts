import { Injectable } from "@angular/core";
import { Observable, catchError, throwError } from "rxjs";
import { BaseHttpService } from "../common/services/base-http.service";
import { InstallationConfigurationService } from "../common/services/installation-configuration.service";
import { FilePersist } from "../models/file/file.model";
import { FileLookup } from "../lookup/file.lookup";
import { HttpParams } from "@angular/common/http";
import { QueryResult } from "../common/models/query-result";

@Injectable({
  providedIn: 'root',
})
export class FileService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}api/files`;
  }

  persistBatchTemporary(models: FormData): Observable<FilePersist[]> {
    const url = `${this.apiBase}/persist/temporary/many`;
    return this.http
      .post<FilePersist[]>(url, models)
      .pipe(catchError((error: any) => throwError(error)));
  }

  query(q: FileLookup): Observable<QueryResult<File>> {
    const url = `${this.apiBase}/query`;
    return this.http
      .post<QueryResult<File>>(url, q)
      .pipe(catchError((error: any) => throwError(error)));
  }

  getSingle(id: string, fields: string[] = []): Observable<File> {
    const url = `${this.apiBase}/${id}`;
    let params = new HttpParams();
        fields.forEach(field => {
          params = params.append('fields', field);
        });
        const options = { params };
    return this.http
      .get<File>(url, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  delete(id: string): Observable<void> {
    const url = `${this.apiBase}/delete/${id}`;
    return this.http
      .post<void>(url)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
