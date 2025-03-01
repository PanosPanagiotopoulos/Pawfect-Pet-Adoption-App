import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  ElementRef,
  ViewChild,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { PhoneInputComponent } from 'src/app/common/ui/phone-input.component';

@Component({
  selector: 'app-personal-info',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormInputComponent,
    PhoneInputComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [formGroup]="form" class="space-y-6" #formContainer>
      <h2 class="text-2xl font-bold text-white mb-6">Προσωπικές Πληροφορίες</h2>

      <app-form-input
        [form]="form"
        controlName="fullName"
        type="text"
        placeholder="Ονοματεπώνυμο"
      >
      </app-form-input>

      <app-form-input
        [form]="form"
        controlName="email"
        type="email"
        placeholder="Διεύθυνση Email"
      >
      </app-form-input>

      <app-phone-input
        [form]="form"
        countryCodeControl="countryCode"
        phoneNumberControl="phoneNumber"
        (phoneChange)="onPhoneChange($event)"
      >
      </app-phone-input>

      <!-- Location Information -->
      <div class="space-y-6">
        <h3 class="text-lg font-medium text-white mb-4">
          Πληροφορίες Τοποθεσίας
        </h3>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <app-form-input
            [form]="getLocationForm()"
            controlName="city"
            type="text"
            placeholder="Πόλη"
          >
          </app-form-input>

          <app-form-input
            [form]="getLocationForm()"
            controlName="zipCode"
            type="text"
            placeholder="Ταχυδρομικός Κώδικας"
          >
          </app-form-input>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <app-form-input
            [form]="getLocationForm()"
            controlName="address"
            type="text"
            placeholder="Διεύθυνση"
          >
          </app-form-input>

          <app-form-input
            [form]="getLocationForm()"
            controlName="number"
            type="text"
            placeholder="Αριθμός"
          >
          </app-form-input>
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

      <div class="flex justify-end pt-6">
        <button
          type="button"
          (click)="onNext()"
          class="px-6 py-2 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg
                 hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 
                 transform hover:-translate-y-1"
        >
          Επόμενο
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
export class PersonalInfoComponent {
  @Input() form!: FormGroup;
  @Output() next = new EventEmitter<void>();
  @ViewChild('formContainer') formContainer!: ElementRef;

  validationErrors: {
    field: string;
    message: string;
    element?: HTMLElement;
  }[] = [];
  showErrorSummary = false;

  getLocationForm(): FormGroup {
    return this.form.get('location') as FormGroup;
  }

  onNext(): void {
    // Reset error summary
    this.validationErrors = [];
    this.showErrorSummary = false;

    // Mark all fields as touched to trigger validation messages
    this.markFormGroupTouched(this.form);

    // Collect validation errors
    this.collectValidationErrors();

    // Show error summary if there are errors
    this.showErrorSummary = this.validationErrors.length > 0;

    if (this.form.valid) {
      this.next.emit();
    } else {
      // Find and scroll to the first invalid field
      this.scrollToFirstInvalidField();
    }
  }

  onPhoneChange(phone: string): void {
    // Update the phone field in the form
    this.form.get('phone')?.setValue(phone);
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
  }

  // Collect validation errors for the error summary
  private collectValidationErrors(): void {
    // Check fullName
    const fullNameControl = this.form.get('fullName');
    if (fullNameControl?.invalid) {
      const element = this.findElementForControl('fullName');
      if (fullNameControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'fullName',
          message: 'Το ονοματεπώνυμο είναι υποχρεωτικό',
          element,
        });
      } else if (fullNameControl.errors?.['minlength']) {
        this.validationErrors.push({
          field: 'fullName',
          message: 'Το ονοματεπώνυμο πρέπει να έχει τουλάχιστον 5 χαρακτήρες',
          element,
        });
      }
    }

    // Check email
    const emailControl = this.form.get('email');
    if (emailControl?.invalid) {
      const element = this.findElementForControl('email');
      if (emailControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'email',
          message: 'Η διεύθυνση email είναι υποχρεωτική',
          element,
        });
      } else if (emailControl.errors?.['email']) {
        this.validationErrors.push({
          field: 'email',
          message: 'Παρακαλώ εισάγετε μια έγκυρη διεύθυνση email',
          element,
        });
      }
    }

    // Check phone number
    const phoneNumberControl = this.form.get('phoneNumber');
    if (phoneNumberControl?.invalid) {
      const element = this.findElementForControl('phoneNumber');
      if (phoneNumberControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'phoneNumber',
          message: 'Ο αριθμός τηλεφώνου είναι υποχρεωτικός',
          element,
        });
      } else if (phoneNumberControl.errors?.['pattern']) {
        this.validationErrors.push({
          field: 'phoneNumber',
          message: 'Παρακαλώ εισάγετε έναν έγκυρο αριθμό τηλεφώνου',
          element,
        });
      }
    }

    // Check location fields
    const locationForm = this.getLocationForm();

    // Check city
    const cityControl = locationForm.get('city');
    if (cityControl?.invalid) {
      const element = this.findElementForControl('city', 'location');
      if (cityControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'city',
          message: 'Η πόλη είναι υποχρεωτική',
          element,
        });
      } else if (cityControl.errors?.['minlength']) {
        this.validationErrors.push({
          field: 'city',
          message: 'Η πόλη πρέπει να έχει τουλάχιστον 2 χαρακτήρες',
          element,
        });
      }
    }

    // Check zipCode
    const zipCodeControl = locationForm.get('zipCode');
    if (zipCodeControl?.invalid) {
      const element = this.findElementForControl('zipCode', 'location');
      if (zipCodeControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'zipCode',
          message: 'Ο ταχυδρομικός κώδικας είναι υποχρεωτικός',
          element,
        });
      } else if (zipCodeControl.errors?.['pattern']) {
        this.validationErrors.push({
          field: 'zipCode',
          message: 'Παρακαλώ εισάγετε έναν έγκυρο ταχυδρομικό κώδικα',
          element,
        });
      }
    }

    // Check address
    const addressControl = locationForm.get('address');
    if (addressControl?.invalid) {
      const element = this.findElementForControl('address', 'location');
      if (addressControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'address',
          message: 'Η διεύθυνση είναι υποχρεωτική',
          element,
        });
      } else if (addressControl.errors?.['minlength']) {
        this.validationErrors.push({
          field: 'address',
          message: 'Η διεύθυνση πρέπει να έχει τουλάχιστον 3 χαρακτήρες',
          element,
        });
      }
    }

    // Check number
    const numberControl = locationForm.get('number');
    if (numberControl?.invalid) {
      const element = this.findElementForControl('number', 'location');
      if (numberControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'number',
          message: 'Ο αριθμός είναι υποχρεωτικός',
          element,
        });
      } else if (numberControl.errors?.['pattern']) {
        this.validationErrors.push({
          field: 'number',
          message: 'Ο αριθμός πρέπει να περιέχει μόνο ψηφία',
          element,
        });
      }
    }
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
    let element = this.formContainer.nativeElement.querySelector(
      selector
    ) as HTMLElement;

    // If not found, try to find by ID
    if (!element) {
      element = this.formContainer.nativeElement.querySelector(
        `#${controlName}`
      ) as HTMLElement;
    }

    return element;
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

  // Scroll to the first invalid field
  private scrollToFirstInvalidField(): void {
    setTimeout(() => {
      try {
        if (this.validationErrors.length > 0) {
          // First scroll to error summary
          const errorSummary =
            this.formContainer.nativeElement.querySelector('.bg-red-500\\/10');
          if (errorSummary) {
            errorSummary.scrollIntoView({ behavior: 'smooth', block: 'start' });
          } else {
            // If no error summary, scroll to the first invalid field
            const firstError = this.validationErrors[0];
            if (firstError.element) {
              this.scrollToErrorField(firstError);
            }
          }
        }
      } catch (error) {
        console.error('Error scrolling to invalid field:', error);
        // Fallback: scroll to the top of the form
        this.formContainer.nativeElement.scrollIntoView({
          behavior: 'smooth',
          block: 'start',
        });
      }
    }, 100);
  }
}
