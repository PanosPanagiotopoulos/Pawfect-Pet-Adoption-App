import { Component, Input, Output, EventEmitter, ElementRef, ViewChild } from '@angular/core';
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
        #trigger
        (click)="toggle()"
        class="transform transition-transform duration-200"
        [class.scale-105]="isOpen"
      >
        <ng-content select="[trigger]"></ng-content>
      </div>

      <!-- Dropdown Menu -->
      <div
        *ngIf="isOpen"
        #dropdownMenu
        [@dropdownAnimation]
        class="absolute z-50 w-48 py-2 bg-gradient-to-b from-gray-900 to-gray-800 rounded-2xl shadow-lg ring-1 ring-white/10"
        [style.left]="dropdownPosition.left"
        [style.top]="dropdownPosition.top"
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
          transform: 'translateY(-10px)',
        }),
        animate(
          '200ms cubic-bezier(0.4, 0, 0.2, 1)',
          style({
            opacity: 1,
            transform: 'translateY(0)',
          })
        ),
      ]),
      transition(':leave', [
        animate(
          '150ms cubic-bezier(0.4, 0, 0.2, 1)',
          style({
            opacity: 0,
            transform: 'translateY(-10px)',
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
  @ViewChild('trigger') triggerEl!: ElementRef;
  @ViewChild('dropdownMenu') dropdownEl!: ElementRef;
  @Input() isOpen = false;
  @Output() isOpenChange = new EventEmitter<boolean>();

  dropdownPosition = {
    left: '0',
    top: '100%'
  };

  toggle(): void {
    this.isOpen = !this.isOpen;
    this.isOpenChange.emit(this.isOpen);

    if (this.isOpen) {
      setTimeout(() => this.updateDropdownPosition(), 0);
    }
  }

  close(): void {
    this.isOpen = false;
    this.isOpenChange.emit(this.isOpen);
  }

  private updateDropdownPosition(): void {
    if (!this.triggerEl || !this.dropdownEl) return;

    const triggerRect = this.triggerEl.nativeElement.getBoundingClientRect();
    const dropdownRect = this.dropdownEl.nativeElement.getBoundingClientRect();
    
    // Center the dropdown under the trigger
    const left = -(dropdownRect.width - triggerRect.width) / 2;
    
    this.dropdownPosition = {
      left: `${left}px`,
      top: `${triggerRect.height + 8}px` // Add 8px gap
    };
  }
}