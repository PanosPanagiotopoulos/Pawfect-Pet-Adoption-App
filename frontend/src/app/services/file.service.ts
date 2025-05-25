import { Injectable } from "@angular/core";
import { Observable, catchError, throwError } from "rxjs";
import { BaseHttpService } from "../common/services/base-http.service";
import { InstallationConfigurationService } from "../common/services/installation-configuration.service";
import { FilePersist } from "../models/file/file.model";
import { FileLookup } from "../lookup/file.lookup";
import { HttpParams } from "@angular/common/http";

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

  persistBatch(models: FilePersist[], fields: string[] = ["*"]): Observable<File[]> {
    const url = `${this.apiBase}/persist`;
    let params = new HttpParams();
        fields.forEach(field => {
          params = params.append('fields', field);
        });
        const options = { params };
    return this.http
      .post<File[]>(url, models, options)
      .pipe(catchError((error: any) => throwError(error)));
  }

  query(q: FileLookup): Observable<File[]> {
    const url = `${this.apiBase}/query`;
    return this.http
      .post<File[]>(url, q)
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
