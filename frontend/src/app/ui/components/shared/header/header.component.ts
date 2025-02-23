import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgIconsModule } from '@ng-icons/core';
import { CommonModule } from '@angular/common';
import { NavLinkComponent } from '../nav-link/nav-link.component';
import { AuthButtonComponent } from '../auth-button/auth-button.component';
import { MobileMenuComponent } from '../mobile-menu/mobile-menu.component';
import { UserAvatarComponent } from '../user-avatar/user-avatar.component';
import { DropdownComponent } from '../dropdown/dropdown.component';
import { DropdownItemComponent } from '../dropdown/dropdown-item.component';

@Component({
  selector: 'app-header',
  template: `
    <nav class="fixed w-full top-0 z-50">
      <!-- Gradient background with animation -->
      <div class="absolute inset-0 bg-gradient-to-r from-gray-900/95 via-gray-800/95 to-gray-900/95 animate-gradient"></div>
      <div class="absolute inset-0 bg-gradient-to-r from-primary-500/30 via-secondary-500/30 to-accent-500/30 animate-gradient-slow opacity-70"></div>
      
      <div class="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex justify-between h-20">
          <!-- Logo and Brand -->
          <div class="flex items-center">
            <a routerLink="/" class="flex items-center group">
              <div class="relative">
                <div class="w-10 h-10 flex items-center justify-center transform group-hover:scale-110 transition-transform duration-300">
                  <ng-icon 
                    name="lucideHeart" 
                    class="text-white animate-float-very-slow"
                    [size]="'32'">
                  </ng-icon>
                </div>
              </div>
              <!-- Brand name with gradient -->
              <h1 class="ml-3 text-3xl font-bold hidden sm:block">
                <span class="bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 bg-clip-text text-transparent animate-gradient">
                  Pawfect
                </span>
              </h1>
            </a>
          </div>

          <!-- Navigation Links - Desktop -->
          <div class="hidden md:flex items-center space-x-8">
            <app-nav-link routerLink="/search">Search</app-nav-link>
            <app-nav-link routerLink="/about">About</app-nav-link>
            <app-nav-link routerLink="/contact">Contact</app-nav-link>
            
            <!-- Show login button when not logged in -->
            <ng-container *ngIf="!isLoggedIn; else userProfile">
              <app-auth-button routerLink="/auth/login">
                Login
              </app-auth-button>
            </ng-container>

            <!-- Show user profile when logged in -->
            <ng-template #userProfile>
              <app-dropdown [(isOpen)]="isProfileOpen">
                <!-- Trigger -->
                <button trigger
                  class="flex items-center group"
                  id="user-menu-button"
                  aria-expanded="false"
                  aria-haspopup="true">
                  <app-user-avatar
                    [imageUrl]="currentUser?.photoUrl"
                    [name]="currentUser?.name">
                  </app-user-avatar>
                </button>

                <!-- Dropdown items -->
                <app-dropdown-item
                  routerLink="/profile"
                  icon="lucideUser">
                  Profile
                </app-dropdown-item>
                <app-dropdown-item
                  icon="lucideLogOut"
                  (click)="logout()">
                  Log out
                </app-dropdown-item>
              </app-dropdown>
            </ng-template>
          </div>

          <!-- Mobile Menu Button -->
          <div class="md:hidden flex items-center">
            <button 
              type="button" 
              class="text-white/90 hover:text-white transition-colors p-2 rounded-lg hover:bg-white/10"
              (click)="toggleMobileMenu()">
              <ng-icon [name]="isMobileMenuOpen ? 'lucideX' : 'lucideMenu'" [size]="'24'"></ng-icon>
            </button>
          </div>
        </div>
      </div>

      <!-- Mobile Menu -->
      <app-mobile-menu
        [isOpen]="isMobileMenuOpen"
        (close)="closeMobileMenu()">
      </app-mobile-menu>
    </nav>

    <!-- Spacer to prevent content from hiding under fixed header -->
    <div class="h-20"></div>
  `,
  styleUrls: ['./header.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NgIconsModule,
    NavLinkComponent,
    AuthButtonComponent,
    MobileMenuComponent,
    UserAvatarComponent,
    DropdownComponent,
    DropdownItemComponent
  ]
})
export class HeaderComponent {
  isProfileOpen = false;
  isMobileMenuOpen = false;
  
  // TODO: Replace with actual auth service integration
  isLoggedIn = true;
  currentUser?: { name?: string; photoUrl?: string; } = undefined;

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
    document.body.style.overflow = this.isMobileMenuOpen ? 'hidden' : '';
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
    document.body.style.overflow = '';
  }

  // TODO: Implement actual logout logic
  logout(): void {
    console.log('Logging out...');
  }
}