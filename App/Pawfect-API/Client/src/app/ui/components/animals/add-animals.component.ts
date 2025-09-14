import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectorRef,
  HostListener,
} from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription, forkJoin } from 'rxjs';
import { trigger, transition, style, animate } from '@angular/animations';

import { AnimalService } from 'src/app/services/animal.service';
import { AnimalTypeService } from 'src/app/services/animal-type.service';
import { BreedService } from 'src/app/services/breed.service';
import { TranslationService } from 'src/app/common/services/translation.service';
import { LogService } from 'src/app/common/services/log.service';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { SnackbarService } from 'src/app/common/services/snackbar.service';
import { MatDialog } from '@angular/material/dialog';
import { ExcelImportDialogComponent } from '../../../common/ui/excel-import-dialog.component';

import {
  AnimalPersist,
  AdoptionStatus,
} from 'src/app/models/animal/animal.model';
import { AnimalType } from 'src/app/models/animal-type/animal-type.model';
import { Breed } from 'src/app/models/breed/breed.model';
import { Gender } from 'src/app/common/enum/gender';
import { FileItem } from 'src/app/models/file/file.model';

import { FileToUrlPipe } from 'src/app/common/tools/file-to-url.pipe';
import { AnimalTypeLookup } from 'src/app/lookup/animal-type-lookup';
import { nameof } from 'ts-simple-nameof';
import { BreedLookup } from 'src/app/lookup/breed-lookup';
import { CanComponentDeactivate } from 'src/app/common/guards/form.guard';
import * as saveAs from 'file-saver';
import { HttpResponse } from '@angular/common/http';

interface AnimalFormData {
  id: string;
  form: FormGroup;
  isCollapsed: boolean;
  title: string;
  dropdownStates: {
    gender: boolean;
    adoptionStatus: boolean;
    animalType: boolean;
    breed: boolean;
  };
}

