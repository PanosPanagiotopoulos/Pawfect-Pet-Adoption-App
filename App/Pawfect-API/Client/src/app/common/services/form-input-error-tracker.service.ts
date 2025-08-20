import { Injectable } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';
import { BehaviorSubject, Observable } from 'rxjs';
import { TranslationService } from './translation.service';

export interface ValidationErrorInfo {
  controlName: string;
  controlPath: string[];
  errorType: string;
  errorMessage: string;
  element?: HTMLElement;
}

@Injectable({
  providedIn: 'root'
})
export class FormInputErrorTrackerService {
  private errorsSubject = new BehaviorSubject<ValidationErrorInfo[]>([]);
  public errors$: Observable<ValidationErrorInfo[]> = this.errorsSubject.asObservable();

  constructor(private translate: TranslationService) {}

  /**
   * Track validation errors in a form group
   * @param formGroup The form group to track
   * @param formContainer The HTML element containing the form
   */
  trackFormErrors(formGroup: FormGroup, formContainer: HTMLElement): ValidationErrorInfo[] {
    const errors: ValidationErrorInfo[] = [];
    this.collectFormErrors(formGroup, [], errors);
    
    // Try to find elements for each error
    errors.forEach(error => {
      error.element = this.findElementForControl(error.controlName, formContainer);
    });
    
    this.errorsSubject.next(errors);
    return errors;
  }

  /**
   * Recursively collect validation errors from a form group
   */
  private collectFormErrors(
    formGroup: FormGroup, 
    parentPath: string[], 
    errors: ValidationErrorInfo[]
  ): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      const currentPath = [...parentPath, key];
      
      if (control instanceof FormGroup) {
        // Recursively check nested form groups
        this.collectFormErrors(control, currentPath, errors);
      } else if (control && control.invalid && (control.touched || control.dirty)) {
        // Process errors for this control
        Object.keys(control.errors || {}).forEach(errorType => {
          errors.push({
            controlName: key,
            controlPath: currentPath,
            errorType,
            errorMessage: this.getErrorMessage(key, errorType, control.errors?.[errorType])
          });
        });
      }
    });
  }

  /**
   * Find the HTML element for a control
   */
  private findElementForControl(controlName: string, formContainer: HTMLElement): HTMLElement | undefined {
    // Try different selectors to find the element
    let element = formContainer.querySelector(`#${controlName}`) as HTMLElement;
    
    if (!element) {
      element = formContainer.querySelector(`[formcontrolname="${controlName}"]`) as HTMLElement;
    }
    
    if (!element) {
      element = formContainer.querySelector(`[name="${controlName}"]`) as HTMLElement;
    }
    
    return element;
  }

  /**
   * Scroll to the first error element
   */
  scrollToFirstError(errors: ValidationErrorInfo[]): void {
    if (errors.length === 0) return;
    
    // Find the first error with an element
    const errorWithElement = errors.find(error => error.element);
    
    if (errorWithElement?.element) {
      // Scroll to the element
      errorWithElement.element.scrollIntoView({ 
        behavior: 'smooth', 
        block: 'center' 
      });
      
      // Focus the element if it's an input
      if (errorWithElement.element instanceof HTMLInputElement || 
          errorWithElement.element instanceof HTMLTextAreaElement || 
          errorWithElement.element instanceof HTMLSelectElement) {
        errorWithElement.element.focus();
      }
    }
  }

  /**
   * Get a user-friendly error message for a validation error
   */
  private getErrorMessage(controlName: string, errorType: string, errorValue: any): string {
    const fieldName = this.formatFieldName(controlName);
    
    switch (errorType) {
      case 'required':
        return this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.REQUIRED_FIELD').replace('{field}', fieldName);
      case 'minlength':
        return this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.MIN_LENGTH')
          .replace('{field}', fieldName)
          .replace('{length}', errorValue.requiredLength);
      case 'maxlength':
        return this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.MAX_LENGTH')
          .replace('{field}', fieldName)
          .replace('{length}', errorValue.requiredLength);
      case 'email':
        return this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.EMAIL_INVALID');
      case 'pattern':
        return this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.PATTERN_INVALID').replace('{field}', fieldName);
      case 'invalidTimeRange':
        return this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.INVALID_TIME_RANGE');
      case 'invalidSocialMedia':
        return this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.INVALID_SOCIAL_MEDIA').replace('{field}', fieldName);
      default:
        return this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_ERROR').replace('{field}', fieldName);
    }
  }

  /**
   * Format a control name to be more user-friendly
   */
  private formatFieldName(controlName: string): string {
    // Convert camelCase to space-separated words
    const formatted = controlName
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, str => str.toUpperCase());
    
    // Special cases - use translation keys
    const specialCases: {[key: string]: string} = {
      'shelterName': this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.SHELTER_NAME'),
      'description': this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.DESCRIPTION'),
      'website': this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.WEBSITE'),
      'facebook': this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.FACEBOOK'),
      'instagram': this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.INSTAGRAM'),
      'monday': this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.MONDAY'),
      'tuesday': this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.TUESDAY'),
      'wednesday': this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.WEDNESDAY'),
      'thursday': this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.THURSDAY'),
      'friday': this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.FRIDAY'),
      'saturday': this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.SATURDAY'),
      'sunday': this.translate.translate('APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.SUNDAY')
    };
    
    return specialCases[controlName] || formatted;
  }

  /**
   * Clear tracked errors
   */
  clearErrors(): void {
    this.errorsSubject.next([]);
  }
}