import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ValidationMessageComponent } from 'src/app/common/ui/validation-message.component';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-form-input',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ValidationMessageComponent, TranslatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [formGroup]="form" class="relative group mb-10">
      <!-- Input field -->
      <input
        [type]="type"
        [formControlName]="controlName"
        [id]="controlName"
        [attr.aria-invalid]="isInvalid"
        [attr.aria-describedby]="controlName + '-error'"
        [class]="inputClass"
        [readOnly]="readonly"
        [class.cursor-not-allowed]="readonly"
        [class.opacity-75]="readonly"
        class="peer w-full px-4 py-3 bg-white/5 border rounded-xl text-white 
               placeholder-transparent
               focus:ring-2 focus:outline-none 
               transition-all duration-300"
        [placeholder]="placeholder"
        [maxLength]="maxLength"
        (input)="onInputChange($event)"
        (blur)="onBlur()"
      />

      <!-- Floating label -->
      <label
        [for]="controlName"
        [class]="labelClass"
        class="absolute text-sm duration-300 transform 
               -translate-y-4 scale-75 top-2 z-10 origin-[0] bg-transparent
               px-2 peer-focus:px-2
               peer-placeholder-shown:scale-100 peer-placeholder-shown:-translate-y-1/2 
               peer-placeholder-shown:top-1/2 peer-focus:top-2 peer-focus:-translate-y-4 
               peer-focus:scale-75 left-1"
      >
        {{ placeholder | translate }}
      </label>

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
        [field]="placeholder"
        [showImmediately]="true"
      >
      </app-validation-message>
    </div>
  `,
})
export class FormInputComponent {
  @Input() form!: FormGroup;
  @Input() controlName!: string;
  @Input() type: string = 'text';
  @Input() placeholder: string = '';
  @Input() maxLength?: string = '200';
  @Input() readonly?: boolean = false;
  @Input() language?: string;
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

  get labelClass(): string {
    const control = this.form.get(this.controlName);
    const isInvalid = control?.invalid && (control?.touched || control?.dirty);

    return `
      text-gray-400
      ${isInvalid ? 'text-red-400' : 'peer-focus:text-primary-400'}
    `;
  }

  onInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.valueChange.emit(input.value);

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
