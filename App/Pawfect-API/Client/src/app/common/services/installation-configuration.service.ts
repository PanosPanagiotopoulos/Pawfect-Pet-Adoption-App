import { Injectable, isDevMode } from '@angular/core';
import { Observable, of } from 'rxjs';
import { environment as envDev } from 'src/environments/environment.Development';
import { environment as envProd } from 'src/environments/environment.Production';

@Injectable({
  providedIn: 'root',
})
export class InstallationConfigurationService {
  private configLoaded = true;
  private config: any;

  constructor() {
    this.initializeConfig();
  }

  get appServiceAddress(): string {
    return this.config.appServiceAddress;
  }

  get notificationsServiceAddress(): string {
    return this.config.notificationsServiceAddress;
  }

  get messengerServiceAddress(): string {
    return this.config.messengerServiceAddress;
  }

  get disableAuth(): boolean {
    return this.config.disableAuth;
  }

  get googleClientId(): string {
    return this.config.googleClientId;
  }

  get baseGoogleEndpoint(): string {
    return this.config.baseGoogleEndpoint;
  }

  get redirectUri(): string {
    return this.config.redirectUri;
  }

  get storageAccountKey(): string {
    return this.config.storageAccountKey;
  }

  get encryptKey(): string {
    return this.config.encryptKey;
  }

  get mainAppApiKey(): string {
    return this.config.mainAppApiKey;
  }

  loadConfig(): Observable<any> {
    // Return the environment configuration immediately
    return of({ production: !isDevMode() });
  }

  isConfigLoaded(): boolean {
    return this.configLoaded;
  }

  waitForConfig(): Promise<void> {
    // No async operation needed, config is immediately available
    return Promise.resolve();
  }

  private initializeConfig(): void {
    const isProduction = !isDevMode();
    const selectedEnv = isProduction ? envProd : envDev;
    
    this.config = {
      appServiceAddress: selectedEnv.appServiceAddress,
      notificationsServiceAddress: selectedEnv.notificationsServiceAddress,
      messengerServiceAddress: selectedEnv.messengerServiceAddress,
      disableAuth: selectedEnv.disableAuth,
      googleClientId: selectedEnv.googleClientId,
      baseGoogleEndpoint: selectedEnv.baseGoogleEndpoint,
      redirectUri: `${window.location.origin}${selectedEnv.redirectPath}`,
      storageAccountKey: selectedEnv.storageAccountKey,
      encryptKey: selectedEnv.encryptKey,
      mainAppApiKey: selectedEnv.mainAppApiKey
    };

    // Log configuration in development only
    if (isDevMode()) {
      console.log('Configuration initialized:', {
        isDevMode: isDevMode(),
        isProduction: isProduction,
        selectedEnvironment: isProduction ? 'Production' : 'Development',
        appServiceAddress: this.config.appServiceAddress,
        notificationsServiceAddress: this.config.notificationsServiceAddress,
        messengerServiceAddress: this.config.messengerServiceAddress,
        disableAuth: this.config.disableAuth
      });
    }
  }
}