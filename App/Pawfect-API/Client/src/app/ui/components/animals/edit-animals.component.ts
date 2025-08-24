import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectorRef,
  HostListener,
} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subscription } from 'rxjs';
import { trigger, transition, style, animate } from '@angular/animations';

import { AnimalService } from 'src/app/services/animal.service';
import { AnimalTypeService } from 'src/app/services/animal-type.service';
import { BreedService } from 'src/app/services/breed.service';
import { TranslationService } from 'src/app/common/services/translation.service';
import { LogService } from 'src/app/common/services/log.service';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { SnackbarService } from 'src/app/common/services/snackbar.service';
import { AuthService } from 'src/app/services/auth.service';

import {
  Animal,
  AnimalPersist,
  AdoptionStatus,
} from 'src/app/models/animal/animal.model';
import { AnimalType } from 'src/app/models/animal-type/animal-type.model';
import { Breed } from 'src/app/models/breed/breed.model';
import { Shelter } from 'src/app/models/shelter/shelter.model';
import { Gender } from 'src/app/common/enum/gender';
import { FileItem, File } from 'src/app/models/file/file.model';

import { FileToUrlPipe } from 'src/app/common/tools/file-to-url.pipe';
import { AnimalTypeLookup } from 'src/app/lookup/animal-type-lookup';
import { nameof } from 'ts-simple-nameof';
import { BreedLookup } from 'src/app/lookup/breed-lookup';
import { CanComponentDeactivate } from 'src/app/common/guards/form.guard';
import { Permission } from 'src/app/common/enum/permission.enum';
import { MatDialog } from '@angular/material/dialog';
import {
  FormLeaveConfirmationDialogComponent,
  ConfirmationDialogData,
} from '../../../common/ui/form-leave-confirmation-dialog.component';

