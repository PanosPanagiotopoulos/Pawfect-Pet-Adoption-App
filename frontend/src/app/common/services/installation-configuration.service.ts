import { Injectable } from '@angular/core';
import { BaseHttpService } from './base-http.service';
import { Observable, forkJoin, map, tap, catchError, of } from 'rxjs';

interface InstallationConfig {
  appServiceAddress?: string;
  disableAuth?: boolean;
  googleClientId?: string;
  googleClientSecret?: string;
}

interface EnvironmentConfig {
  googleClientId: string;
  googleClientSecret: string;
}

@Injectable({
  providedIn: 'root',
})
export class InstallationConfigurationService {
  constructor(private readonly http: BaseHttpService) {}

  private _appServiceAddress: string = './';
  get appServiceAddress(): string {
    return this._appServiceAddress || './';
  }

  private _disableAuth: boolean = true;
  get disableAuth(): boolean {
    return this._disableAuth;
  }

  private _googleClientId: string = '';
  get googleClientId(): string {
    return this._googleClientId;
  }

  private _googleClientSecret: string = '';
  get googleClientSecret(): string {
    return this._googleClientSecret;
  }

  loadConfig(): Observable<InstallationConfig> {
    // Load both configuration files in parallel
    const configFile$ = this.http.get<InstallationConfig>('configs/config.json').pipe(
      catchError(error => {
        console.warn('Failed to load config.json:', error);
        return of({} as InstallationConfig);
      })
    );
    
    const environmentFile$ = this.http.get<EnvironmentConfig>('configs/environment.json').pipe(
      catchError(error => {
        console.warn('Failed to load environment.json:', error);
        return of({} as EnvironmentConfig);
      })
    );

    return forkJoin({
      config: configFile$,
      environment: environmentFile$,
    }).pipe(
      map((result) => {
        // Combine the configurations
        const combinedConfig: InstallationConfig = {
          ...result.config,
          googleClientId: result.environment.googleClientId,
          googleClientSecret: result.environment.googleClientSecret,
        };
        return combinedConfig;
      }),
      tap((config: InstallationConfig) => {
        // Set all configuration values with fallbacks
        this._appServiceAddress = config.appServiceAddress ?? './';
        this._disableAuth = config.disableAuth ?? false;
        this._googleClientId = config.googleClientId ?? '';
        this._googleClientSecret = config.googleClientSecret ?? '';
        
      }),
      catchError(error => {
        console.error('Error loading configuration:', error);
        return of({} as InstallationConfig);
      })
    );
  }
}