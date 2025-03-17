import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { User } from '../models/user/user.model';
import {
  LoggedAccount,
  LoginPayload,
  RegisterPayload,
  OtpPayload,
} from '../models/auth/auth.model';
import { jwtDecode } from 'jwt-decode';
import { JwtPayload } from '../common/models/jwt.model';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private _loggedAccount: LoggedAccount | null = null;
  private readonly authStateSubject = new BehaviorSubject<boolean>(false);
  public authState$ = this.authStateSubject.asObservable();

  constructor(
    private readonly installationConfiguration: InstallationConfigurationService,
    private readonly http: BaseHttpService
  ) {
    // Initialize auth state on service creation
    this.authStateSubject.next(!!this.getToken());
  }

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}auth`;
  }

  // Authentication functions
  login(email: string, password: string): Observable<LoggedAccount> {
    const url = `${this.apiBase}/login`;
    const payload: LoginPayload = { email, password, loginProvider: 1 };

    return this.http.post<LoggedAccount>(url, payload).pipe(
      tap((response: LoggedAccount) => {
        if (response.isVerified) {
          this.setLoggedAccount(response);
        }
      }),
      catchError((error: any) => throwError(error))
    );
  }

  loginWithGoogle(code: string): Observable<LoggedAccount> {
    const url = `${this.apiBase}/login`;
    const payload: LoginPayload = {
      email: '',
      password: '',
      loginProvider: 2,
      providerAccessCode: code,
    };

    return this.http.post<LoggedAccount>(url, payload).pipe(
      tap((response: LoggedAccount) => {
        // Only set logged account if email is verified
        if (response.isVerified) {
          this.setLoggedAccount(response);
        }
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

  register(registerData: RegisterPayload): Observable<User> {
    const formData = new FormData();

    // Append the user object as JSON
    formData.append('user', JSON.stringify(registerData.user));

    // Append the file separately if it exists
    if (registerData.user.attachedPhoto) {
      formData.append(
        'AttachedPhoto',
        registerData.user.attachedPhoto,
        registerData.user.attachedPhoto.name
      );
    }

    if (registerData.shelter) {
      formData.append('shelter', JSON.stringify(registerData.shelter));
    }

    const url = `${this.apiBase}/register/unverified`;
    return this.http
      .post<User>(url, formData)
      .pipe(catchError((error: any) => throwError(error)));
  }

  registerWithGoogle(authCode: string): Observable<User> {
    const url = `${this.apiBase}/register/unverified/google`;
    return this.http
      .post<User>(url, { providerAccessCode: authCode })
      .pipe(catchError((error: any) => throwError(error)));
  }

  sendOtp(otpPayload: OtpPayload): Observable<void> {
    const url = `${this.apiBase}/send/otp`;
    return this.http
      .post<void>(url, otpPayload)
      .pipe(catchError((error: any) => throwError(error)));
  }

  verifyOtp(otpPayload: OtpPayload): Observable<void> {
    const url = `${this.apiBase}/verify-otp`;
    return this.http
      .post<void>(url, otpPayload)
      .pipe(catchError((error: any) => throwError(error)));
  }

  sendVerificationEmail(email: string): Observable<void> {
    const url = `${this.apiBase}/send/email-verification`;
    return this.http
      .post<void>(url, { email })
      .pipe(catchError((error: any) => throwError(error)));
  }

  verifyEmail(token: string): Observable<User> {
    const url = `${this.apiBase}/verify-email`;
    return this.http
      .post<User>(url, { token })
      .pipe(catchError((error: any) => throwError(error)));
  }

  requestPasswordReset(email: string): Observable<void> {
    const url = `${this.apiBase}/send/reset-password`;
    return this.http
      .post<void>(url, { email })
      .pipe(catchError((error: any) => throwError(error)));
  }

  verifyResetPasswordToken(token: string): Observable<User> {
    const url = `${this.apiBase}/verify-reset-password-token`;
    return this.http
      .post<User>(url, { token })
      .pipe(catchError((error: any) => throwError(error)));
  }

  resetPassword(email: string, password: string): Observable<void> {
    const url = `${this.apiBase}/reset-password`;
    return this.http
      .post<void>(url, { email, password })
      .pipe(catchError((error: any) => throwError(error)));
  }

  verifyUser(userId: string): Observable<void> {
    const url = `${this.apiBase}/verify-user`;
    return this.http
      .post<void>(url, { id: userId })
      .pipe(catchError((error: any) => throwError(error)));
  }

  // Token and Authentication Status Helpers

  isLoggedIn(): Observable<boolean> {
    return this.authState$;
  }

  getToken(): string | null {
    const account = this.loadLoggedAccount();
    return account ? account.token : null;
  }

  getUserId(): string | null {
    const token = this.getToken();
    if (!token) return null;

    try {
      const decoded: JwtPayload = jwtDecode<JwtPayload>(token);
      return decoded.nameid;
    } catch (error) {
      console.error('Error decoding JWT token:', error);
      return null;
    }
  }

  getUserRole(): string | null {
    const account = this.loadLoggedAccount();
    return account ? account.role.toString() : null;
  }

  private setLoggedAccount(account: LoggedAccount): void {
    this._loggedAccount = account;
    sessionStorage.setItem('loggedAccount', JSON.stringify(account));
    this.authStateSubject.next(true);
  }

  private clearLoggedAccount(): void {
    this._loggedAccount = null;
    sessionStorage.removeItem('loggedAccount');
    this.authStateSubject.next(false);
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
