import {
  NgModule,
  CUSTOM_ELEMENTS_SCHEMA,
  APP_INITIALIZER,
} from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
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
  lucideMail
} from '@ng-icons/lucide';

import { AppComponent } from './app.component';
import { HomeModule } from './ui/components/home/home.module';
import { lastValueFrom } from 'rxjs';
import { InstallationConfigurationService } from './common/services/installation-configuration.service';
import { HttpClientModule } from '@angular/common/http';
import { BaseHttpService } from './common/services/base-http.service';
import { HeaderComponent } from './ui/components/home/shared/header/header.component';

// Configurations
export function initializeApp(configService: InstallationConfigurationService) {
  return () => lastValueFrom(configService.loadConfig());
}

@NgModule({
  declarations: [AppComponent],
  imports: [
    BrowserModule,
    RouterModule.forRoot(routes, {
      scrollPositionRestoration: 'enabled', 
      anchorScrolling: 'enabled', 
    }),
    CommonModule,
    FormsModule,
    HttpClientModule,
    ReactiveFormsModule,
    NgIconsModule.withIcons({
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
      lucideMail
    }),
    HomeModule,
    HeaderComponent
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
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AppModule {}