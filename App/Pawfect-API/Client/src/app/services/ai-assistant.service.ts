import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CompletionsRequest, CompletionsResponse } from '../models/ai-assistant/ai-assistant';

@Injectable({
  providedIn: 'root',
})
export class AiAssistantService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}api/ai`;
  }

  completions(request: CompletionsRequest): Observable<CompletionsResponse> {
    const url = `${this.apiBase}/completions`;
    return this.http
      .post<CompletionsResponse>(url, request)
      .pipe(catchError((error: any) => throwError(error)));
  }
}