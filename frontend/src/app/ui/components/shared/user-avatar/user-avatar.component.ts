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
      class="relative rounded-full overflow-hidden bg-gradient-to-br from-primary-500 to-accent-500 flex items-center justify-center">
      <img 
        *ngIf="imageUrl" 
        [src]="imageUrl" 
        [alt]="name || 'User avatar'"
        class="w-full h-full object-cover" />
      <ng-icon 
        *ngIf="!imageUrl"
        name="lucideUser"
        [size]="size === 'sm' ? '16' : '20'"
        class="text-white">
      </ng-icon>
    </div>
  `
})
export class UserAvatarComponent {
  @Input() imageUrl?: string;
  @Input() name?: string;
  @Input() size: 'sm' | 'md' = 'md';
}