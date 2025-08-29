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
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { AboutComponent } from './about/about.component';
import { ContactComponent } from './contact/contact.component';

@NgModule({
  declarations: [HomeComponent],
  imports: [
    CommonModule,
    RouterModule.forChild([
      { path: '', component: HomeComponent },
      { path: 'about', component: AboutComponent },
      { path: 'contact', component: ContactComponent },
      { path: '**', component: NotFoundComponent },
    ]),
    NgIconsModule,
    HeaderComponent,
    FooterComponent,
    FeatureCardComponent,
    HeroSectionComponent,
    AiMatchingSectionComponent,
    AnimationDirective,
    TranslatePipe,
    AboutComponent,
    ContactComponent
  ],
  exports: [HomeComponent],
})
export class HomeModule {}