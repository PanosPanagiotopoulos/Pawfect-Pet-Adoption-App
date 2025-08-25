import {
  Component,
  Input,
  Output,
  EventEmitter,
  ViewChild,
  ElementRef,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AdoptionApplicationService } from 'src/app/services/adoption-application.service';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import {
  FormInputErrorTrackerService,
  ValidationErrorInfo,
} from 'src/app/common/services/form-input-error-tracker.service';
import { AuthService } from 'src/app/services/auth.service';
import { nameof } from 'ts-simple-nameof';

import { Animal } from 'src/app/models/animal/animal.model';
import {
  AdoptionApplication,
  AdoptionApplicationPersist,
  ApplicationStatus,
} from 'src/app/models/adoption-application/adoption-application.model';
import { File, FileItem } from 'src/app/models/file/file.model';
import { Shelter } from 'src/app/models/shelter/shelter.model';
import { User } from 'src/app/models/user/user.model';
import { AnimalType } from 'src/app/models/animal-type/animal-type.model';
import { Breed } from 'src/app/models/breed/breed.model';
import { Permission } from 'src/app/common/enum/permission.enum';
import { ErrorDetails } from 'src/app/common/ui/error-message-banner.component';

@Component({
  selector: 'app-adoption-form',
  template: `
    <div
      class="bg-white/5 backdrop-blur-lg rounded-2xl p-6 border border-white/10"
    >
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

      <app-error-message-banner [error]="error"></app-error-message-banner>

      <app-form-error-summary
        *ngIf="showErrorSummary"
        [errors]="validationErrors"
        [title]="'APP.ADOPT.ERROR_SUMMARY_TITLE' | translate"
      ></app-form-error-summary>

      <form
        [formGroup]="applicationForm"
        (ngSubmit)="onSubmit()"
        #formContainer
      >
        <div class="space-y-6">
          <div
            *ngIf="isEditMode"
            class="mb-6 p-4 bg-blue-500/10 border border-blue-500/20 rounded-lg"
          >
            <div class="flex items-start space-x-3">
              <ng-icon
                name="lucideInfo"
                [size]="'20'"
                class="text-blue-400 mt-0.5 flex-shrink-0"
              ></ng-icon>
              <div>
                <h4 class="text-blue-300 font-medium mb-1">
                  {{ 'APP.ADOPT.STATUS_MANAGEMENT_INFO_TITLE' | translate }}
                </h4>
                <p class="text-blue-300/80 text-sm">
                  {{ 'APP.ADOPT.STATUS_MANAGEMENT_INFO_MESSAGE' | translate }}
                </p>
              </div>
            </div>
          </div>

          <app-text-area-input
            [form]="applicationForm"
            controlName="applicationDetails"
            [label]="'APP.ADOPT.WHY_ADOPT_QUESTION' | translate"
            [rows]="4"
            [hint]="'APP.ADOPT.WHY_ADOPT_HINT' | translate"
          ></app-text-area-input>

          <app-file-drop-area
            [form]="applicationForm"
            controlName="attachedFiles"
            [label]="'APP.ADOPT.SUPPORTING_DOCUMENTS' | translate"
            [hint]="'APP.ADOPT.SUPPORTING_DOCUMENTS_HINT' | translate"
            [multiple]="true"
            accept=".pdf,.doc,.docx,.jpg,.jpeg,.png"
            [maxFileSize]="10 * 1024 * 1024"
            [maxFiles]="5"
            [existingFiles]="existingFiles"
            (filesChange)="onFilesChange($event)"
            (uploadStateChange)="onFileUploadStateChange($event)"
          ></app-file-drop-area>

          <div
            *ngIf="
              isEditMode && canManageApplicationStatus && isCurrentUserShelter
            "
            class="space-y-4"
          >
            <div class="border-t border-white/10 pt-6">
              <h4
                class="text-lg font-semibold text-white mb-4 flex items-center space-x-2"
              >
                <ng-icon
                  name="lucideSettings"
                  [size]="'20'"
                  class="text-primary-400"
                ></ng-icon>
                <span>{{
                  'APP.ADOPT.APPLICATION_STATUS_MANAGEMENT' | translate
                }}</span>
              </h4>

              <div class="mb-4">
                <label class="block text-sm font-medium text-gray-400 mb-2">{{
                  'APP.ADOPT.CURRENT_STATUS' | translate
                }}</label>
                <div class="flex items-center space-x-3">
                  <span
                    class="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium border"
                    [ngClass]="
                      getStatusChipClass(
                        adoptionApplication?.status || ApplicationStatus.Pending
                      )
                    "
                  >
                    {{
                      getStatusTranslationKey(
                        adoptionApplication?.status || ApplicationStatus.Pending
                      ) | translate
                    }}
                  </span>
                </div>
              </div>

              <div class="space-y-3">
                <label class="block text-sm font-medium text-gray-400">{{
                  'APP.ADOPT.UPDATE_STATUS' | translate
                }}</label>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-3">
                  <div *ngFor="let option of statusOptions" class="relative">
                    <input
                      type="radio"
                      [id]="'status-' + option.value"
                      [value]="option.value"
                      formControlName="status"
                      class="sr-only peer"
                    />
                    <label
                      [for]="'status-' + option.value"
                      class="flex items-center justify-center p-3 rounded-lg border-2 cursor-pointer transition-all duration-200 border-white/20 bg-white/5 text-gray-300 hover:border-primary-500/50 hover:bg-primary-500/10 peer-checked:border-primary-500 peer-checked:bg-primary-500/20 peer-checked:text-white"
                    >
                      <span class="text-sm font-medium">{{
                        option.label | translate
                      }}</span>
                    </label>
                  </div>
                </div>
              </div>

              <div
                class="mt-4 p-3 bg-amber-500/10 border border-amber-500/20 rounded-lg"
              >
                <div class="flex items-start space-x-2">
                  <ng-icon
                    name="lucideBadgeAlert"
                    [size]="'16'"
                    class="text-amber-400 mt-0.5 flex-shrink-0"
                  ></ng-icon>
                  <p class="text-sm text-amber-300">
                    {{ 'APP.ADOPT.STATUS_CHANGE_WARNING' | translate }}
                  </p>
                </div>
              </div>
            </div>
          </div>

          <div
            *ngIf="!isEditMode || canEdit"
            class="flex gap-3 justify-end pt-6"
          >
            <button
              *ngIf="isEditMode && canDelete"
              type="button"
              (click)="onDelete()"
              [disabled]="isDeletingApplication || isSubmitting"
              class="flex items-center gap-2 px-6 py-2 bg-gradient-to-r from-red-600 to-red-700 text-white rounded-lg font-medium shadow-md hover:shadow-lg hover:-translate-y-0.5 transition-all duration-300 disabled:opacity-50 disabled:cursor-not-allowed disabled:transform-none disabled:hover:shadow-md"
            >
              <ng-icon
                [name]="isDeletingApplication ? 'lucideLoader' : 'lucideTrash2'"
                class="w-5 h-5"
                [ngClass]="{ 'animate-spin': isDeletingApplication }"
              ></ng-icon>
              <span>
                {{
                  isDeletingApplication
                    ? ('APP.ADOPT.DELETING_APPLICATION' | translate)
                    : ('APP.ADOPT.DELETE_APPLICATION' | translate)
                }}
              </span>
            </button>
            <button
              type="submit"
              [disabled]="isSubmitting || isDeletingApplication || isUploadingFiles"
              class="relative flex-1 px-6 py-2 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 transform hover:-translate-y-1 disabled:opacity-70 disabled:transform-none disabled:hover:shadow-none"
            >
              <span [class.opacity-0]="isSubmitting || isUploadingFiles">
                <ng-container *ngIf="!isUploadingFiles">
                  {{
                    isEditMode
                      ? ('APP.ADOPT.UPDATE_APPLICATION' | translate)
                      : ('APP.ADOPT.SUBMIT_APPLICATION' | translate)
                  }}
                </ng-container>
                <ng-container *ngIf="isUploadingFiles">
                  {{ 'APP.ADOPT.UPLOADING_FILES' | translate }}
                </ng-container>
              </span>

              <div
                *ngIf="isSubmitting || isUploadingFiles"
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
  `,
})
export class AdoptionFormComponent implements OnInit, OnChanges {
  @Input() animal!: Animal;
  @Input() adoptionApplication?: AdoptionApplication;
  @Input() isEditMode: boolean = false;
  @Input() canEdit: boolean = true;
  @Input() canDelete: boolean = false;
  @Input() isDeletingApplication: boolean = false;
  @Output() applicationSubmitted = new EventEmitter<string>();
  @Output() deleteRequested = new EventEmitter<void>();
  @Output() fileUploadStateChange = new EventEmitter<boolean>();
  @Output() submittingStateChange = new EventEmitter<boolean>();
  @ViewChild('formContainer') formContainer!: ElementRef;

