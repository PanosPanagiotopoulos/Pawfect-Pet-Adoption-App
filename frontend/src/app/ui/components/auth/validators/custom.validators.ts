import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';
import { UserRole } from 'src/app/models/user/user.model';

export class CustomValidators {
  static passwordValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }

      const hasUpperCase = /[A-Z]/.test(control.value);
      const hasLowerCase = /[a-z]/.test(control.value);
      const hasNumber = /\d/.test(control.value);
      const hasSpecialChar = /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(control.value);
      const isLongEnough = control.value.length >= 8;

      const errors: ValidationErrors = {};

      if (!isLongEnough) errors['minlength'] = { requiredLength: 8 };
      if (!hasUpperCase) errors['uppercase'] = true;
      if (!hasLowerCase) errors['lowercase'] = true;
      if (!hasNumber) errors['number'] = true;
      if (!hasSpecialChar) errors['specialChar'] = true;

      return Object.keys(errors).length ? errors : null;
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

      const patterns = {
        facebook: /^https?:\/\/(www\.)?facebook\.com\/[a-zA-Z0-9.]+$/,
        instagram: /^https?:\/\/(www\.)?instagram\.com\/[a-zA-Z0-9._]+$/
      };

      const valid = patterns[platform].test(control.value);
      return valid ? null : { invalidSocialMedia: true };
    };
  }

  static operatingHoursValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }

      const [openTime, closeTime] = control.value.split(',');
      if (!openTime || !closeTime) {
        return { pattern: true };
      }

      const timePattern = /^([01]?[0-9]|2[0-3]):[0-5][0-9]$/;
      if (!timePattern.test(openTime) || !timePattern.test(closeTime)) {
        return { pattern: true };
      }

      const [openHour, openMinute] = openTime.split(':').map(Number);
      const [closeHour, closeMinute] = closeTime.split(':').map(Number);

      if (closeHour < openHour || (closeHour === openHour && closeMinute <= openMinute)) {
        return { invalidTimeRange: true };
      }

      return null;
    };
  }

  static roleValidator(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const validRoles = [UserRole.User, UserRole.Shelter, UserRole.Admin];
      return validRoles.includes(control.value) ? null : { invalidRole: true };
    };
  }
}