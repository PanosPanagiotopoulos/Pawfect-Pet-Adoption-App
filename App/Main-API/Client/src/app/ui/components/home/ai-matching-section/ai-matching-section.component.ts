import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgIconsModule } from '@ng-icons/core';
import { AnimationDirective } from '../shared/directives/animation.directive';
import { lucideHeart, lucideMessageCircle } from '@ng-icons/lucide';

@Component({
  selector: 'app-ai-matching-section',
  standalone: true,
  imports: [CommonModule, NgIconsModule, AnimationDirective],
  template: `
    <div
      appAnimation
      [animationDelay]="0"
      class="bg-gray-800/50 backdrop-blur-lg border border-gray-700/50 rounded-3xl shadow-xl p-6 sm:p-8 md:p-12 mb-20 transform hover:scale-[1.02] transition-all duration-300"
      style="transform: scale(0.95);"
    >
      <div class="grid md:grid-cols-2 gap-8 md:gap-12 items-center">
        <div class="space-y-6 md:space-y-8">
          <h2
            appAnimation
            [animationDelay]="300"
            class="text-3xl md:text-4xl font-bold gradient-text"
            style="transform: translateX(-50px);"
          >
            Αντιστοίχιση με Τεχνητή Νοημοσύνη
          </h2>
          <div class="space-y-4 md:space-y-6">
            <div
              appAnimation
              [animationDelay]="400"
              class="flex items-start transform hover:-translate-y-1 transition-transform duration-300"
              style="transform: translateY(20px);"
            >
              <div class="flex-shrink-0">
                <div
                  class="flex items-center justify-center h-12 w-12 rounded-xl bg-gradient-to-br from-primary-500 to-accent-500 text-white shadow-lg"
                >
                  <ng-icon name="lucideHeart" [size]="'24'"></ng-icon>
                </div>
              </div>
              <div class="ml-4">
                <h3 class="text-lg md:text-xl font-semibold text-white">
                  Αντιστοίχιση Προτιμήσεων
                </h3>
                <p class="mt-2 text-sm md:text-base text-gray-400">
                  Το σύστημά μας χρησιμοποιεί έξυπνες τεχνικές και τεχνητή νοημοσύνη για να αναλύσει τις προτιμήσεις σας 
                  και να βρει το κατάλληλο κατοικίδιο για εσάς
                </p>
              </div>
            </div>
            <div
              appAnimation
              [animationDelay]="500"
              class="flex items-start transform hover:-translate-y-1 transition-transform duration-300"
              style="transform: translateY(20px);"
            >
              <div class="flex-shrink-0">
                <div
                  class="flex items-center justify-center h-12 w-12 rounded-xl bg-gradient-to-br from-secondary-500 to-primary-500 text-white shadow-lg"
                >
                  <ng-icon name="lucideMessageCircle" [size]="'24'"></ng-icon>
                </div>
              </div>
              <div class="ml-4">
                <h3 class="text-lg md:text-xl font-semibold text-white">
                  Άμεση Επικοινωνία
                </h3>
                <p class="mt-2 text-sm md:text-base text-gray-400">
                  Συνδεθείτε απευθείας με καταφύγια και ιδιοκτήτες κατοικιδίων μέσω του ασφαλούς συστήματος μηνυμάτων μας
                </p>
              </div>
            </div>
          </div>
        </div>
        <div
          appAnimation
          [animationDelay]="600"
          class="relative block"
          style="transform: translateX(50px);"
        >
          <div
            class="absolute inset-0 bg-gradient-to-br from-primary-500/20 to-accent-500/20 rounded-2xl transform rotate-6"
          ></div>
          <img
            src="https://images.unsplash.com/photo-1450778869180-41d0601e046e?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80"
            alt="Χαρούμενος σκύλος"
            class="rounded-2xl shadow-xl relative z-10 w-full h-auto object-cover"
          />
        </div>
      </div>
    </div>
  `,
})
export class AiMatchingSectionComponent {}