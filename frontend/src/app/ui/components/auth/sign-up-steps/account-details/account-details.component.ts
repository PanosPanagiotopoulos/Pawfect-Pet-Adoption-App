import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FormInputComponent } from '../../shared/form-input/form-input.component';

@Component({
  selector: 'app-account-details',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormInputComponent
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [formGroup]="form" class="space-y-6">
      <h2 class="text-2xl font-bold text-white mb-6">Account Details</h2>

      <app-form-input
        [form]="form"
        controlName="password"
        type="password"
        placeholder="Password">
      </app-form-input>

      <div class="text-sm text-gray-400 space-y-1 mt-2">
        <p>Password must contain:</p>
        <ul class="list-disc list-inside pl-4">
          <li>At least 8 characters</li>
          <li>One uppercase letter</li>
          <li>One lowercase letter</li>
          <li>One number</li>
          <li>One special character</li>
        </ul>
      </div>

      <div class="flex justify-between pt-6">
        <button
          type="button"
          (click)="onBack()"
          class="px-6 py-2 border border-gray-600 text-gray-300 rounded-lg
                 hover:bg-white/5 transition-all duration-300"
        >
          Back
        </button>

        <button
          type="button"
          (click)="onNext()"
          class="px-6 py-2 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg
                 hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 
                 transform hover:-translate-y-1"
          [disabled]="!form.valid"
        >
          Next
        </button>
      </div>
    </div>
  `
})
export class AccountDetailsComponent {
  @Input() form!: FormGroup;
  @Output() next = new EventEmitter<void>();
  @Output() back = new EventEmitter<void>();

  onNext(): void {
    if (this.form.valid) {
      this.next.emit();
    } else {
      Object.keys(this.form.controls).forEach(key => {
        const control = this.form.get(key);
        if (control?.invalid) {
          control.markAsTouched();
        }
      });
    }
  }

  onBack(): void {
    this.back.emit();
  }
}