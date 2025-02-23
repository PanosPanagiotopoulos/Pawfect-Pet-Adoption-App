import { Component } from '@angular/core';

@Component({
  selector: 'app-hero-section',
  template: `
    <div class="text-center mb-20 animate-fade-in">
      <h1 class="text-4xl sm:text-5xl md:text-6xl lg:text-7xl font-bold text-white mb-8 animate-gradient">
        Find Your <span class="gradient-text">Pawfect</span> Match
      </h1>
      <p class="text-lg sm:text-xl md:text-2xl text-gray-300 max-w-3xl mx-auto leading-relaxed animate-slide-up px-4">
        Discover your new best friend through our innovative pet matching
        system. Every swipe brings you closer to unconditional love.
      </p>
    </div>
  `
})
export class HeroSectionComponent {}