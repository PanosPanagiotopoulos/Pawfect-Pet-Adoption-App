import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-form-field',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="mb-8">
      <h3 *ngIf="title" class="text-lg font-medium text-white mb-4">{{ title }}</h3>
      <div [class]="containerClass">
        <ng-content></ng-content>
      </div>
      <p *ngIf="hint" class="mt-2 text-sm text-gray-400">{{ hint }}</p>
    </div>
  `
})
export class FormFieldComponent {
  @Input() title?: string;
  @Input() hint?: string;
  @Input() containerClass: string = '';
}