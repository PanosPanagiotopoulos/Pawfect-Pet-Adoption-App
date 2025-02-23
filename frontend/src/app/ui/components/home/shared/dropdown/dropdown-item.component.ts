import { Component, Input, Output, EventEmitter } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgIconsModule } from '@ng-icons/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dropdown-item',
  standalone: true,
  imports: [CommonModule, RouterLink, NgIconsModule],
  template: `
    <a
      [routerLink]="routerLink"
      (click)="handleClick()"
      class="relative flex items-center px-4 py-2.5 text-sm text-gray-300 hover:text-white transition-all duration-200 group"
      role="menuitem"
      tabindex="-1">
      <!-- Hover background effect -->
      <div class="absolute inset-0 bg-white/0 group-hover:bg-white/10 transition-colors duration-200"></div>
      
      <!-- Icon with animation -->
      <ng-icon 
        *ngIf="icon"
        [name]="icon"
        size="18"
        class="relative mr-3 transition-transform duration-200 group-hover:scale-110 group-hover:text-primary-400">
      </ng-icon>
      
      <!-- Content with slide animation -->
      <span class="relative transition-transform duration-200 group-hover:translate-x-0.5">
        <ng-content></ng-content>
      </span>
    </a>
  `
})
export class DropdownItemComponent {
  @Input() routerLink?: string;
  @Input() icon?: string;
  @Output() click = new EventEmitter<void>();

  handleClick(): void {
    this.click.emit();
  }
}