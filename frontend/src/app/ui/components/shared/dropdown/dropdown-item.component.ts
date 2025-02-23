import { Component, Input } from '@angular/core';
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
      class="flex items-center px-4 py-2 text-sm text-gray-300 hover:text-white hover:bg-white/10 transition-colors"
      role="menuitem"
      tabindex="-1">
      <ng-icon 
        *ngIf="icon"
        [name]="icon"
        size="16"
        class="mr-3">
      </ng-icon>
      <ng-content></ng-content>
    </a>
  `
})
export class DropdownItemComponent {
  @Input() routerLink?: string;
  @Input() icon?: string;
}