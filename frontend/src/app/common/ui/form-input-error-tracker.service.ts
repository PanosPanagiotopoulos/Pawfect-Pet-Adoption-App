import { Injectable } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';
import { BehaviorSubject, Observable } from 'rxjs';

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

  constructor() {}

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
        return `Το πεδίο "${fieldName}" είναι υποχρεωτικό`;
      case 'minlength':
        return `Το πεδίο "${fieldName}" πρέπει να έχει τουλάχιστον ${errorValue.requiredLength} χαρακτήρες`;
      case 'maxlength':
        return `Το πεδίο "${fieldName}" δεν μπορεί να υπερβαίνει τους ${errorValue.requiredLength} χαρακτήρες`;
      case 'email':
        return `Παρακαλώ εισάγετε ένα έγκυρο email`;
      case 'pattern':
        return `Το πεδίο "${fieldName}" δεν έχει έγκυρη μορφή`;
      case 'invalidTimeRange':
        return `Η ώρα κλεισίματος πρέπει να είναι μετά την ώρα ανοίγματος`;
      case 'invalidSocialMedia':
        return `Η διεύθυνση ${fieldName} δεν είναι έγκυρη`;
      default:
        return `Το πεδίο "${fieldName}" περιέχει σφάλμα`;
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
    
    // Special cases
    const specialCases: {[key: string]: string} = {
      'shelterName': 'Όνομα Καταφυγίου',
      'description': 'Περιγραφή',
      'website': 'Ιστοσελίδα',
      'facebook': 'Facebook',
      'instagram': 'Instagram',
      'monday': 'Δευτέρα',
      'tuesday': 'Τρίτη',
      'wednesday': 'Τετάρτη',
      'thursday': 'Πέμπτη',
      'friday': 'Παρασκευή',
      'saturday': 'Σάββατο',
      'sunday': 'Κυριακή'
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