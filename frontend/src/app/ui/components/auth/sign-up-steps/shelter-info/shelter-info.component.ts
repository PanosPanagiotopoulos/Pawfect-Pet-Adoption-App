import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  ElementRef,
  ViewChild,
  ChangeDetectorRef,
  OnInit
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormGroup,
  ReactiveFormsModule,
  FormsModule,
  AbstractControl,
} from '@angular/forms';
import { NgIconsModule } from '@ng-icons/core';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { TextAreaInputComponent } from 'src/app/common/ui/text-area-input.component';

@Component({
  selector: 'app-shelter-info',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    FormInputComponent,
    TextAreaInputComponent,
    NgIconsModule
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [formGroup]="form" class="space-y-6" #formContainer>
      <h2 class="text-2xl font-bold text-white mb-6">Πληροφορίες Καταφυγίου</h2>

      <app-form-input
        [form]="getShelterForm()"
        controlName="shelterName"
        type="text"
        placeholder="Όνομα Καταφυγίου"
      >
      </app-form-input>

      <app-text-area-input
        [form]="getShelterForm()"
        controlName="description"
        label="Περιγραφή"
        placeholder="Περιγράψτε το καταφύγιό σας"
        [rows]="4"
        hint="Παρακαλώ παρέχετε μια σύντομη περιγραφή του καταφυγίου σας (τουλάχιστον 10 χαρακτήρες)"
        (valueChange)="updateDescription($event)"
      >
      </app-text-area-input>

      <app-form-input
        [form]="getShelterForm()"
        controlName="website"
        type="url"
        placeholder="Ιστοσελίδα (Προαιρετικό)"
      >
      </app-form-input>

      <!-- Social Media Section -->
      <div class="space-y-4">
        <h3 class="text-lg font-medium text-white">
          Κοινωνικά Δίκτυα (Προαιρετικό)
        </h3>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <app-form-input
            [form]="getSocialMediaForm()"
            controlName="facebook"
            type="url"
            placeholder="Facebook URL"
          >
          </app-form-input>

          <app-form-input
            [form]="getSocialMediaForm()"
            controlName="instagram"
            type="url"
            placeholder="Instagram URL"
          >
          </app-form-input>
        </div>
      </div>

      <!-- Operating Hours Section -->
      <div class="space-y-4">
        <div class="flex items-center justify-between">
          <h3 class="text-lg font-medium text-white">Ώρες Λειτουργίας</h3>
          <span class="text-sm text-gray-400">(Προαιρετικό)</span>
        </div>

        <div class="bg-gray-800/50 rounded-xl p-4 space-y-4">
          <div *ngFor="let day of days" class="mb-4 border border-gray-700/50 rounded-xl p-4 hover:border-gray-600/70 transition-colors">
            <div class="flex justify-between items-center mb-3">
              <h4 class="text-white font-medium">{{ day }}</h4>
              
              <!-- Closed toggle -->
              <div class="flex items-center">
                <label class="relative inline-flex items-center cursor-pointer">
                  <input
                    type="checkbox"
                    [(ngModel)]="closedDays[day]"
                    [ngModelOptions]="{standalone: true}"
                    (change)="onClosedChange(day)"
                    class="sr-only peer"
                  />
                  <div
                    class="w-11 h-6 bg-gray-700 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-red-600"
                  ></div>
                </label>
                <span class="ml-3 text-sm text-gray-300">{{ closedDays[day] ? 'Κλειστό' : 'Ανοιχτό' }}</span>
              </div>
            </div>
            
            <!-- Time selection (shown only when not closed) -->
            <div *ngIf="!closedDays[day]" class="flex items-center justify-between">
              <div class="flex-1 mr-4">
                <label class="block text-sm text-gray-400 mb-1">Ώρα ανοίγματος</label>
                <input 
                  type="text" 
                  [value]="openTimes[day]"
                  (input)="onTimeInput($event, day, 'open')"
                  (blur)="formatTime(day, 'open')"
                  class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white 
                         focus:border-primary-500/50 focus:ring-2 focus:ring-primary-500/20 focus:outline-none"
                  placeholder="HH:MM"
                  pattern="([01]?[0-9]|2[0-3]):[0-5][0-9]"
                />
              </div>
              
              <span class="text-gray-400 mx-2 self-end mb-3">έως</span>
              
              <div class="flex-1 ml-4">
                <label class="block text-sm text-gray-400 mb-1">Ώρα κλεισίματος</label>
                <input 
                  type="text" 
                  [value]="closeTimes[day]"
                  (input)="onTimeInput($event, day, 'close')"
                  (blur)="formatTime(day, 'close')"
                  class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white 
                         focus:border-primary-500/50 focus:ring-2 focus:ring-primary-500/20 focus:outline-none"
                  placeholder="HH:MM"
                  pattern="([01]?[0-9]|2[0-3]):[0-5][0-9]"
                />
              </div>
            </div>
            
            <!-- Error message -->
            <div *ngIf="timeErrors[day]" class="text-red-400 text-sm mt-2">{{ timeErrors[day] }}</div>
          </div>
        </div>
      </div>

      <!-- Error summary section -->
      <div
        *ngIf="showErrorSummary"
        class="bg-red-500/10 border border-red-500/30 rounded-lg p-4 my-4 animate-fadeIn"
      >
        <h3 class="text-red-400 font-medium mb-2 flex items-center">
          <span class="mr-2">⚠️</span> Παρακαλώ διορθώστε τα παρακάτω σφάλματα:
        </h3>
        <ul class="list-disc list-inside text-sm text-red-400 space-y-1">
          <li
            *ngFor="let error of validationErrors"
            class="cursor-pointer hover:underline"
            (click)="scrollToErrorField(error)"
          >
            {{ error.message }}
          </li>
        </ul>
      </div>

      <div class="flex justify-between pt-6">
        <button
          type="button"
          (click)="onBack()"
          class="px-6 py-2 border border-gray-600 text-gray-300 rounded-lg
                 hover:bg-white/5 transition-all duration-300"
        >
          Πίσω
        </button>

        <button
          type="button"
          (click)="onSubmit()"
          class="px-6 py-2 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg
                 hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 
                 transform hover:-translate-y-1 cursor-pointer"
        >
          Ολοκλήρωση Εγγραφής
        </button>
      </div>
    </div>
  `,
  styles: [
    `
      @keyframes fadeIn {
        from {
          opacity: 0;
          transform: translateY(-10px);
        }
        to {
          opacity: 1;
          transform: translateY(0);
        }
      }

      .animate-fadeIn {
        animation: fadeIn 0.3s ease-out forwards;
      }
    `,
  ],
})
export class ShelterInfoComponent implements OnInit {
  @Input() form!: FormGroup;
  @Output() back = new EventEmitter<void>();
  @Output() submit = new EventEmitter<void>();
  @ViewChild('formContainer') formContainer!: ElementRef;

  days = [
    'Δευτέρα',
    'Τρίτη',
    'Τετάρτη',
    'Πέμπτη',
    'Παρασκευή',
    'Σάββατο',
    'Κυριακή',
  ];
  
  // Track closed status for each day
  closedDays: {[key: string]: boolean} = {};
  
  // Track open and close times for each day
  openTimes: {[key: string]: string} = {};
  closeTimes: {[key: string]: string} = {};
  
  // Track validation errors for each day
  timeErrors: {[key: string]: string | null} = {};
  
  // Track if any operating hours have been modified
  operatingHoursModified: boolean = false;
  
  validationErrors: {
    field: string;
    message: string;
    element?: HTMLElement;
  }[] = [];
  showErrorSummary = false;

  constructor(private cdr: ChangeDetectorRef) {
    // Initialize default values
    this.days.forEach(day => {
      this.closedDays[day] = false;
      this.openTimes[day] = '';
      this.closeTimes[day] = '';
      this.timeErrors[day] = null;
    });
  }

  ngOnInit() {
    // Initialize time values from form
    this.days.forEach(day => {
      const dayKey = this.getDayKey(day);
      const operatingHours = this.getOperatingHoursForm();
      const value = operatingHours.get(dayKey)?.value;
      
      if (value === 'closed') {
        this.closedDays[day] = true;
        this.operatingHoursModified = true;
      } else if (value) {
        const [open, close] = value.split(',');
        if (open) {
          this.openTimes[day] = open;
          this.operatingHoursModified = true;
        }
        if (close) {
          this.closeTimes[day] = close;
          this.operatingHoursModified = true;
        }
      }
    });
  }

  getShelterForm(): FormGroup {
    return this.form.get('shelter') as FormGroup;
  }

  getSocialMediaForm(): FormGroup {
    return this.getShelterForm().get('socialMedia') as FormGroup;
  }

  getOperatingHoursForm(): FormGroup {
    return this.getShelterForm().get('operatingHours') as FormGroup;
  }

  getOperatingHoursValue(day: string): string {
    const dayKey = this.getDayKey(day);
    const operatingHours = this.getOperatingHoursForm();
    return operatingHours.get(dayKey)?.value || '';
  }

  isDescriptionInvalid(): boolean {
    const control = this.getShelterForm().get('description');
    return !!(control?.invalid && (control?.touched || control?.dirty));
  }

  updateDescription(value: string): void {
    const descriptionControl = this.getShelterForm().get('description');
    if (descriptionControl) {
      descriptionControl.setValue(value);
      descriptionControl.markAsDirty();
      descriptionControl.updateValueAndValidity();
      this.cdr.markForCheck();
    }
  }

  onClosedChange(day: string): void {
    const dayKey = this.getDayKey(day);
    const operatingHours = this.getOperatingHoursForm();
    
    // Mark as modified
    this.operatingHoursModified = true;
    
    if (this.closedDays[day]) {
      // Set to closed
      operatingHours.get(dayKey)?.setValue('closed');
      this.timeErrors[day] = null;
    } else {
      // Set to open with empty times initially
      if (!this.openTimes[day] && !this.closeTimes[day]) {
        this.openTimes[day] = '';
        this.closeTimes[day] = '';
        operatingHours.get(dayKey)?.setValue('');
      } else {
        // Use existing times if available
        const timeValue = `${this.openTimes[day]},${this.closeTimes[day]}`;
        operatingHours.get(dayKey)?.setValue(timeValue);
        
        // Validate time range
        this.validateTimeRange(day);
      }
    }
    
    this.cdr.markForCheck();
  }

  onTimeInput(event: Event, day: string, type: 'open' | 'close'): void {
    const input = event.target as HTMLInputElement;
    let value = input.value;
    
    // Mark as modified
    this.operatingHoursModified = true;
    
    // Allow only digits and colon
    value = value.replace(/[^\d:]/g, '');
    
    // Enforce strict HH:MM format as the user types
    if (value.length > 0) {
      // If user enters more than 2 digits without a colon, insert it
      if (!value.includes(':') && value.length > 2) {
        value = value.substring(0, 2) + ':' + value.substring(2);
      }
      
      // Limit to 5 characters (HH:MM)
      if (value.length > 5) {
        value = value.substring(0, 5);
      }
      
      // Validate hours (00-23)
      if (value.includes(':')) {
        const hours = value.split(':')[0];
        if (hours.length === 2) {
          const hoursNum = parseInt(hours, 10);
          if (hoursNum > 23) {
            value = '23' + value.substring(2);
          }
        }
      }
      
      // Validate minutes (00-59)
      if (value.includes(':') && value.length > 3) {
        const minutes = value.split(':')[1];
        if (minutes.length === 2) {
          const minutesNum = parseInt(minutes, 10);
          if (minutesNum > 59) {
            value = value.split(':')[0] + ':59';
          }
        }
      }
    }
    
    // Store the value
    if (type === 'open') {
      this.openTimes[day] = value;
    } else {
      this.closeTimes[day] = value;
    }
    
    // Update the input value
    input.value = value;
    
    // Update form value if both times are set
    if (this.openTimes[day] && this.closeTimes[day]) {
      this.updateFormValue(day);
      this.validateTimeRange(day);
    }
  }

  formatTime(day: string, type: 'open' | 'close'): void {
    let time = type === 'open' ? this.openTimes[day] : this.closeTimes[day];
    
    // Format as HH:MM
    if (time) {
      // Remove any non-digit or non-colon characters
      time = time.replace(/[^\d:]/g, '');
      
      // If there's no colon but there are digits, insert a colon
      if (!time.includes(':') && time.length > 0) {
        // If only one digit for hours, pad with 0
        if (time.length === 1) {
          time = '0' + time + ':00';
        } 
        // If two digits for hours
        else if (time.length === 2) {
          time = time + ':00';
        }
        // If more than 2 digits, format properly
        else {
          time = time.substring(0, 2) + ':' + time.substring(2);
        }
      }
      
      // Split into hours and minutes
      let [hours, minutes] = time.split(':');
      
      // Validate and format hours
      let hoursNum = parseInt(hours || '0', 10);
      if (isNaN(hoursNum) || hoursNum > 23) hoursNum = 0;
      hours = hoursNum.toString().padStart(2, '0');
      
      // Validate and format minutes
      let minutesNum = parseInt(minutes || '0', 10);
      if (isNaN(minutesNum) || minutesNum > 59) minutesNum = 0;
      minutes = minutesNum.toString().padStart(2, '0');
      
      // Combine formatted time
      time = `${hours}:${minutes}`;
      
      // Update the time
      if (type === 'open') {
        this.openTimes[day] = time;
      } else {
        this.closeTimes[day] = time;
      }
      
      // Update the form value
      this.updateFormValue(day);
      
      // Validate time range
      this.validateTimeRange(day);
      
      this.cdr.markForCheck();
    }
  }

  updateFormValue(day: string): void {
    const dayKey = this.getDayKey(day);
    const operatingHours = this.getOperatingHoursForm();
    
    if (!this.closedDays[day]) {
      // Only update if both times are valid
      if (this.isValidTimeFormat(this.openTimes[day]) && this.isValidTimeFormat(this.closeTimes[day])) {
        const timeValue = `${this.openTimes[day]},${this.closeTimes[day]}`;
        operatingHours.get(dayKey)?.setValue(timeValue);
        operatingHours.get(dayKey)?.markAsTouched();
        operatingHours.get(dayKey)?.markAsDirty();
      } else {
        // If either time is invalid, set an empty value
        operatingHours.get(dayKey)?.setValue('');
      }
    }
  }

  isValidTimeFormat(time: string): boolean {
    // Check if time matches HH:MM format (00:00 to 23:59)
    return /^([01]\d|2[0-3]):([0-5]\d)$/.test(time);
  }

  validateTimeRange(day: string): void {
    const openTime = this.openTimes[day];
    const closeTime = this.closeTimes[day];
    
    // Clear previous errors
    this.timeErrors[day] = null;
    
    // Check if both times are provided
    if (openTime && closeTime) {
      // Check if both times are in valid format
      if (!this.isValidTimeFormat(openTime)) {
        this.timeErrors[day] = 'Η ώρα ανοίγματος πρέπει να είναι σε μορφή ΩΩ:ΛΛ (π.χ. 09:00)';
        this.setTimeRangeError(day);
        return;
      }
      
      if (!this.isValidTimeFormat(closeTime)) {
        this.timeErrors[day] = 'Η ώρα κλεισίματος πρέπει να είναι σε μορφή ΩΩ:ΛΛ (π.χ. 17:00)';
        this.setTimeRangeError(day);
        return;
      }
      
      // Check if close time is after open time
      if (openTime >= closeTime) {
        this.timeErrors[day] = 'Η ώρα κλεισίματος πρέπει να είναι μετά την ώρα ανοίγματος';
        this.setTimeRangeError(day);
        return;
      }
      
      // If we get here, the time range is valid
      const dayKey = this.getDayKey(day);
      const control = this.getOperatingHoursForm().get(dayKey);
      if (control?.errors?.['invalidTimeRange']) {
        // Clear only the invalidTimeRange error
        const errors = {...control.errors};
        delete errors['invalidTimeRange'];
        if (Object.keys(errors).length === 0) {
          control.setErrors(null);
        } else {
          control.setErrors(errors);
        }
      }
    } else if ((openTime && !closeTime) || (!openTime && closeTime)) {
      // If only one time is provided
      this.timeErrors[day] = 'Πρέπει να συμπληρώσετε και τις δύο ώρες';
      this.setTimeRangeError(day);
    }
  }

  setTimeRangeError(day: string): void {
    const dayKey = this.getDayKey(day);
    this.getOperatingHoursForm().get(dayKey)?.setErrors({ invalidTimeRange: true });
  }

  private getDayKey(day: string): string {
    const dayMap: { [key: string]: string } = {
      'Δευτέρα': 'monday',
      'Τρίτη': 'tuesday',
      'Τετάρτη': 'wednesday',
      'Πέμπτη': 'thursday',
      'Παρασκευή': 'friday',
      'Σάββατο': 'saturday',
      'Κυριακή': 'sunday',
    };
    return dayMap[day] || day.toLowerCase();
  }

  onBack(): void {
    this.back.emit();
  }

  onSubmit(): void {
    // Reset error summary
    this.validationErrors = [];
    this.showErrorSummary = false;

    // Mark all fields as touched to show validation errors
    this.markFormGroupTouched(this.getShelterForm());
    
    // Validate operating hours if any have been modified
    if (this.operatingHoursModified) {
      this.validateOperatingHours();
    }

    if (this.getShelterForm().valid) {
      this.submit.emit();
    } else {
      // Collect validation errors for summary
      this.collectValidationErrors();

      // Show error summary
      this.showErrorSummary = this.validationErrors.length > 0;

      // Find and scroll to the first invalid field
      this.scrollToFirstInvalidField();

      // Ensure UI updates
      this.cdr.markForCheck();
    }
  }

  // Validate that if any operating hours are set, all days must have valid values
  validateOperatingHours(): void {
    if (this.operatingHoursModified) {
      let hasAnyTimeSet = false;
      let hasInvalidDay = false;
      
      // Check if any day has times set
      this.days.forEach(day => {
        const dayKey = this.getDayKey(day);
        const value = this.getOperatingHoursForm().get(dayKey)?.value;
        
        if (value && value !== 'closed' && value !== '') {
          hasAnyTimeSet = true;
        }
      });
      
      // If any day has times set, all days must have valid values
      if (hasAnyTimeSet) {
        this.days.forEach(day => {
          const dayKey = this.getDayKey(day);
          const value = this.getOperatingHoursForm().get(dayKey)?.value;
          
          // If not closed and not a valid time range, mark as invalid
          if (value !== 'closed' && (value === '' || !value)) {
            this.timeErrors[day] = 'Πρέπει να ορίσετε ώρες λειτουργίας ή να επιλέξετε "Κλειστό"';
            this.setTimeRangeError(day);
            hasInvalidDay = true;
          }
        });
      }
      
      // If any day is invalid, show error summary
      if (hasInvalidDay) {
        this.validationErrors.push({
          field: 'operatingHours',
          message: 'Πρέπει να ορίσετε ώρες λειτουργίας για όλες τις ημέρες ή να τις επιλέξετε ως "Κλειστό"'
        });
      }
    }
  }

  // Helper method to mark all controls in a form group as touched
  private markFormGroupTouched(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach((key) => {
      const control = formGroup.get(key);
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      } else if (control) {
        control.markAsTouched();
        control.markAsDirty();
        control.updateValueAndValidity();
      }
    });
    this.cdr.markForCheck();
  }

  // Collect validation errors for the error summary
  private collectValidationErrors(): void {
    // Check shelter name
    const shelterNameControl = this.getShelterForm().get('shelterName');
    if (shelterNameControl?.invalid) {
      const element = this.findElementForControl('shelterName');
      if (shelterNameControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'shelterName',
          message: 'Το όνομα καταφυγίου είναι υποχρεωτικό',
          element,
        });
      } else if (shelterNameControl.errors?.['minlength']) {
        this.validationErrors.push({
          field: 'shelterName',
          message:
            'Το όνομα καταφυγίου πρέπει να έχει τουλάχιστον 3 χαρακτήρες',
          element,
        });
      }
    }

    // Check description
    const descriptionControl = this.getShelterForm().get('description');
    if (descriptionControl?.invalid) {
      const element = this.findElementForControl('description');
      if (descriptionControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'description',
          message: 'Η περιγραφή είναι υποχρεωτική',
          element,
        });
      } else if (descriptionControl.errors?.['minlength']) {
        this.validationErrors.push({
          field: 'description',
          message: 'Η περιγραφή πρέπει να έχει τουλάχιστον 10 χαρακτήρες',
          element,
        });
      }
    }

    // Check website
    const websiteControl = this.getShelterForm().get('website');
    if (websiteControl?.invalid && websiteControl.value) {
      const element = this.findElementForControl('website');
      this.validationErrors.push({
        field: 'website',
        message: 'Η διεύθυνση ιστοσελίδας δεν είναι έγκυρη',
        element,
      });
    }

    // Check social media
    const facebookControl = this.getSocialMediaForm().get('facebook');
    if (facebookControl?.invalid && facebookControl.value) {
      const element = this.findElementForControl('facebook', 'socialMedia');
      this.validationErrors.push({
        field: 'facebook',
        message: 'Η διεύθυνση Facebook δεν είναι έγκυρη',
        element,
      });
    }

    const instagramControl = this.getSocialMediaForm().get('instagram');
    if (instagramControl?.invalid && instagramControl.value) {
      const element = this.findElementForControl('instagram', 'socialMedia');
      this.validationErrors.push({
        field: 'instagram',
        message: 'Η διεύθυνση Instagram δεν είναι έγκυρη',
        element,
      });
    }

    // Check operating hours
    const operatingHoursForm = this.getOperatingHoursForm();
    
    // Check each day for time range errors
    this.days.forEach(day => {
      const dayKey = this.getDayKey(day);
      const control = operatingHoursForm.get(dayKey);
      
      if (control?.invalid && !this.closedDays[day]) {
        // Find the input element for this day
        const dayElement = this.findDayElement(day);
        
        if (control.errors?.['invalidTimeRange']) {
          this.validationErrors.push({
            field: dayKey,
            message: this.timeErrors[day] || `Σφάλμα στις ώρες λειτουργίας για ${day}`,
            element: dayElement
          });
        }
      }
    });
  }

  // Find element for a control
  private findElementForControl(
    controlName: string,
    groupName?: string
  ): HTMLElement | undefined {
    let selector = '';

    if (groupName) {
      // For nested controls
      selector = `[formcontrolname="${controlName}"]`;
    } else {
      // For direct controls
      selector = `[formcontrolname="${controlName}"]`;
    }

    // Try to find the element
    let element = this.formContainer?.nativeElement.querySelector(
      selector
    ) as HTMLElement;

    // If not found, try to find by ID
    if (!element) {
      element = this.formContainer?.nativeElement.querySelector(
        `#${controlName}`
      ) as HTMLElement;
    }

    // If still not found and it's a textarea, try to find it differently
    if (!element && controlName === 'description') {
      element = this.formContainer?.nativeElement.querySelector(
        'textarea'
      ) as HTMLElement;
    }

    return element;
  }

  // Find element for a specific day
  private findDayElement(day: string): HTMLElement | undefined {
    if (!this.formContainer) return undefined;
    
    // Try to find the day container
    const dayElements = this.formContainer.nativeElement.querySelectorAll('h4');
    let dayElement: HTMLElement | undefined;
    
    for (let i = 0; i < dayElements.length; i++) {
      if (dayElements[i].textContent?.trim() === day) {
        // Found the day heading, get the parent container
        dayElement = dayElements[i].closest('.border-gray-700\\/50') as HTMLElement;
        break;
      }
    }
    
    return dayElement;
  }

  // Scroll to a specific error field
  scrollToErrorField(error: {
    field: string;
    message: string;
    element?: HTMLElement;
  }): void {
    if (error.element) {
      // Highlight the element
      this.highlightElement(error.element);

      // Scroll to the element
      error.element.scrollIntoView({ behavior: 'smooth', block: 'center' });

      // Focus the element if it's an input
      if (
        error.element instanceof HTMLInputElement ||
        error.element instanceof HTMLTextAreaElement ||
        error.element instanceof HTMLSelectElement
      ) {
        error.element.focus();
      }
    } else {
      // If no element is found, try to find it again
      const element = this.findElementForControl(error.field);
      if (element) {
        this.highlightElement(element);
        element.scrollIntoView({ behavior: 'smooth', block: 'center' });
        if (
          element instanceof HTMLInputElement ||
          element instanceof HTMLTextAreaElement ||
          element instanceof HTMLSelectElement
        ) {
          element.focus();
        }
      }
    }
  }

  // Add highlight effect to an element
  private highlightElement(element: HTMLElement): void {
    // Add a temporary highlight class
    element.classList.add('highlight-error');

    // Remove the class after animation completes
    setTimeout(() => {
      element.classList.remove('highlight-error');
    }, 1500);
  }

  // Scroll to the first invalid field with enhanced error handling
  private scrollToFirstInvalidField(): void {
    setTimeout(() => {
      try {
        if (this.validationErrors.length > 0) {
          // First scroll to error summary
          const errorSummary =
            this.formContainer?.nativeElement.querySelector('.bg-red-500\\/10');
          if (errorSummary) {
            errorSummary.scrollIntoView({ behavior: 'smooth', block: 'start' });
          } else {
            // If no error summary, scroll to the first invalid field
            const firstError = this.validationErrors[0];
            if (firstError.element) {
              this.scrollToErrorField(firstError);
            } else {
              // Try to find the first invalid control
              const invalidControls = this.findInvalidControls(
                this.getShelterForm()
              );
              if (invalidControls.length > 0) {
                const controlName = this.getControlName(invalidControls[0]);
                const element = this.findElementForControl(controlName);
                if (element) {
                  this.highlightElement(element);
                  element.scrollIntoView({
                    behavior: 'smooth',
                    block: 'center',
                  });
                }
              }
            }
          }
        }
      } catch (error) {
        console.error('Error scrolling to invalid field:', error);
        // Fallback: scroll to the top of the form
        if (this.formContainer) {
          this.formContainer.nativeElement.scrollIntoView({
            behavior: 'smooth',
            block: 'start',
          });
        }
      }
    }, 100);
  }

  // Find all invalid controls in a form group
  private findInvalidControls(formGroup: FormGroup): AbstractControl[] {
    const invalidControls: AbstractControl[] = [];
    const controls = formGroup.controls;

    Object.keys(controls).forEach((controlName) => {
      const control = controls[controlName];

      if (control instanceof FormGroup) {
        // If it's a nested form group, recursively find invalid controls
        invalidControls.push(...this.findInvalidControls(control));
      } else if (control && control.invalid) {
        invalidControls.push(control);
      }
    });

    return invalidControls;
  }

  // Get the name of a control
  private getControlName(control: AbstractControl): string {
    let controlName = '';

    // Try to find in the shelter form
    const shelterForm = this.getShelterForm();
    Object.keys(shelterForm.controls).forEach((name) => {
      if (shelterForm.controls[name] === control) {
        controlName = name;
      }
    });

    // If not found, try in nested forms
    if (!controlName) {
      // Check in social media form
      const socialMediaForm = this.getSocialMediaForm();
      Object.keys(socialMediaForm.controls).forEach((name) => {
        if (socialMediaForm.controls[name] === control) {
          controlName = name;
        }
      });

      // Check in operating hours form
      if (!controlName) {
        const operatingHoursForm = this.getOperatingHoursForm();
        Object.keys(operatingHoursForm.controls).forEach((name) => {
          if (operatingHoursForm.controls[name] === control) {
            controlName = name;
          }
        });
      }
    }

    return controlName;
  }
}