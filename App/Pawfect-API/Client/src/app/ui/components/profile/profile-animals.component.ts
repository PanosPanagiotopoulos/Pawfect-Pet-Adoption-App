import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  OnInit,
  OnChanges,
  SimpleChanges,
  ChangeDetectorRef,
  OnDestroy,
} from '@angular/core';
import { Router } from '@angular/router';
import { Animal, AdoptionStatus } from 'src/app/models/animal/animal.model';
import { Gender } from 'src/app/common/enum/gender';
import { AnimalService } from 'src/app/services/animal.service';
import { AnimalTypeService } from 'src/app/services/animal-type.service';
import { BreedService } from 'src/app/services/breed.service';
import { AnimalLookup } from 'src/app/lookup/animal-lookup';
import { AnimalTypeLookup } from 'src/app/lookup/animal-type-lookup';
import { BreedLookup } from 'src/app/lookup/breed-lookup';
import { PageEvent } from '@angular/material/paginator';
import { LogService } from 'src/app/common/services/log.service';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { Subscription } from 'rxjs';
import { TranslationService } from 'src/app/common/services/translation.service';
import { Permission } from 'src/app/common/enum/permission.enum';
import { AuthService } from 'src/app/services/auth.service';
import { trigger, transition, style, animate } from '@angular/animations';
import { nameof } from 'ts-simple-nameof';
import { Breed } from 'src/app/models/breed/breed.model';
import { AnimalType } from 'src/app/models/animal-type/animal-type.model';
import { File } from 'src/app/models/file/file.model';
import { Shelter } from 'src/app/models/shelter/shelter.model';

@Component({
  selector: 'app-profile-animals',
  templateUrl: './profile-animals.component.html',
  styleUrls: ['./profile-animals.component.scss'],
  animations: [
    trigger('slideDown', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-40px)' }),
        animate(
          '400ms cubic-bezier(0.4,0,0.2,1)',
          style({ opacity: 1, transform: 'translateY(0)' })
        ),
      ]),
      transition(':leave', [
        animate(
          '300ms cubic-bezier(0.4,0,0.2,1)',
          style({ opacity: 0, transform: 'translateY(-40px)' })
        ),
      ]),
    ]),
  ],
})
export class ProfileAnimalsComponent implements OnInit, OnChanges, OnDestroy {
  @Input() viewingAnimalsShelterId: string | null = null;
  @Output() addAnimal = new EventEmitter<void>();

  currentUserShelterId: string | null = null;
  animals: Animal[] = [];
  totalAnimals = 0;
  pageSize = 6;
  pageIndex = 0;
  isLoading = false;
  error: string | null = null;
  skeletonRows = Array(6);
  canEditAnimals = false;
  filterPanelVisible = false;
  adoptionStatusDropdownOpen = false;
  genderDropdownOpen = false;
  animalTypeDropdownOpen = false;
  breedDropdownOpen = false;

  // Filter data
  animalTypes: AnimalType[] = [];
  filteredBreeds: Breed[] = [];
  isLoadingAnimalTypes = false;
  isLoadingBreeds = false;