@Component({
  selector: 'app-add-animals',
  standalone: false,
  templateUrl: './add-animals.component.html',
  styleUrls: ['./add-animals.component.scss'],
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
    trigger('slideDown', [
      transition(':enter', [
        style({ height: 0, opacity: 0 }),
        animate('0.3s ease-out', style({ height: '*', opacity: 1 })),
      ]),
      transition(':leave', [
        animate('0.3s ease-in', style({ height: 0, opacity: 0 })),
      ]),
    ]),
  ],
})
export class AddAnimalsComponent
  implements OnInit, OnDestroy, CanComponentDeactivate
{
  animalForms: AnimalFormData[] = [];
  currentFormIndex = 0;
  isLoading = false;
  isSaving = false;
  isLoadingBreeds = false;
  isUploadingFiles = false;
  isLoadingExcelAnimals = false;
  error: string | null = null;

  animalTypes: AnimalType[] = [];
  filteredBreeds: Breed[] = [];

  // Image preview slider state
  currentImageIndex: { [formId: string]: number } = {};
  formImageFiles: { [formId: string]: FileItem[] } = {};
  slideDirection: { [formId: string]: 'left' | 'right' | 'none' } = {};

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
      value: AdoptionStatus.Adopted,
      label: 'APP.ANIMALS.ADOPTION_STATUS.ADOPTED',
    },
  ];

  private subscriptions: Subscription[] = [];

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private animalService: AnimalService,
    private animalTypeService: AnimalTypeService,
    private breedService: BreedService,
    private translationService: TranslationService,
    private log: LogService,
    private errorHandler: ErrorHandlerService,
    private snackbarService: SnackbarService,
    private cdr: ChangeDetectorRef,
    private dialog: MatDialog
  ) {}

  ngOnInit(): void {
    this.loadInitialData();
    this.addNewAnimalForm();
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((sub) => sub.unsubscribe());

    // Clean up file URLs to prevent memory leaks
    Object.values(this.formImageFiles).forEach((files) => {
      const fileObjects = files.map((item) => item.file);
      FileToUrlPipe.revokeUrls(fileObjects);
    });
    
    // Ensure body scrolling is re-enabled if component is destroyed
    document.body.classList.remove('no-scroll');
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
        this.filteredBreeds = []; // Start with empty breeds until animal type is selected
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading = false;
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
          Validators.maxLength(1000),
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
          Validators.maxLength(500),
        ],
      ],
      animalTypeId: [null, Validators.required],
      breedId: [null, Validators.required],
      attachedPhotosIds: [[]],
      adoptionStatus: [AdoptionStatus.Available, Validators.required],
    });
  }

  addNewAnimalForm(): void {
    const newId = `animal-${Date.now()}-${Math.random()
      .toString(36)
      .substr(2, 9)}`;
    const newForm = this.createAnimalForm();

    const formData: AnimalFormData = {
      id: newId,
      form: newForm,
      isCollapsed: false,
      title: 'APP.ANIMALS.ADD.NEW_ANIMAL_TITLE', // Store the translation key instead of translated text
      dropdownStates: {
        gender: false,
        adoptionStatus: false,
        animalType: false,
        breed: false,
      },
    };

    // Collapse all existing forms
    this.animalForms.forEach((form) => (form.isCollapsed = true));

    // Add new form
    this.animalForms.push(formData);
    this.currentFormIndex = this.animalForms.length - 1;

    this.cdr.markForCheck();
  }

  toggleFormCollapse(index: number): void {
    const form = this.animalForms[index];
    form.isCollapsed = !form.isCollapsed;

    if (!form.isCollapsed) {
      this.currentFormIndex = index;
      // Collapse other forms to show only one at a time
      this.animalForms.forEach((f, i) => {
        if (i !== index) f.isCollapsed = true;
      });
    }

    this.cdr.markForCheck();
  }

  removeAnimalForm(index: number): void {
    if (this.animalForms.length <= 1) return;

    this.animalForms.splice(index, 1);

    // Adjust current form index
    if (this.currentFormIndex >= this.animalForms.length) {
      this.currentFormIndex = this.animalForms.length - 1;
    }

    // Ensure at least one form is expanded
    if (
      this.animalForms.length > 0 &&
      this.animalForms.every((f) => f.isCollapsed)
    ) {
      this.animalForms[this.currentFormIndex].isCollapsed = false;
    }

    this.cdr.markForCheck();
  }

  onAnimalTypeChange(formIndex: number, animalTypeId: string): void {
    const form = this.animalForms[formIndex].form;
    form.patchValue({ animalTypeId, breedId: null });

    // Query breeds filtered by the selected animal type
    this.loadBreedsForAnimalType(animalTypeId);

    this.cdr.markForCheck();
  }

  private loadBreedsForAnimalType(animalTypeId: string): void {
    this.isLoadingBreeds = true;
    this.filteredBreeds = []; // Clear current breeds while loading

    const breedsQuery: BreedLookup = {
      ids: undefined,
      typeIds: [animalTypeId], // Filter by selected animal type
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

  onFilesChange(formIndex: number, files: FileItem[]): void {
    const form = this.animalForms[formIndex].form;
    const formData = this.animalForms[formIndex];

    const fileIds = files
      .filter((file) => file.persistedId)
      .map((file) => file.persistedId!);

    form.patchValue({ attachedPhotosIds: fileIds });

    // Store files for image preview
    this.formImageFiles[formData.id] = files.filter((file) =>
      file.file.type.startsWith('image/')
    );

    // Reset image slider index when files change
    this.currentImageIndex[formData.id] = 0;

    this.cdr.markForCheck();
  }

  onUploadStateChange(isUploading: boolean): void {
    this.isUploadingFiles = isUploading;
    this.cdr.markForCheck();
  }

  // Image preview slider methods
  getImageFiles(formData: AnimalFormData): FileItem[] {
    return this.formImageFiles[formData.id] || [];
  }

  getCurrentImageIndex(formId: string): number {
    return this.currentImageIndex[formId] || 0;
  }

  getCurrentImageFile(formData: AnimalFormData): File | null {
    const files = this.getImageFiles(formData);
    const index = this.getCurrentImageIndex(formData.id);
    return files[index]?.file || null;
  }

  nextImage(formId: string, totalImages: number): void {
    if (totalImages <= 0) return;
    const current = this.getCurrentImageIndex(formId);
    this.slideDirection[formId] = 'left';
    this.currentImageIndex[formId] = (current + 1) % totalImages;
    this.cdr.markForCheck();

    // Reset slide direction after animation
    setTimeout(() => {
      this.slideDirection[formId] = 'none';
      this.cdr.markForCheck();
    }, 300);
  }

  previousImage(formId: string, totalImages: number): void {
    if (totalImages <= 0) return;
    const current = this.getCurrentImageIndex(formId);
    this.slideDirection[formId] = 'right';
    this.currentImageIndex[formId] =
      current === 0 ? totalImages - 1 : current - 1;
    this.cdr.markForCheck();

    // Reset slide direction after animation
    setTimeout(() => {
      this.slideDirection[formId] = 'none';
      this.cdr.markForCheck();
    }, 300);
  }

  goToImage(formId: string, index: number): void {
    if (index < 0) return;
    const current = this.getCurrentImageIndex(formId);

    // Determine slide direction based on index change
    if (index > current) {
      this.slideDirection[formId] = 'left';
    } else if (index < current) {
      this.slideDirection[formId] = 'right';
    } else {
      this.slideDirection[formId] = 'none';
    }

    this.currentImageIndex[formId] = index;
    this.cdr.markForCheck();

    // Reset slide direction after animation
    setTimeout(() => {
      this.slideDirection[formId] = 'none';
      this.cdr.markForCheck();
    }, 300);
  }

  getSlideDirection(formId: string): string {
    return this.slideDirection[formId] || 'none';
  }

  trackByError(index: number, error: string): string {
    return error;
  }

  getValidFormsCount(): number {
    return this.animalForms.filter(
      (formData) => this.getFormValidationSummary(formData.form).valid
    ).length;
  }

  getFormTitle(formData: AnimalFormData): string {
    const nameValue = formData.form.get('name')?.value;
    if (nameValue && nameValue.trim()) {
      return nameValue.trim();
    }
    // Use translation service to get current translation
    return this.translationService.translate(formData.title);
  }

  getFormValidationSummary(form: FormGroup): {
    valid: boolean;
    errors: number;
  } {
    const errors = Object.keys(form.controls).filter(
      (key) =>
        form.get(key)?.invalid &&
        (form.get(key)?.touched || form.get(key)?.dirty)
    ).length;

    return {
      valid: form.valid && form.touched,
      errors,
    };
  }

  getFormValidationErrors(form: FormGroup): string[] {
    const errors: string[] = [];

    Object.keys(form.controls).forEach((key) => {
      const control = form.get(key);
      if (control?.invalid && (control?.touched || control?.dirty)) {
        const fieldTranslationKey = this.getFieldTranslationKey(key);

        if (control.errors?.['required']) {
          errors.push(
            `APP.ANIMALS.ADD.VALIDATION.${key.toUpperCase()}_REQUIRED`
          );
        }
        if (control.errors?.['minlength']) {
          const requiredLength = control.errors['minlength'].requiredLength;
          errors.push(
            `APP.ANIMALS.ADD.VALIDATION.${key.toUpperCase()}_MIN_LENGTH`
          );
        }
        if (control.errors?.['maxlength']) {
          const maxLength = control.errors['maxlength'].requiredLength;
          errors.push(
            `APP.ANIMALS.ADD.VALIDATION.${key.toUpperCase()}_MAX_LENGTH`
          );
        }
        if (control.errors?.['min']) {
          const min = control.errors['min'].min;
          errors.push(
            `APP.ANIMALS.ADD.VALIDATION.${key.toUpperCase()}_MIN_VALUE`
          );
        }
        if (control.errors?.['max']) {
          const max = control.errors['max'].max;
          errors.push(
            `APP.ANIMALS.ADD.VALIDATION.${key.toUpperCase()}_MAX_VALUE`
          );
        }
      }
    });

    return errors;
  }

  private getFieldTranslationKey(fieldName: string): string {
    const fieldMap: { [key: string]: string } = {
      name: 'APP.ANIMALS.ADD.NAME',
      age: 'APP.ANIMALS.ADD.AGE',
      gender: 'APP.ANIMALS.ADD.GENDER',
      description: 'APP.ANIMALS.ADD.DESCRIPTION',
      weight: 'APP.ANIMALS.ADD.WEIGHT',
      healthStatus: 'APP.ANIMALS.ADD.HEALTH_STATUS',
      animalTypeId: 'APP.ANIMALS.ADD.ANIMAL_TYPE',
      breedId: 'APP.ANIMALS.ADD.BREED',
      attachedPhotosIds: 'APP.ANIMALS.ADD.PHOTOS',
      adoptionStatus: 'APP.ANIMALS.ADD.ADOPTION_STATUS',
    };

    return fieldMap[fieldName] || fieldName;
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

  async saveAnimals(): Promise<void> {
    if (this.isSaving || this.isUploadingFiles) return;

    // Validate all forms
    const invalidForms = this.animalForms.filter((formData) => {
      this.markFormGroupTouched(formData.form);
      return formData.form.invalid;
    });

    if (invalidForms.length > 0) {
      this.snackbarService.showError({
        message: this.translationService
          .translate('APP.ANIMALS.ADD.VALIDATION_ERROR')
          .replace('{count}', invalidForms.length.toString()),
        subMessage: this.translationService.translate(
          'APP.ANIMALS.ADD.FIX_ERRORS_MESSAGE'
        ),
      });

      // Expand first invalid form
      invalidForms[0].isCollapsed = false;
      this.currentFormIndex = this.animalForms.indexOf(invalidForms[0]);

      this.cdr.markForCheck();
      return;
    }

    this.isSaving = true;
    this.cdr.markForCheck();

    try {
      const animalsToSave: AnimalPersist[] = this.animalForms.map(
        (formData) => {
          const formValue = formData.form.value;
          return {
            id: formData.id,
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
        }
      );

      const saveSub = this.animalService.persistBatch(animalsToSave).subscribe({
        next: (savedAnimals) => {
          this.log.logFormatted({
            message: 'Successfully saved animals',
            data: { count: savedAnimals.length },
          });

          this.snackbarService.showSuccess({
            message: this.translationService.translate(
              'APP.ANIMALS.ADD.SAVE_SUCCESS'
            ),
            subMessage: this.translationService
              .translate('APP.ANIMALS.ADD.ANIMALS_SAVED_COUNT')
              .replace('{count}', savedAnimals.length.toString()),
          });

          // Navigate back to profile
          this.router.navigate(['/profile'], {
            queryParams: { tab: 'my-animals' },
          });
        },
        error: (err) => {
          this.isSaving = false;
          this.snackbarService.showError({
            message: this.translationService.translate(
              'APP.ANIMALS.ADD.SAVE_ERROR'
            ),
            subMessage: this.translationService.translate(
              'APP.COMMONS.TRY_AGAIN'
            ),
          });
          this.errorHandler.handleError(err);
          this.log.logFormatted({
            message: 'Failed to save animals',
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
          'APP.ANIMALS.ADD.SAVE_ERROR'
        ),
        subMessage: this.translationService.translate('APP.COMMONS.TRY_AGAIN'),
      });
      this.cdr.markForCheck();
    }
  }

  downloadTemplate(): void {
    this.animalService.getImportExcel().subscribe({
      next: (response: HttpResponse<Blob>) => {
        const contentDisposition = response.headers.get('Content-Disposition');
        let filename = 'template.xlsx';

        if (contentDisposition) {
          let match = contentDisposition.match(/filename\*=([^']*)''([^;]+)/);
          if (match && match[2]) {
            filename = decodeURIComponent(match[2]);
          } else {
            match = contentDisposition.match(/filename="?([^"]+)"?/);
            if (match && match[1]) {
              filename = match[1];
            }
          }
        }

        saveAs(response.body!, filename);
      },
      error: (err) => {
      },
    });
  }

  cancel(): void {
    this.router.navigate(['/profile'], {
      queryParams: { tab: 'my-animals' },
    });
  }

  openExcelImportDialog(): void {
    // Disable body scrolling using the same approach as form-guard
    document.body.classList.add('no-scroll');
    
    const dialogRef = this.dialog.open(ExcelImportDialogComponent, {
      width: '600px',
      maxWidth: '90vw',
      maxHeight: '90vh',
      disableClose: true,
      hasBackdrop: true,
      backdropClass: 'excel-import-backdrop',
      panelClass: 'excel-import-panel',
      autoFocus: false,
      restoreFocus: false,
    });

    dialogRef.afterClosed().subscribe({
      next: (result: AnimalPersist[] | null) => {
        // Re-enable body scrolling
        document.body.classList.remove('no-scroll');
        
        if (result && result.length > 0) {
          this.isLoadingExcelAnimals = true;
          this.cdr.markForCheck();
          
          // Add a small delay to show loading state
          setTimeout(() => {
            this.prefillFormsFromExcel(result);
            this.isLoadingExcelAnimals = false;
            this.snackbarService.showSuccess({
              message: this.translationService
                .translate('APP.ANIMALS.EXCEL_IMPORT.SUCCESS_MESSAGE')
                .replace('{count}', result.length.toString()),
            });
            this.cdr.markForCheck();
          }, 500);
        }
      },
      error: () => {
        // Ensure body scrolling is re-enabled even on error
        document.body.classList.remove('no-scroll');
      }
    });
  }

  private prefillFormsFromExcel(animals: AnimalPersist[]): void {
    // Clear existing forms
    this.animalForms = [];
    this.currentFormIndex = 0;

    // Create forms for each imported animal
    animals.forEach((animal, index) => {
      const newId = `animal-${Date.now()}-${index}`;
      const newForm = this.createAnimalForm();

      // Prefill form with animal data
      newForm.patchValue({
        name: animal.name || '',
        age: animal.age || null,
        gender: animal.gender || null,
        description: animal.description || '',
        weight: animal.weight || null,
        healthStatus: animal.healthStatus || '',
        animalTypeId: animal.animalTypeId || null,
        breedId: animal.breedId || null,
        attachedPhotosIds: animal.attachedPhotosIds || [],
        adoptionStatus: animal.adoptionStatus || AdoptionStatus.Available,
      });

      // Mark form as touched to show validation
      newForm.markAsTouched();

      const formData: AnimalFormData = {
        id: newId,
        form: newForm,
        isCollapsed: index !== 0, // Expand first form, collapse others
        title: animal.name || 'APP.ANIMALS.ADD.NEW_ANIMAL_TITLE',
        dropdownStates: {
          gender: false,
          adoptionStatus: false,
          animalType: false,
          breed: false,
        },
      };

      this.animalForms.push(formData);
    });

    // Load breeds for the first animal if it has an animal type
    if (this.animalForms.length > 0) {
      const firstForm = this.animalForms[0];
      const animalTypeId = firstForm.form.get('animalTypeId')?.value;
      if (animalTypeId) {
        this.loadBreedsForAnimalType(animalTypeId);
      }
    }

    this.cdr.markForCheck();
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

  // Get translation key for gender options (for template use)
  getGenderTranslationKey(value: Gender): string {
    const option = this.genderOptions.find((opt) => opt.value === value);
    return option ? option.label : '';
  }

  // Get translation key for adoption status options (for template use)
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

  // Helper methods for dropdown management
  toggleDropdown(
    formIndex: number,
    dropdownType: keyof AnimalFormData['dropdownStates']
  ): void {
    const formData = this.animalForms[formIndex];

    // Close all other dropdowns for this form
    Object.keys(formData.dropdownStates).forEach((key) => {
      if (key !== dropdownType) {
        formData.dropdownStates[key as keyof AnimalFormData['dropdownStates']] =
          false;
      }
    });

    // Toggle the requested dropdown
    formData.dropdownStates[dropdownType] =
      !formData.dropdownStates[dropdownType];
    this.cdr.markForCheck();
  }

  closeDropdown(
    formIndex: number,
    dropdownType: keyof AnimalFormData['dropdownStates']
  ): void {
    this.animalForms[formIndex].dropdownStates[dropdownType] = false;
    this.cdr.markForCheck();
  }

  // Close dropdowns when clicking outside
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;

    // Close all dropdowns if clicking outside
    if (!target.closest('.relative')) {
      this.animalForms.forEach((formData) => {
        Object.keys(formData.dropdownStates).forEach((key) => {
          formData.dropdownStates[
            key as keyof AnimalFormData['dropdownStates']
          ] = false;
        });
      });
      this.cdr.markForCheck();
    }
  }

  // CanComponentDeactivate implementation
  canDeactivate(): boolean {
    return !this.hasUnsavedChanges();
  }

  hasUnsavedChanges(): boolean {
    // Don't show guard dialog if files are currently uploading
    if (this.isUploadingFiles) {
      return false;
    }

    // Check if any form has meaningful changes (only dirty, not just touched)
    return this.animalForms.some(
      (formData) => formData.form.dirty
    );
  }
}