@Component({
  selector: 'app-edit-animals',
  standalone: false,
  templateUrl: './edit-animals.component.html',
  styleUrls: ['./edit-animals.component.scss'],
  animations: [
    trigger('fadeInLeft', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(-50px)' }),
        animate(
          '0.6s ease-out',
          style({ opacity: 1, transform: 'translateX(0)' })
        ),
      ]),
    ]),
    trigger('fadeInRight', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(50px)' }),
        animate(
          '0.6s ease-out',
          style({ opacity: 1, transform: 'translateX(0)' })
        ),
      ]),
    ]),
  ],
})
export class EditAnimalsComponent
  implements OnInit, OnDestroy, CanComponentDeactivate
{
  animalForm: FormGroup;
  animal: Animal | null = null;
  animalId: string = '';
  isLoading = false;
  isSaving = false;
  isLoadingBreeds = false;
  isUploadingFiles = false;
  isDeleting = false;
  error: string | null = null;
  currentUserShelterId?: string;
  private formSaved = false;
  private formDeleted = false;

  animalTypes: AnimalType[] = [];
  filteredBreeds: Breed[] = [];

  // Image preview slider state
  currentImageIndex = 0;
  formImageFiles: FileItem[] = [];
  slideDirection: 'left' | 'right' | 'none' = 'none';

  // Dropdown states
  dropdownStates = {
    gender: false,
    adoptionStatus: false,
    animalType: false,
    breed: false,
  };

  // Options
  readonly genderOptions = [
    { value: Gender.Male, label: 'APP.COMMONS.MALE' },
    { value: Gender.Female, label: 'APP.COMMONS.FEMALE' },
  ];

  readonly adoptionStatusOptions = [
    {
      value: AdoptionStatus.Available,
      label: 'APP.ANIMALS.ADOPTION_STATUS.AVAILABLE',
    },
    {
      value: AdoptionStatus.Pending,
      label: 'APP.ANIMALS.ADOPTION_STATUS.PENDING',
    },
    {
      value: AdoptionStatus.Adopted,
      label: 'APP.ANIMALS.ADOPTION_STATUS.ADOPTED',
    },
  ];

  private subscriptions: Subscription[] = [];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute,
    private animalService: AnimalService,
    private animalTypeService: AnimalTypeService,
    private breedService: BreedService,
    private translationService: TranslationService,
    private log: LogService,
    private errorHandler: ErrorHandlerService,
    private snackbarService: SnackbarService,
    private cdr: ChangeDetectorRef,
    private authService: AuthService,
    private dialog: MatDialog
  ) {
    this.animalForm = this.createAnimalForm();
    this.currentUserShelterId =
      this.authService.getUserShelterId() || undefined;
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.animalId = params['animalId'];

      // Reset form state flags
      this.formSaved = false;
      this.formDeleted = false;

      if (this.animalId) {
        // Start loading state
        this.isLoading = true;
        this.loadInitialData();
        this.loadAnimal();
      } else {
        this.router.navigate(['/404']);
      }
    });
  }

  private checkEditPermissions(): void {
    // Check if user owns this animal's shelter
    if (
      !this.authService.hasPermission(Permission.EditAnimals) &&
      this.animal &&
      !this.currentUserShelterId &&
      this.animal.shelter?.id !== this.currentUserShelterId
    ) {
      this.navigateToUnauthorized();
      return;
    }
  }

  private navigateToUnauthorized(): void {
    this.router.navigate(['/unauthorized'], {
      queryParams: {
        message: 'You do not have permission to edit this animal',
        returnUrl: `/animals/view/${this.animalId}`,
      },
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((sub) => sub.unsubscribe());

    // Clean up file URLs to prevent memory leaks
    if (this.formImageFiles.length > 0) {
      const fileObjects = this.formImageFiles.map((item) => item.file);
      FileToUrlPipe.revokeUrls(fileObjects);
    }
  }

  loadInitialData(): void {
    this.isLoading = true;

    const animalTypesQuery: AnimalTypeLookup = {
      ids: undefined,
      name: undefined,
      sortBy: [],
      offset: 0,
      pageSize: 1000,
      fields: [
        nameof<AnimalType>((x) => x.id),
        nameof<AnimalType>((x) => x.name),
      ],
    };

    const loadSub = this.animalTypeService.query(animalTypesQuery).subscribe({
      next: (data) => {
        this.animalTypes = data.items;
        // Don't set isLoading = false here, let loadAnimal() handle it
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading = false; // Set loading to false on error
        this.snackbarService.showError({
          message: this.translationService.translate(
            'APP.ANIMALS.ADD.LOAD_ANIMAL_TYPES_ERROR'
          ),
          subMessage: this.translationService.translate(
            'APP.COMMONS.TRY_AGAIN'
          ),
        });
        this.errorHandler.handleError(err);
        this.log.logFormatted({
          message: 'Failed to load animal types',
          error: err,
        });
        this.cdr.markForCheck();
      },
    });

    this.subscriptions.push(loadSub);
  }

  loadAnimal(): void {
    const fields = [
      nameof<Animal>((x) => x.id),
      nameof<Animal>((x) => x.name),
      nameof<Animal>((x) => x.age),
      nameof<Animal>((x) => x.gender),
      nameof<Animal>((x) => x.description),
      nameof<Animal>((x) => x.weight),
      nameof<Animal>((x) => x.healthStatus),
      nameof<Animal>((x) => x.adoptionStatus),
      [
        nameof<Animal>((x) => x.animalType),
        nameof<AnimalType>((x) => x.id),
      ].join('.'),
      [
        nameof<Animal>((x) => x.animalType),
        nameof<AnimalType>((x) => x.name),
      ].join('.'),
      [nameof<Animal>((x) => x.breed), nameof<Breed>((x) => x.id)].join('.'),
      [nameof<Animal>((x) => x.breed), nameof<Breed>((x) => x.name)].join('.'),
      [nameof<Animal>((x) => x.shelter), nameof<Shelter>((x) => x.id)].join(
        '.'
      ),
      [nameof<Animal>((x) => x.attachedPhotos), nameof<File>((x) => x.id)].join(
        '.'
      ),
      [
        nameof<Animal>((x) => x.attachedPhotos),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),
      [
        nameof<Animal>((x) => x.attachedPhotos),
        nameof<File>((x) => x.filename),
      ].join('.'),
      [
        nameof<Animal>((x) => x.attachedPhotos),
        nameof<File>((x) => x.size),
      ].join('.'),
      [
        nameof<Animal>((x) => x.attachedPhotos),
        nameof<File>((x) => x.mimeType),
      ].join('.'),
    ];

    const animalSub = this.animalService
      .getSingle(this.animalId, fields)
      .subscribe({
        next: (animal) => {
          this.animal = animal;

          // Check permissions after loading animal data
          this.checkEditPermissions();

          this.populateForm(animal);

          // Load breeds for the animal type
          if (animal.animalType?.id) {
            this.loadBreedsForAnimalType(animal.animalType.id);
          }

          // Finish loading - no need to wait for delete permission check
          this.isLoading = false;
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.isLoading = false;
          this.snackbarService.showError({
            message: this.translationService.translate(
              'APP.ANIMALS.EDIT.LOAD_ERROR'
            ),
            subMessage: this.translationService.translate(
              'APP.COMMONS.TRY_AGAIN'
            ),
          });
          this.errorHandler.handleError(err);
          this.log.logFormatted({
            message: 'Failed to load animal',
            error: err,
            data: { animalId: this.animalId },
          });
          this.cdr.markForCheck();
        },
      });

    this.subscriptions.push(animalSub);
  }

  private populateForm(animal: Animal): void {
    const formData = {
      name: animal.name || '',
      age: animal.age || null,
      gender: animal.gender || null,
      description: animal.description || '',
      weight: animal.weight || null,
      healthStatus: animal.healthStatus || '',
      animalTypeId: animal.animalType?.id || null,
      breedId: animal.breed?.id || null,
      attachedPhotosIds: animal.attachedPhotos?.map((photo) => photo.id) || [],
      adoptionStatus: animal.adoptionStatus || AdoptionStatus.Available,
    };

    this.animalForm.patchValue(formData);

    // Convert existing photos to FileItem format for preview
    if (animal.attachedPhotos && animal.attachedPhotos.length > 0) {
      this.formImageFiles = animal.attachedPhotos.map((photo) =>
        this.convertFileToFileItem(photo)
      );
      this.currentImageIndex = 0;
    }

    // Use setTimeout to ensure all child components have finished initializing
    setTimeout(() => {
      // Mark form as pristine and untouched after initial population
      this.animalForm.markAsPristine();
      this.animalForm.markAsUntouched();

      // Check individual controls
      Object.keys(this.animalForm.controls).forEach((key) => {
        const control = this.animalForm.get(key);
      });
    }, 0);
  }

  private convertFileToFileItem(file: File): FileItem {
    const fileName = file.filename || 'Unknown File';
    const fileSize = file.size || 0;
    const mimeType = file.mimeType || 'application/octet-stream';

    const mockFile = new globalThis.File([new Blob()], fileName, {
      type: mimeType,
      lastModified: Date.now(),
    });

    if (fileSize > 0) {
      Object.defineProperty(mockFile, 'size', {
        value: fileSize,
        writable: false,
      });
    }

    return {
      file: mockFile,
      addedAt: Date.now(),
      persistedId: file.id,
      isPersisting: false,
      uploadFailed: false,
      isExisting: true,
      sourceUrl: file.sourceUrl,
    };
  }

  private createAnimalForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      age: [null, [Validators.required, Validators.min(0), Validators.max(30)]],
      gender: [null, Validators.required],
      description: [
        '',
        [
          Validators.required,
          Validators.minLength(10),
        ],
      ],
      weight: [
        null,
        [Validators.required, Validators.min(0.1), Validators.max(200)],
      ],
      healthStatus: [
        '',
        [
          Validators.required,
          Validators.minLength(5),
        ],
      ],
      animalTypeId: [null, Validators.required],
      breedId: [null, Validators.required],
      attachedPhotosIds: [[]],
      adoptionStatus: [AdoptionStatus.Available, Validators.required],
    });
  }

  onAnimalTypeChange(animalTypeId: string): void {
    this.animalForm.patchValue({ animalTypeId, breedId: null });
    this.loadBreedsForAnimalType(animalTypeId);
    this.cdr.markForCheck();
  }

  private loadBreedsForAnimalType(animalTypeId: string): void {
    this.isLoadingBreeds = true;
    this.filteredBreeds = [];

    const breedsQuery: BreedLookup = {
      ids: undefined,
      typeIds: [animalTypeId],
      createdFrom: undefined,
      createdTill: undefined,
      offset: 0,
      pageSize: 1000,
      fields: [nameof<Breed>((x) => x.id), nameof<Breed>((x) => x.name)],
      sortBy: [],
    };

    const breedsSub = this.breedService.query(breedsQuery).subscribe({
      next: (breedsResult) => {
        this.filteredBreeds = breedsResult.items;
        this.isLoadingBreeds = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoadingBreeds = false;
        this.snackbarService.showError({
          message: this.translationService.translate(
            'APP.ANIMALS.ADD.LOAD_BREEDS_ERROR'
          ),
          subMessage: this.translationService.translate(
            'APP.COMMONS.TRY_AGAIN'
          ),
        });
        this.errorHandler.handleError(err);
        this.log.logFormatted({
          message: 'Failed to load breeds for animal type',
          error: err,
          data: { animalTypeId },
        });
        this.cdr.markForCheck();
      },
    });

    this.subscriptions.push(breedsSub);
  }

  onFilesChange(files: FileItem[]): void {
    const fileIds = files
      .filter((file) => file.persistedId)
      .map((file) => file.persistedId!);

    this.animalForm.patchValue({ attachedPhotosIds: fileIds });

    // Store files for image preview
    this.formImageFiles = files.filter((file) =>
      file.file.type.startsWith('image/')
    );

    // Reset image slider index when files change
    this.currentImageIndex = 0;

    this.cdr.markForCheck();
  }

  onUploadStateChange(isUploading: boolean): void {
    this.isUploadingFiles = isUploading;
    this.cdr.markForCheck();
  }

  // Image preview slider methods
  getImageFiles(): FileItem[] {
    return this.formImageFiles || [];
  }

  getCurrentImageIndex(): number {
    return this.currentImageIndex || 0;
  }

  getCurrentImageFile(): globalThis.File | null {
    const files = this.getImageFiles();
    const index = this.getCurrentImageIndex();
    return files[index]?.file || null;
  }

  getCurrentImageFileName(): string {
    const files = this.getImageFiles();
    const index = this.getCurrentImageIndex();
    const fileItem = files[index];
    return fileItem?.file?.name || '';
  }

  getCurrentImageUrl(): string {
    return this.getImageUrlByIndex(this.getCurrentImageIndex());
  }

  getImageUrlByIndex(index: number): string {
    const files = this.getImageFiles();
    const fileItem = files[index];

    if (!fileItem) return '';

    // For existing files, use the sourceUrl
    if (fileItem.isExisting && fileItem.sourceUrl) {
      return fileItem.sourceUrl;
    }

    // For new files, create object URL
    if (fileItem.file) {
      return URL.createObjectURL(fileItem.file);
    }

    return '';
  }

  nextImage(totalImages: number): void {
    if (totalImages <= 0) return;
    const current = this.getCurrentImageIndex();
    this.slideDirection = 'left';
    this.currentImageIndex = (current + 1) % totalImages;
    this.cdr.markForCheck();

    setTimeout(() => {
      this.slideDirection = 'none';
      this.cdr.markForCheck();
    }, 300);
  }

  previousImage(totalImages: number): void {
    if (totalImages <= 0) return;
    const current = this.getCurrentImageIndex();
    this.slideDirection = 'right';
    this.currentImageIndex = current === 0 ? totalImages - 1 : current - 1;
    this.cdr.markForCheck();

    setTimeout(() => {
      this.slideDirection = 'none';
      this.cdr.markForCheck();
    }, 300);
  }

  goToImage(index: number): void {
    if (index < 0) return;
    const current = this.getCurrentImageIndex();

    if (index > current) {
      this.slideDirection = 'left';
    } else if (index < current) {
      this.slideDirection = 'right';
    } else {
      this.slideDirection = 'none';
    }

    this.currentImageIndex = index;
    this.cdr.markForCheck();

    setTimeout(() => {
      this.slideDirection = 'none';
      this.cdr.markForCheck();
    }, 300);
  }

  getSlideDirection(): string {
    return this.slideDirection || 'none';
  }

  getFormValidationErrors(): string[] {
    const errors: string[] = [];

    Object.keys(this.animalForm.controls).forEach((key) => {
      const control = this.animalForm.get(key);
      if (control?.invalid && (control?.touched || control?.dirty)) {
        if (control.errors?.['required']) {
          errors.push(
            `APP.ANIMALS.ADD.VALIDATION.${key.toUpperCase()}_REQUIRED`
          );
        }
        if (control.errors?.['minlength']) {
          errors.push(
            `APP.ANIMALS.ADD.VALIDATION.${key.toUpperCase()}_MIN_LENGTH`
          );
        }
        if (control.errors?.['maxlength']) {
          errors.push(
            `APP.ANIMALS.ADD.VALIDATION.${key.toUpperCase()}_MAX_LENGTH`
          );
        }
        if (control.errors?.['min']) {
          errors.push(
            `APP.ANIMALS.ADD.VALIDATION.${key.toUpperCase()}_MIN_VALUE`
          );
        }
        if (control.errors?.['max']) {
          errors.push(
            `APP.ANIMALS.ADD.VALIDATION.${key.toUpperCase()}_MAX_VALUE`
          );
        }
      }
    });

    return errors;
  }

  markFormGroupTouched(formGroup: FormGroup): void {
    Object.keys(formGroup.controls).forEach((key) => {
      const control = formGroup.get(key);
      control?.markAsTouched();

      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      }
    });
  }

  async saveAnimal(): Promise<void> {
    if (this.isSaving || this.isUploadingFiles) return;

    // Check validity first without marking as touched
    if (this.animalForm.invalid) {
      // Log individual control errors
      Object.keys(this.animalForm.controls).forEach((key) => {
        const control = this.animalForm.get(key);
      });

      // Only mark as touched if there are validation errors to show them
      this.markFormGroupTouched(this.animalForm);

      this.snackbarService.showError({
        message: this.translationService.translate(
          'APP.ANIMALS.EDIT.VALIDATION_ERROR'
        ),
        subMessage: this.translationService.translate(
          'APP.ANIMALS.ADD.FIX_ERRORS_MESSAGE'
        ),
      });
      this.cdr.markForCheck();
      return;
    }

    this.isSaving = true;
    this.cdr.markForCheck();

    try {
      const formValue = this.animalForm.value;
      const animalToSave: AnimalPersist = {
        id: this.animalId,
        name: formValue.name,
        age: formValue.age,
        gender: formValue.gender,
        description: formValue.description,
        weight: formValue.weight,
        healthStatus: formValue.healthStatus,
        animalTypeId: formValue.animalTypeId,
        breedId: formValue.breedId,
        attachedPhotosIds: formValue.attachedPhotosIds || [],
        adoptionStatus: formValue.adoptionStatus,
      };

      const saveSub = this.animalService.persist(animalToSave).subscribe({
        next: (savedAnimal) => {
          this.formSaved = true; // Mark form as saved to prevent guard dialog

          this.log.logFormatted({
            message: 'Successfully updated animal',
            data: { animalId: savedAnimal.id },
          });

          this.snackbarService.showSuccess({
            message: this.translationService.translate(
              'APP.ANIMALS.EDIT.SAVE_SUCCESS'
            ),
            subMessage: this.translationService.translate(
              'APP.ANIMALS.EDIT.ANIMAL_UPDATED'
            ),
          });

          // Mark form as pristine after successful save
          this.animalForm.markAsPristine();

          // Navigate back to profile
          this.router.navigate(['/profile'], {
            queryParams: { tab: 'my-animals' },
          });
        },
        error: (err) => {
          this.isSaving = false;
          this.snackbarService.showError({
            message: this.translationService.translate(
              'APP.ANIMALS.EDIT.SAVE_ERROR'
            ),
            subMessage: this.translationService.translate(
              'APP.COMMONS.TRY_AGAIN'
            ),
          });
          this.errorHandler.handleError(err);
          this.log.logFormatted({
            message: 'Failed to update animal',
            error: err,
          });
          this.cdr.markForCheck();
        },
      });

      this.subscriptions.push(saveSub);
    } catch (error) {
      this.isSaving = false;
      this.snackbarService.showError({
        message: this.translationService.translate(
          'APP.ANIMALS.EDIT.SAVE_ERROR'
        ),
        subMessage: this.translationService.translate('APP.COMMONS.TRY_AGAIN'),
      });
      this.cdr.markForCheck();
    }
  }

  cancel(): void {
    this.router.navigate(['/profile'], {
      queryParams: { tab: 'my-animals' },
    });
  }

  // Dropdown helper methods
  getGenderLabel(value: Gender): string {
    const option = this.genderOptions.find((opt) => opt.value === value);
    return option ? this.translationService.translate(option.label) : '';
  }

  getAdoptionStatusLabel(value: AdoptionStatus): string {
    const option = this.adoptionStatusOptions.find(
      (opt) => opt.value === value
    );
    return option ? this.translationService.translate(option.label) : '';
  }

  getGenderTranslationKey(value: Gender): string {
    const option = this.genderOptions.find((opt) => opt.value === value);
    return option ? option.label : '';
  }

  getAdoptionStatusTranslationKey(value: AdoptionStatus): string {
    const option = this.adoptionStatusOptions.find(
      (opt) => opt.value === value
    );
    return option ? option.label : '';
  }

  getAnimalTypeLabel(value: string): string {
    const animalType = this.animalTypes.find((type) => type.id === value);
    return animalType?.name || '';
  }

  getBreedLabel(value: string): string {
    const breed = this.filteredBreeds.find((b) => b.id === value);
    return breed?.name || '';
  }

  toggleDropdown(dropdownType: keyof typeof this.dropdownStates): void {
    // Close all other dropdowns
    Object.keys(this.dropdownStates).forEach((key) => {
      if (key !== dropdownType) {
        this.dropdownStates[key as keyof typeof this.dropdownStates] = false;
      }
    });

    // Toggle the requested dropdown
    this.dropdownStates[dropdownType] = !this.dropdownStates[dropdownType];
    this.cdr.markForCheck();
  }

  closeDropdown(dropdownType: keyof typeof this.dropdownStates): void {
    this.dropdownStates[dropdownType] = false;
    this.cdr.markForCheck();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;

    if (!target.closest('.relative')) {
      Object.keys(this.dropdownStates).forEach((key) => {
        this.dropdownStates[key as keyof typeof this.dropdownStates] = false;
      });
      this.cdr.markForCheck();
    }
  }

  // CanComponentDeactivate implementation
  canDeactivate(): boolean {
    return !this.hasUnsavedChanges();
  }

  hasUnsavedChanges(): boolean {
    // Don't show guard dialog if form was saved or deleted
    if (this.formSaved || this.formDeleted) {
      return false;
    }

    // Don't show guard dialog if files are currently uploading
    if (this.isUploadingFiles) {
      return false;
    }

    // Only consider meaningful changes, not just touched state
    return this.animalForm.dirty;
  }

  // Permission check for delete - check if current shelter owns the animal
  canDeleteAnimal(): boolean {
    // Check if current shelter owns this animal (will be set in loadAnimal)
    return (
      this.currentUserShelterId === this.animal?.shelter?.id ||
      this.authService.hasPermission(Permission.DeleteAnimals)
    );
  }

  // Show delete confirmation dialog
  showDeleteConfirmation(): void {
    const dialogData: ConfirmationDialogData = {
      title: this.translationService.translate(
        'APP.ANIMALS.EDIT.DELETE_CONFIRMATION_TITLE'
      ),
      message: this.translationService.translate(
        'APP.ANIMALS.EDIT.DELETE_CONFIRMATION_MESSAGE'
      ),
      confirmText: this.translationService.translate(
        'APP.ANIMALS.EDIT.DELETE_CONFIRM'
      ),
      cancelText: this.translationService.translate(
        'APP.ANIMALS.EDIT.DELETE_CANCEL'
      ),
      icon: 'lucideTrash2',
      confirmButtonClass: 'btn-danger',
    };

    const dialogRef = this.dialog.open(FormLeaveConfirmationDialogComponent, {
      width: '28rem',
      maxWidth: '90vw',
      disableClose: false,
      hasBackdrop: true,
      backdropClass: 'form-guard-backdrop',
      panelClass: 'form-guard-panel',
      autoFocus: false,
      restoreFocus: false,
      data: dialogData,
    });

    // Disable body scrolling
    document.body.classList.add('no-scroll');

    dialogRef.afterClosed().subscribe((confirmed: boolean) => {
      // Re-enable body scrolling
      document.body.classList.remove('no-scroll');

      if (confirmed) {
        this.deleteAnimal();
      }
    });
  }

  // Delete animal method
  async deleteAnimal(): Promise<void> {
    if (this.isDeleting || !this.animal?.id) return;

    this.isDeleting = true;
    this.cdr.markForCheck();

    try {
      const deleteSub = this.animalService.delete(this.animal.id).subscribe({
        next: () => {
          this.formDeleted = true; // Mark form as deleted to prevent guard dialog

          this.snackbarService.showSuccess({
            message: this.translationService.translate(
              'APP.ANIMALS.EDIT.DELETE_SUCCESS'
            ),
          });

          // Navigate back to profile
          this.router.navigate(['/profile'], {
            queryParams: { tab: 'my-animals' },
          });
        },
        error: (err) => {
          this.isDeleting = false;
          this.snackbarService.showError({
            message: this.translationService.translate(
              'APP.ANIMALS.EDIT.DELETE_ERROR'
            ),
            subMessage: this.translationService.translate(
              'APP.COMMONS.TRY_AGAIN'
            ),
          });
          this.errorHandler.handleError(err);
          this.log.logFormatted({
            message: 'Failed to delete animal',
            error: err,
            data: { animalId: this.animal?.id },
          });
          this.cdr.markForCheck();
        },
      });

      this.subscriptions.push(deleteSub);
    } catch (error) {
      this.isDeleting = false;
      this.snackbarService.showError({
        message: this.translationService.translate(
          'APP.ANIMALS.EDIT.DELETE_ERROR'
        ),
        subMessage: this.translationService.translate('APP.COMMONS.TRY_AGAIN'),
      });
      this.cdr.markForCheck();
    }
  }
}
