import { Component, Input } from '@angular/core';
import { NgIconsModule } from '@ng-icons/core';
import { CommonModule } from '@angular/common';
import { AnimationDirective } from '../shared/directives/animation.directive';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-feature-card',
  standalone: true,
  imports: [CommonModule, NgIconsModule, AnimationDirective, TranslatePipe],
  template: `
    <div
      appAnimation
      [animationDelay]="0"
      class="relative p-8 rounded-2xl shadow-xl transform hover:-translate-y-1 transition-all duration-300 overflow-hidden"
      style="transform: scale(0.95);"
    >
      <!-- Gradient background with animation -->
      <div
        class="absolute inset-0 bg-gradient-to-r from-gray-900/95 via-gray-800/95 to-gray-900/95 animate-gradient"
      ></div>
      <div
        class="absolute inset-0 bg-gradient-to-r from-primary-500/30 via-secondary-500/30 to-accent-500/30 animate-gradient-slow opacity-70"
      ></div>

      <!-- Content -->
      <div class="relative z-10">
        <div
          [class]="
            'rounded-full w-16 h-16 flex items-center justify-center mb-6 mx-auto ' +
            bgColor
          "
        >
          <ng-icon [name]="icon" [class]="iconColor" [size]="'32'"></ng-icon>
        </div>
        <h3 class="text-2xl font-semibold mb-4 text-white">{{ title }}</h3>
        <p class="text-gray-200 leading-relaxed">{{ description }}</p>
      </div>
    </div>
  `,
})
export class FeatureCardComponent {
  @Input() icon!: string;
  @Input() title!: string;
  @Input() description!: string;
  @Input() bgColor!: string;
  @Input() iconColor!: string;
  @Input() gradientClass!: string;
}