  ApplicationStatus = ApplicationStatus;
  public applicationForm: FormGroup;
  isSubmitting = false;
  isUploadingFiles = false;
  error?: ErrorDetails;
  validationErrors: ValidationErrorInfo[] = [];
  showErrorSummary = false;
  existingFiles: File[] = [];
  canManageApplicationStatus = false;
  isCurrentUserShelter = false;

  readonly statusOptions = [
    {
      value: ApplicationStatus.Pending,
      label: 'APP.PROFILE-PAGE.APPLICATION_STATUS.PENDING',
    },
    {
      value: ApplicationStatus.Available,
      label: 'APP.PROFILE-PAGE.APPLICATION_STATUS.APPROVED',
    },
    {
      value: ApplicationStatus.Rejected,
      label: 'APP.PROFILE-PAGE.APPLICATION_STATUS.REJECTED',
    },
  ];

  constructor(
    private fb: FormBuilder,
    private adoptionApplicationService: AdoptionApplicationService,
    private errorHandler: ErrorHandlerService,
    private formErrorTracker: FormInputErrorTrackerService,
    private authService: AuthService
  ) {
    this.applicationForm = this.fb.group({
      applicationDetails: ['', [Validators.required, Validators.minLength(50)]],
      attachedFiles: [[]],
      status: [{ value: ApplicationStatus.Pending, disabled: true }],
    });
  }

