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
  OnDestroy
} from '@angular/core';
import { Animal, AdoptionStatus } from 'src/app/models/animal/animal.model';
import { Gender } from 'src/app/common/enum/gender';
import { AnimalService } from 'src/app/services/animal.service';
import { AnimalLookup } from 'src/app/lookup/animal-lookup';
import { PageEvent } from '@angular/material/paginator';
import { LogService } from 'src/app/common/services/log.service';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { Subscription } from 'rxjs';
import { TranslationService } from 'src/app/common/services/translation.service';
import { Permission } from 'src/app/common/enum/permission.enum';
import { AuthService } from 'src/app/services/auth.service';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-profile-animals',
  templateUrl: './profile-animals.component.html',
  styleUrls: ['./profile-animals.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    trigger('slideDown', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-40px)' }),
        animate('400ms cubic-bezier(0.4,0,0.2,1)', style({ opacity: 1, transform: 'translateY(0)' }))
      ]),
      transition(':leave', [
        animate('300ms cubic-bezier(0.4,0,0.2,1)', style({ opacity: 0, transform: 'translateY(-40px)' }))
      ])
    ])
  ]
})
export class ProfileAnimalsComponent implements OnInit, OnChanges, OnDestroy {
  @Input() shelterId: string | null = null;
  @Output() viewDetails = new EventEmitter<Animal>();
  @Output() addAnimal = new EventEmitter<void>();

  animals: Animal[] = [];
  totalAnimals = 0;
  pageSize = 6;
  pageIndex = 0;
  isLoading = false;
  error: string | null = null;
  skeletonRows = Array(6);
  canEditAnimals = false;

