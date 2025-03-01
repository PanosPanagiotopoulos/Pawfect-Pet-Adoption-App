import {
  Component,
  Input,
  OnInit,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ValidationMessageComponent } from './validation-message.component';

@Component({
  selector: 'app-time-input',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    ValidationMessageComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [formGroup]="form" class="space-y-2 mb-8">
      <label
        [for]="controlName"
        class="block text-sm font-medium text-gray-400 capitalize mb-2"
      >
        {{ controlName }}
      </label>

      <!-- Closed toggle -->
      <div class="flex items-center mb-3">
        <label class="relative inline-flex items-center cursor-pointer">
          <input
            type="checkbox"
            [(ngModel)]="isClosed"
            [ngModelOptions]="{ standalone: true }"
            (change)="onClosedChange()"
            class="sr-only peer"
          />
          <div
            class="w-11 h-6 bg-gray-700 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-red-600"
          ></div>
        </label>
        <span class="ml-3 text-sm text-gray-300">{{
          isClosed ? 'Κλειστό' : 'Ανοιχτό'
        }}</span>
      </div>

      <!-- Time inputs (shown only when not closed) -->
      <div *ngIf="!isClosed" class="flex items-center space-x-4">
        <div class="flex-1">
          <input
            type="time"
            [id]="controlName + '-open'"
            [value]="openingTime"
            (change)="onTimeChange($event, 'open')"
            (blur)="onTimeBlur()"
            class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white
                   placeholder-gray-500 focus:border-primary-500/50 focus:ring-2 
                   focus:ring-primary-500/20 focus:outline-none transition-all duration-300"
            [class.border-red-500]="isTimeInvalid"
            [attr.aria-label]="'Ώρα ανοίγματος για ' + controlName"
            [attr.aria-invalid]="isTimeInvalid"
            [attr.aria-describedby]="controlName + '-error'"
          />
          <div class="text-xs text-gray-500 mt-1">Ώρα ανοίγματος</div>
        </div>

        <span class="text-gray-400">έως</span>

        <div class="flex-1">
          <input
            type="time"
            [id]="controlName + '-close'"
            [value]="closingTime"
            (change)="onTimeChange($event, 'close')"
            (blur)="onTimeBlur()"
            class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white
                   placeholder-gray-500 focus:border-primary-500/50 focus:ring-2 
                   focus:ring-primary-500/20 focus:outline-none transition-all duration-300"
            [class.border-red-500]="isTimeInvalid"
            [attr.aria-label]="'Ώρα κλεισίματος για ' + controlName"
            [attr.aria-invalid]="isTimeInvalid"
            [attr.aria-describedby]="controlName + '-error'"
          />
          <div class="text-xs text-gray-500 mt-1">Ώρα κλεισίματος</div>
        </div>
      </div>

      <!-- Error messages -->
      <app-validation-message
        [id]="controlName + '-error'"
        [control]="form.get(controlName)"
        field="Ώρες λειτουργίας"
        [showImmediately]="true"
      >
      </app-validation-message>
    </div>
  `,
})
export class TimeInputComponent implements OnInit {
  @Input() form!: FormGroup;
  @Input() controlName!: string;
  @Output() timeChange = new EventEmitter<string>();

  openingTime: string = '09:00';
  closingTime: string = '17:00';
  isClosed: boolean = false;

  get isTimeInvalid(): boolean {
    const control = this.form.get(this.controlName);
    return !!(control?.invalid && (control?.touched || control?.dirty));
  }

  ngOnInit() {
    const currentValue = this.form.get(this.controlName)?.value;
    if (currentValue) {
      if (currentValue === 'closed') {
        this.isClosed = true;
      } else {
        const [open, close] = currentValue.split(',');
        this.openingTime = open || '09:00';
        this.closingTime = close || '17:00';
      }
    }
  }

  onClosedChange(): void {
    if (this.isClosed) {
      // Set value to 'closed'
      this.form.get(this.controlName)?.setValue('closed');
      this.form.get(this.controlName)?.setErrors(null);
    } else {
      // Restore previous time values
      this.form
        .get(this.controlName)
        ?.setValue(`${this.openingTime},${this.closingTime}`);
      // Validate time range
      this.validateTimeRange();
    }

    // Mark as dirty to trigger validation immediately
    this.form.get(this.controlName)?.markAsDirty();
    this.form.get(this.controlName)?.updateValueAndValidity();

    // Emit the change
    this.timeChange.emit(this.form.get(this.controlName)?.value);
  }

  onTimeChange(event: Event, type: 'open' | 'close'): void {
    const input = event.target as HTMLInputElement;

    if (type === 'open') {
      this.openingTime = input.value;
    } else {
      this.closingTime = input.value;
    }

    // Validate time range
    this.validateTimeRange();

    // Mark as dirty to trigger validation immediately
    this.form.get(this.controlName)?.markAsDirty();
    this.form.get(this.controlName)?.updateValueAndValidity();

    // Update form control value
    const timeValue = `${this.openingTime},${this.closingTime}`;
    this.form.get(this.controlName)?.setValue(timeValue);
    this.timeChange.emit(timeValue);
  }

  onTimeBlur(): void {
    // Mark as touched on blur
    this.form.get(this.controlName)?.markAsTouched();
    this.form.get(this.controlName)?.updateValueAndValidity();
  }

  private validateTimeRange(): void {
    if (this.openingTime && this.closingTime) {
      if (this.openingTime >= this.closingTime) {
        this.form.get(this.controlName)?.setErrors({ invalidTimeRange: true });
      } else {
        this.form.get(this.controlName)?.setErrors(null);
      }
    }
  }
}