  ngOnInit() {
    this.initializePermissions();
    this.loadExistingData();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (
      changes['adoptionApplication'] &&
      !changes['adoptionApplication'].firstChange
    ) {
      this.loadExistingData();
    }
  }

  getStatusTranslationKey(status: ApplicationStatus): string {
    switch (status) {
      case ApplicationStatus.Pending:
        return 'APP.PROFILE-PAGE.APPLICATION_STATUS.PENDING';
      case ApplicationStatus.Available:
        return 'APP.PROFILE-PAGE.APPLICATION_STATUS.APPROVED';
      case ApplicationStatus.Rejected:
        return 'APP.PROFILE-PAGE.APPLICATION_STATUS.REJECTED';
      default:
        return 'APP.PROFILE-PAGE.APPLICATION_STATUS.PENDING';
    }
  }

  getStatusChipClass(status: ApplicationStatus): string {
    switch (status) {
      case ApplicationStatus.Pending:
        return 'bg-amber-500/20 text-amber-300 border-amber-500/30';
      case ApplicationStatus.Available:
        return 'bg-green-500/20 text-green-300 border-green-500/30';
      case ApplicationStatus.Rejected:
        return 'bg-red-500/20 text-red-300 border-red-500/30';
      default:
        return 'bg-gray-500/20 text-gray-300 border-gray-500/30';
    }
  }

  onFilesChange(files: FileItem[]) {
    const fileIds = files.map((f) => f.persistedId!).filter((id) => id);
    this.applicationForm.get('attachedFiles')?.setValue(fileIds);
  }

  onFileUploadStateChange(isUploading: boolean) {
    this.isUploadingFiles = isUploading;
    this.fileUploadStateChange.emit(isUploading);
  }

  onSubmit() {
    this.validationErrors = [];
    this.showErrorSummary = false;
    this.error = undefined;

    // Mark all fields as touched to show validation errors
    this.markFormGroupTouched(this.applicationForm);
    
    // Check individual controls
    Object.keys(this.applicationForm.controls).forEach(key => {
      const control = this.applicationForm.get(key);
    });

    if (this.applicationForm.valid) {
      this.isSubmitting = true;
      this.submittingStateChange.emit(true);

      const statusToUse = this.determineStatusToUse();
      const application = this.buildApplicationPersist(statusToUse);

      this.adoptionApplicationService
        .persist(application, this.getPersistFields())
        .subscribe({
          next: (model: AdoptionApplication) => {
            this.isSubmitting = false;
            this.submittingStateChange.emit(false);
            this.applicationSubmitted.emit(model.id);
          },
          error: (error) => {
            this.isSubmitting = false;
            this.submittingStateChange.emit(false);
            this.error = this.errorHandler.handleError(error);
            this.applicationSubmitted.emit('');
          },
        });
    } else {
      // Log form validation state for debugging
      // Log individual control errors
      Object.keys(this.applicationForm.controls).forEach(key => {
        const control = this.applicationForm.get(key);
        if (control?.invalid) {
        }
      });
      
      // Track form errors and show error summary
      this.validationErrors = this.formErrorTracker.trackFormErrors(
        this.applicationForm,
        this.formContainer.nativeElement
      );
      
      
      this.showErrorSummary = true;
      
      // Scroll to first error
      if (this.validationErrors.length > 0) {
        this.formErrorTracker.scrollToFirstError(this.validationErrors);
      }
      
      // Also show a general error message
      this.error = {
        title: 'Validation Error',
        message: 'Please fix the errors below and try again.',
        type: 'error'
      } as ErrorDetails;
      
    }
  }

