import { Injectable } from '@angular/core';
import { BaseComponent } from '../ui/base-component';
import { BaseHttpService } from './base-http.service';
import { Observable, tap } from 'rxjs';

interface InstallationConfig {
  appServiceAddress?: string;
  disableAuth?: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class InstallationConfigurationService extends BaseComponent {
  constructor(private http: BaseHttpService) {
    super();
  }

  private _appServiceAddress: string = './';
  get appServiceAddress(): string {
    return this._appServiceAddress || './';
  }

  private _disableAuth: boolean = true;
  get disableAuth(): boolean {
    return this._disableAuth;
  }

  loadConfig(): Observable<InstallationConfig> {
    return this.http.get<InstallationConfig>('configs/config.json').pipe(
      tap((config: InstallationConfig) => {
        this._appServiceAddress = config.appServiceAddress || './';
        this._disableAuth = config.disableAuth || false;
        console.log(this.appServiceAddress, this.disableAuth);
      })
    );
  }
}