  // Filter panel state and lookup
  lookup: AnimalLookup = {
    offset: 0,
    pageSize: 6,
    fields: ['id', 'name', 'breed.name', 'attachedPhotos.sourceUrl', 'adoptionStatus', 'age', 'gender'],
    sortBy: [],
    sortDescending: true,
    adoptionStatuses: [],
    genders: [],
    ageFrom: undefined,
    ageTo: undefined,
    query: '',
    shelterIds: [],
  };
  filterPanelVisible = false;
  adoptionStatusDropdownOpen = false;
  genderDropdownOpen = false;
  // Filter options
  readonly adoptionStatusOptions = [
    { value: AdoptionStatus.Available, label: 'APP.PROFILE-PAGE.ADOPTION_STATUS.AVAILABLE' },
    { value: AdoptionStatus.Pending, label: 'APP.PROFILE-PAGE.ADOPTION_STATUS.PENDING' },
    { value: AdoptionStatus.Adopted, label: 'APP.PROFILE-PAGE.ADOPTION_STATUS.ADOPTED' },
  ];
  readonly genderOptions = [
    { value: Gender.Male, label: 'APP.COMMONS.MALE' },
    { value: Gender.Female, label: 'APP.COMMONS.FEMALE' },
  ];
  // Getters for template compatibility
  get adoptionStatusFilter(): AdoptionStatus[] {
    return this.lookup.adoptionStatuses ?? [];
  }
  get genderFilter(): Gender[] {
    return this.lookup.genders ?? [];
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

  private dataSub?: Subscription;
  private translationSub?: Subscription;

  constructor(
    private animalService: AnimalService,
    private log: LogService,
    private errorHandler: ErrorHandlerService,
    private cdr: ChangeDetectorRef,
    public translationService: TranslationService,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.canEditAnimals = this.authService.hasPermission(Permission.EditAnimals);
    if (this.shelterId) {
      this.lookup.shelterIds = [this.shelterId];
      this.loadAnimals();
    }
    // Subscribe to language changes to trigger change detection for imperative translations
    this.translationSub = this.translationService.languageChanged$.subscribe(() => {
      this.cdr.markForCheck();
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['shelterId'] && !changes['shelterId'].firstChange && changes['shelterId'].currentValue) {
      this.pageIndex = 0;
      this.loadAnimals();
    }
  }

  ngOnDestroy() {
    this.translationSub?.unsubscribe();
    if (this.dataSub) {
      this.dataSub.unsubscribe();
    }
  }

  loadAnimals(event?: PageEvent) {
    if (!this.shelterId) {
      return;
    }

    if (this.dataSub) {
      this.dataSub.unsubscribe();
    }

    this.isLoading = true;
    this.error = null;
    this.cdr.markForCheck();

    if (event) {
      this.pageIndex = event.pageIndex;
      this.lookup.pageSize = event.pageSize;
    }
    this.lookup.offset = this.pageIndex * this.lookup.pageSize;
    this.lookup.shelterIds = [this.shelterId];

    this.dataSub = this.animalService.query(this.lookup).subscribe({
      next: (data) => {
        this.animals = data.items;
        // Use the count from the API for totalAnimals
        this.totalAnimals = data.count;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = 'APP.PROFILE-PAGE.ANIMALS.LOAD_ERROR';
        this.isLoading = false;
        this.errorHandler.handleError(err);
        this.log.logFormatted({ message: 'Failed to load shelter animals', error: err });
        this.cdr.markForCheck();
      }
    });
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
    if (this.canEditAnimals) {
      this.viewDetails.emit(animal);
    }
  }

  onAddAnimalClick() {
    this.addAnimal.emit();
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
      case AdoptionStatus.Pending:
        return 'bg-amber-200/80 text-amber-900';
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
      case AdoptionStatus.Pending:
        return 'lucideClock';
      case AdoptionStatus.Adopted:
        return 'lucideHeart';
      default:
        return 'lucideCircleHelp';
    }
  }

  public AdoptionStatus = AdoptionStatus;

  getAdoptionStatusTranslationKey(status: AdoptionStatus | undefined): string {
    switch (status) {
      case AdoptionStatus.Available:
        return 'APP.PROFILE-PAGE.ADOPTION_STATUS.AVAILABLE';
      case AdoptionStatus.Pending:
        return 'APP.PROFILE-PAGE.ADOPTION_STATUS.PENDING';
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

  // Filter panel methods
  toggleFilterPanel() {
    this.filterPanelVisible = !this.filterPanelVisible;
    this.cdr.markForCheck();
  }
  toggleAdoptionStatus(status: AdoptionStatus) {
    if (!this.lookup.adoptionStatuses) this.lookup.adoptionStatuses = [];
    if (this.lookup.adoptionStatuses.includes(status)) {
      this.lookup.adoptionStatuses = this.lookup.adoptionStatuses.filter(s => s !== status);
    } else {
      this.lookup.adoptionStatuses = [...this.lookup.adoptionStatuses, status];
    }
    this.pageIndex = 0;
    this.lookup.offset = 0;
    this.loadAnimals();
  }
  toggleGender(gender: Gender) {
    if (!this.lookup.genders) this.lookup.genders = [];
    if (this.lookup.genders.includes(gender)) {
      this.lookup.genders = this.lookup.genders.filter(g => g !== gender);
    } else {
      this.lookup.genders = [...this.lookup.genders, gender];
    }
    this.pageIndex = 0;
    this.lookup.offset = 0;
    this.loadAnimals();
  }
  onAgeFromChange(value: string) {
    const num = value ? Math.max(1, +value) : undefined;
    this.lookup.ageFrom = num;
    this.pageIndex = 0;
    this.lookup.offset = 0;
    this.loadAnimals();
  }
  onAgeToChange(value: string) {
    const num = value ? Math.max(1, +value) : undefined;
    this.lookup.ageTo = num;
    this.pageIndex = 0;
    this.lookup.offset = 0;
    this.loadAnimals();
  }
  onSearchQueryChange(value: string) {
    this.lookup.query = value;
    this.pageIndex = 0;
    this.lookup.offset = 0;
    this.loadAnimals();
  }
  clearFilters() {
    this.lookup.adoptionStatuses = [];
    this.lookup.genders = [];
    this.lookup.ageFrom = undefined;
    this.lookup.ageTo = undefined;
    this.lookup.query = '';
    this.pageIndex = 0;
    this.lookup.offset = 0;
    this.loadAnimals();
  }
} 