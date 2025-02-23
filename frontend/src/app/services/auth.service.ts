import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { User } from '../models/user/user.model';
import {
  LoggedAccount,
  LoginPayload,
  RegisterPayload,
} from '../models/auth/auth.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private _loggedAccount: LoggedAccount | null = null;

  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService
  ) {}

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}auth`;
  }

  // Authentication functions
  login(email: string, password: string): Observable<LoggedAccount> {
    const url = `${this.apiBase}/login`;
    const payload: LoginPayload = { email, password, loginProvider: 1 };

    return this.http.post<LoggedAccount>(url, payload).pipe(
      tap((response: LoggedAccount) => {
        this.setLoggedAccount(response);
      }),
      catchError((error: any) => throwError(error))
    );
  }

  loginWithGoogle(accessToken: string): Observable<LoggedAccount> {
    const url = `${this.apiBase}/login`;
    const payload: LoginPayload = {
      email: '',
      password: '',
      loginProvider: 2,
      providerAccessCode: accessToken,
    };

    return this.http.post<LoggedAccount>(url, payload).pipe(
      tap((response: LoggedAccount) => {
        this.setLoggedAccount(response);
      }),
      catchError((error: any) => throwError(error))
    );
  }

  logout(): Observable<void> {
    const url = `${this.apiBase}/logout`;
    return this.http.post<void>(url, {}).pipe(
      tap(() => {
        this.clearLoggedAccount();
      }),
      catchError((error: any) => throwError(error))
    );
  }

  register(registerData: RegisterPayload): Observable<string> {
    const url = `${this.apiBase}/register/unverified`;
    return this.http
      .post<string>(url, registerData)
      .pipe(catchError((error: any) => throwError(error)));
  }

  verifyEmail(email: string, token: string, userId: string): Observable<void> {
    const url = `${this.apiBase}/verify-email`;
    return this.http
      .post<void>(url, { email, token, id: userId })
      .pipe(catchError((error: any) => throwError(error)));
  }

  sendVerificationEmail(email: string): Observable<void> {
    const url = `${this.apiBase}/send/email-verification`;
    return this.http
      .post<void>(url, { email })
      .pipe(catchError((error: any) => throwError(error)));
  }

  verifyOtp(
    phone: string,
    otp: string,
    userId: string,
    email: string
  ): Observable<void> {
    const url = `${this.apiBase}/verify-otp`;
    return this.http
      .post<void>(url, { phone, otp, id: userId, email })
      .pipe(catchError((error: any) => throwError(error)));
  }

  sendOtp(phone: string): Observable<void> {
    const url = `${this.apiBase}/send/otp`;
    return this.http
      .post<void>(url, { phone })
      .pipe(catchError((error: any) => throwError(error)));
  }

  requestPasswordReset(email: string): Observable<void> {
    const url = `${this.apiBase}/send/reset-password`;
    return this.http
      .post<void>(url, { email })
      .pipe(catchError((error: any) => throwError(error)));
  }

  resetPassword(
    email: string,
    newPassword: string,
    token: string
  ): Observable<void> {
    const url = `${this.apiBase}/reset-password`;
    return this.http
      .post<void>(url, { email, newPassword, token })
      .pipe(catchError((error: any) => throwError(error)));
  }

  // Token and Authentication Status Helpers

  isLoggedIn(): boolean {
    return !!this.getToken();
  }

  getToken(): string | null {
    const account = this.loadLoggedAccount();
    return account ? account.token : null;
  }

  getUserRole(): string | null {
    const account = this.loadLoggedAccount();
    return account ? account.role.toString() : null;
  }

  private setLoggedAccount(account: LoggedAccount): void {
    this._loggedAccount = account;
    sessionStorage.setItem('loggedAccount', JSON.stringify(account));
  }

  private clearLoggedAccount(): void {
    this._loggedAccount = null;
    sessionStorage.removeItem('loggedAccount');
  }

  private loadLoggedAccount(): LoggedAccount | null {
    if (!this._loggedAccount) {
      const stored = sessionStorage.getItem('loggedAccount');
      if (stored) {
        this._loggedAccount = JSON.parse(stored) as LoggedAccount;
      }
    }
    return this._loggedAccount;
  }
}
