import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnimationDirective } from '../shared/directives/animation.directive';
import { TranslationService } from 'src/app/common/services/translation.service';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { NgIconsModule } from '@ng-icons/core';

@Component({
  selector: 'app-hero-section',
  standalone: true,
  imports: [CommonModule, AnimationDirective, TranslatePipe, NgIconsModule],
  template: `
    <div class="text-center mb-20">
      <div>
        <ng-icon name="lucidePawPrint" style="width: 2rem; height: 2rem;" class="text-primary-400 animate-bounce ml-[-1.5rem]" aria-label="Paw Print"></ng-icon>
      </div>
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
    const title = this.translationService.translate('APP.HOME-PAGE.FIND_PET_TITLE') || '';

    return title.replace('Pawfect', '<span class="gradient-text">Pawfect</span>');
  }
}