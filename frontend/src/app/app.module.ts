import { NgModule, APP_INITIALIZER } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { routes } from './app.routes';
import { NgIconsModule } from '@ng-icons/core';
import {
  lucideInfo,
  lucideMapPin,
  lucideMenu,
  lucideX,
  lucideHeart,
  lucideUser,
  lucideSearch,
  lucideMessageCircle,
  lucideHouse,
  lucidePhone,
  lucideMail,
  lucideLogOut,
  lucideClock,
  lucideUpload,
  lucideFile,
  lucideCheck,
} from '@ng-icons/lucide';

import { AppComponent } from './app.component';
import { HomeModule } from './ui/components/home/home.module';
import { InstallationConfigurationService } from './common/services/installation-configuration.service';
import { HttpClientModule } from '@angular/common/http';
import { BaseHttpService } from './common/services/base-http.service';
import { HeaderComponent } from './ui/components/home/shared/header/header.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

// Factory function for APP_INITIALIZER
export function initializeApp(
  installationConfigService: InstallationConfigurationService
) {
  return () => installationConfigService.loadConfig().toPromise();
}

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    RouterModule.forRoot(routes, {
      scrollPositionRestoration: 'enabled',
      anchorScrolling: 'enabled',
    }),
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    MatFormFieldModule,
    MatInputModule,
    NgIconsModule.withIcons({
      lucideHeart,
      lucideSearch,
      lucideMessageCircle,
      lucidePhone,
      lucideMail,
      lucideMenu,
      lucideUser,
      lucideX,
      lucideHouse,
      lucideInfo,
      lucideLogOut,
      lucideClock,
      lucideUpload,
      lucideFile,
      lucideCheck,
    }),
    HomeModule,
    HeaderComponent,
  ],
  providers: [
    InstallationConfigurationService,
    BaseHttpService,
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [InstallationConfigurationService],
      multi: true,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
