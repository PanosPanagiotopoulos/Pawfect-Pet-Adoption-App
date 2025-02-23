import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-dropdown',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="relative inline-block">
      <!-- Trigger -->
      <div
        (click)="toggle()"
        class="transform transition-transform duration-200"
        [class.scale-105]="isOpen"
      >
        <ng-content select="[trigger]"></ng-content>
      </div>

      <!-- Dropdown Menu -->
      <div
        *ngIf="isOpen"
        [@dropdownAnimation]
        class="absolute z-50 w-48 py-2 bg-gradient-to-b from-gray-900 to-gray-800 rounded-2xl shadow-lg ring-1 ring-white/10 transform -translate-x-1/2 left-1/2 mt-4"
        role="menu"
        aria-orientation="vertical"
        aria-labelledby="user-menu-button"
        tabindex="-1"
      >
        <ng-content></ng-content>
      </div>

      <!-- Backdrop -->
      <div
        *ngIf="isOpen"
        class="fixed inset-0 z-40"
        [@backdropAnimation]
        (click)="close()"
      ></div>
    </div>
  `,
  animations: [
    trigger('dropdownAnimation', [
      transition(':enter', [
        style({
          opacity: 0,
          transform: 'translate(-50%, -20px) scale(0.95)',
        }),
        animate(
          '200ms cubic-bezier(0.4, 0, 0.2, 1)',
          style({
            opacity: 1,
            transform: 'translate(-50%, 0) scale(1)',
          })
        ),
      ]),
      transition(':leave', [
        animate(
          '150ms cubic-bezier(0.4, 0, 0.2, 1)',
          style({
            opacity: 0,
            transform: 'translate(-50%, -20px) scale(0.95)',
          })
        ),
      ]),
    ]),
    trigger('backdropAnimation', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('200ms cubic-bezier(0.4, 0, 0.2, 1)', style({ opacity: 1 })),
      ]),
      transition(':leave', [
        animate('150ms cubic-bezier(0.4, 0, 0.2, 1)', style({ opacity: 0 })),
      ]),
    ]),
  ],
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
