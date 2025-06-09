import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, ChangeDetectorRef, ViewChildren, QueryList, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ValidationMessageComponent } from './validation-message.component';

@Component({
  selector: 'app-otp-input',
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

      <!-- Hidden input for form control -->
      <input
        type="hidden"
        [formControlName]="controlName"
        [id]="controlName"
      />

      <!-- OTP Input Boxes -->
      <div class="flex justify-center space-x-2 sm:space-x-4">
        <ng-container *ngFor="let i of [0, 1, 2, 3, 4, 5]">
          <div class="relative">
            <input
              type="text"
              class="w-10 h-14 sm:w-12 sm:h-16 text-center text-xl sm:text-2xl font-bold bg-white/5 border border-white/10 rounded-lg text-white 
                     focus:border-primary-500/50 focus:ring-2 focus:ring-primary-500/20 focus:outline-none transition-all duration-300"
              [class.border-red-500]="isInvalid"
              maxlength="1"
              (input)="onDigitInput($event, i)"
              (keydown)="onKeyDown($event, i)"
              (focus)="onFocus(i)"
              (paste)="onPaste($event)"
              (blur)="onBlur()"
              #digitInput
            />
          </div>
        </ng-container>
      </div>

      <!-- Error message -->
      <div class="mt-4 text-center">
        <app-validation-message
          [id]="controlName + '-error'"
          [control]="form.get(controlName)"
          [field]="label"
          [showImmediately]="true">
        </app-validation-message>
      </div>

      <!-- Hint text -->
      <p *ngIf="hint && !isInvalid" class="mt-2 text-sm text-gray-400 text-center">
        {{ hint }}
      </p>
    </div>
  `,
  styles: [`
    /* Add animation for focus indicator */
    @keyframes pulse {
      0%, 100% { border-color: rgba(124, 58, 237, 0.3); }
      50% { border-color: rgba(124, 58, 237, 0.8); }
    }
    
    input:focus {
      animation: pulse 1.5s cubic-bezier(0.4, 0, 0.6, 1) infinite;
      border-color: rgba(124, 58, 237, 0.5);
    }
  `]
})
export class OtpInputComponent implements AfterViewInit {
  @Input() form!: FormGroup;
  @Input() controlName!: string;
  @Input() label: string = 'Κωδικός OTP';
  @Input() hint?: string;
  @Input() length: number = 6;
  @Output() completed = new EventEmitter<string>();
  
  @ViewChildren('digitInput') digitInputs!: QueryList<ElementRef>;
  
  private otpValue: string = '';

  constructor(private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    // Initialize from existing value if present
    const currentValue = this.form.get(this.controlName)?.value || '';
    if (currentValue) {
      this.otpValue = currentValue;
      // We'll distribute the digits in afterViewInit
    }
  }

  ngAfterViewInit() {
    // Distribute existing value to input boxes
    if (this.otpValue) {
      const digits = this.otpValue.split('');
      const inputs = this.digitInputs.toArray();
      digits.forEach((digit, index) => {
        if (index < inputs.length) {
          inputs[index].nativeElement.value = digit;
        }
      });
    }
    
    // Focus the first empty input or the first input if all are filled
    setTimeout(() => this.focusInput(), 100);
  }

  get isInvalid(): boolean {
    const control = this.form.get(this.controlName);
    return !!(control?.invalid && (control?.touched || control?.dirty));
  }

  onDigitInput(event: Event, index: number): void {
    const input = event.target as HTMLInputElement;
    let value = input.value;
    
    // Allow only numbers
    if (value && !/^\d+$/.test(value)) {
      input.value = '';
      return;
    }
    
    // Take only the last character if multiple were somehow entered
    if (value.length > 1) {
      value = value.slice(-1);
      input.value = value;
    }
    
    // Update the OTP value
    this.updateOtpValue();
    
    // Auto-focus next input
    if (value && index < this.length - 1) {
      const inputs = this.digitInputs.toArray();
      if (inputs[index + 1]) {
        inputs[index + 1].nativeElement.focus();
      }
    }
    
    // Mark main control as touched
    this.form.get(this.controlName)?.markAsTouched();
  }

  onKeyDown(event: KeyboardEvent, index: number): void {
    const input = event.target as HTMLInputElement;
    const inputs = this.digitInputs.toArray();
    
    // Handle backspace
    if (event.key === 'Backspace') {
      if (input.value === '') {
        // Move to previous input if current is empty
        if (index > 0) {
          inputs[index - 1].nativeElement.focus();
          inputs[index - 1].nativeElement.value = '';
          this.updateOtpValue();
        }
      } else {
        // Clear current input
        input.value = '';
        this.updateOtpValue();
      }
    }
    
    // Handle arrow keys
    if (event.key === 'ArrowLeft' && index > 0) {
      inputs[index - 1].nativeElement.focus();
    }
    
    if (event.key === 'ArrowRight' && index < this.length - 1) {
      inputs[index + 1].nativeElement.focus();
    }
    
    // Handle paste event
    if ((event.metaKey || event.ctrlKey) && event.key === 'v') {
      // Let the paste event handler handle this
      return;
    }
    
    // Prevent non-numeric input
    if (
      !/^\d$/.test(event.key) && // Not a digit
      event.key !== 'Backspace' &&
      event.key !== 'Delete' &&
      event.key !== 'Tab' &&
      event.key !== 'ArrowLeft' &&
      event.key !== 'ArrowRight'
    ) {
      event.preventDefault();
    }
  }

  onFocus(index: number): void {
    // Select the content of the input when focused
    setTimeout(() => {
      const inputs = this.digitInputs.toArray();
      if (inputs[index]) {
        inputs[index].nativeElement.select();
      }
    }, 0);
  }

  onBlur(): void {
    // Mark as touched on blur
    this.form.get(this.controlName)?.markAsTouched();
    
    // Update OTP value
    this.updateOtpValue();
  }

  // Update the OTP value based on the input fields
  private updateOtpValue(): void {
    setTimeout(() => {
      const inputs = this.digitInputs.toArray();
      const value = inputs.map(input => input.nativeElement.value || '').join('');
      
      this.otpValue = value;
      this.form.get(this.controlName)?.setValue(value);
      
      // Emit completed event when all digits are filled
      if (value.length === this.length) {
        this.completed.emit(value);
      }
      
      this.cdr.markForCheck();
    }, 0);
  }

  // Handle paste event
  onPaste(event: ClipboardEvent): void {
    event.preventDefault();
    
    const clipboardData = event.clipboardData;
    if (!clipboardData) return;
    
    const pastedText = clipboardData.getData('text');
    const digits = pastedText.replace(/\D/g, '').substring(0, this.length);
    
    if (digits.length > 0) {
      const inputs = this.digitInputs.toArray();
      
      // Fill in as many inputs as we have digits
      digits.split('').forEach((digit, index) => {
        if (index < inputs.length) {
          inputs[index].nativeElement.value = digit;
        }
      });
      
      // Focus the next empty input or the last input if all are filled
      const focusIndex = Math.min(digits.length, this.length - 1);
      inputs[focusIndex].nativeElement.focus();
      
      // Update the OTP value
      this.updateOtpValue();
    }
  }

  // Focus the first input (can be called from parent)
  focusInput(): void {
    const inputs = this.digitInputs.toArray();
    if (inputs && inputs.length > 0) {
      // Find first empty input
      const emptyIndex = inputs.findIndex(input => !input.nativeElement.value);
      const focusIndex = emptyIndex >= 0 ? emptyIndex : 0;
      inputs[focusIndex].nativeElement.focus();
    }
  }
}