  lookup: AnimalLookup = {
    offset: 0,
    pageSize: 6,
    fields: [
      nameof<Animal>(x => x.id),
      nameof<Animal>(x => x.name),
      [nameof<Animal>(x => x.breed), nameof<Breed>(x => x.name)].join('.'),
      [nameof<Animal>(x => x.attachedPhotos), nameof<File>(x => x.sourceUrl)].join('.'),
      nameof<Animal>(x => x.adoptionStatus),
      nameof<Animal>(x => x.age),
      nameof<Animal>(x => x.gender),
      [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.id)].join('.'),
      [nameof<Animal>(x => x.animalType), nameof<AnimalType>(x => x.id)].join('.'),
      [nameof<Animal>(x => x.animalType), nameof<AnimalType>(x => x.name)].join('.'),
    ],
    sortBy: [],
    sortDescending: true,
    adoptionStatuses: [],
    genders: [],
    animalTypeIds: [],
    breedIds: [],
    ageFrom: undefined,
    ageTo: undefined,
    query: '',
    shelterIds: [],
    useVectorSearch: true,
    useSemanticSearch: true,
  };

  readonly adoptionStatusOptions = [
    {
      value: AdoptionStatus.Available,
      label: 'APP.PROFILE-PAGE.ADOPTION_STATUS.AVAILABLE',
    },
    {
      value: AdoptionStatus.Adopted,
      label: 'APP.PROFILE-PAGE.ADOPTION_STATUS.ADOPTED',
    },
  ];

  readonly genderOptions = [
    { value: Gender.Male, label: 'APP.COMMONS.MALE' },
    { value: Gender.Female, label: 'APP.COMMONS.FEMALE' },
  ];

  AdoptionStatus = AdoptionStatus;

  private dataSub?: Subscription;
  private translationSub?: Subscription;

  get adoptionStatusFilter(): AdoptionStatus[] {
    return this.lookup.adoptionStatuses ?? [];
  }

  get genderFilter(): Gender[] {
    return this.lookup.genders ?? [];
  }

  get animalTypeFilter(): string[] {
    return this.lookup.animalTypeIds ?? [];
  }

  get breedFilter(): string[] {
    return this.lookup.breedIds ?? [];
  }

  get ageFrom(): number | undefined {
    return this.lookup.ageFrom;
  }

  get ageTo(): number | undefined {
    return this.lookup.ageTo;
  }

  get searchQuery(): string {
    return this.lookup.query ?? '';
  }

  get sortDescending(): boolean {
    return !!this.lookup.sortDescending;
  }

  get pageSizeValue(): number {
    return this.lookup.pageSize;
  }

  set pageSizeValue(val: number) {
    this.lookup.pageSize = val;
  }

  constructor(
    private animalService: AnimalService,
    private animalTypeService: AnimalTypeService,
    private breedService: BreedService,
    private log: LogService,
    private errorHandler: ErrorHandlerService,
    private cdr: ChangeDetectorRef,
    public translationService: TranslationService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    if (this.viewingAnimalsShelterId) {
      this.currentUserShelterId = this.authService.getUserShelterId();
      this.lookup.shelterIds = [this.viewingAnimalsShelterId];
      this.loadInitialData();
      this.loadAnimals();
    }
    this.translationSub = this.translationService.languageChanged$.subscribe(
      () => {
        this.cdr.markForCheck();
      }
    );
  }

  ngOnChanges(changes: SimpleChanges) {
    if (
      changes['shelterId'] &&
      !changes['shelterId'].firstChange &&
      changes['shelterId'].currentValue
    ) {
      this.resetPagination();
      this.loadAnimals();
    }
  }

  ngOnDestroy() {
    this.translationSub?.unsubscribe();
    this.dataSub?.unsubscribe();
  }

  loadInitialData(): void {
    this.isLoadingAnimalTypes = true;
    this.isLoadingBreeds = true;
    this.cdr.markForCheck();

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

    const breedsQuery: BreedLookup = {
      ids: undefined,
      typeIds: undefined, // Load all breeds regardless of animal type
      createdFrom: undefined,
      createdTill: undefined,
      offset: 0,
      pageSize: 1000,
      fields: [nameof<Breed>((x) => x.id), nameof<Breed>((x) => x.name)],
      sortBy: [],
    };

    const loadSub = this.animalTypeService.query(animalTypesQuery).subscribe({
      next: (data) => {
        this.animalTypes = data.items;
        this.isLoadingAnimalTypes = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoadingAnimalTypes = false;
        this.errorHandler.handleError(err);
        this.log.logFormatted({
          message: 'Failed to load animal types',
          error: err,
        });
        this.cdr.markForCheck();
      },
    });

    const breedsSub = this.breedService.query(breedsQuery).subscribe({
      next: (breedsResult) => {
        this.filteredBreeds = breedsResult.items;
        this.isLoadingBreeds = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoadingBreeds = false;
        this.errorHandler.handleError(err);
        this.log.logFormatted({
          message: 'Failed to load breeds',
          error: err,
        });
        this.cdr.markForCheck();
      },
    });
  }

  loadBreedsForAnimalTypes(animalTypeIds: string[]): void {
    // This method is no longer needed since we load all breeds upfront
    // But keeping it for backward compatibility
    if (!animalTypeIds || animalTypeIds.length === 0) {
      // Don't clear breeds - keep all available
      return;
    }

    // Filter breeds based on selected animal types if any are selected
    if (animalTypeIds.length > 0) {
      this.filteredBreeds = this.filteredBreeds.filter(breed => 
        breed.animalType?.id && animalTypeIds.includes(breed.animalType.id)
      );
    }
    
    this.cdr.markForCheck();
  }

  loadAnimals(event?: PageEvent) {
    if (!this.viewingAnimalsShelterId) return;

    this.dataSub?.unsubscribe();
    this.updatePagination(event);
    this.isLoading = true;
    this.error = null;
    this.cdr.markForCheck();

    this.lookup.shelterIds = [this.viewingAnimalsShelterId];

    this.dataSub = this.animalService.query(this.lookup).subscribe({
      next: (data) => {
        this.animals = data.items;
        this.totalAnimals = data.count;
        this.isLoading = false;
        this.cdr.markForCheck();

        this.canEditAnimals = this.authService.hasPermission(Permission.EditAnimals) ||
                          (!!this.currentUserShelterId && this.animals.filter(a => a.shelter?.id === this.currentUserShelterId).length == this.animals.length);
      },
      error: (err) => {
        this.error = 'APP.PROFILE-PAGE.ANIMALS.LOAD_ERROR';
        this.isLoading = false;
        this.errorHandler.handleError(err);
        this.log.logFormatted({
          message: 'Failed to load shelter animals',
          error: err,
        });
        this.cdr.markForCheck();
      },
    });
  }

  private updatePagination(event?: PageEvent): void {
    if (event) {
      this.lookup.offset = event.pageIndex;
      this.pageIndex = event.pageIndex;
      this.lookup.pageSize = event.pageSize;
    }
  }

  private resetPagination(): void {
    this.pageIndex = 0;
    this.lookup.offset = 0;
  }

  reloadPage(): void {
    window.location.reload();
  }

  onImageError(event: Event) {
    const target = event.target as HTMLImageElement | null;
    if (target) {
      target.src = 'assets/placeholder.jpg';
    }
  }

  onCardClick(animal: Animal) {
    // Always go to view page when clicking the card/row
    this.router.navigate(['/animals/view', animal.id]);
  }

  onEditClick(animal: Animal, event: Event) {
    // Stop event propagation to prevent card click
    event.stopPropagation();
    this.router.navigate(['/animals/edit', animal.id]);
  }

  onAddAnimalClick() {
    // Navigate to add animals page
    this.router.navigate(['/animals/new']);
  }

  canAddAnimals(): boolean {
    // Only allow adding animals if user can edit animals and has a shelter
    return this.canEditAnimals && this.authService.hasPermission(Permission.CreateAnimals);
  }

  onPageSizeChange(event: Event) {
    const value = (event.target as HTMLSelectElement | null)?.value;
    if (value) {
      this.pageSize = +value;
      this.pageIndex = 0;
      this.loadAnimals();
    }
  }

  getStatusChipClass(status: AdoptionStatus | undefined): string {
    switch (status) {
      case AdoptionStatus.Available:
        return 'bg-green-200/80 text-green-900';
      case AdoptionStatus.Adopted:
        return 'bg-blue-200/80 text-blue-900';
      default:
        return 'bg-gray-200/80 text-gray-900';
    }
  }

  getStatusIcon(status: AdoptionStatus | undefined): string {
    switch (status) {
      case AdoptionStatus.Available:
        return 'lucideCheck';
      case AdoptionStatus.Adopted:
        return 'lucideHeart';
      default:
        return 'lucideCircleHelp';
    }
  }

  getAdoptionStatusTranslationKey(status: AdoptionStatus | undefined): string {
    switch (status) {
      case AdoptionStatus.Available:
        return 'APP.PROFILE-PAGE.ADOPTION_STATUS.AVAILABLE';
      case AdoptionStatus.Adopted:
        return 'APP.PROFILE-PAGE.ADOPTION_STATUS.ADOPTED';
      default:
        return 'APP.PROFILE-PAGE.ADOPTION_STATUS.UNKNOWN';
    }
  }

  getGenderTranslationKey(gender: Gender | undefined): string {
    switch (gender) {
      case Gender.Male:
        return 'APP.COMMONS.MALE';
      case Gender.Female:
        return 'APP.COMMONS.FEMALE';
      default:
        return 'APP.COMMONS.UNKNOWN';
    }
  }

  getAnimalTypeLabel(value: string): string {
    const animalType = this.animalTypes.find((type) => type.id === value);
    return animalType?.name || '';
  }

  getBreedLabel(value: string): string {
    const breed = this.filteredBreeds.find((b) => b.id === value);
    return breed?.name || '';
  }

  // Filter panel methods
  toggleFilterPanel() {
    this.filterPanelVisible = !this.filterPanelVisible;
    this.cdr.markForCheck();
  }

  toggleAdoptionStatus(status: AdoptionStatus) {
    if (!this.lookup.adoptionStatuses) this.lookup.adoptionStatuses = [];
    if (this.lookup.adoptionStatuses.includes(status)) {
      this.lookup.adoptionStatuses = this.lookup.adoptionStatuses.filter(
        (s) => s !== status
      );
    } else {
      this.lookup.adoptionStatuses = [...this.lookup.adoptionStatuses, status];
    }
    this.resetPagination();
    this.loadAnimals();
  }

  toggleGender(gender: Gender) {
    if (!this.lookup.genders) this.lookup.genders = [];
    if (this.lookup.genders.includes(gender)) {
      this.lookup.genders = this.lookup.genders.filter((g) => g !== gender);
    } else {
      this.lookup.genders = [...this.lookup.genders, gender];
    }
    this.resetPagination();
    this.loadAnimals();
  }

  toggleAnimalType(animalTypeId: string) {
    if (!this.lookup.animalTypeIds) this.lookup.animalTypeIds = [];
    if (this.lookup.animalTypeIds.includes(animalTypeId)) {
      this.lookup.animalTypeIds = this.lookup.animalTypeIds.filter(
        (id) => id !== animalTypeId
      );
    } else {
      this.lookup.animalTypeIds = [...this.lookup.animalTypeIds, animalTypeId];
    }
    
    // No need to update breeds when animal types change since all breeds are loaded
    // But we can optionally filter breeds based on selected animal types
    if (this.lookup.animalTypeIds.length > 0) {
      this.loadBreedsForAnimalTypes(this.lookup.animalTypeIds);
    }
    
    this.resetPagination();
    this.loadAnimals();
  }

  toggleBreed(breedId: string) {
    if (!this.lookup.breedIds) this.lookup.breedIds = [];
    if (this.lookup.breedIds.includes(breedId)) {
      this.lookup.breedIds = this.lookup.breedIds.filter(
        (id) => id !== breedId
      );
    } else {
      this.lookup.breedIds = [...this.lookup.breedIds, breedId];
    }
    this.resetPagination();
    this.loadAnimals();
  }

  onAgeFromChange(value: string) {
    this.lookup.ageFrom = value ? Math.max(1, +value) : undefined;
    this.resetPagination();
    this.loadAnimals();
  }

  onAgeToChange(value: string) {
    this.lookup.ageTo = value ? Math.max(1, +value) : undefined;
    this.resetPagination();
    this.loadAnimals();
  }

  onSearchQueryChange(value: string) {
    this.lookup.query = value;
    this.resetPagination();
    this.loadAnimals();
  }

  clearFilters() {
    this.lookup.adoptionStatuses = [];
    this.lookup.genders = [];
    this.lookup.animalTypeIds = [];
    this.lookup.breedIds = [];
    this.lookup.ageFrom = undefined;
    this.lookup.ageTo = undefined;
    this.lookup.query = '';
    this.filteredBreeds = [];
    this.resetPagination();
    this.loadAnimals();
  }
}