  private markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
      
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  onDelete(): void {
    this.deleteRequested.emit();
  }

  private initializePermissions() {
    this.canManageApplicationStatus = this.authService.hasPermission(
      Permission.EditAdoptionApplications
    );

    if (this.isEditMode && this.adoptionApplication && this.animal?.shelter) {
      const currentUserEmail = this.authService.getUserEmail();
      this.isCurrentUserShelter =
        currentUserEmail === this.animal.shelter.user?.email;
    }

    if (this.canManageApplicationStatus && this.isCurrentUserShelter) {
      this.applicationForm.get('status')?.enable();
    }
  }

  private loadExistingData() {
    if (this.isEditMode && this.adoptionApplication) {
      
      this.applicationForm.patchValue({
        applicationDetails: this.adoptionApplication.applicationDetails || '',
        status: this.adoptionApplication.status || ApplicationStatus.Pending,
      });

      this.existingFiles = this.adoptionApplication.attachedFiles || [];

      const existingFileIds = this.existingFiles
        .map((file) => file.id || file)
        .filter((id) => id && typeof id === 'string');

      this.applicationForm.get('attachedFiles')?.setValue(existingFileIds);
      this.initializePermissions();
      
      // Use setTimeout to ensure all child components have finished initializing
      setTimeout(() => {
        // Mark as pristine and untouched to prevent unnecessary form guard dialogs
        this.applicationForm.markAsPristine();
        this.applicationForm.markAsUntouched();
        
      }, 0);
    } else {
    }
  }

  private determineStatusToUse(): ApplicationStatus {
    if (
      this.isEditMode &&
      this.canManageApplicationStatus &&
      this.isCurrentUserShelter
    ) {
      return (
        this.applicationForm.get('status')?.value || ApplicationStatus.Pending
      );
    } else {
      return this.isEditMode
        ? this.adoptionApplication?.status || ApplicationStatus.Pending
        : ApplicationStatus.Pending;
    }
  }

  private buildApplicationPersist(
    statusToUse: ApplicationStatus
  ): AdoptionApplicationPersist {
    return {
      id: this.isEditMode ? this.adoptionApplication?.id || '' : '',
      animalId: this.isEditMode
        ? this.adoptionApplication?.animal?.id!
        : this.animal.id!,
      shelterId: this.isEditMode
        ? this.adoptionApplication?.shelter?.id!
        : this.animal.shelter?.id!,
      status: statusToUse,
      applicationDetails: this.applicationForm.get('applicationDetails')?.value,
      attachedFilesIds: this.applicationForm.get('attachedFiles')?.value || [],
    };
  }

  private getPersistFields(): string[] {
    return [
      nameof<AdoptionApplication>((x) => x.id),
      nameof<AdoptionApplication>((x) => x.status),
      nameof<AdoptionApplication>((x) => x.applicationDetails),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.id),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.name),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.gender),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.description),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.attachedPhotos),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.adoptionStatus),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.weight),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.healthStatus),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.animalType),
        nameof<AnimalType>((x) => x.name),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.breed),
        nameof<Breed>((x) => x.name),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.shelter),
        nameof<Shelter>((x) => x.shelterName),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.shelter),
        nameof<Shelter>((x) => x.description),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.shelter),
        nameof<Shelter>((x) => x.shelterName),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.shelter),
        nameof<Shelter>((x) => x.description),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.shelter),
        nameof<Shelter>((x) => x.website),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.shelter),
        nameof<Shelter>((x) => x.socialMedia),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.shelter),
        nameof<Shelter>((x) => x.operatingHours),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.shelter),
        nameof<Shelter>((x) => x.user),
        nameof<User>((x) => x.profilePhoto),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.attachedFiles),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.attachedFiles),
        nameof<File>((x) => x.filename),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.attachedFiles),
        nameof<File>((x) => x.fileType),
      ].join('.'),
    ];
  }
}
