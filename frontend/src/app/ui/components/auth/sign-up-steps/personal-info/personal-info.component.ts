import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FormInputComponent } from '../../shared/form-input/form-input.component';
import { PhoneInputComponent } from '../../shared/phone-input/phone-input.component';

@Component({
  selector: 'app-personal-info',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormInputComponent,
    PhoneInputComponent
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [formGroup]="form" class="space-y-6">
      <h2 class="text-2xl font-bold text-white mb-6">Personal Information</h2>
      
      <app-form-input
        [form]="form"
        controlName="fullName"
        type="text"
        placeholder="Full Name">
      </app-form-input>

      <app-form-input
        [form]="form"
        controlName="email"
        type="email"
        placeholder="Email Address">
      </app-form-input>

      <app-phone-input
        [form]="form"
        countryCodeControl="countryCode"
        phoneNumberControl="phoneNumber">
      </app-phone-input>

      <div class="flex justify-end pt-6">
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
export class PersonalInfoComponent {
  @Input() form!: FormGroup;
  @Output() next = new EventEmitter<void>();

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
}