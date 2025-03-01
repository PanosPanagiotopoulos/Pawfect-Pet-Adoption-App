import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ValidationMessageComponent } from './validation-message.component';

@Component({
  selector: 'app-text-area-input',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ValidationMessageComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [formGroup]="form" class="relative group mb-10">
      <!-- Label -->
      <label
        [for]="controlName"
        class="block text-sm font-medium text-gray-400 mb-2"
      >
        {{ label }}
      </label>

      <!-- Textarea field -->
      <textarea
        [formControlName]="controlName"
        [id]="controlName"
        [attr.aria-invalid]="isInvalid"
        [attr.aria-describedby]="controlName + '-error'"
        [class]="inputClass"
        class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white 
               placeholder-gray-500 focus:border-primary-500/50 focus:ring-2 
               focus:ring-primary-500/20 focus:outline-none transition-all duration-300"
        [placeholder]="placeholder"
        [rows]="rows"
        (input)="onInputChange($event)"
        (blur)="onBlur()"
      ></textarea>

      <!-- Background gradient effect -->
      <div
        class="absolute inset-0 rounded-xl bg-gradient-to-r from-primary-500/10 
               via-secondary-500/10 to-accent-500/10 opacity-0 
               group-hover:opacity-50 peer-focus:opacity-100 -z-10 
               transition-opacity duration-500"
      ></div>

      <!-- Error message -->
      <app-validation-message
        [id]="controlName + '-error'"
        [control]="form.get(controlName)"
        [field]="label"
        [showImmediately]="true"
      >
      </app-validation-message>

      <!-- Hint text -->
      <p *ngIf="hint && !isInvalid" class="mt-2 text-sm text-gray-400">
        {{ hint }}
      </p>
    </div>
  `,
})
export class TextAreaInputComponent {
  @Input() form!: FormGroup;
  @Input() controlName!: string;
  @Input() label: string = '';
  @Input() placeholder: string = '';
  @Input() hint?: string;
  @Input() rows: number = 3;
  @Output() valueChange = new EventEmitter<any>();

  get isInvalid(): boolean {
    const control = this.form.get(this.controlName);
    return !!(control?.invalid && (control?.touched || control?.dirty));
  }

  get inputClass(): string {
    const control = this.form.get(this.controlName);
    const isInvalid = control?.invalid && (control?.touched || control?.dirty);

    return `
      border-white/10
      ${
        isInvalid
          ? 'border-red-500 focus:border-red-500 focus:ring-red-500/20'
          : 'focus:border-primary-500/50 focus:ring-primary-500/20'
      }
    `;
  }

  onInputChange(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    this.valueChange.emit(textarea.value);

    // Mark as dirty to trigger validation immediately
    const control = this.form.get(this.controlName);
    if (control) {
      control.markAsDirty();
      control.updateValueAndValidity();
    }
  }

  onBlur(): void {
    // Mark as touched on blur to ensure validation shows
    const control = this.form.get(this.controlName);
    if (control) {
      control.markAsTouched();
      control.updateValueAndValidity();
    }
  }
}
