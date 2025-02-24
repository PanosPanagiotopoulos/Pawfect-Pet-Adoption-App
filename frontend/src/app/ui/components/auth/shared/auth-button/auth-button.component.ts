import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgIconsModule } from '@ng-icons/core';

@Component({
  selector: 'app-auth-button',
  standalone: true,
  imports: [CommonModule, NgIconsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <button
      [type]="type"
      [disabled]="isLoading || disabled"
      [class]="buttonClass"
      class="group relative w-full flex justify-center items-center py-3 px-4 border border-transparent 
             text-sm font-medium rounded-xl text-white transition-all duration-300 
             transform hover:-translate-y-0.5 disabled:opacity-50 disabled:hover:translate-y-0"
    >
      <div class="absolute inset-0 rounded-xl bg-gradient-to-r from-primary-500/0 
                  via-white/5 to-primary-500/0 group-hover:animate-shimmer -z-10"></div>
      
      <ng-icon
        *ngIf="icon && !isLoading"
        [name]="icon"
        class="mr-2 group-hover:scale-110 transition-transform duration-300"
        [size]="'20'"
      ></ng-icon>

      <svg
        *ngIf="isLoading"
        class="animate-spin -ml-1 mr-3 h-5 w-5 text-white"
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
      >
        <circle
          class="opacity-25"
          cx="12"
          cy="12"
          r="10"
          stroke="currentColor"
          stroke-width="4"
        ></circle>
        <path
          class="opacity-75"
          fill="currentColor"
          d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
        ></path>
      </svg>

      <span class="relative">
        <ng-content></ng-content>
      </span>
    </button>
  `
})
export class AuthButtonComponent {
  @Input() type: 'button' | 'submit' = 'button';
  @Input() isLoading: boolean = false;
  @Input() disabled: boolean = false;
  @Input() icon?: string;
  @Input() buttonClass: string = 'bg-gradient-to-r from-primary-600 to-accent-600 hover:shadow-lg hover:shadow-primary-500/20';
}