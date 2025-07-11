import { Injectable } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { BehaviorSubject, Observable, throwError, Subject, shareReplay } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { User } from '../models/user/user.model';
import {
  LoggedAccount,
  LoginPayload,
  RegisterPayload,
  OtpPayload,
} from '../models/auth/auth.model';
import { SecureStorageService } from '../common/services/secure-storage.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly authStateSubject = new BehaviorSubject<boolean>(false);
  public authState$ = this.authStateSubject.asObservable();
  private meRequest$: Observable<LoggedAccount> | null = null;

  constructor(
    private readonly installationConfiguration: InstallationConfigurationService,
    private readonly http: BaseHttpService,
    private readonly secureStorage: SecureStorageService
  ) {
    // Initialize auth state after configuration is loaded
    this.installationConfiguration.waitForConfig().then(() => {
      const loggedAccount = this.secureStorage.getItem<LoggedAccount>(this.installationConfiguration.storageAccountKey);
      if (loggedAccount) {
        this.authStateSubject.next(true);
      }
    }).catch(error => {
      console.warn('Failed to load configuration, skipping auth state initialization:', error);
    });
  }

  private get apiBase(): string {
    return `${this.installationConfiguration.appServiceAddress}auth`;
  }

  // Authentication functions
  login(email: string, password: string): Observable<LoggedAccount> {
    const url = `${this.apiBase}/login`;
    const payload: LoginPayload = { email, password, loginProvider: 1 };

    this.secureStorage.removeItem(this.installationConfiguration.storageAccountKey);

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
    
    this.secureStorage.removeItem(this.installationConfiguration.storageAccountKey);

    return this.http.post<LoggedAccount>(url, payload).pipe(
      tap((response: LoggedAccount) => {
        if (response.isVerified) {
          this.setLoggedAccount(response);
        }
      }),
      catchError((error: any) => throwError(error))
    );
  }

  refresh(): Observable<LoggedAccount> {
    const url = `${this.apiBase}/refresh`;

    return this.http.post<LoggedAccount>(url).pipe(
      tap((response: LoggedAccount) => {
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
      catchError((error: any) => {
        this.clearLoggedAccount();
        return throwError(() => error);
      })
    );
  }

  me(): Observable<LoggedAccount> {
    // If there's already a me request in progress, return that
    if (this.meRequest$) {
      return this.meRequest$;
    }

    // Create new me request
    const url = `${this.apiBase}/me`;
    this.meRequest$ = this.http.post<LoggedAccount>(url).pipe(
      tap((response: LoggedAccount) => {
        if (response.isVerified) {
          this.setLoggedAccount(response);
        } else {
          this.clearLoggedAccount();
        }
        // Clear the request after completion
        this.meRequest$ = null;
      }),
      catchError((error: any) => {
        this.clearLoggedAccount();
        // Clear the request after error
        this.meRequest$ = null;
        return throwError(error);
      }),
      // Share the result with all subscribers
      shareReplay(1)
    );

    return this.meRequest$;
  }

  register(registerData: RegisterPayload): Observable<User> {
    const url = `${this.apiBase}/register/unverified`;
    return this.http
      .post<User>(url, registerData)
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

  isLoggedInSync(): boolean {
    return this.authStateSubject.value;
  }

  getUserEmail(): string | null {
    return this.loadLoggedAccount()?.email ?? null;
  }

  hasPermission(permission: string): boolean {
    const permissions = this.loadLoggedAccount()?.permissions || [];
    return permissions.includes(permission);
  }

  hasAnyPermission(permissions: string[]): boolean {
    const userPermissions = this.loadLoggedAccount()?.permissions || [];
    return permissions.some((permission) => userPermissions.includes(permission));
  }

  getUserRoles(): string[] | null {
    return this.loadLoggedAccount()?.roles || null;
  }

  private setLoggedAccount(account: LoggedAccount): void {
    this.secureStorage.setItem(this.installationConfiguration.storageAccountKey, account);
    this.authStateSubject.next(true);
  }

  private clearLoggedAccount(): void {
    this.secureStorage.removeItem(this.installationConfiguration.storageAccountKey);
    this.authStateSubject.next(false);
  }

  private loadLoggedAccount(): LoggedAccount | null {
    return this.secureStorage.getItem<LoggedAccount>(this.installationConfiguration.storageAccountKey);
  }
}
