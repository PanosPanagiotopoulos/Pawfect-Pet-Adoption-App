import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AnimationDirective } from '../shared/directives/animation.directive';

@Component({
  selector: 'app-hero-section',
  standalone: true,
  imports: [CommonModule, AnimationDirective],
  template: `
    <div class="text-center mb-20">
      <h1
        appAnimation
        [animationDelay]="200"
        class="text-4xl sm:text-5xl md:text-6xl lg:text-7xl font-bold text-white mb-8"
        style="transform: scale(0.95);"
      >
        Find Your <span class="gradient-text">Pawfect</span> Match
      </h1>
      <p
        appAnimation
        [animationDelay]="400"
        class="text-lg sm:text-xl md:text-2xl text-gray-300 max-w-3xl mx-auto leading-relaxed px-4"
        style="transform: translateY(20px);"
      >
        Discover your new best friend through our innovative pet matching
        system. Every swipe brings you closer to unconditional love.
      </p>
    </div>
  `,
})
export class HeroSectionComponent {}
