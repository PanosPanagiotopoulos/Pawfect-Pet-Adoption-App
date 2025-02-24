import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FormInputComponent } from '../../shared/form-input/form-input.component';
import { TimePickerComponent } from '../../shared/time-picker/time-picker.component';

@Component({
  selector: 'app-preferences',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormInputComponent,
    TimePickerComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [formGroup]="form" class="space-y-6">
      <h2 class="text-2xl font-bold text-white mb-6">Shelter Information</h2>

      <app-form-input
        [form]="getShelterForm()"
        controlName="shelterName"
        type="text"
        placeholder="Shelter Name"
      >
      </app-form-input>

      <div class="space-y-3">
        <label class="block text-sm font-medium text-gray-400"
          >Description</label
        >
        <textarea
          [formControl]="getShelterDescriptionControl()"
          rows="3"
          class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white 
                 placeholder-gray-500 focus:border-primary-500/50 focus:ring-2 
                 focus:ring-primary-500/20 focus:outline-none transition-all duration-300"
          placeholder="Tell us about your shelter"
        >
        </textarea>
      </div>

      <app-form-input
        [form]="getShelterForm()"
        controlName="website"
        type="url"
        placeholder="Website (Optional)"
      >
      </app-form-input>

      <div class="space-y-6">
        <h3 class="text-lg font-medium text-white">Operating Hours</h3>
        <div class="space-y-4">
          <app-time-picker
            *ngFor="let day of days"
            [day]="day"
            (timeChange)="onTimeChange($event)"
          >
          </app-time-picker>
        </div>
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
          (click)="onSubmit()"
          class="px-6 py-2 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg
                 hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 
                 transform hover:-translate-y-1"
          [disabled]="!form.valid"
        >
          Complete Registration
        </button>
      </div>
    </div>
  `,
})
export class PreferencesComponent {
  @Input() form!: FormGroup;
  @Output() back = new EventEmitter<void>();
  @Output() submit = new EventEmitter<void>();

  days = [
    'Monday',
    'Tuesday',
    'Wednesday',
    'Thursday',
    'Friday',
    'Saturday',
    'Sunday',
  ];

  getShelterForm(): FormGroup {
    return this.form.get('shelter') as FormGroup;
  }

  getShelterDescriptionControl() {
    return this.getShelterForm().get('description');
  }

  onTimeChange(event: any): void {
    const operatingHours = this.getShelterForm().get('operatingHours');
    if (operatingHours) {
      operatingHours.patchValue({
        [event.day.toLowerCase()]: `${event.openTime},${event.closeTime}`,
      });
    }
  }

  onBack(): void {
    this.back.emit();
  }

  onSubmit(): void {
    if (this.form.valid) {
      this.submit.emit();
    } else {
      Object.keys(this.form.controls).forEach((key) => {
        const control = this.form.get(key);
        if (control?.invalid) {
          control.markAsTouched();
        }
      });
    }
  }
}
