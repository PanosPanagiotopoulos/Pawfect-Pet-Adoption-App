import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="relative">
      <!-- Trigger -->
      <div (click)="toggle()">
        <ng-content select="[trigger]"></ng-content>
      </div>

      <!-- Dropdown Menu -->
      <div 
        *ngIf="isOpen"
        class="absolute right-0 mt-2 w-48 rounded-xl shadow-lg py-1 bg-gradient-to-b from-gray-900 to-gray-800 ring-1 ring-black ring-opacity-5 backdrop-blur-lg z-50"
        role="menu"
        aria-orientation="vertical"
        aria-labelledby="user-menu-button"
        tabindex="-1">
        <ng-content></ng-content>
      </div>

      <!-- Backdrop -->
      <div 
        *ngIf="isOpen" 
        class="fixed inset-0 z-40"
        (click)="close()">
      </div>
    </div>
  `
})
export class DropdownComponent {
  @Input() isOpen = false;
  @Output() isOpenChange = new EventEmitter<boolean>();

  toggle(): void {
    this.isOpen = !this.isOpen;
    this.isOpenChange.emit(this.isOpen);
  }

  close(): void {
    this.isOpen = false;
    this.isOpenChange.emit(this.isOpen);
  }
}