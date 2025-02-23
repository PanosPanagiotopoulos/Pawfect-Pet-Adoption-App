import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-time-input',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div [formGroup]="form" class="space-y-2 mb-8">
      <label [for]="controlName" class="block text-sm font-medium text-gray-400 capitalize mb-2">
        {{ controlName }}
      </label>
      <div class="flex items-center space-x-4">
        <div class="flex-1">
          <input
            type="time"
            [id]="controlName + '-open'"
            [value]="openingTime"
            (change)="onTimeChange($event, 'open')"
            class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white
                   placeholder-gray-500 focus:border-primary-500/50 focus:ring-2 
                   focus:ring-primary-500/20 focus:outline-none transition-all duration-300"
          />
          <div class="text-xs text-gray-500 mt-1">Opening Time</div>
        </div>
        
        <span class="text-gray-400">to</span>
        
        <div class="flex-1">
          <input
            type="time"
            [id]="controlName + '-close'"
            [value]="closingTime"
            (change)="onTimeChange($event, 'close')"
            class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white
                   placeholder-gray-500 focus:border-primary-500/50 focus:ring-2 
                   focus:ring-primary-500/20 focus:outline-none transition-all duration-300"
          />
          <div class="text-xs text-gray-500 mt-1">Closing Time</div>
        </div>
      </div>

      <div 
        *ngIf="form.get(controlName)?.errors?.['invalidTimeRange']" 
        class="text-red-400 text-sm mt-2"
      >
        Closing time must be after opening time
      </div>
      
      <div 
        *ngIf="form.get(controlName)?.errors?.['pattern']" 
        class="text-red-400 text-sm mt-2"
      >
        Please enter valid times in 24-hour format
      </div>
    </div>
  `
})
export class TimeInputComponent implements OnInit {
  @Input() form!: FormGroup;
  @Input() controlName!: string;
  
  openingTime: string = '09:00';
  closingTime: string = '17:00';

  ngOnInit() {
    const currentValue = this.form.get(this.controlName)?.value;
    if (currentValue) {
      const [open, close] = currentValue.split(',');
      this.openingTime = open || '09:00';
      this.closingTime = close || '17:00';
    }
  }

  onTimeChange(event: Event, type: 'open' | 'close'): void {
    const input = event.target as HTMLInputElement;
    
    if (type === 'open') {
      this.openingTime = input.value;
    } else {
      this.closingTime = input.value;
    }

    // Validate time range
    if (this.openingTime && this.closingTime) {
      if (this.openingTime >= this.closingTime) {
        this.form.get(this.controlName)?.setErrors({ invalidTimeRange: true });
      } else {
        this.form.get(this.controlName)?.setErrors(null);
      }
    }

    // Update form control value
    this.form.get(this.controlName)?.setValue(`${this.openingTime},${this.closingTime}`);
  }
}