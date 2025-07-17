import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NgIconsModule } from '@ng-icons/core';
import { UserAvatarComponent } from '../user-avatar/user-avatar.component';
import { User } from 'src/app/models/user/user.model';
import { TranslationService, SupportedLanguage, LanguageOption } from 'src/app/common/services/translation.service';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-mobile-menu',
  standalone: true,
  imports: [CommonModule, RouterLink, NgIconsModule, UserAvatarComponent, TranslatePipe],
  template: `
    <div
      class="fixed inset-0 bg-black/20 backdrop-blur-sm transition-opacity duration-300"
      [class.opacity-100]="isOpen"
      [class.opacity-0]="!isOpen"
      [class.pointer-events-auto]="isOpen"
      [class.pointer-events-none]="!isOpen"
      (click)="close.emit()"
    ></div>

    <div
      class="fixed right-0 top-0 h-full w-[80%] max-w-sm bg-gradient-to-b from-gray-900 to-gray-800 transform transition-transform duration-300 ease-out shadow-2xl"
      [class.translate-x-0]="isOpen"
      [class.translate-x-full]="!isOpen"
    >
      <!-- Header -->
      <div class="flex items-center justify-end p-4 border-b border-gray-700">
        <button
          (click)="close.emit()"
          class="p-2 text-gray-400 hover:text-white transition-colors rounded-lg hover:bg-white/10"
        >
          <ng-icon name="lucideX" [size]="'24'"></ng-icon>
        </button>
      </div>

      <!-- User Profile Section (if logged in) -->
      <div *ngIf="isLoggedIn" class="p-4 border-b border-gray-700">
        <a
          routerLink="/profile"
          (click)="close.emit()"
          class="flex items-center space-x-3 p-3 rounded-xl text-gray-300 hover:text-white hover:bg-white/10 transition-all duration-300 group"
        >
          <app-user-avatar
            [imageUrl]="currentUser?.profilePhoto?.sourceUrl"
            [name]="currentUser?.fullName"
          >
          </app-user-avatar>
          <div>
            <p class="text-white font-medium">
              {{ currentUser?.fullName || ('APP.COMMONS.USER' | translate) }}
            </p>
            <p class="text-gray-400 text-sm">{{ 'APP.COMMONS.VIEW_PROFILE' | translate }}</p>
          </div>
        </a>
      </div>

      <!-- Navigation Links -->
      <nav class="p-4 space-y-2">
        <ng-container *ngFor="let item of menuItems">
          <a
            [routerLink]="item.route"
            (click)="close.emit()"
            class="flex items-center space-x-3 p-3 rounded-xl text-gray-300 hover:text-white hover:bg-white/10 transition-all duration-300 group"
          >
            <div
              class="flex items-center justify-center w-10 h-10 rounded-lg bg-gradient-to-br"
              [class]="item.gradient"
            >
              <ng-icon
                [name]="item.icon"
                [size]="'20'"
                class="transform group-hover:scale-110 transition-transform"
              ></ng-icon>
            </div>
            <span class="font-medium">{{ item.label | translate }}</span>
          </a>
        </ng-container>
      </nav>

      <!-- Auth Section -->
      <div
        class="absolute bottom-0 left-0 right-0 p-4 border-t border-gray-700"
      >
        <!-- Language Selector (bottom, width fits content) -->
        <div class="relative mb-3 mx-auto w-fit" role="group" aria-label="Language switcher">
          <button (click)="toggleLangDropdown()"
                  class="flex items-center gap-1 px-3 py-1 rounded-md bg-primary-700 text-white font-medium text-sm hover:bg-primary-800 transition-colors outline-none focus:ring-0"
                  aria-haspopup="listbox"
                  [attr.aria-expanded]="showLangDropdown">
            <span *ngIf="currentLanguageObj">
              <span *ngIf="currentLanguageObj.flag" aria-hidden="true" class="text-base">{{ currentLanguageObj.flag }}</span>
              <span> {{ currentLanguageObj.label }}</span>
            </span>
            <svg class="ml-1 w-4 h-4 text-white opacity-80" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24" stroke-linecap="round" stroke-linejoin="round"><path d="M18 15l-6-6-6 6"/></svg>
          </button>
          <ul class="absolute left-1/2 translate-x-[-50%] bottom-full mb-1 w-fit min-w-full bg-primary-700 rounded-md z-50 border border-primary-800 py-1 text-white transition-all duration-200 ease-in-out pointer-events-none"
              [class.opacity-100]="showLangDropdown"
              [class.opacity-0]="!showLangDropdown"
              [class.transform]="true"
              [class.scale-100]="showLangDropdown"
              [class.scale-95]="!showLangDropdown"
              [class.pointer-events-auto]="showLangDropdown"
              [class.pointer-events-none]="!showLangDropdown"
              role="listbox">
            <li *ngFor="let lang of supportedLanguages"
                (click)="selectLanguage(lang.code)"
                [class.bg-primary-600]="currentLanguage === lang.code"
                [class.bg-primary-700]="currentLanguage !== lang.code"
                [class.text-white]="currentLanguage === lang.code"
                [class.text-gray-300]="currentLanguage !== lang.code"
                [class.border-l-4]="currentLanguage === lang.code"
                [class.border-primary-400]="currentLanguage === lang.code"
                class="flex items-center gap-2 px-3 py-2 cursor-pointer hover:bg-primary-600 focus:bg-primary-600 transition-all duration-200 text-sm relative"
                [attr.aria-selected]="currentLanguage === lang.code"
                role="option">
              <span *ngIf="lang.flag" aria-hidden="true" class="text-base">{{ lang.flag }}</span>
              <span class="font-medium">{{ lang.label }}</span>
              <ng-icon *ngIf="currentLanguage === lang.code" 
                      name="lucideCheck" 
                      class="ml-auto text-primary-300" 
                      [size]="'16'">
              </ng-icon>
            </li>
          </ul>
        </div>
        <!-- End Language Selector -->
        <ng-container *ngIf="!isLoggedIn; else logoutButton">
          <button
            (click)="navigateToLogin()"
            [disabled]="isLoginRoute()"
            class="flex items-center justify-center space-x-2 w-full p-3 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-xl hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 transform hover:-translate-y-1 disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:translate-y-0 disabled:hover:shadow-none"
          >
            <ng-icon name="lucideUser" [size]="'20'"></ng-icon>
            <span class="font-medium">{{ 'APP.COMMONS.LOGIN' | translate }}</span>
          </button>
        </ng-container>

        <ng-template #logoutButton>
          <button
            (click)="onLogout()"
            class="flex items-center justify-center space-x-2 w-full p-3 border border-red-500/30 text-red-500 rounded-xl hover:bg-red-500/10 transition-all duration-300"
          >
            <ng-icon name="lucideLogOut" [size]="'20'"></ng-icon>
            <span class="font-medium">{{ 'APP.COMMONS.LOGOUT' | translate }}</span>
          </button>
        </ng-template>
      </div>
    </div>
  `,
})
export class MobileMenuComponent {
  @Input() isOpen = false;
  @Input() isLoggedIn = false;
  @Input() currentUser?: User;
  @Input() currentLanguage!: SupportedLanguage;
  @Input() supportedLanguages: LanguageOption[] = [];
  @Output() close = new EventEmitter<void>();
  @Output() logout = new EventEmitter<void>();
  @Output() languageChange = new EventEmitter<SupportedLanguage>();

  menuItems = [
    {
      label: 'APP.COMMONS.ABOUT_US',
      route: '/about',
      icon: 'lucideInfo',
      gradient: 'from-accent-600/20 to-accent-500/20',
    },
    {
      label: 'APP.COMMONS.CONTACT',
      route: '/contact',
      icon: 'lucideMessageCircle',
      gradient: 'from-primary-600/20 to-accent-500/20',
    },
  ];

  showLangDropdown = false;

  constructor(private router: Router) {}

  onLogout(): void {
    this.logout.emit();
  }

  navigateToLogin(): void {
    if (!this.isLoginRoute()) {
      this.router.navigate(['/auth/login']);
      this.close.emit();
    }
  }

  isLoginRoute(): boolean {
    return this.router.url === '/auth/login';
  }

  toggleLangDropdown() {
    this.showLangDropdown = !this.showLangDropdown;
  }

  selectLanguage(lang: SupportedLanguage) {
    this.languageChange.emit(lang);
    this.showLangDropdown = false;
  }

  get currentLanguageObj(): LanguageOption | undefined {
    return this.supportedLanguages.find(l => l.code === this.currentLanguage);
  }
}