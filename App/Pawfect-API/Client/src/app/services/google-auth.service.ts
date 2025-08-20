import { Injectable } from '@angular/core';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Router } from '@angular/router';
import { SecureStorageService } from '../common/services/secure-storage.service';

export interface GoogleOAuthConfig {
  clientId: string;
  redirectUri: string;
  scope: string;
  baseGoogleEndpoint: string;
}

@Injectable({
  providedIn: 'root',
})
export class GoogleAuthService {
  private readonly defaultScope = 'email profile';
  private readonly signupScope =
    'email profile https://www.googleapis.com/auth/user.addresses.read https://www.googleapis.com/auth/user.phonenumbers.read';

  constructor(
    private readonly installationConfig: InstallationConfigurationService,
    private readonly router: Router,
    private readonly secureStorageService: SecureStorageService
  ) {}

  getAuthUrl(isSignup: boolean = false, state: string = ''): string {
    const config = this.getOAuthConfig(isSignup);
    const params = new URLSearchParams({
      client_id: config.clientId,
      redirect_uri: config.redirectUri,
      response_type: 'code',
      scope: config.scope,
      state: state || this.generateState(isSignup),
      prompt: 'consent',
      access_type: 'offline',
    });

    return `${config.baseGoogleEndpoint}?${params.toString()}`;
  }

  private getOAuthConfig(isSignup: boolean): GoogleOAuthConfig {
    const clientId = this.installationConfig.googleClientId;
    if (!clientId) {
      throw new Error('Google Client ID not configured');
    }

    const redirectUri = this.installationConfig.redirectUri;
    const baseGoogleEndpoint = this.installationConfig.baseGoogleEndpoint;

    if (!redirectUri || !baseGoogleEndpoint) {
      throw new Error('Google OAuth configuration is incomplete');
    }

    return {
      clientId,
      redirectUri,
      scope: isSignup ? this.signupScope : this.defaultScope,
      baseGoogleEndpoint,
    };
  }

  private generateState(isSignup: boolean): string {
    // Try to get returnUrl from current query params
    let returnUrl = '';
    try {
      const urlParams = new URLSearchParams(window.location.search);
      returnUrl = urlParams.get('returnUrl') || '';
    } catch {}
    const stateObj: any = {
      timestamp: Date.now(),
      isSignup,
      origin: window.location.pathname,
    };
    if (returnUrl) {
      stateObj.returnUrl = returnUrl;
    }
    return btoa(JSON.stringify(stateObj));
  }

  handleAuthCallback(params: URLSearchParams): void {
    const code = params.get('code');
    const state = params.get('state');
    const error = params.get('error');

    if (error) {
      console.error('Google OAuth error:', error);
      this.router.navigate(['/auth/login']);
      return;
    }

    if (!code || !state) {
      console.error('Invalid OAuth callback');
      this.router.navigate(['/auth/login']);
      return;
    }

    try {
      const decodedState = JSON.parse(atob(state));

      const origin: string | null = decodedState.origin as string;
      const returnUrl: string | null = decodedState.returnUrl || null;
      // Store the auth code temporarily
      this.secureStorageService.setItem('googleAuthCode', code);

      // Redirect back to the original page
      this.router.navigate(
        [origin.includes('sign-up') ? '/auth/sign-up' : '/auth/login'],
        {
          queryParams: returnUrl ? { mode: 'google', returnUrl } : { mode: 'google' },
        }
      );
    } catch (e) {
      console.error('Invalid state parameter:', e);
      this.router.navigate(['/auth/login']);
    }
  }

  // Add methods for initiating login and signup
  initiateLogin(): void {
    const authUrl = this.getAuthUrl(false);
    window.location.href = authUrl;
  }

  initiateSignup(): void {
    const authUrl = this.getAuthUrl(true);
    window.location.href = authUrl;
  }

  // Helper method to check if we have a pending Google auth code
  hasPendingAuth(): boolean {
    return !!this.secureStorageService.getItem<string>('googleAuthCode');
  }

  // Get and clear the pending auth code
  getPendingAuthCode(): string | null {
    const code = this.secureStorageService.getItem<string>('googleAuthCode');
    if (code) {
      this.secureStorageService.removeItem('googleAuthCode');
    }
    return code;
  }
}
