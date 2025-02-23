import { Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { NgIconsModule } from '@ng-icons/core';
import { 
  lucideX,
  lucideSearch,
  lucideInfo,
  lucideMessageCircle,
  lucideUser,
  lucideHouse
} from '@ng-icons/lucide';

@Component({
  selector: 'app-mobile-menu',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink,
    NgIconsModule.withIcons({
      lucideX,
      lucideHouse,
      lucideSearch,
      lucideInfo,
      lucideMessageCircle,
      lucideUser
    })
  ],
  template: `
    <div 
      class="fixed inset-0 bg-gray-900/50 backdrop-blur-sm transition-opacity duration-300"
      [class.opacity-100]="isOpen"
      [class.opacity-0]="!isOpen"
      [class.pointer-events-auto]="isOpen"
      [class.pointer-events-none]="!isOpen"
      (click)="close.emit()"
    ></div>
    
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
        <a routerLink="/auth/login"
           (click)="close.emit()"
           class="flex items-center justify-center space-x-2 w-full p-3 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-xl hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 transform hover:-translate-y-1">
          <ng-icon name="lucideUser" [size]="'20'"></ng-icon>
          <span class="font-medium">Login</span>
        </a>
      </div>
    </div>
  `
})
export class MobileMenuComponent {
  @Input() isOpen = false;
  @Output() close = new EventEmitter<void>();

  menuItems = [
    {
      label: 'Home',
      route: '/',
      icon: 'lucideHouse',
      gradient: 'from-primary-600/20 to-primary-500/20'
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
}