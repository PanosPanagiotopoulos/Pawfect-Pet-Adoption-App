import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { UserVailabilityCheck } from '../models/user-availability/user-vailability-check.model';
import { UserAvailabilityResult } from '../models/user-availability/user-availability-result.model';

@Injectable({
  providedIn: 'root',
})
export class UserAvailabilityService {
  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}auth`;
  }

  checkAvailability(input: UserVailabilityCheck): Observable<UserAvailabilityResult> {
    const url = `${this.apiBase}/check-availability`;
    return this.http
      .post<UserAvailabilityResult>(url, input)
      .pipe(catchError((error: any) => throwError(error)));
  }
}
