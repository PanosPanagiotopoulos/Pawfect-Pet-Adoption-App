import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class CustomValidators {
  static passwordValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }

      const hasUpperCase = /[A-Z]/.test(control.value);
      const hasLowerCase = /[a-z]/.test(control.value);
      const hasNumber = /\d/.test(control.value);
      const hasSpecialChar = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(
        control.value
      );
      const isLongEnough = control.value.length >= 8;

      const errors: ValidationErrors = {};

      if (!isLongEnough) {
        errors['minlength'] = { requiredLength: 8, actualLength: control.value.length };
      }
      if (!hasUpperCase) {
        errors['uppercase'] = { message: 'At least one uppercase letter required' };
      }
      if (!hasLowerCase) {
        errors['lowercase'] = { message: 'At least one lowercase letter required' };
      }
      if (!hasNumber) {
        errors['number'] = { message: 'At least one number required' };
      }
      if (!hasSpecialChar) {
        errors['specialChar'] = { message: 'At least one special character required' };
      }

      return Object.keys(errors).length > 0 ? errors : null;
    };
  }

  static matchValidator(matchTo: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.parent) {
        return null;
      }

      const matchControl = control.parent.get(matchTo);
      if (!matchControl) {
        return null;
      }

      if (control.value !== matchControl.value) {
        return { mismatch: { message: 'Passwords do not match' } };
      }

      return null;
    };
  }

  static phoneNumberValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }

      const valid = /^\+?[1-9]\d{1,14}$/.test(control.value);
      return valid ? null : { pattern: true };
    };
  }

  static zipCodeValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }

      const valid = /^\d{5}(-\d{4})?$/.test(control.value);
      return valid ? null : { pattern: true };
    };
  }

  static socialMediaValidator(platform: 'facebook' | 'instagram'): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }

      // Simplified validation - just check if the URL contains the platform name
      const containsPlatform = control.value.toLowerCase().includes(platform);
      return containsPlatform ? null : { invalidSocialMedia: true };
    };
  }

  static operatingHoursValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }

      if (control.value === 'closed') {
        return null;
      }

      // If the value is empty, it's valid (we'll alidate at the form level if any day has a value)
      if (control.value === '') {
        return null;
      }

      const timePattern =
        /^([01]?[0-9]|2[0-3]):[0-5][0-9],([01]?[0-9]|2[0-3]):[0-5][0-9]$/;
      if (!timePattern.test(control.value)) {
        return { pattern: true };
      }

      const [openTime, closeTime] = control.value.split(',');

      // Check if close time is after open time
      if (openTime >= closeTime) {
        return { invalidTimeRange: true };
      }

      return null;
    };
  }

  static operatingHoursGroupValidator(): ValidatorFn {
    return (formGroup: AbstractControl): ValidationErrors | null => {
      if (!(formGroup instanceof AbstractControl)) {
        return null;
      }

      const controls = (formGroup as any).controls;
      if (!controls) {
        return null;
      }

      // Check if any day has a value set
      const days = [
        'monday',
        'tuesday',
        'wednesday',
        'thursday',
        'friday',
        'saturday',
        'sunday',
      ];
      let hasAnyValue = false;

      for (const day of days) {
        const control = controls[day];
        if (control && control.value && control.value !== '') {
          hasAnyValue = true;
          break;
        }
      }

      // If no day has a value, all are valid (optional)
      if (!hasAnyValue) {
        return null;
      }

      // If any day has a value, all days must have valid values
      let hasInvalidDay = false;
      for (const day of days) {
        const control = controls[day];
        if (control && control.value === '') {
          hasInvalidDay = true;
          control.setErrors({ required: true });
        }
      }

      return hasInvalidDay ? { invalidOperatingHours: true } : null;
    };
  }
}
