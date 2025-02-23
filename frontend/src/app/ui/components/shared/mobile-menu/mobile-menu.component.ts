import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NgIconsModule } from '@ng-icons/core';
import { UserAvatarComponent } from '../user-avatar/user-avatar.component';

@Component({
  selector: 'app-mobile-menu',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink,
    NgIconsModule,
    UserAvatarComponent
  ],
  template: `
    <div 
      class="fixed inset-0 bg-gray-900/50 backdrop-blur-sm transition-opacity duration-300"
      [class.opacity-100]="isOpen"
      [class.opacity-0]="!isOpen"
      [class.pointer-events-auto]="isOpen"
      [class.pointer-events-none]="!isOpen"
      (click)="close.emit()">
    </div>
    
    <div 
      class="fixed right-0 top-0 h-full w-[80%] max-w-sm bg-gradient-to-b from-gray-900 to-gray-800 transform transition-transform duration-300 ease-out shadow-2xl"
      [class.translate-x-0]="isOpen"
      [class.translate-x-full]="!isOpen">
      
      <!-- Header -->
      <div class="flex items-center justify-between p-4 border-b border-gray-700">
        <h2 class="text-xl font-bold text-white">Menu</h2>
        <button 
          (click)="close.emit()"
          class="p-2 text-gray-400 hover:text-white transition-colors rounded-lg hover:bg-white/10">
          <ng-icon name="lucideX" [size]="'24'"></ng-icon>
        </button>
      </div>

      <!-- User Profile Section (if logged in) -->
      <div *ngIf="isLoggedIn" class="p-4 border-b border-gray-700">
        <div class="flex items-center space-x-3">
          <app-user-avatar
            [imageUrl]="currentUser?.photoUrl"
            [name]="currentUser?.name">
          </app-user-avatar>
          <div>
            <p class="text-white font-medium">{{ currentUser?.name || 'User' }}</p>
            <p class="text-gray-400 text-sm">View Profile</p>
          </div>
        </div>
      </div>

      <!-- Navigation Links -->
      <nav class="p-4 space-y-2">
        <ng-container *ngFor="let item of menuItems">
          <a [routerLink]="item.route"
             (click)="close.emit()"
             class="flex items-center space-x-3 p-3 rounded-xl text-gray-300 hover:text-white hover:bg-white/10 transition-all duration-300 group">
            <div class="flex items-center justify-center w-10 h-10 rounded-lg bg-gradient-to-br"
                 [class]="item.gradient">
              <ng-icon [name]="item.icon" [size]="'20'" class="transform group-hover:scale-110 transition-transform"></ng-icon>
            </div>
            <span class="font-medium">{{ item.label }}</span>
          </a>
        </ng-container>
      </nav>

      <!-- Auth Section -->
      <div class="absolute bottom-0 left-0 right-0 p-4 border-t border-gray-700">
        <ng-container *ngIf="!isLoggedIn; else logoutButton">
          <a routerLink="/auth/login"
             (click)="close.emit()"
             class="flex items-center justify-center space-x-2 w-full p-3 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-xl hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 transform hover:-translate-y-1">
            <ng-icon name="lucideUser" [size]="'20'"></ng-icon>
            <span class="font-medium">Login</span>
          </a>
        </ng-container>
        
        <ng-template #logoutButton>
          <button
            (click)="logout()"
            class="flex items-center justify-center space-x-2 w-full p-3 border border-red-500/30 text-red-500 rounded-xl hover:bg-red-500/10 transition-all duration-300">
            <ng-icon name="lucideLogOut" [size]="'20'"></ng-icon>
            <span class="font-medium">Logout</span>
          </button>
        </ng-template>
      </div>
    </div>
  `
})
export class MobileMenuComponent {
  @Input() isOpen = false;
  @Output() close = new EventEmitter<void>();

  // TODO: Replace with actual auth service integration
  isLoggedIn = false;
  currentUser?: { name?: string; photoUrl?: string; } = undefined;

  menuItems = [
    {
      label: 'Home',
      route: '/',
      icon: 'lucideHouse',
      gradient: 'from-primary-600/20 to-primary-500/20'
    },
    {
      label: 'Search',
      route: '/search',
      icon: 'lucideSearch',
      gradient: 'from-secondary-600/20 to-secondary-500/20'
    },
    {
      label: 'About',
      route: '/about',
      icon: 'lucideInfo',
      gradient: 'from-accent-600/20 to-accent-500/20'
    },
    {
      label: 'Contact',
      route: '/contact',
      icon: 'lucideMessageCircle',
      gradient: 'from-primary-600/20 to-accent-500/20'
    }
  ];

  // TODO: Implement actual logout logic
  logout(): void {
    console.log('Logging out...');
    this.close.emit();
  }
}