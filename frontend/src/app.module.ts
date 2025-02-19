import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { routes } from './app.routes';
import { NgIconsModule } from '@ng-icons/core';

import {
  lucideHeart,
  lucideX,
  lucideSearch,
  lucideHeart as lucideLove, // alias for the Love icon
  lucideMapPin,
  lucidePawPrint,
  lucideInfo,
} from '@ng-icons/lucide';

import { AppComponent } from './app.component';
import { HeaderComponent } from './components/header/header.component';
import { PetCardComponent } from './components/pet-card/pet-card.component';
import { SwipeComponent } from './components/swipe/swipe.component';
import { HomeComponent } from './components/home/home.component';

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    PetCardComponent,
    SwipeComponent,
    HomeComponent,
  ],
  imports: [
    BrowserModule,
    RouterModule.forRoot(routes),
    CommonModule,
    FormsModule,
    NgIconsModule.withIcons({
      heart: lucideHeart,
      x: lucideX,
      search: lucideSearch,
      love: lucideLove,
      mapPin: lucideMapPin,
      paw: lucidePawPrint,
      info: lucideInfo
    }),
  ],
  bootstrap: [AppComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AppModule {}
