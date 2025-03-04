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

      <!-- Profile Picture Upload - Improved Layout with Preview Below -->
      <div class="mb-8">
        <h3 class="text-lg font-medium text-white mb-4">Φωτογραφία Προφίλ</h3>
        
        <!-- File drop area -->
        <div class="w-full">
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
        </div>
        
        <!-- Preview of selected profile photo - Always below the dropzone -->
        <div 
          *ngIf="profilePhotoPreview || isPhotoLoading" 
          class="mt-6 flex justify-center"
        >
          <div class="relative group w-32 h-32 rounded-full overflow-hidden border-2 border-primary-500/50">
            <!-- Loading spinner -->
            <div
              *ngIf="isPhotoLoading"
              class="absolute inset-0 flex items-center justify-center bg-gray-800/70 z-10"
            >
              <div
                class="w-10 h-10 border-4 border-primary-400 border-t-transparent rounded-full animate-spin"
              ></div>
            </div>

            <!-- Image preview -->
            <img
              *ngIf="profilePhotoPreview"
              [src]="profilePhotoPreview"
              alt="Profile preview"
              class="w-full h-full object-cover transition-transform duration-300 group-hover:scale-110"
            />

            <!-- Placeholder -->
            <div
              *ngIf="!profilePhotoPreview && !isPhotoLoading"
              class="w-full h-full bg-gray-700 flex items-center justify-center"
            >
              <ng-icon
                name="lucideUser"
                [size]="'32'"
                class="text-gray-400"
              ></ng-icon>
            </div>

            <!-- Remove button -->
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
        
        <!-- Status messages -->
        <div class="mt-2 text-center">
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

  profilePhotoPreview: string | null = null;
  isPhotoLoading = false;
  photoUploadSuccess = false;
  photoUploadError: string | null = null;

  constructor(private cdr: ChangeDetectorRef) {}

  getLocationForm(): FormGroup {
    return this.form.get('location') as FormGroup;
  }

  onNext(): void {
    if (this.form.valid) {
      this.next.emit();
    } else {
      this.form.markAllAsTouched();
    }
  }

  onPhoneChange(phone: string): void {
    // Update the phone field in the form
    this.form.get('phone')?.setValue(phone);
  }

  onProfilePhotoChange(files: File[]): void {
    this.photoUploadSuccess = false;
    this.photoUploadError = null;
    if (files.length > 0) {
      const file: File = files[0];

      // Validate file type
      if (!file.type.match('image.*')) {
        this.photoUploadError =
          'Μη έγκυρος τύπος αρχείου. Επιτρέπονται μόνο εικόνες.';
        this.profilePhotoPreview = null;
        this.form.get('profilePhoto')?.setErrors({ invalidType: true });
        this.cdr.markForCheck();
        return;
      }

      // Validate file size
      if (file.size > 2 * 1024 * 1024) {
        this.photoUploadError =
          'Το μέγεθος της εικόνας δεν πρέπει να υπερβαίνει τα 2MB.';
        this.profilePhotoPreview = null;
        this.form.get('profilePhoto')?.setErrors({ invalidSize: true });
        this.cdr.markForCheck();
        return;
      }

      // Show loading state
      this.isPhotoLoading = true;
      this.cdr.markForCheck();

      // Create a preview URL for the selected image
      this.createImagePreview(file);
      
      // Set the file in the form control
      this.form.get('profilePhoto')?.setValue(file);
      this.form.get('profilePhoto')?.updateValueAndValidity();
    } else {
      this.profilePhotoPreview = null;
      this.cdr.markForCheck();
    }
  }

  removeProfilePhoto(): void {
    // Clear the profile photo from the form
    this.form.get('profilePhoto')?.setValue(null);
    this.profilePhotoPreview = null;
    this.photoUploadSuccess = false;
    this.photoUploadError = null;

    // Reset the file input
    const fileInput =
      this.formContainer.nativeElement.querySelector('input[type="file"]');
    if (fileInput) {
      fileInput.value = '';
    }

    this.cdr.markForCheck();
  }

  private createImagePreview(file: File): void {
    // Only process image files
    if (!file.type.match('image.*')) {
      this.isPhotoLoading = false;
      return;
    }

    const reader = new FileReader();

    reader.onload = (e: any) => {
      // Simulate a slight delay to show loading state
      setTimeout(() => {
        this.isPhotoLoading = false;
        this.profilePhotoPreview = e.target.result;
        this.photoUploadSuccess = true;
        this.cdr.markForCheck();
      }, 800);
    };

    // Handle errors
    reader.onerror = () => {
      this.isPhotoLoading = false;
      this.photoUploadError = 'Σφάλμα κατά τη φόρτωση της εικόνας.';
      this.cdr.markForCheck();
    };

    reader.readAsDataURL(file);
  }
}