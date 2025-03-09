import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgIconsModule } from '@ng-icons/core';
import { HomeComponent } from './home.component';
import { FooterComponent } from './footer/footer.component';
import { HeroSectionComponent } from './hero-section/hero-section.component';
import { AiMatchingSectionComponent } from './ai-matching-section/ai-matching-section.component';
import { FeatureCardComponent } from './feature-card/feature-card.component';
import { HeaderComponent } from './shared/header/header.component';
import { AnimationDirective } from './shared/directives/animation.directive';
import { NotFoundComponent } from '../not-found/not-found.component';

@NgModule({
  declarations: [HomeComponent],
  imports: [
    CommonModule,
    RouterModule.forChild([
      { path: '', component: HomeComponent },
      { path: '**', component: NotFoundComponent },
    ]),
    NgIconsModule,
    HeaderComponent,
    FooterComponent,
    FeatureCardComponent,
    HeroSectionComponent,
    AiMatchingSectionComponent,
    AnimationDirective,
  ],
  exports: [HomeComponent],
})
export class HomeModule {}