import { Component, Input, Output, EventEmitter, ViewChild, ElementRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Animal } from 'src/app/models/animal/animal.model';
import { AdoptionApplicationService } from 'src/app/services/adoption-application.service';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { ErrorDetails } from 'src/app/common/ui/error-message-banner.component';
import { File, FileItem } from 'src/app/models/file/file.model';
import { ValidationErrorInfo, FormInputErrorTrackerService } from 'src/app/common/ui/form-input-error-tracker.service';
import { AdoptionApplication, AdoptionApplicationPersist } from 'src/app/models/adoption-application/adoption-application.model';
import { nameof } from 'ts-simple-nameof';
import { Shelter } from 'src/app/models/shelter/shelter.model';
import { Breed } from 'src/app/models/breed/breed.model';
import { User } from 'src/app/models/user/user.model';
import { AnimalType } from 'src/app/models/animal-type/animal-type.model';

@Component({
  selector: 'app-adoption-form',
  template: `
    <div class="bg-white/5 backdrop-blur-lg rounded-2xl p-6 border border-white/10">
      <!-- Animal Summary -->
      <div class="mb-8 p-4">
        <div class="flex items-center space-x-4">
          <img 
            [src]="animal.attachedPhotos?.[0]?.sourceUrl || '/assets/placeholder.jpg'" 
            [alt]="animal.name"
            class="w-20 h-20 rounded-lg object-cover"
          />
          <div>
            <h3 class="text-xl font-semibold text-white">{{ animal.name }}</h3>
            <p class="text-gray-400">{{ animal.breed?.name }}</p>
          </div>
        </div>
      </div>

      <!-- Error Banner -->
      <app-error-message-banner [error]="error"></app-error-message-banner>

      <!-- Form Error Summary -->
      <app-form-error-summary 
        *ngIf="showErrorSummary"
        [errors]="validationErrors"
        title="Παρακαλώ διορθώστε τα παρακάτω σφάλματα:"
      ></app-form-error-summary>

      <!-- Application Form -->
      <form [formGroup]="applicationForm" (ngSubmit)="onSubmit()" #formContainer>
        <div class="space-y-6">
          <!-- Application Details -->
          <app-text-area-input
            [form]="applicationForm"
            controlName="applicationDetails"
            label="Γιατί θέλετε να υιοθετήσετε αυτό το κατοικίδιο;"
            [rows]="4"
            hint="Περιγράψτε γιατί πιστεύετε ότι είστε ο κατάλληλος για να υιοθετήσετε αυτό το κατοικίδιο"
          ></app-text-area-input>

          <!-- Supporting Documents -->
          <app-file-drop-area
            [form]="applicationForm"
            controlName="attachedFiles"
            label="Υποστηρικτικά Έγγραφα"
            hint="Προσθέστε έγγραφα που υποστηρίζουν την αίτησή σας (προαιρετικό)"
            [multiple]="true"
            accept=".pdf,.doc,.docx,.jpg,.jpeg,.png"
            [maxFileSize]="5 * 1024 * 1024"
            [maxFiles]="3"
            (filesChange)="onFilesChange($event)"
          ></app-file-drop-area>

          <!-- Submit Button -->
          <div class="flex justify-end pt-6">
            <button
              type="submit"
              [disabled]="isSubmitting"
              class="relative px-6 py-2 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg 
                     hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 
                     transform hover:-translate-y-1 disabled:opacity-70 disabled:transform-none 
                     disabled:hover:shadow-none"
            >
              <span [class.opacity-0]="isSubmitting">Υποβολή Αίτησης</span>
              
              <!-- Loading Spinner -->
              <div
                *ngIf="isSubmitting"
                class="absolute inset-0 flex items-center justify-center"
              >
                <svg
                  class="animate-spin h-5 w-5 text-white"
                  xmlns="http://www.w3.org/2000/svg"
                  fill="none"
                  viewBox="0 0 24 24"
                >
                  <circle
                    class="opacity-25"
                    cx="12"
                    cy="12"
                    r="10"
                    stroke="currentColor"
                    stroke-width="4"
                  ></circle>
                  <path
                    class="opacity-75"
                    fill="currentColor"
                    d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                  ></path>
                </svg>
              </div>
            </button>
          </div>
        </div>
      </form>
    </div>
  `
})
export class AdoptionFormComponent {
  @Input() animal!: Animal;
  @Output() applicationSubmitted = new EventEmitter<boolean>();
  @ViewChild('formContainer') formContainer!: ElementRef;

