import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { map, catchError, shareReplay } from 'rxjs/operators';
import { environment as envDev } from 'src/environments/environment.Development';
import { environment as envProd } from 'src/environments/environment.Production';

interface AppConfig {
  production: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class InstallationConfigurationService {
  private configLoaded = false;
  private config: any;

  constructor(private readonly http: HttpClient) {}

  private _appServiceAddress: string = './';
  get appServiceAddress(): string {
    return this._appServiceAddress || './';
  }

  private _notificationsServiceAddress: string = './';
  get notificationsServiceAddress(): string {
    return this._notificationsServiceAddress || './';
  }

  private _disableAuth: boolean = true;
  get disableAuth(): boolean {
    return this._disableAuth;
  }

  private _googleClientId: string = '';
  get googleClientId(): string {
    return this._googleClientId;
  }

  private _baseGoogleEndpoint: string = '';
  get baseGoogleEndpoint(): string {
    return this._baseGoogleEndpoint;
  }

  private _redirectUri: string = '';
  get redirectUri(): string {
    return this._redirectUri;
  }

  private _storageAccountKey: string = '';
  get storageAccountKey(): string {
    return this._storageAccountKey;
  }

  private _encryptKey: string = '';
  get encryptKey(): string {
    return this._encryptKey;
  }

  private _mainAppApiKey: string = '';
  get mainAppApiKey(): string {
    return this._mainAppApiKey;
  }

  private readonly config$ = this.http.get<AppConfig>('/configs/config.json').pipe(
    map((appConfig) => {
      this.applyRuntimeConfig(appConfig);
      return appConfig;
    }),
    catchError((error) => {
      console.log('Failed to load config.json, using dev environment\n', JSON.stringify(error, null, 2));
      const fallback: AppConfig = { production: false };
      this.applyRuntimeConfig(fallback);
      return of(fallback);
    }),
    shareReplay(1) // Cache the response for all future subscribers
  );

  loadConfig(): Observable<AppConfig> {
    return this.config$;
  }

  isConfigLoaded(): boolean {
    return this.configLoaded;
  }

  waitForConfig(): Promise<void> {
    return this.loadConfig().toPromise().then(() => {});
  }

  private applyRuntimeConfig(appConfig: AppConfig): void {
    const envConfig = appConfig.production ? envProd : envDev;

    this.config = envConfig;
    this._appServiceAddress = envConfig.appServiceAddress;
    this._notificationsServiceAddress = envConfig.notificationsServiceAddress;
    this._disableAuth = envConfig.disableAuth;
    this._googleClientId = envConfig.googleClientId;
    this._baseGoogleEndpoint = envConfig.baseGoogleEndpoint;
    this._redirectUri = `${window.location.origin}${envConfig.redirectPath}`;
    this._storageAccountKey = envConfig.storageAccountKey;
    this._encryptKey = envConfig.encryptKey;
    this._mainAppApiKey = envConfig.mainAppApiKey;
    this.configLoaded = true;
  }
}
