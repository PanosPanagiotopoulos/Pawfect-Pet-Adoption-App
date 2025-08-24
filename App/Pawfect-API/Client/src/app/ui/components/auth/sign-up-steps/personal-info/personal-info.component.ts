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
  OnDestroy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { PhoneInputComponent } from 'src/app/common/ui/phone-input.component';
import { FileDropAreaComponent } from 'src/app/common/ui/file-drop-area.component';
import { NgIconsModule } from '@ng-icons/core';
import { GoogleSignupLoadingComponent } from './google-signup-loading.component';
import { ErrorMessageBannerComponent } from 'src/app/common/ui/error-message-banner.component';
import { ErrorDetails } from 'src/app/common/ui/error-message-banner.component';
import { FileItem } from 'src/app/models/file/file.model';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { TranslationService } from 'src/app/common/services/translation.service';
import { UserAvailabilityService } from 'src/app/services/user-availability.service';
import { UserVailabilityCheck } from 'src/app/models/user-availability/user-vailability-check.model';
import { UserAvailabilityResult } from 'src/app/models/user-availability/user-availability-result.model';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';
import { AuthProvider } from 'src/app/common/enum/auth-provider.enum';
import { CustomValidators } from '../../validators/custom.validators';

interface ValidationError {
  field: string;
  message: string;
  element?: HTMLElement;
}