  applicationForm: FormGroup;
  isSubmitting = false;
  error?: ErrorDetails;
  validationErrors: ValidationErrorInfo[] = [];
  showErrorSummary = false;

  constructor(
    private fb: FormBuilder,
    private adoptionApplicationService: AdoptionApplicationService,
    private errorHandler: ErrorHandlerService,
    private formErrorTracker: FormInputErrorTrackerService
  ) {
    this.applicationForm = this.fb.group({
      applicationDetails: ['', [Validators.required, Validators.minLength(50)]],
      attachedFiles: [[]]
    });
  }

  onFilesChange(files: FileItem[]) {
    const fileIds = files.map(f => f.persistedId!).filter(id => id);
    this.applicationForm.get('attachedFiles')?.setValue(fileIds);
  }

  onSubmit() {
    this.validationErrors = [];
    this.showErrorSummary = false;
    this.error = undefined;

    if (this.applicationForm.valid) {
      this.isSubmitting = true;

      const application: AdoptionApplicationPersist = {
        id: '',
        animalId: this.animal.id!,
        shelterId: this.animal.shelter?.id!,
        status: 1,
        applicationDetails: this.applicationForm.get('applicationDetails')?.value,
        attachedFilesIds: this.applicationForm.get('attachedFiles')?.value || []
      };

      this.adoptionApplicationService.persist
      (
        application,
        [
          nameof<AdoptionApplication>(x => x.id),
          nameof<AdoptionApplication>(x => x.status),
          nameof<AdoptionApplication>(x => x.applicationDetails),
          
          [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.id)].join('.'),
          [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.name)].join('.'),
          [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.gender)].join('.'),
          [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.description)].join('.'),
          [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.attachedPhotos), , nameof<File>(x => x.sourceUrl)].join('.'),
          [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.adoptionStatus)].join('.'),
          [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.weight)].join('.'),
          [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.healthStatus)].join('.'),
          [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.animalType), nameof<AnimalType>(x => x.name)].join('.'),
          [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.breed), nameof<Breed>(x => x.name)].join('.'),
          [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.shelterName)].join('.'),
          [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.description)].join('.'),

          [nameof<AdoptionApplication>(x => x.shelter), nameof<Shelter>(x => x.shelterName)].join('.'),
          
          [nameof<AdoptionApplication>(x => x.shelter), nameof<Shelter>(x => x.shelterName)].join('.'),
          [nameof<AdoptionApplication>(x => x.shelter), nameof<Shelter>(x => x.description)].join('.'),
          [nameof<AdoptionApplication>(x => x.shelter), nameof<Shelter>(x => x.website)].join('.'),
          [nameof<AdoptionApplication>(x => x.shelter), nameof<Shelter>(x => x.socialMedia)].join('.'),
          [nameof<AdoptionApplication>(x => x.shelter), nameof<Shelter>(x => x.operatingHours)].join('.'),
          [nameof<AdoptionApplication>(x => x.shelter), nameof<Shelter>(x => x.user), nameof<User>(x => x.profilePhoto), nameof<File>(x => x.sourceUrl)].join('.'),
         

          [nameof<AdoptionApplication>(x => x.attachedFiles), nameof<File>(x => x.sourceUrl)].join('.'),
          [nameof<AdoptionApplication>(x => x.attachedFiles), nameof<File>(x => x.fileName)].join('.'),
          [nameof<AdoptionApplication>(x => x.attachedFiles), nameof<File>(x => x.fileType)].join('.'),
        ]
      ).subscribe({
        next: () => {
          this.isSubmitting = false;
          this.applicationSubmitted.emit(true);
        },
        error: (error) => {
          this.isSubmitting = false;
          this.error = this.errorHandler.handleError(error);
          this.applicationSubmitted.emit(false);
        }
      });
    } else {
      this.validationErrors = this.formErrorTracker.trackFormErrors(
        this.applicationForm,
        this.formContainer.nativeElement
      );
      this.showErrorSummary = true;
      this.formErrorTracker.scrollToFirstError(this.validationErrors);
    }
  }
}