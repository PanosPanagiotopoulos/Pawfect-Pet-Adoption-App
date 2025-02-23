import { NgModule } from '@angular/core';
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
  lucideMail,
  lucideLogOut,
} from '@ng-icons/lucide';

import { AppComponent } from './app.component';
import { HomeModule } from './ui/components/home/home.module';
import { InstallationConfigurationService } from './common/services/installation-configuration.service';
import { HttpClientModule } from '@angular/common/http';
import { BaseHttpService } from './common/services/base-http.service';
import { HeaderComponent } from './ui/components/home/shared/header/header.component';

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
    }),
    HomeModule,
    HeaderComponent,
  ],
  providers: [InstallationConfigurationService, BaseHttpService],
  bootstrap: [AppComponent],
})
export class AppModule {}
