import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NgIconsModule } from '@ng-icons/core';
import { CommonModule } from '@angular/common';
import {
  lucidePhone,
  lucideMail,
  lucideFacebook,
  lucideInstagram,
} from '@ng-icons/lucide';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-footer',
  standalone: true,
  imports: [CommonModule, RouterModule, NgIconsModule, TranslatePipe],
  template: `
    <div class="relative bg-gray-900 text-white py-12 border-t border-gray-800">
      <div class="max-w-7xl mx-auto px-4">
        <!-- Logo Section -->
        <div class="flex items-center md:-ml-8 mb-8 -mx-9 sm:mx-0">
          <div class="flex items-center justify-start w-full px-4 sm:px-0">
            <div class="relative flex-shrink-0">
              <div class="flex items-center justify-center">
                <img
                  src="https://ibb.co/mF2MZyKz"
                  alt="Pawfect Logo"
                  class="object-contain w-24 h-24 sm:w-20 sm:h-20 md:w-24 md:h-24"
                />
              </div>
            </div>
            <h2 class="text-2xl sm:text-3xl md:text-4xl font-bold flex-1">
              <span
                class="bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 bg-clip-text text-transparent"
              >
                Pawfect
              </span>
            </h2>
          </div>
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-8">
          <div>
            <h3 class="text-xl font-semibold mb-4">
              {{ 'APP.HOME-PAGE.FOOTER_BRAND' | translate }}
            </h3>
            <p class="text-gray-400">
              {{ 'APP.HOME-PAGE.FOOTER_SLOGAN' | translate }}
            </p>
          </div>
          <div>
            <h4 class="text-lg font-medium mb-4">
              {{ 'APP.HOME-PAGE.FOOTER_QUICK_LINKS' | translate }}
            </h4>
            <ul class="space-y-2">
              <li>
                <a
                  routerLink="/about"
                  class="text-gray-400 hover:text-white transition-colors"
                  >{{ 'APP.COMMONS.ABOUT_US' | translate }}</a
                >
              </li>
              <li>
                <a
                  routerLink="/contact"
                  class="text-gray-400 hover:text-white transition-colors"
                  >{{ 'APP.COMMONS.CONTACT' | translate }}</a
                >
              </li>
            </ul>
          </div>
          <div>
            <h4 class="text-lg font-medium mb-4">
              {{ 'APP.HOME-PAGE.FOOTER_CONTACT_US' | translate }}
            </h4>
            <ul class="space-y-2 text-gray-400">
              <li class="flex items-center">
                <ng-icon
                  name="lucidePhone"
                  [size]="'24'"
                  class="mr-2"
                ></ng-icon>
                {{ 'APP.HOME-PAGE.FOOTER_PHONE' | translate }}
              </li>
              <li class="flex items-center">
                <ng-icon name="lucideMail" [size]="'24'" class="mr-2"></ng-icon>
                {{ 'APP.HOME-PAGE.FOOTER_EMAIL' | translate }}
              </li>
            </ul>
          </div>
          <div>
            <h4 class="text-lg font-medium mb-4">
              {{ 'APP.HOME-PAGE.FOOTER_FOLLOW_US' | translate }}
            </h4>
            <div class="flex space-x-4">
              <a
                href="https://facebook.com/pawfectpetadoption"
                target="_blank"
                rel="noopener noreferrer"
                class="text-gray-400 hover:text-blue-400 transition-colors"
                [attr.aria-label]="'APP.CONTACT.SOCIAL_FACEBOOK' | translate"
              >
                <ng-icon name="lucideFacebook" [size]="'24'"></ng-icon>
              </a>
              <a
                href="https://instagram.com/pawfectpets_official"
                target="_blank"
                rel="noopener noreferrer"
                class="text-gray-400 hover:text-pink-400 transition-colors"
                [attr.aria-label]="'APP.CONTACT.SOCIAL_INSTAGRAM' | translate"
              >
                <ng-icon name="lucideInstagram" [size]="'24'"></ng-icon>
              </a>
            </div>
          </div>
        </div>
        <div
          class="mt-8 pt-8 border-t border-gray-800 text-center text-gray-400"
        >
          <p>
            &copy; {{ currentYear }}
            {{ 'APP.HOME-PAGE.FOOTER_BRAND' | translate }}.
            {{ 'APP.HOME-PAGE.FOOTER_COPYRIGHT' | translate }}
          </p>
        </div>
      </div>
    </div>
  `,
})
export class FooterComponent {
  currentYear = new Date().getFullYear();
}
