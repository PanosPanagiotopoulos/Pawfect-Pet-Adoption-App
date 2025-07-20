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
  lucideTriangle,
  lucidePawPrint,
  lucideCake,
  lucideDog,
  lucideHeartPulse,
  lucideScale,
  lucideActivity,
  lucideMapPinXInside,
  lucideMoon,
  lucideFacebook,
  lucideInstagram,
  lucideGlobe,
  lucideClock1,
  lucideMapPin,
  lucideMailbox,
  lucideChevronUp,
  lucideChevronDown,
  lucideHousePlus,
  lucideUsers, 
  lucideRuler, 
  lucideBuilding,
  lucideCircle,
  lucideCircleHelp,
  lucideEye,
  lucideEyeOff,
  lucideShapes,
  lucideVenetianMask,
  lucideCakeSlice,
  lucideFileText,
  lucideMap,
  lucideInbox,
  lucidePencil,
  lucideFilter,
  lucideArrowUpDown,
  lucideCircleX,
  lucideBadgeAlert,
  lucideListOrdered,
  lucideFileQuestion,
  lucideArrowDownWideNarrow,
  lucideArrowUpWideNarrow,
  lucideRefreshCw,
} from '@ng-icons/lucide';

import { AppComponent } from './app.component';
import { InstallationConfigurationService } from './common/services/installation-configuration.service';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { UnauthorizedInterceptor } from 'src/app/common/tools/unauthorised.interceptor';
import { BaseHttpService } from './common/services/base-http.service';
import { HeaderComponent } from './ui/components/home/shared/header/header.component';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { ErrorHandlerService } from './common/services/error-handler.service';
import { CookiesInterceptor } from './common/tools/cookies.interceptor';
import { ApiKeyInterceptor } from './common/tools/api-key.interceptor';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { TranslationService } from './common/services/translation.service';
import { LucideAngularModule, LucideMapPin, LucideUser, LucideFile, LucideCheck, LucideBuilding, LucidePawPrint, LucideAlertCircle, LucideInstagram, LucideFacebook, LucidePlus, LucideMailbox, LucideEye, LucideXOctagon } from 'lucide-angular';

export function initializeApp(
  installationConfigService: InstallationConfigurationService,
  translationService: TranslationService
) {
  return () => Promise.all([
    installationConfigService.loadConfig().toPromise(),
    translationService.initialize().toPromise()
  ]);
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
    MatSnackBarModule,
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
      lucidePencil,
      lucideMapPinXInside,
      lucideTriangle,
      lucidePawPrint,
      lucideCake,
      lucideDog,
      lucideHeartPulse,
      lucideScale,
      lucideActivity,
      lucideMoon,
      lucideFileText,
      lucideInbox,
      lucideFacebook,
      lucideInstagram,
      lucideGlobe,
      lucideClock1,
      lucideMapPin,
      lucideArrowUpDown,
      lucideMailbox,
      lucideChevronUp,
      lucideChevronDown,
      lucideHousePlus,
      lucideUsers, 
      lucideRuler, 
      lucideBuilding,
      lucideCircleHelp,
      lucideCircle,
      lucideEye,
      lucideEyeOff,
      lucideShapes,
      lucideVenetianMask,
      lucideCakeSlice,
      map: lucideMap,
      eyeOff: lucideEyeOff,
      file: lucideFile,
      inbox: lucideInbox,
      pencil: lucidePencil,
      lucideFilter,
      lucideCircleX,
      lucideBadgeAlert,
      lucideListOrdered,
      lucideFileQuestion,
      lucideArrowDownWideNarrow,
      lucideArrowUpWideNarrow,
      lucideRefreshCw,
    }),
    HeaderComponent,
    TranslatePipe,
  ],
  providers: [
    InstallationConfigurationService,
    TranslationService,
    BaseHttpService,
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [InstallationConfigurationService, TranslationService],
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: CookiesInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: ApiKeyInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: UnauthorizedInterceptor,
      multi: true,
    },
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
