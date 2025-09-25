import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  ElementRef,
  ViewChild,
  ChangeDetectorRef,
  OnInit,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormGroup,
  ReactiveFormsModule,
  FormsModule,
  AbstractControl,
  Validators,
  ValidatorFn,
  ValidationErrors,
} from '@angular/forms';
import { NgIconsModule } from '@ng-icons/core';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { TextAreaInputComponent } from 'src/app/common/ui/text-area-input.component';
import {
  ErrorMessageBannerComponent,
  ErrorDetails,
} from 'src/app/common/ui/error-message-banner.component';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { CustomValidators } from 'src/app/ui/components/auth/validators/custom.validators';

@Component({
  selector: 'app-shelter-info',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    FormInputComponent,
    TextAreaInputComponent,
    NgIconsModule,
    ErrorMessageBannerComponent,
    TranslatePipe,
  ],
  templateUrl: './shelter-info.component.html',
})
export class ShelterInfoComponent implements OnInit {
  @Input() form!: FormGroup;
  @Input() isLoading = false;
  @Input() language!: string;
  @Output() back = new EventEmitter<void>();
  @Output() submit = new EventEmitter<void>();
  @ViewChild('formContainer') formContainer!: ElementRef;

  error?: ErrorDetails;

  days = [
    'Δευτέρα',
    'Τρίτη',
    'Τετάρτη',
    'Πέμπτη',
    'Παρασκευή',
    'Σάββατο',
    'Κυριακή',
  ];

  closedDays: { [key: string]: boolean } = {};
  openTimes: { [key: string]: string } = {};
  closeTimes: { [key: string]: string } = {};
  timeErrors: { [key: string]: string | null } = {};
  operatingHoursModified: boolean = false;

  validationErrors: {
    field: string;
    message: string;
    element?: HTMLElement;
  }[] = [];
  showErrorSummary = false;

  hasUnsavedChanges(): boolean {
    const shelterForm = this.getShelterForm();
    const socialForm = this.getSocialMediaForm();
    const hoursForm = this.getOperatingHoursForm();
    const anyDirty =
      !!shelterForm?.dirty || !!socialForm?.dirty || !!hoursForm?.dirty;
    return anyDirty || this.operatingHoursModified;
  }

  constructor(private cdr: ChangeDetectorRef) {
    this.days.forEach((day) => {
      this.closedDays[day] = false;
      this.openTimes[day] = '';
      this.closeTimes[day] = '';
      this.timeErrors[day] = null;
    });
  }

  ngOnInit() {
    // Set up validators for shelter form fields
    this.setupShelterValidators();

    this.days.forEach((day) => {
      const dayKey = this.getDayKey(day);
      const operatingHours = this.getOperatingHoursForm();
      const value = operatingHours.get(dayKey)?.value;

      if (value === 'closed') {
        this.closedDays[day] = true;
        this.operatingHoursModified = true;
      } else if (value && value.includes(',')) {
        const [open, close] = value.split(',');
        if (open) {
          this.openTimes[day] = open;
          this.operatingHoursModified = true;
        }
        if (close) {
          this.closeTimes[day] = close;
          this.operatingHoursModified = true;
        }
      } else {
        this.openTimes[day] = '';
        this.closeTimes[day] = '';
        operatingHours.get(dayKey)?.setValue('');
      }
    });

    this.updateOperatingHoursValidators();
  }

  private setupShelterValidators(): void {
    const shelterForm = this.getShelterForm();

    // Set up shelter name validators
    shelterForm
      .get('shelterName')
      ?.setValidators([Validators.required, Validators.minLength(3)]);

    // Set up description validators
    shelterForm
      .get('description')
      ?.setValidators([Validators.required, Validators.minLength(10)]);

    // Set up website validator (optional field)
    shelterForm
      .get('website')
      ?.setValidators([Validators.pattern(/^https?:\/\/.+\..+$/)]);

    // Set up social media validators (all optional)
    const socialMediaForm = this.getSocialMediaForm();
    socialMediaForm
      .get('facebook')
      ?.setValidators([this.createOptionalSocialMediaValidator('facebook')]);
    socialMediaForm
      .get('instagram')
      ?.setValidators([this.createOptionalSocialMediaValidator('instagram')]);

    // Set up operating hours validators
    const operatingHoursForm = this.getOperatingHoursForm();
    Object.keys(operatingHoursForm.controls).forEach((key) => {
      operatingHoursForm
        .get(key)
        ?.setValidators([CustomValidators.operatingHoursValidator()]);
    });

    // Update validity for all controls
    shelterForm.get('shelterName')?.updateValueAndValidity();
    shelterForm.get('description')?.updateValueAndValidity();
    shelterForm.get('website')?.updateValueAndValidity();
    socialMediaForm.get('facebook')?.updateValueAndValidity();
    socialMediaForm.get('instagram')?.updateValueAndValidity();
    operatingHoursForm.updateValueAndValidity();
  }

