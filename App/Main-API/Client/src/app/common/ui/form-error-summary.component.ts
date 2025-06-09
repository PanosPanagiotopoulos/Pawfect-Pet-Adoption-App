import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ValidationErrorInfo } from './form-input-error-tracker.service';
@Component({
  selector: 'app-form-error-summary',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div *ngIf="errors.length > 0" 
         class="bg-red-500/10 border border-red-500/30 rounded-lg p-4 my-4 animate-fadeIn"
         role="alert"
         aria-live="assertive">
      <h3 class="text-red-400 font-medium mb-2 flex items-center">
        <span class="mr-2">⚠️</span> {{ title }}
      </h3>
      <ul class="list-disc list-inside text-sm text-red-400 space-y-1">
        <li *ngFor="let error of errors" 
            (click)="scrollToError(error)"
            class="cursor-pointer hover:underline">
          {{ error.errorMessage }}
        </li>
      </ul>
    </div>
  `,
  styles: [`
    @keyframes fadeIn {
      from { opacity: 0; transform: translateY(-10px); }
      to { opacity: 1; transform: translateY(0); }
    }
    
    .animate-fadeIn {
      animation: fadeIn 0.3s ease-out forwards;
    }
  `]
})
export class FormErrorSummaryComponent {
  @Input() errors: ValidationErrorInfo[] = [];
  @Input() title: string = 'Παρακαλώ διορθώστε τα παρακάτω σφάλματα:';

  scrollToError(error: ValidationErrorInfo): void {
    if (error.element) {
      // Scroll to the element
      error.element.scrollIntoView({ 
        behavior: 'smooth', 
        block: 'center' 
      });
      
      // Focus the element if it's an input
      if (error.element instanceof HTMLInputElement || 
          error.element instanceof HTMLTextAreaElement || 
          error.element instanceof HTMLSelectElement) {
        error.element.focus();
      }
      
      // Add a highlight effect
      this.highlightElement(error.element);
    }
  }

  private highlightElement(element: HTMLElement): void {
    // Add a temporary highlight class
    element.classList.add('highlight-error');
    
    // Remove the class after animation completes
    setTimeout(() => {
      element.classList.remove('highlight-error');
    }, 1500);
  }
}