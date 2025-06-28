import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnimationDirective } from '../shared/directives/animation.directive';
import { TranslationService } from 'src/app/common/services/translation.service';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-hero-section',
  standalone: true,
  imports: [CommonModule, AnimationDirective, TranslatePipe],
  template: `
    <div class="text-center mb-20">
      <h1
        appAnimation
        [animationDelay]="200"
        class="text-4xl sm:text-5xl md:text-6xl lg:text-7xl font-bold text-white mb-8"
        style="transform: scale(0.95);"
        [innerHTML]="getTitleWithGradient()"
      >
      </h1>
      <p
        appAnimation
        [animationDelay]="400"
        class="text-lg sm:text-xl md:text-2xl text-gray-300 max-w-3xl mx-auto leading-relaxed px-4"
        style="transform: translateY(20px);"
      >
        {{ 'APP.HOME-PAGE.FIND_PET_DESC' | translate }}
      </p>
    </div>
  `,
})
export class HeroSectionComponent {
  constructor(private translationService: TranslationService) {}

  getTitleWithGradient(): string {
    // Get the translated title
    const title = this.translationService.translate('APP.HOME-PAGE.FIND_PET_TITLE') || '';
    // Replace 'Pawfect' with a span for both EN and GR
    // Greek: 'Βρείτε το Pawfect ζωάκι σας'
    // English: 'Find your Pawfect pet'
    return title.replace('Pawfect', '<span class="gradient-text">Pawfect</span>');
  }
}