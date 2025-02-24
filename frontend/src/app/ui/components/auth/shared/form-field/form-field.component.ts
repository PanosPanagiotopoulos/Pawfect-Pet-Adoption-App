import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-form-field',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="form-field-container mb-8">
      <h3 *ngIf="title" class="text-lg font-medium text-white mb-4">{{ title }}</h3>
      <div [class]="containerClass">
        <ng-content></ng-content>
      </div>
      <p *ngIf="hint" class="mt-2 text-sm text-gray-400">{{ hint }}</p>
    </div>
  `,
  styles: [`
    .form-field-container {
      position: relative;
    }
  `]
})
export class FormFieldComponent {
  @Input() title?: string;
  @Input() hint?: string;
  @Input() containerClass: string = '';
}