  private createOptionalSocialMediaValidator(
    platform: 'facebook' | 'instagram'
  ): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      // If no value is provided, it's valid (optional field)
      if (!control.value || control.value.trim() === '') {
        return null;
      }

      // If value is provided, validate it contains the platform name
      const containsPlatform = control.value.toLowerCase().includes(platform);
      return containsPlatform ? null : { invalidSocialMedia: true };
    };
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

    this.operatingHoursModified = true;

    if (this.closedDays[day]) {
      operatingHours.get(dayKey)?.setValue('closed');
      this.timeErrors[day] = null;
    } else {
      this.openTimes[day] = '';
      this.closeTimes[day] = '';
      operatingHours.get(dayKey)?.setValue('');
    }

    this.updateOperatingHoursValidators();
    this.cdr.markForCheck();
  }

  onTimeInput(event: Event, day: string, type: 'open' | 'close'): void {
    const input = event.target as HTMLInputElement;
    let value = input.value;

    this.operatingHoursModified = true;
    value = value.replace(/[^\d:]/g, '');

    if (value.length > 0) {
      if (!value.includes(':') && value.length > 2) {
        value = value.substring(0, 2) + ':' + value.substring(2);
      }

      if (value.length > 5) {
        value = value.substring(0, 5);
      }

      if (value.includes(':')) {
        const hours = value.split(':')[0];
        if (hours.length === 2) {
          const hoursNum = parseInt(hours, 10);
          if (hoursNum > 23) {
            value = '23' + value.substring(2);
          }
        }
      }

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

    if (type === 'open') {
      this.openTimes[day] = value;
    } else {
      this.closeTimes[day] = value;
    }

    input.value = value;

    if (this.openTimes[day] && this.closeTimes[day]) {
      this.updateFormValue(day);
      this.validateTimeRange(day);
    }

    this.updateOperatingHoursValidators();
  }

  formatTime(day: string, type: 'open' | 'close'): void {
    let time = type === 'open' ? this.openTimes[day] : this.closeTimes[day];

    if (time) {
      time = time.replace(/[^\d:]/g, '');

      if (!time.includes(':') && time.length > 0) {
        if (time.length === 1) {
          time = '0' + time + ':00';
        } else if (time.length === 2) {
          time = time + ':00';
        } else {
          time = time.substring(0, 2) + ':' + time.substring(2);
        }
      }

      let [hours, minutes] = time.split(':');

      let hoursNum = parseInt(hours || '0', 10);
      if (isNaN(hoursNum) || hoursNum > 23) hoursNum = 0;
      hours = hoursNum.toString().padStart(2, '0');

      let minutesNum = parseInt(minutes || '0', 10);
      if (isNaN(minutesNum) || minutesNum > 59) minutesNum = 0;
      minutes = minutesNum.toString().padStart(2, '0');

      time = `${hours}:${minutes}`;

      if (type === 'open') {
        this.openTimes[day] = time;
      } else {
        this.closeTimes[day] = time;
      }

      this.updateFormValue(day);
      this.validateTimeRange(day);
      this.updateOperatingHoursValidators();
      this.cdr.markForCheck();
    }
  }

  updateFormValue(day: string): void {
    const dayKey = this.getDayKey(day);
    const operatingHours = this.getOperatingHoursForm();

    if (!this.closedDays[day]) {
      if (
        this.isValidTimeFormat(this.openTimes[day]) &&
        this.isValidTimeFormat(this.closeTimes[day])
      ) {
        const timeValue = `${this.openTimes[day]},${this.closeTimes[day]}`;
        operatingHours.get(dayKey)?.setValue(timeValue);
        operatingHours.get(dayKey)?.markAsTouched();
        operatingHours.get(dayKey)?.markAsDirty();
      } else {
        operatingHours.get(dayKey)?.setValue('');
      }
    }
  }

  isValidTimeFormat(time: string): boolean {
    return /^([01]\d|2[0-3]):([0-5]\d)$/.test(time);
  }

  validateTimeRange(day: string): void {
    const openTime = this.openTimes[day];
    const closeTime = this.closeTimes[day];

    this.timeErrors[day] = null;

    if (openTime && closeTime) {
      if (!this.isValidTimeFormat(openTime)) {
        this.timeErrors[day] =
          'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.OPENING_TIME_FORMAT';
        this.setTimeRangeError(day);
        return;
      }

      if (!this.isValidTimeFormat(closeTime)) {
        this.timeErrors[day] =
          'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.CLOSING_TIME_FORMAT';
        this.setTimeRangeError(day);
        return;
      }

      if (openTime >= closeTime) {
        this.timeErrors[day] =
          'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.TIME_RANGE_INVALID';
        this.setTimeRangeError(day);
        return;
      }

      const dayKey = this.getDayKey(day);
      const control = this.getOperatingHoursForm().get(dayKey);
      if (control?.errors?.['invalidTimeRange']) {
        const errors = { ...control.errors };
        delete errors['invalidTimeRange'];
        if (Object.keys(errors).length === 0) {
          control.setErrors(null);
        } else {
          control.setErrors(errors);
        }
      }
    } else if ((openTime && !closeTime) || (!openTime && closeTime)) {
      this.timeErrors[day] =
        'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.BOTH_TIMES_REQUIRED';
      this.setTimeRangeError(day);
    }
  }

  setTimeRangeError(day: string): void {
    const dayKey = this.getDayKey(day);
    this.getOperatingHoursForm()
      .get(dayKey)
      ?.setErrors({ invalidTimeRange: true });
  }

  getDayKey(day: string): string {
    const dayMap: { [key: string]: string } = {
      Δευτέρα: 'monday',
      Τρίτη: 'tuesday',
      Τετάρτη: 'wednesday',
      Πέμπτη: 'thursday',
      Παρασκευή: 'friday',
      Σάββατο: 'saturday',
      Κυριακή: 'sunday',
    };
    return dayMap[day] || day.toLowerCase();
  }

  hasAnyOperatingHoursSet(): boolean {
    return this.days.some((day) => {
      if (this.closedDays[day]) return true;

      const dayKey = this.getDayKey(day);
      const value = this.getOperatingHoursForm().get(dayKey)?.value;
      return value && value !== '' && value !== 'closed';
    });
  }

  updateOperatingHoursValidators(): void {
    const hasAnyHoursSet = this.hasAnyOperatingHoursSet();
    const operatingHoursForm = this.getOperatingHoursForm();

    this.days.forEach((day) => {
      const dayKey = this.getDayKey(day);
      const control = operatingHoursForm.get(dayKey);

      if (hasAnyHoursSet) {
        if (this.closedDays[day]) {
          control?.setErrors(null);
        } else if (!this.openTimes[day] || !this.closeTimes[day]) {
          this.timeErrors[day] =
            'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.HOURS_OR_CLOSED_REQUIRED';
          control?.setErrors({ required: true });
        } else {
          this.validateTimeRange(day);
        }
      } else {
        control?.setErrors(null);
        this.timeErrors[day] = null;
      }
    });

    this.cdr.markForCheck();
  }

  hasAnyNonClosedDay(): boolean {
    return this.days.some((day) => {
      if (!this.closedDays[day]) {
        const dayKey = this.getDayKey(day);
        const value = this.getOperatingHoursForm().get(dayKey)?.value;
        return value && value !== '' && value !== 'closed';
      }
      return false;
    });
  }

  validateOperatingHours(): void {
    let hasInvalidDay = false;

    this.days.forEach((day) => {
      const dayKey = this.getDayKey(day);
      const control = this.getOperatingHoursForm().get(dayKey);
      const value = control?.value;

      if (this.closedDays[day]) {
        control?.setErrors(null);
        this.timeErrors[day] = null;
        return;
      }

      if (!value || value === '') {
        this.timeErrors[day] =
          'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.HOURS_OR_CLOSED_REQUIRED';
        control?.setErrors({ required: true });
        hasInvalidDay = true;
      } else if (value !== 'closed') {
        if (control?.errors?.['invalidTimeRange']) {
          hasInvalidDay = true;
        }
      }
    });

    if (hasInvalidDay) {
      this.validationErrors.push({
        field: 'operatingHours',
        message: 'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.ALL_DAYS_REQUIRED',
      });
    }
  }

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

  private collectValidationErrors(): void {
    const shelterNameControl = this.getShelterForm().get('shelterName');
    if (shelterNameControl?.invalid) {
      const element = this.findElementForControl('shelterName');
      if (shelterNameControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'shelterName',
          message: 'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.SHELTER_NAME_REQUIRED',
          element,
        });
      } else if (shelterNameControl.errors?.['minlength']) {
        this.validationErrors.push({
          field: 'shelterName',
          message: 'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.SHELTER_NAME_MINLENGTH',
          element,
        });
      }
    }

    const descriptionControl = this.getShelterForm().get('description');
    if (descriptionControl?.invalid) {
      const element = this.findElementForControl('description');
      if (descriptionControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'description',
          message: 'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.DESCRIPTION_REQUIRED',
          element,
        });
      } else if (descriptionControl.errors?.['minlength']) {
        this.validationErrors.push({
          field: 'description',
          message: 'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.DESCRIPTION_MINLENGTH',
          element,
        });
      }
    }

    const websiteControl = this.getShelterForm().get('website');
    if (websiteControl?.invalid && websiteControl.value) {
      const element = this.findElementForControl('website');
      this.validationErrors.push({
        field: 'website',
        message: 'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.WEBSITE_INVALID',
        element,
      });
    }

    const facebookControl = this.getSocialMediaForm().get('facebook');
    if (
      facebookControl?.invalid &&
      facebookControl.value &&
      facebookControl.value.trim() !== ''
    ) {
      const element = this.findElementForControl('facebook', 'socialMedia');
      this.validationErrors.push({
        field: 'facebook',
        message: 'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.FACEBOOK_INVALID',
        element,
      });
    }

    const instagramControl = this.getSocialMediaForm().get('instagram');
    if (
      instagramControl?.invalid &&
      instagramControl.value &&
      instagramControl.value.trim() !== ''
    ) {
      const element = this.findElementForControl('instagram', 'socialMedia');
      this.validationErrors.push({
        field: 'instagram',
        message: 'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.INSTAGRAM_INVALID',
        element,
      });
    }

    if (this.hasAnyOperatingHoursSet()) {
      const operatingHoursForm = this.getOperatingHoursForm();

      this.days.forEach((day) => {
        const dayKey = this.getDayKey(day);
        const control = operatingHoursForm.get(dayKey);

        if (control?.invalid && !this.closedDays[day]) {
          const dayElement = this.findDayElement(day);

          if (control.errors?.['invalidTimeRange']) {
            this.validationErrors.push({
              field: dayKey,
              message:
                this.timeErrors[day] ||
                'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.OPERATING_HOURS_ERROR',
              element: dayElement,
            });
          } else if (control.errors?.['required']) {
            this.validationErrors.push({
              field: dayKey,
              message: 'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.DAY_HOURS_REQUIRED',
              element: dayElement,
            });
          }
        }
      });
    }
  }

  private findElementForControl(
    controlName: string,
    groupName?: string
  ): HTMLElement | undefined {
    let selector = groupName
      ? `[formcontrolname="${controlName}"]`
      : `[formcontrolname="${controlName}"]`;

    let element = this.formContainer?.nativeElement.querySelector(
      selector
    ) as HTMLElement;

    if (!element) {
      element = this.formContainer?.nativeElement.querySelector(
        `#${controlName}`
      ) as HTMLElement;
    }

    if (!element && controlName === 'description') {
      element = this.formContainer?.nativeElement.querySelector(
        'textarea'
      ) as HTMLElement;
    }

    return element;
  }

  private findDayElement(day: string): HTMLElement | undefined {
    if (!this.formContainer) return undefined;

    const dayElements = this.formContainer.nativeElement.querySelectorAll('h4');
    let dayElement: HTMLElement | undefined;

    for (let i = 0; i < dayElements.length; i++) {
      if (dayElements[i].textContent?.trim() === day) {
        dayElement = dayElements[i].closest(
          '.border-gray-700\\/50'
        ) as HTMLElement;
        break;
      }
    }

    return dayElement;
  }

  scrollToErrorField(error: {
    field: string;
    message: string;
    element?: HTMLElement;
  }): void {
    if (error.element) {
      this.highlightElement(error.element);
      error.element.scrollIntoView({ behavior: 'smooth', block: 'center' });
      if (
        error.element instanceof HTMLInputElement ||
        error.element instanceof HTMLTextAreaElement ||
        error.element instanceof HTMLSelectElement
      ) {
        error.element.focus();
      }
    } else {
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

  private highlightElement(element: HTMLElement): void {
    element.classList.add('highlight-error');
    setTimeout(() => {
      element.classList.remove('highlight-error');
    }, 1500);
  }

  private scrollToFirstInvalidField(): void {
    setTimeout(() => {
      try {
        if (this.validationErrors.length > 0) {
          const errorSummary =
            this.formContainer?.nativeElement.querySelector('.bg-red-500\\/10');
          if (errorSummary) {
            errorSummary.scrollIntoView({ behavior: 'smooth', block: 'start' });
          } else {
            const firstError = this.validationErrors[0];
            if (firstError.element) {
              this.scrollToErrorField(firstError);
            } else {
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
        if (this.formContainer) {
          this.formContainer.nativeElement.scrollIntoView({
            behavior: 'smooth',
            block: 'start',
          });
        }
      }
    }, 100);
  }

  private findInvalidControls(formGroup: FormGroup): AbstractControl[] {
    const invalidControls: AbstractControl[] = [];
    const controls = formGroup.controls;

    Object.keys(controls).forEach((controlName) => {
      const control = controls[controlName];
      if (control instanceof FormGroup) {
        invalidControls.push(...this.findInvalidControls(control));
      } else if (control && control.invalid) {
        invalidControls.push(control);
      }
    });

    return invalidControls;
  }

  private getControlName(control: AbstractControl): string {
    let controlName = '';

    const shelterForm = this.getShelterForm();
    Object.keys(shelterForm.controls).forEach((name) => {
      if (shelterForm.controls[name] === control) {
        controlName = name;
      }
    });

    if (!controlName) {
      const socialMediaForm = this.getSocialMediaForm();
      Object.keys(socialMediaForm.controls).forEach((name) => {
        if (socialMediaForm.controls[name] === control) {
          controlName = name;
        }
      });

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

  submitForm(): void {
    this.validationErrors = [];
    this.showErrorSummary = false;

    this.markFormGroupTouched(this.getShelterForm());

    const operatingHoursForm = this.getOperatingHoursForm();
    this.days.forEach((day) => {
      const dayKey = this.getDayKey(day);
      const control = operatingHoursForm.get(dayKey);

      if (this.closedDays[day]) {
        control?.setErrors(null);
        this.timeErrors[day] = null;
      }
    });

    const needsHoursValidation =
      this.operatingHoursModified && this.hasAnyNonClosedDay();

    if (!this.hasAnyOperatingHoursSet()) {
      this.days.forEach((day) => {
        const dayKey = this.getDayKey(day);
        operatingHoursForm.get(dayKey)?.setErrors(null);
        this.timeErrors[day] = null;
      });
      operatingHoursForm.updateValueAndValidity();
      this.operatingHoursModified = false;
    } else if (needsHoursValidation) {
      this.validateOperatingHours();
    }

    this.getShelterForm().updateValueAndValidity({
      onlySelf: false,
      emitEvent: true,
    });

    const isNameValid =
      !!this.getShelterForm().get('shelterName')?.value &&
      !this.getShelterForm().get('shelterName')?.errors;
    const isDescriptionValid =
      !!this.getShelterForm().get('description')?.value &&
      !this.getShelterForm().get('description')?.errors;

    if (isNameValid && isDescriptionValid) {
      this.submit.emit();
      return;
    }

    this.collectValidationErrors();
    this.showErrorSummary = this.validationErrors.length > 0;
    this.scrollToFirstInvalidField();
    this.cdr.markForCheck();
  }
}
