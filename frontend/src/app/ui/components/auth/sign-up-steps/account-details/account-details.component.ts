import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  ViewChild,
  ElementRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormGroup,
  ReactiveFormsModule,
  FormControl,
  Validators,
} from '@angular/forms';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';

@Component({
  selector: 'app-account-details',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormInputComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [formGroup]="form" class="space-y-6" #formContainer>
      <h2 class="text-2xl font-bold text-white mb-6">Στοιχεία Λογαριασμού</h2>

      <app-form-input
        [form]="form"
        controlName="password"
        type="password"
        placeholder="Κωδικός πρόσβασης"
      >
      </app-form-input>

      <app-form-input
        [form]="form"
        controlName="confirmPassword"
        type="password"
        placeholder="Επιβεβαίωση κωδικού"
      >
      </app-form-input>

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
          >
            {{ error.message }}
          </li>
        </ul>
      </div>

      <div class="text-sm text-gray-400 space-y-1 mt-2">
        <p>Ο κωδικός πρέπει να περιέχει:</p>
        <ul class="list-disc list-inside pl-4">
          <li>Τουλάχιστον 8 χαρακτήρες</li>
          <li>Ένα κεφαλαίο γράμμα</li>
          <li>Ένα πεζό γράμμα</li>
          <li>Έναν αριθμό</li>
          <li>Έναν ειδικό χαρακτήρα</li>
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
export class AccountDetailsComponent {
  @Input() form!: FormGroup;
  @Output() next = new EventEmitter<void>();
  @Output() back = new EventEmitter<void>();
  @ViewChild('formContainer') formContainer!: ElementRef;

  validationErrors: {
    field: string;
    message: string;
  }[] = [];
  showErrorSummary = false;

  passwordMismatch(): boolean {
    const password = this.form.get('password')?.value;
    const confirmPassword = this.form.get('confirmPassword')?.value;
    return (
      password !== confirmPassword &&
      !!this.form.get('confirmPassword')?.touched
    );
  }

  onNext(): void {
    // Reset error summary
    this.validationErrors = [];
    this.showErrorSummary = false;

    // Mark all fields as touched to trigger validation messages
    this.markFormGroupTouched(this.form);

    // Check if passwords match
    const password = this.form.get('password')?.value;
    const confirmPassword = this.form.get('confirmPassword')?.value;

    if (password !== confirmPassword) {
      this.form.get('confirmPassword')?.setErrors({ mismatch: true });
      // Mark as touched to show validation errors
      this.form.get('confirmPassword')?.markAsTouched();
    }

    // Collect validation errors
    this.collectValidationErrors();

    // Show error summary if there are errors
    this.showErrorSummary = this.validationErrors.length > 0;

    if (this.form.valid && !this.passwordMismatch()) {
      this.next.emit();
    }
  }

  onBack(): void {
    this.back.emit();
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
    // Check password
    const passwordControl = this.form.get('password');
    if (passwordControl?.invalid) {
      if (passwordControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'password',
          message: 'Ο κωδικός πρόσβασης είναι υποχρεωτικός',
        });
      } else {
        // Check specific password validation errors
        if (passwordControl.errors?.['minlength']) {
          this.validationErrors.push({
            field: 'password',
            message: 'Ο κωδικός πρέπει να έχει τουλάχιστον 8 χαρακτήρες',
          });
        }

        if (passwordControl.errors?.['uppercase']) {
          this.validationErrors.push({
            field: 'password',
            message:
              'Ο κωδικός πρέπει να περιέχει τουλάχιστον ένα κεφαλαίο γράμμα',
          });
        }

        if (passwordControl.errors?.['lowercase']) {
          this.validationErrors.push({
            field: 'password',
            message: 'Ο κωδικός πρέπει να περιέχει τουλάχιστον ένα πεζό γράμμα',
          });
        }

        if (passwordControl.errors?.['number']) {
          this.validationErrors.push({
            field: 'password',
            message: 'Ο κωδικός πρέπει να περιέχει τουλάχιστον έναν αριθμό',
          });
        }

        if (passwordControl.errors?.['specialChar']) {
          this.validationErrors.push({
            field: 'password',
            message:
              'Ο κωδικός πρέπει να περιέχει τουλάχιστ on έναν ειδικό χαρακτήρα',
          });
        }
      }
    }

    // Check confirm password
    const confirmPasswordControl = this.form.get('confirmPassword');
    if (confirmPasswordControl?.invalid || this.passwordMismatch()) {
      if (confirmPasswordControl?.errors?.['required']) {
        this.validationErrors.push({
          field: 'confirmPassword',
          message: 'Η επιβεβαίωση κωδικού είναι υποχρεωτική',
        });
      } else if (
        this.passwordMismatch() ||
        confirmPasswordControl?.errors?.['mismatch']
      ) {
        this.validationErrors.push({
          field: 'confirmPassword',
          message: 'Οι κωδικοί δεν ταιριάζουν',
        });
      }
    }
  }
}