interface AvailabilityStatus {
  isChecking: boolean;
  isAvailable?: boolean;
  message?: string;
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
    TranslatePipe,
  ],
  template: `
    <div
      [formGroup]="form"
      class="space-y-6 w-full max-w-2xl mx-auto px-4 sm:px-6"
      #formContainer
    >
      <app-google-signup-loading
        [isLoading]="isExternalProviderLoading"
      ></app-google-signup-loading>

      <!-- Google Data Banner - Only show if still in Google mode -->
      <div
        *ngIf="hasGooglePopulatedFields() && isGoogleAuthenticated()"
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
              {{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.GOOGLE_POPULATED_TITLE' | translate }}
            </h3>
            <p class="text-gray-400 text-sm mt-1">
              {{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.GOOGLE_POPULATED_DESC' | translate }}
            </p>
          </div>
        </div>
      </div>

      <h2 class="text-2xl sm:text-3xl font-bold text-white mb-6">
        {{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.TITLE' | translate }}
      </h2>

      <!-- Form Fields -->
      <div class="space-y-6">
        <!-- Full Name Field -->
        <div class="form-field-container">
          <app-form-input
            [form]="form"
            controlName="fullName"
            type="text"
            [placeholder]="'APP.AUTH.SIGNUP.PERSONAL_INFO.FULL_NAME_PLACEHOLDER' | translate"
            [readonly]="form.get('fullName')?.disabled && isGoogleAuthenticated()"
          ></app-form-input>
          <!-- Only show Google hint if field is ACTUALLY disabled AND we're in Google mode -->
          <div *ngIf="form.get('fullName')?.disabled && isGoogleAuthenticated()" class="google-hint">
            <div class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1">
              <span class="text-primary-600 text-xs font-bold">G</span>
            </div>
            <span>{{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.FROM_GOOGLE' | translate }}</span>
          </div>
        </div>

        <!-- Email Field -->
        <div class="form-field-container">
          <app-form-input
            [form]="form"
            controlName="email"
            type="email"
            [placeholder]="'APP.AUTH.SIGNUP.PERSONAL_INFO.EMAIL_PLACEHOLDER' | translate"
            [readonly]="form.get('email')?.disabled && isGoogleAuthenticated()"
          ></app-form-input>
          
          <!-- Show Google hint ONLY if field is ACTUALLY disabled AND we're still in Google mode -->
          <div *ngIf="form.get('email')?.disabled && isGoogleAuthenticated()" class="google-hint">
            <div class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1">
              <span class="text-primary-600 text-xs font-bold">G</span>
            </div>
            <span>{{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.FROM_GOOGLE' | translate }}</span>
          </div>
          
          <!-- Show availability status ONLY if field is NOT disabled OR we're not in Google mode -->
          <div *ngIf="!form.get('email')?.disabled || !isGoogleAuthenticated()">
            <div *ngIf="emailAvailability.isChecking" class="availability-status checking">
              <div class="availability-spinner"></div>
              <span>{{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.CHECKING_EMAIL' | translate }}</span>
            </div>
            <div *ngIf="!emailAvailability.isChecking && emailAvailability.isAvailable === true" class="availability-status available">
              <ng-icon name="lucideCheck" [size]="'16'"></ng-icon>
              <span>{{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.EMAIL_AVAILABLE' | translate }}</span>
            </div>
            <div *ngIf="!emailAvailability.isChecking && emailAvailability.isAvailable === false" class="availability-status unavailable">
              <ng-icon name="lucideX" [size]="'16'"></ng-icon>
              <span>{{ emailAvailability.message || ('APP.AUTH.SIGNUP.PERSONAL_INFO.EMAIL_UNAVAILABLE' | translate) }}</span>
            </div>
          </div>
        </div>

        <!-- Phone Field -->
        <div class="form-field-container">
          <app-phone-input
            [form]="form"
            countryCodeControl="countryCode"
            phoneNumberControl="phoneNumber"
            [phonePlaceholder]="'APP.AUTH.SIGNUP.PERSONAL_INFO.PHONE_PLACEHOLDER' | translate"
            [phoneFieldLabel]="'APP.AUTH.SIGNUP.PERSONAL_INFO.PHONE_PLACEHOLDER' | translate"
            [readonly]="
              (form.get('phoneNumber')?.disabled ||
              form.get('countryCode')?.disabled) && isGoogleAuthenticated()
            "
            (phoneChange)="onPhoneChange($event)"
          ></app-phone-input>
          
          <!-- Show Google hint ONLY if fields are disabled AND we're still in Google mode -->
          <div
            *ngIf="
              (form.get('phoneNumber')?.disabled ||
              form.get('countryCode')?.disabled) && isGoogleAuthenticated()
            "
            class="google-hint"
          >
            <div
              class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1"
            >
              <span class="text-primary-600 text-xs font-bold">G</span>
            </div>
            <span>{{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.FROM_GOOGLE' | translate }}</span>
          </div>
          
          <!-- Show availability status ONLY if fields are NOT disabled OR we're not in Google mode -->
          <div *ngIf="(!form.get('phoneNumber')?.disabled && !form.get('countryCode')?.disabled) || !isGoogleAuthenticated()">
            <div *ngIf="phoneAvailability.isChecking" class="availability-status checking">
              <div class="availability-spinner"></div>
              <span>{{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.CHECKING_PHONE' | translate }}</span>
            </div>
            <div *ngIf="!phoneAvailability.isChecking && phoneAvailability.isAvailable === true" class="availability-status available">
              <ng-icon name="lucideCheck" [size]="'16'"></ng-icon>
              <span>{{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.PHONE_AVAILABLE' | translate }}</span>
            </div>
            <div *ngIf="!phoneAvailability.isChecking && phoneAvailability.isAvailable === false" class="availability-status unavailable">
              <ng-icon name="lucideX" [size]="'16'"></ng-icon>
              <span>{{ phoneAvailability.message || ('APP.AUTH.SIGNUP.PERSONAL_INFO.PHONE_UNAVAILABLE' | translate) }}</span>
            </div>
          </div>
        </div>

        <!-- Location Information -->
        <div class="space-y-6">
          <h3 class="text-lg font-medium text-white">
            {{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.LOCATION_TITLE' | translate }}
          </h3>

          <div
            class="grid grid-cols-1 sm:grid-cols-2 gap-x-[1px] gap-y-2 sm:gap-x-2"
          >
            <div class="form-field-container">
              <app-form-input
                [form]="getLocationForm()"
                controlName="city"
                type="text"
                [placeholder]="'APP.AUTH.SIGNUP.PERSONAL_INFO.CITY_PLACEHOLDER' | translate"
                [readonly]="getLocationForm().get('city')?.disabled && isGoogleAuthenticated()"
              ></app-form-input>
              <div
                *ngIf="getLocationForm().get('city')?.disabled && isGoogleAuthenticated()"
                class="google-hint"
              >
                <div
                  class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1"
                >
                  <span class="text-primary-600 text-xs font-bold">G</span>
                </div>
                <span>{{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.FROM_GOOGLE' | translate }}</span>
              </div>
            </div>

            <div class="form-field-container">
              <app-form-input
                [form]="getLocationForm()"
                controlName="zipCode"
                type="text"
                [placeholder]="'APP.AUTH.SIGNUP.PERSONAL_INFO.ZIP_PLACEHOLDER' | translate"
                [readonly]="getLocationForm().get('zipCode')?.disabled && isGoogleAuthenticated()"
              ></app-form-input>
              <div
                *ngIf="getLocationForm().get('zipCode')?.disabled && isGoogleAuthenticated()"
                class="google-hint"
              >
                <div
                  class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1"
                >
                  <span class="text-primary-600 text-xs font-bold">G</span>
                </div>
                <span>{{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.FROM_GOOGLE' | translate }}</span>
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
                [placeholder]="'APP.AUTH.SIGNUP.PERSONAL_INFO.ADDRESS_PLACEHOLDER' | translate"
                [readonly]="getLocationForm().get('address')?.disabled && isGoogleAuthenticated()"
              ></app-form-input>
              <div
                *ngIf="getLocationForm().get('address')?.disabled && isGoogleAuthenticated()"
                class="google-hint"
              >
                <div
                  class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1"
                >
                  <span class="text-primary-600 text-xs font-bold">G</span>
                </div>
                <span>{{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.FROM_GOOGLE' | translate }}</span>
              </div>
            </div>

            <div class="form-field-container">
              <app-form-input
                [form]="getLocationForm()"
                controlName="number"
                type="text"
                [placeholder]="'APP.AUTH.SIGNUP.PERSONAL_INFO.ADDRESS_NUMBER' | translate"
                [readonly]="getLocationForm().get('number')?.disabled && isGoogleAuthenticated()"
              ></app-form-input>
              <div
                *ngIf="getLocationForm().get('number')?.disabled && isGoogleAuthenticated()"
                class="google-hint"
              >
                <div
                  class="h-4 w-4 rounded-full bg-white flex items-center justify-center mr-1"
                >
                  <span class="text-primary-600 text-xs font-bold">G</span>
                </div>
                <span>{{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.FROM_GOOGLE' | translate }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Profile Picture Section -->
        <div class="space-y-6">
          <h3 class="text-lg font-medium text-white">
            {{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.PROFILE_PHOTO_TITLE' | translate }}
          </h3>
          <app-file-drop-area
            [form]="form"
            controlName="profilePhoto"
            [label]="'APP.AUTH.SIGNUP.PERSONAL_INFO.PHOTO_LABEL' | translate"
            [hint]="'APP.AUTH.SIGNUP.PERSONAL_INFO.PHOTO_HINT' | translate"
            accept=".jpg,.jpeg,.png"
            [multiple]="false"
            [maxFileSize]="10 * 1024 * 1024"
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
              {{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.PHOTO_SUCCESS' | translate }}
            </div>
            <div
              *ngIf="photoUploadError"
              class="text-sm text-red-400 animate-fadeIn"
            >
              {{ photoUploadError | translate }}
            </div>
          </div>
        </div>
      </div>

      <!-- Navigation -->
      <div class="flex justify-end pt-6">
        <button
          type="button"
          (click)="onNext()"
          [disabled]="!form.valid || hasAvailabilityErrors()"
          class="w-full sm:w-auto px-6 py-3 sm:py-2 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg
                 hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 
                 transform hover:-translate-y-1 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none"
        >
          {{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.NEXT' | translate }}
        </button>
      </div>
      
      <!-- Error Summary -->
      <div
        *ngIf="showErrorSummary"
        class="bg-red-500/10 border border-red-500/30 rounded-lg p-4 my-4 animate-fadeIn"
      >
        <h3 class="text-red-400 font-medium mb-2 flex items-center">
          <span class="mr-2">⚠️</span> {{ 'APP.AUTH.SIGNUP.PERSONAL_INFO.ERROR_SUMMARY_TITLE' | translate }}
        </h3>
        <ul class="list-disc list-inside text-sm text-red-400 space-y-1">
          <li
            *ngFor="let error of validationErrors"
            class="cursor-pointer hover:underline"
            (click)="scrollToErrorField(error)"
          >
            {{ error.message | translate }}
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

      .availability-status {
        @apply absolute top-full left-0 flex items-center text-xs mt-1 px-2 py-1 rounded-md animate-slideIn;
      }

      .availability-status.checking {
        @apply text-blue-400 bg-blue-900/10 border border-blue-500/20;
      }

      .availability-status.available {
        @apply text-green-400 bg-green-900/10 border border-green-500/20;
      }

      .availability-status.unavailable {
        @apply text-red-400 bg-red-900/10 border border-red-500/20;
      }

      .availability-spinner {
        @apply w-4 h-4 border-2 border-blue-400 border-t-transparent rounded-full animate-spin mr-1;
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
export class PersonalInfoComponent implements OnInit, OnDestroy {
  @Input() form!: FormGroup;
  @Input() isLoading = false;
  @Input() isExternalProviderLoading = false;
  @Output() next = new EventEmitter<void>();
  @Output() googleModeDisabled = new EventEmitter<void>();
  @ViewChild('formContainer') formContainer!: ElementRef;

  profilePhotoPreview: string | null = null;
  isPhotoLoading = false;
  photoUploadSuccess = false;
  photoUploadError: string | null = null;
  error?: ErrorDetails;
  validationErrors: ValidationError[] = [];
  showErrorSummary = false;

  // Availability check properties
  emailAvailability: AvailabilityStatus = { isChecking: false };
  phoneAvailability: AvailabilityStatus = { isChecking: false };
  
  private destroy$ = new Subject<void>();
  private emailSubject = new Subject<string>();
  private phoneSubject = new Subject<string>();
  private lastEmailValue: string | null = null;
  private lastPhoneValue: string | null = null;

  constructor(
    private cdr: ChangeDetectorRef,
    private translationService: TranslationService,
    private userAvailabilityService: UserAvailabilityService
  ) {}

  ngOnInit(): void {
    debugger;
    // Initialize last values
    this.lastEmailValue = this.form.get('email')?.value || null;
    this.lastPhoneValue = this.form.get('phoneNumber')?.value || null;
    
    this.setupAvailabilityChecks();
    this.checkGoogleDataAvailability();
    
    // Check initial values for availability
    this.checkInitialValuesAvailability();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  hasUnsavedChanges(): boolean {
    const mainDirty = !!this.form && this.form.dirty;
    const locationForm = this.getLocationForm();
    const locationDirty = !!locationForm && locationForm.dirty;
    const photoDirty = this.profilePhotoPreview !== null;
    return mainDirty || locationDirty || photoDirty;
  }

  hasAvailabilityErrors(): boolean {
    return (
      (this.emailAvailability.isAvailable === false && !this.form.get('email')?.disabled) ||
      (this.phoneAvailability.isAvailable === false && !this.form.get('phoneNumber')?.disabled)
    );
  }

  getLocationForm(): FormGroup {
    return this.form.get('location') as FormGroup;
  }

  onNext(): void {
    this.validationErrors = [];
    this.showErrorSummary = false;

    if (this.form.valid && !this.hasAvailabilityErrors()) {
      this.next.emit();
    } else {
      this.markFormGroupTouched(this.form);
      this.collectValidationErrors();
      this.showErrorSummary = true;
      this.scrollToFirstError();
      this.cdr.markForCheck();
    }
  }

  onPhoneChange(phone: string): void {
    // Trigger phone availability check when phone changes
    if (!this.form.get('phoneNumber')?.disabled) {
      // Only trigger if the value actually changed
      if (phone !== this.lastPhoneValue) {
        this.lastPhoneValue = phone;
        this.phoneAvailability = { isChecking: false }; // Reset status first
        
        if (phone && phone.trim().length > 0) {
          const countryCode = this.form.get('countryCode')?.value || '+30';
          const fullPhone = `${countryCode}${phone.replace(/\s+/g, '')}`; // Remove spaces
          
          // Only check if it's a valid phone format
          if (this.isValidPhoneForAvailabilityCheck(fullPhone)) {
            this.phoneSubject.next(fullPhone);
          }
        } else {
          // Reset availability status if phone is empty
          this.phoneAvailability = { isChecking: false };
          this.cdr.markForCheck();
        }
      }
    }
  }

  onProfilePhotoChange(files: FileItem[]): void {
    if (files.length === 0) {
      this.profilePhotoPreview = null;
      this.photoUploadSuccess = false;
      this.photoUploadError = null;
      return;
    }

    const fileItem = files[0];
    const file = fileItem.file;
    this.isPhotoLoading = true;
    this.photoUploadSuccess = false;
    this.photoUploadError = null;

    // Validate file type
    if (!file.type.startsWith('image/')) {
      this.isPhotoLoading = false;
      this.photoUploadError = this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.INVALID_FILE_TYPE');
      this.cdr.markForCheck();
      return;
    }

    // Validate file size (10MB)
    if (file.size > 10 * 1024 * 1024) {
      this.isPhotoLoading = false;
      this.photoUploadError = this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.INVALID_FILE_SIZE');
      this.cdr.markForCheck();
      return;
    }

    this.createImagePreview(file);
  }

  hasGooglePopulatedFields(): boolean {
    const fullNameDisabled = this.form.get('fullName')?.disabled;
    const emailDisabled = this.form.get('email')?.disabled;
    const phoneDisabled = this.form.get('phoneNumber')?.disabled;
    const countryCodeDisabled = this.form.get('countryCode')?.disabled;
    const locationForm = this.getLocationForm();
    const cityDisabled = locationForm.get('city')?.disabled;
    const zipCodeDisabled = locationForm.get('zipCode')?.disabled;

    return !!(
      fullNameDisabled ||
      emailDisabled ||
      phoneDisabled ||
      countryCodeDisabled ||
      cityDisabled ||
      zipCodeDisabled
    );
  }

  removeProfilePhoto(): void {
    this.profilePhotoPreview = null;
    this.photoUploadSuccess = false;
    this.photoUploadError = null;
    this.form.patchValue({ profilePhoto: null });
    this.cdr.markForCheck();
  }

  isGoogleAuthenticated(): boolean {
    const authProvider = this.form.get('authProvider')?.value;
    const isGoogleProvider = authProvider === AuthProvider.Google;
    
    // Also check if we still have Google-populated fields as a backup
    const hasGoogleFields = this.hasGooglePopulatedFields();
    
    return isGoogleProvider;
  }

  private createImagePreview(file: File): void {
    const reader = new FileReader();

    reader.onload = () => {
      this.profilePhotoPreview = reader.result as string;
      this.isPhotoLoading = false;
      this.photoUploadSuccess = true;
      this.cdr.markForCheck();
    };

    reader.onerror = () => {
      this.isPhotoLoading = false;
      this.photoUploadError = this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.PHOTO_LOAD_ERROR');
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
          message: this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.FULL_NAME_REQUIRED'),
          element,
        });
      } else if (fullNameControl.errors?.['minlength']) {
        this.validationErrors.push({
          field: 'fullName',
          message: this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.FULL_NAME_MINLENGTH'),
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
          message: this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.EMAIL_REQUIRED'),
          element,
        });
      } else if (emailControl.errors?.['email']) {
        this.validationErrors.push({
          field: 'email',
          message: this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.EMAIL_INVALID'),
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
          message: this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.PHONE_REQUIRED'),
          element,
        });
      } else if (phoneControl.errors?.['pattern']) {
        this.validationErrors.push({
          field: 'phoneNumber',
          message: this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.PHONE_INVALID'),
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
          message: this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.CITY_REQUIRED'),
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
          message: this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.ZIP_REQUIRED'),
          element,
        });
      } else if (zipCodeControl.errors?.['pattern']) {
        this.validationErrors.push({
          field: 'zipCode',
          message: this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.ZIP_INVALID'),
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
          message: this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.ADDRESS_REQUIRED'),
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
          message: this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.NUMBER_REQUIRED'),
          element,
        });
      } else if (numberControl.errors?.['pattern']) {
        this.validationErrors.push({
          field: 'number',
          message: this.translationService.translate('APP.AUTH.SIGNUP.PERSONAL_INFO.ERRORS.NUMBER_INVALID'),
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

  private setupAvailabilityChecks(): void {
    // Setup email availability check with debounce
    this.emailSubject
      .pipe(
        debounceTime(800),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe((email) => {
        if (email && this.isValidEmail(email)) {
          this.checkEmailAvailability(email);
        } else {
          // Reset availability status if email is invalid or empty
          this.emailAvailability = { isChecking: false };
          this.cdr.markForCheck();
        }
      });

    // Setup phone availability check with debounce
    this.phoneSubject
      .pipe(
        debounceTime(800),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe((phone) => {
        if (phone && this.isValidPhoneForAvailabilityCheck(phone)) {
          this.checkPhoneAvailability(phone);
        } else {
          // Reset availability status if phone is invalid or empty
          this.phoneAvailability = { isChecking: false };
          this.cdr.markForCheck();
        }
      });

    // Listen to email changes
    this.form.get('email')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((email) => {
        if (!this.form.get('email')?.disabled) {
          if (email !== this.lastEmailValue) {
            this.lastEmailValue = email;
            this.emailAvailability = { isChecking: false };
            if (email && email.trim().length > 0) {
              this.emailSubject.next(email.trim());
            }
          }
        }
        this.cdr.markForCheck();
      });

    // Listen to phone number changes
    this.form.get('phoneNumber')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((phoneNumber) => {
        if (!this.form.get('phoneNumber')?.disabled) {
          if (phoneNumber !== this.lastPhoneValue) {
            this.lastPhoneValue = phoneNumber;
            this.phoneAvailability = { isChecking: false };
            
            if (phoneNumber && phoneNumber.trim().length > 0) {
              const countryCode = this.form.get('countryCode')?.value || '+30';
              const fullPhone = `${countryCode}${phoneNumber.replace(/\s+/g, '')}`;
              
              if (this.isValidPhoneForAvailabilityCheck(fullPhone)) {
                this.phoneSubject.next(fullPhone);
              }
            }
          }
        }
        this.cdr.markForCheck();
      });

    // Listen to country code changes
    this.form.get('countryCode')?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe((countryCode) => {
        const phoneNumber = this.form.get('phoneNumber')?.value;
        if (phoneNumber && countryCode && !this.form.get('phoneNumber')?.disabled) {
          const fullPhone = `${countryCode}${phoneNumber.replace(/\s+/g, '')}`;
          
          if (this.isValidPhoneForAvailabilityCheck(fullPhone)) {
            // Reset last phone value to trigger new check
            this.lastPhoneValue = phoneNumber;
            this.phoneAvailability = { isChecking: false };
            this.phoneSubject.next(fullPhone);
          }
        }
      });
  }

  private checkGoogleDataAvailability(): void {
    if (this.isGoogleAuthenticated() && this.hasGooglePopulatedFields()) {
      const email = this.form.get('email')?.value;
      const phoneNumber = this.form.get('phoneNumber')?.value;
      const countryCode = this.form.get('countryCode')?.value;

      const checkData: UserVailabilityCheck = {};
      
      if (email) {
        checkData.email = email.trim();
      }
      
      if (phoneNumber && countryCode) {
        // Format phone consistently
        checkData.phone = `${countryCode}${phoneNumber.replace(/\s+/g, '')}`;
      }

      if (checkData.email || checkData.phone) {
        // Show loading state while checking Google data
        this.emailAvailability = checkData.email ? { isChecking: true } : { isChecking: false };
        this.phoneAvailability = checkData.phone ? { isChecking: true } : { isChecking: false };
        this.cdr.markForCheck();

        this.userAvailabilityService.checkAvailability(checkData).subscribe({
          next: (result: UserAvailabilityResult) => {
            let shouldDisableGoogleMode = false;

            // Update availability status for email
            if (checkData.email) {
              this.emailAvailability = {
                isChecking: false,
                isAvailable: result.isEmailAvailable,
                message: result.emailMessage
              };
              if (result.isEmailAvailable === false) {
                shouldDisableGoogleMode = true;
              }
            }

            // Update availability status for phone
            if (checkData.phone) {
              this.phoneAvailability = {
                isChecking: false,
                isAvailable: result.isPhoneAvailable,
                message: result.phoneMessage
              };
              if (result.isPhoneAvailable === false) {
                shouldDisableGoogleMode = true;
              }
            }

            if (shouldDisableGoogleMode) {
              this.revertGoogleSignupProcess();
            }

            this.cdr.markForCheck();
          },
          error: (error) => {
            console.error('Error checking Google data availability:', error);
            
            // On error, revert Google signup process to prevent issues
            this.revertGoogleSignupProcess();
            
            this.cdr.markForCheck();
          }
        });
      }
    }
  }

  private revertGoogleSignupProcess(): void {
    // Store current values before clearing
    const currentValues = {
      fullName: this.form.get('fullName')?.value || '',
      email: this.form.get('email')?.value || '',
      phoneNumber: this.form.get('phoneNumber')?.value || '',
      countryCode: this.form.get('countryCode')?.value || '+30',
      city: this.getLocationForm().get('city')?.value || '',
      zipCode: this.getLocationForm().get('zipCode')?.value || '',
      address: this.getLocationForm().get('address')?.value || '',
      number: this.getLocationForm().get('number')?.value || ''
    };

    // Reset auth provider to local FIRST
    this.form.get('authProvider')?.setValue(AuthProvider.Local);
    this.form.get('authProviderId')?.setValue(null);

    // Enable all form controls BEFORE clearing values
    this.form.get('email')?.enable();
    this.form.get('fullName')?.enable();
    this.form.get('phoneNumber')?.enable();
    this.form.get('countryCode')?.enable();

    const locationForm = this.getLocationForm();
    locationForm.get('city')?.enable();
    locationForm.get('zipCode')?.enable();
    locationForm.get('address')?.enable();
    locationForm.get('number')?.enable();

    // Enable password fields in parent form
    this.form.get('password')?.enable();
    this.form.get('confirmPassword')?.enable();

    // Force change detection to update UI immediately
    this.cdr.detectChanges();

    // Clear values to trigger change detection
    this.form.patchValue({
      fullName: '',
      email: '',
      phoneNumber: '',
      countryCode: '+30'
    });

    locationForm.patchValue({
      city: '',
      zipCode: '',
      address: '',
      number: ''
    });

    // Force another change detection cycle
    this.cdr.detectChanges();

    // Use setTimeout to ensure DOM updates are complete
    setTimeout(() => {
      // Now set the actual values
      this.form.patchValue({
        fullName: currentValues.fullName,
        email: currentValues.email,
        phoneNumber: currentValues.phoneNumber,
        countryCode: currentValues.countryCode
      });

      locationForm.patchValue({
        city: currentValues.city,
        zipCode: currentValues.zipCode,
        address: currentValues.address,
        number: currentValues.number
      });

      // Reset availability status completely
      this.emailAvailability = { isChecking: false };
      this.phoneAvailability = { isChecking: false };
      
      // Reset tracking variables to force new checks
      this.lastEmailValue = null;
      this.lastPhoneValue = null;

      // Mark form as dirty since we've made changes
      this.form.markAsDirty();
      locationForm.markAsDirty();

      // Final change detection
      this.cdr.markForCheck();

      // Emit event to parent component
      this.googleModeDisabled.emit();
    }, 150);
  }

  private checkEmailAvailability(email: string): void {
    // Don't check if we're in Google mode and field is disabled
    if (this.isGoogleAuthenticated() && this.form.get('email')?.disabled) {
      return;
    }

    this.emailAvailability = { isChecking: true };
    this.cdr.markForCheck();

    this.userAvailabilityService.checkAvailability({ email }).subscribe({
      next: (result: UserAvailabilityResult) => {
        this.emailAvailability = {
          isChecking: false,
          isAvailable: result.isEmailAvailable,
          message: result.emailMessage
        };
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.emailAvailability = { isChecking: false };
        console.error('Error checking email availability:', error);
        this.cdr.markForCheck();
      }
    });
  }

  private checkPhoneAvailability(phone: string): void {
    // Don't check if we're in Google mode and field is disabled
    if (this.isGoogleAuthenticated() && this.form.get('phoneNumber')?.disabled) {
      return;
    }

    this.phoneAvailability = { isChecking: true };
    this.cdr.markForCheck();

    this.userAvailabilityService.checkAvailability({ phone }).subscribe({
      next: (result: UserAvailabilityResult) => {
        this.phoneAvailability = {
          isChecking: false,
          isAvailable: result.isPhoneAvailable,
          message: result.phoneMessage
        };
        this.cdr.markForCheck();
      },
      error: (error) => {
        this.phoneAvailability = { isChecking: false };
        console.error('Error checking phone availability:', error);
        this.cdr.markForCheck();
      }
    });
  }

  private isValidPhoneForAvailabilityCheck(phone: string): boolean {
    // Remove any spaces and check if it's a valid international phone format
    const cleanPhone = phone.replace(/\s+/g, '');
    // Should start with + followed by country code (1-4 digits) and then at least 8 more digits
    const phoneRegex = /^\+\d{1,4}\d{8,15}$/;
    return phoneRegex.test(cleanPhone);
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  private checkInitialValuesAvailability(): void {
    // Check initial email value
    const initialEmail = this.form.get('email')?.value;
    if (initialEmail && !this.form.get('email')?.disabled && this.isValidEmail(initialEmail)) {
      this.lastEmailValue = initialEmail;
      this.emailSubject.next(initialEmail.trim());
    }

    // Check initial phone value
    const initialPhone = this.form.get('phoneNumber')?.value;
    const initialCountryCode = this.form.get('countryCode')?.value;
    if (initialPhone && initialCountryCode && !this.form.get('phoneNumber')?.disabled) {
      const fullPhone = `${initialCountryCode}${initialPhone.replace(/\s+/g, '')}`;
      if (this.isValidPhoneForAvailabilityCheck(fullPhone)) {
        this.lastPhoneValue = initialPhone;
        this.phoneSubject.next(fullPhone);
      }
    }
  }
}