import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  ElementRef,
  ViewChild,
  ChangeDetectorRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { PhoneInputComponent } from 'src/app/common/ui/phone-input.component';
import { FileDropAreaComponent } from 'src/app/common/ui/file-drop-area.component';
import { NgIconsModule } from '@ng-icons/core';
import { GoogleSignupLoadingComponent } from './google-signup-loading.component';
import { ErrorMessageBannerComponent } from 'src/app/common/ui/error-message-banner.component';
import { ErrorDetails } from 'src/app/common/ui/error-message-banner.component';
import { FileItem } from 'src/app/models/file/file.model';

interface ValidationError {
  field: string;
  message: string;
  element?: HTMLElement;
}

@Component({
  selector: 'app-personal-info',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormInputComponent,
    PhoneInputComponent,
    FileDropAreaComponent,
    NgIconsModule,
    GoogleSignupLoadingComponent,
    ErrorMessageBannerComponent,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div
      [formGroup]="form"
      class="space-y-6 w-full max-w-2xl mx-auto px-4 sm:px-6"
      #formContainer
    >
      <app-google-signup-loading
        [isLoading]="isExternalProviderLoading"
      ></app-google-signup-loading>

      <!-- Google Data Banner -->
      <div
        *ngIf="hasGooglePopulatedFields()"
        class="mb-6 p-4 rounded-lg bg-primary-900/30 border border-primary-500/30 animate-fadeIn"
      >
        <div
          class="flex flex-col sm:flex-row items-start sm:items-center gap-4"
        >
          <div class="flex-shrink-0">
            <div
              class="h-8 w-8 rounded-full bg-primary-600 flex items-center justify-center"
            >
              <span class="text-white text-sm font-bold">G</span>
            </div>
          </div>
          <div>
            <h3 class="text-primary-400 font-medium">
              Έχουμε συμπληρώσει κάποια στοιχεία από το Google
            </h3>
            <p class="text-gray-400 text-sm mt-1">
              Τα πεδία που έχουν συμπληρωθεί αυτόματα είναι επισημασμένα και δεν
              μπορούν να αλλαχθούν.
            </p>
          </div>
        </div>
      </div>

      <h2 class="text-2xl sm:text-3xl font-bold text-white mb-6">
        Προσωπικές Πληροφορίες
      </h2>

      <!-- Form Fields -->
      <div class="space-y-6">
        <!-- Each form field is wrapped in a relative container -->
        <div class="form-field-container">
          <app-form-input
            [form]="form"
            controlName="fullName"
            type="text"
            placeholder="Ονοματεπώνυμο"
            [readonly]="form.get('fullName')?.disabled"
          ></app-form-input>
          <div *ngIf="form.get('fullName')?.disabled" class="google-hint">
            <div
              class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1"
            >
              <span class="text-primary-600 text-xs font-bold">G</span>
            </div>
            <span>Από Google</span>
          </div>
        </div>

        <div class="form-field-container">
          <app-form-input
            [form]="form"
            controlName="email"
            type="email"
            placeholder="Email"
            [readonly]="form.get('email')?.disabled"
          ></app-form-input>
          <div *ngIf="form.get('email')?.disabled" class="google-hint">
            <div
              class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1"
            >
              <span class="text-primary-600 text-xs font-bold">G</span>
            </div>
            <span>Από Google</span>
          </div>
        </div>

        <div class="form-field-container">
          <app-phone-input
            [form]="form"
            countryCodeControl="countryCode"
            phoneNumberControl="phoneNumber"
            [readonly]="
              form.get('phoneNumber')?.disabled ||
              form.get('countryCode')?.disabled
            "
            (phoneChange)="onPhoneChange($event)"
          ></app-phone-input>
          <div
            *ngIf="
              form.get('phoneNumber')?.disabled ||
              form.get('countryCode')?.disabled
            "
            class="google-hint"
          >
            <div
              class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1"
            >
              <span class="text-primary-600 text-xs font-bold">G</span>
            </div>
            <span>Από Google</span>
          </div>
        </div>

        <!-- Location Information -->
        <div class="space-y-6">
          <h3 class="text-lg font-medium text-white">Πληροφορίες Τοποθεσίας</h3>

          <div
            class="grid grid-cols-1 sm:grid-cols-2 gap-x-[1px] gap-y-2 sm:gap-x-2"
          >
            <div class="form-field-container">
              <app-form-input
                [form]="getLocationForm()"
                controlName="city"
                type="text"
                placeholder="Πόλη"
                [readonly]="getLocationForm().get('city')?.disabled"
              ></app-form-input>
              <div
                *ngIf="getLocationForm().get('city')?.disabled"
                class="google-hint"
              >
                <div
                  class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1"
                >
                  <span class="text-primary-600 text-xs font-bold">G</span>
                </div>
                <span>Από Google</span>
              </div>
            </div>

            <div class="form-field-container">
              <app-form-input
                [form]="getLocationForm()"
                controlName="zipCode"
                type="text"
                placeholder="Ταχυδρομικός Κώδικας"
                [readonly]="getLocationForm().get('zipCode')?.disabled"
              ></app-form-input>
              <div
                *ngIf="getLocationForm().get('zipCode')?.disabled"
                class="google-hint"
              >
                <div
                  class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1"
                >
                  <span class="text-primary-600 text-xs font-bold">G</span>
                </div>
                <span>Από Google</span>
              </div>
            </div>
          </div>

          <div
            class="grid grid-cols-1 sm:grid-cols-2 gap-x-[1px] gap-y-2 sm:gap-x-6"
          >
            <div class="form-field-container">
              <app-form-input
                [form]="getLocationForm()"
                controlName="address"
                type="text"
                placeholder="Διεύθυνση"
                [readonly]="getLocationForm().get('address')?.disabled"
              ></app-form-input>
              <div
                *ngIf="getLocationForm().get('address')?.disabled"
                class="google-hint"
              >
                <div
                  class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1"
                >
                  <span class="text-primary-600 text-xs font-bold">G</span>
                </div>
                <span>Από Google</span>
              </div>
            </div>

            <div class="form-field-container">
              <app-form-input
                [form]="getLocationForm()"
                controlName="number"
                type="text"
                placeholder="Αριθμός"
                [readonly]="getLocationForm().get('number')?.disabled"
              ></app-form-input>
              <div
                *ngIf="getLocationForm().get('number')?.disabled"
                class="google-hint"
              >
                <div
                  class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1"
                >
                  <span class="text-primary-600 text-xs font-bold">G</span>
                </div>
                <span>Από Google</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Profile Picture Section -->
        <div class="space-y-6">
          <h3 class="text-lg font-medium text-white">Φωτογραφία Προφίλ</h3>
          <app-file-drop-area
            [form]="form"
            controlName="profilePhoto"
            label="Επιλογή Φωτογραφίας"
            hint="Ανεβάστε μια φωτογραφία προφίλ (μέγιστο μέγεθος: 2MB)"
            accept=".jpg,.jpeg,.png"
            [multiple]="false"
            [maxFileSize]="2 * 1024 * 1024"
            (filesChange)="onProfilePhotoChange($event)"
          ></app-file-drop-area>

          <!-- Photo Preview -->
          <div
            *ngIf="profilePhotoPreview || isPhotoLoading"
            class="flex justify-center"
          >
            <div
              class="relative group w-24 h-24 sm:w-32 sm:h-32 rounded-full overflow-hidden border-2 border-primary-500/50"
            >
              <div
                *ngIf="isPhotoLoading"
                class="absolute inset-0 flex items-center justify-center bg-gray-800/70 z-10"
              >
                <div
                  class="w-8 h-8 sm:w-10 sm:h-10 border-4 border-primary-400 border-t-transparent rounded-full animate-spin"
                ></div>
              </div>
              <img
                *ngIf="profilePhotoPreview"
                [src]="profilePhotoPreview"
                alt="Profile preview"
                class="w-full h-full object-cover transition-transform duration-300 group-hover:scale-110"
              />
              <button
                *ngIf="profilePhotoPreview"
                type="button"
                (click)="removeProfilePhoto()"
                class="absolute -top-2 -right-2 bg-red-500 text-white rounded-full p-1 opacity-0 group-hover:opacity-100 transition-opacity"
                aria-label="Remove profile photo"
              >
                <ng-icon name="lucideX" [size]="'16'"></ng-icon>
              </button>
            </div>
          </div>

          <!-- Photo Status Messages -->
          <div class="text-center">
            <div
              *ngIf="photoUploadSuccess"
              class="text-sm text-green-400 animate-fadeIn"
            >
              Η φωτογραφία φορτώθηκε επιτυχώς
            </div>
            <div
              *ngIf="photoUploadError"
              class="text-sm text-red-400 animate-fadeIn"
            >
              {{ photoUploadError }}
            </div>
          </div>
        </div>
      </div>

      <!-- Navigation -->
      <div class="flex justify-end pt-6">
        <button
          type="button"
          (click)="onNext()"
          class="w-full sm:w-auto px-6 py-3 sm:py-2 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg
                 hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 
                 transform hover:-translate-y-1"
        >
          Επόμενο
        </button>
      </div>
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

      <!-- Error Banner -->
      <app-error-message-banner [error]="error"></app-error-message-banner>
    </div>
  `,
  styles: [
    `
      .form-field-container {
        @apply relative mb-6;
      }

      .google-hint {
        @apply absolute top-full left-0 flex items-center text-xs text-white mt-1 px-2 py-1 rounded-md bg-primary-900/10 border border-primary-500/20 animate-slideIn;
      }

      @keyframes slideIn {
        from {
          @apply opacity-0 -translate-y-1;
        }
        to {
          @apply opacity-100 translate-y-0;
        }
      }

      .animate-fadeIn {
        @apply animate-[fadeIn_0.3s_ease-out];
      }

      @keyframes fadeIn {
        from {
          @apply opacity-0;
        }
        to {
          @apply opacity-100;
        }
      }
    `,
  ],
})
export class PersonalInfoComponent {
  @Input() form!: FormGroup;
  @Input() isLoading = false;
  @Input() isExternalProviderLoading = false;
  @Output() next = new EventEmitter<void>();
  @ViewChild('formContainer') formContainer!: ElementRef;

  profilePhotoPreview: string | null = null;
  isPhotoLoading = false;
  photoUploadSuccess = false;
  photoUploadError: string | null = null;
  error?: ErrorDetails;
  validationErrors: ValidationError[] = [];
  showErrorSummary = false;

  constructor(private cdr: ChangeDetectorRef) {}

  getLocationForm(): FormGroup {
    return this.form.get('location') as FormGroup;
  }

  onNext(): void {
    this.validationErrors = [];
    this.showErrorSummary = false;
    this.error = undefined;

    if (this.form.valid) {
      this.next.emit();
    } else {
      this.markFormGroupTouched(this.form);
      this.collectValidationErrors();
      this.showErrorSummary = this.validationErrors.length > 0;
      this.scrollToFirstError();
    }
  }

  onPhoneChange(phone: string): void {
    this.form.get('phone')?.setValue(phone);
  }

  onProfilePhotoChange(files: FileItem[]): void {
    this.photoUploadSuccess = false;
    this.photoUploadError = null;
    if (files.length > 0) {
      const profilePhoto: FileItem = files[0];

      if (!profilePhoto.file.type.match('image.*')) {
        this.photoUploadError =
          'Μη έγκυρος τύπος αρχείου. Επιτρέπονται μόνο εικόνες.';
        this.profilePhotoPreview = null;
        this.form.get('profilePhoto')?.setErrors({ invalidType: true });
        this.cdr.markForCheck();
        return;
      }

      if (profilePhoto.file.size > 2 * 1024 * 1024) {
        this.photoUploadError =
          'Το μέγεθος της εικόνας δεν πρέπει να υπερβαίνει τα 2MB.';
        this.profilePhotoPreview = null;
        this.form.get('profilePhoto')?.setErrors({ invalidSize: true });
        this.cdr.markForCheck();
        return;
      }

      this.isPhotoLoading = true;
      this.cdr.markForCheck();

      this.createImagePreview(profilePhoto.file);

      this.form.get('profilePhoto')?.setValue(profilePhoto.persistedId);
      this.form.get('profilePhoto')?.updateValueAndValidity();
    } else {
      this.profilePhotoPreview = null;
      this.cdr.markForCheck();
    }
  }

  hasGooglePopulatedFields(): boolean {
    const disabledControls = [
      this.form.get('fullName')?.disabled,
      this.form.get('email')?.disabled,
      this.form.get('phoneNumber')?.disabled,
      this.form.get('countryCode')?.disabled,
      this.getLocationForm()?.get('city')?.disabled,
      this.getLocationForm()?.get('zipCode')?.disabled,
      this.getLocationForm()?.get('address')?.disabled,
      this.getLocationForm()?.get('number')?.disabled,
    ];

    return disabledControls.some((isDisabled) => isDisabled === true);
  }

  removeProfilePhoto(): void {
    this.form.get('profilePhoto')?.setValue(null);
    this.profilePhotoPreview = null;
    this.photoUploadSuccess = false;
    this.photoUploadError = null;

    const fileInput =
      this.formContainer.nativeElement.querySelector('input[type="file"]');
    if (fileInput) {
      fileInput.value = '';
    }

    this.cdr.markForCheck();
  }

  private createImagePreview(file: File): void {
    if (!file.type.match('image.*')) {
      this.isPhotoLoading = false;
      return;
    }

    const reader = new FileReader();

    reader.onload = (e: any) => {
      setTimeout(() => {
        this.isPhotoLoading = false;
        this.profilePhotoPreview = e.target.result;
        this.photoUploadSuccess = true;
        this.cdr.markForCheck();
      }, 800);
    };

    reader.onerror = () => {
      this.isPhotoLoading = false;
      this.photoUploadError = 'Σφάλμα κατά τη φόρτωση της εικόνας.';
      this.cdr.markForCheck();
    };

    reader.readAsDataURL(file);
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
  }

  private collectValidationErrors(): void {
    // Personal Info Validation
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

    const emailControl = this.form.get('email');
    if (emailControl?.invalid) {
      const element = this.findElementForControl('email');
      if (emailControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'email',
          message: 'Το email είναι υποχρεωτικό',
          element,
        });
      } else if (emailControl.errors?.['email']) {
        this.validationErrors.push({
          field: 'email',
          message: 'Παρακαλώ εισάγετε ένα έγκυρο email',
          element,
        });
      }
    }

    // Phone Validation
    const phoneControl = this.form.get('phoneNumber');
    if (phoneControl?.invalid) {
      const element = this.findElementForControl('phoneNumber');
      if (phoneControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'phoneNumber',
          message: 'Ο αριθμός τηλεφώνου είναι υποχρεωτικός',
          element,
        });
      } else if (phoneControl.errors?.['pattern']) {
        this.validationErrors.push({
          field: 'phoneNumber',
          message: 'Παρακαλώ εισάγετε έναν έγκυρο αριθμό τηλεφώνου',
          element,
        });
      }
    }

    // Location Validation
    const locationForm = this.getLocationForm();
    const cityControl = locationForm.get('city');
    if (cityControl?.invalid) {
      const element = this.findElementForControl('city', 'location');
      if (cityControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'city',
          message: 'Η πόλη είναι υποχρεωτική',
          element,
        });
      }
    }

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

    const addressControl = locationForm.get('address');
    if (addressControl?.invalid) {
      const element = this.findElementForControl('address', 'location');
      if (addressControl.errors?.['required']) {
        this.validationErrors.push({
          field: 'address',
          message: 'Η διεύθυνση είναι υποχρεωτική',
          element,
        });
      }
    }

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
          message: 'Παρακαλώ εισάγετε έναν έγκυρο αριθμό',
          element,
        });
      }
    }
  }

  private findElementForControl(
    controlName: string,
    groupName?: string
  ): HTMLElement | undefined {
    let selector = groupName
      ? `[formGroupName="${groupName}"] [formControlName="${controlName}"]`
      : `[formControlName="${controlName}"]`;

    let element = this.formContainer?.nativeElement.querySelector(
      selector
    ) as HTMLElement;

    if (!element) {
      element = this.formContainer?.nativeElement.querySelector(
        `#${controlName}`
      ) as HTMLElement;
    }

    return element;
  }

  scrollToErrorField(error: ValidationError): void {
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
    }
  }

  private highlightElement(element: HTMLElement): void {
    element.classList.add('highlight-error');
    setTimeout(() => {
      element.classList.remove('highlight-error');
    }, 1500);
  }

  private scrollToFirstError(): void {
    if (this.validationErrors.length > 0) {
      const errorSummary =
        this.formContainer?.nativeElement.querySelector('.bg-red-500\\/10');
      if (errorSummary) {
        errorSummary.scrollIntoView({ behavior: 'smooth', block: 'start' });
      } else {
        const firstError = this.validationErrors[0];
        if (firstError.element) {
          this.scrollToErrorField(firstError);
        }
      }
    }
  }
}
