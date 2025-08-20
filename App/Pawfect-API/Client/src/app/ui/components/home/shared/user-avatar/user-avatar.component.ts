import { Component, Input } from '@angular/core';
import { NgIconsModule } from '@ng-icons/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-avatar',
  standalone: true,
  imports: [CommonModule, NgIconsModule],
  template: `
    <div 
      [class]="size === 'sm' ? 'w-8 h-8' : 'w-10 h-10'"
      class="relative rounded-full overflow-hidden transition-transform duration-200 hover:scale-105 group">
      <!-- Gradient background -->
      <div class="absolute inset-0 bg-gradient-to-br from-primary-500 to-accent-500 transition-opacity duration-200 group-hover:opacity-80"></div>
      
      <!-- Glow effect -->
      <div class="absolute inset-0 bg-primary-500/30 blur-md opacity-0 group-hover:opacity-100 transition-opacity duration-200"></div>
      
      <!-- Image or fallback icon -->
      <div class="relative flex items-center justify-center w-full h-full">
        <img 
          [src]="imageUrl || 'assets/placeholder.jpg'" 
          [alt]="name || 'User avatar'"
          class="w-full h-full object-cover transition-transform duration-200 group-hover:scale-110"
          (error)="onImageError($event)" />
        <ng-icon 
          *ngIf="showFallbackIcon"
          name="lucideUser"
          [size]="size === 'sm' ? '16' : '20'"
          class="text-white transition-transform duration-200 group-hover:scale-110">
        </ng-icon>
      </div>
    </div>
  `
})
export class UserAvatarComponent {
  @Input() imageUrl?: string;
  @Input() name?: string;
  @Input() size: 'sm' | 'md' = 'md';
  
  showFallbackIcon = false;

  onImageError(event: Event): void {
    const target = event.target as HTMLImageElement;
    if (target) {
      target.style.display = 'none';
      this.showFallbackIcon = true;
    }
  }
}