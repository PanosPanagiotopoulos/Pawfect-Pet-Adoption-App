import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { HomeComponent } from './home.component';
import { NgIconsModule } from '@ng-icons/core';
import {
  lucideHeart,
  lucideSearch,
  lucideMessageCircle,
  lucidePhone,
  lucideMail,
  lucideMenu,
  lucideUser,
  lucideX,
  lucideHouse,
  lucideInfo
} from '@ng-icons/lucide';
import { HeaderComponent } from './shared/header/header.component';
import { FeatureCardComponent } from './feature-card/feature-card.component';
import { FooterComponent } from './footer/footer.component';
import { HeroSectionComponent } from './hero-section/hero-section.component';
import { AiMatchingSectionComponent } from './ai-matching-section/ai-matching-section.component';
import { AnimationDirective } from './shared/directives/animation.directive';

@NgModule({
  declarations: [
    HomeComponent,
    FeatureCardComponent,
    FooterComponent,
    HeroSectionComponent,
    AiMatchingSectionComponent,
    AnimationDirective
  ],
  imports: [
    CommonModule,
    RouterModule.forChild([{ path: '', component: HomeComponent }]),
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
      lucideInfo
    }),
    HeaderComponent
  ],
  exports: [HomeComponent]
})
export class HomeModule {}