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
  @Input() shelterId: string | null = null;
  @Output() addAnimal = new EventEmitter<void>();

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

  lookup: AnimalLookup = {
    offset: 0,
    pageSize: 6,
    fields: [
      'id',
      'name',
      'breed.name',
      'attachedPhotos.sourceUrl',
      'adoptionStatus',
      'age',
      'gender',
      'shelter.id',
    ],
    sortBy: [],
    sortDescending: true,
    adoptionStatuses: [],
    genders: [],
    ageFrom: undefined,
    ageTo: undefined,
    query: '',
    shelterIds: [],
  };

  readonly adoptionStatusOptions = [
    {
      value: AdoptionStatus.Available,
      label: 'APP.PROFILE-PAGE.ADOPTION_STATUS.AVAILABLE',
    },
    {
      value: AdoptionStatus.Pending,
      label: 'APP.PROFILE-PAGE.ADOPTION_STATUS.PENDING',
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
    private log: LogService,
    private errorHandler: ErrorHandlerService,
    private cdr: ChangeDetectorRef,
    public translationService: TranslationService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.canEditAnimals = this.authService.hasPermission(
      Permission.EditAnimals
    );
    if (this.shelterId) {
      this.lookup.shelterIds = [this.shelterId];
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

  loadAnimals(event?: PageEvent) {
    if (!this.shelterId) return;

    this.dataSub?.unsubscribe();
    this.updatePagination(event);
    this.isLoading = true;
    this.error = null;
    this.cdr.markForCheck();

    this.lookup.shelterIds = [this.shelterId];

    this.dataSub = this.animalService.query(this.lookup).subscribe({
      next: (data) => {
        this.animals = data.items;
        this.totalAnimals = data.count;
        this.isLoading = false;
        this.cdr.markForCheck();
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
      this.pageIndex = event.pageIndex;
      this.lookup.pageSize = event.pageSize;
    }
    this.lookup.offset = this.pageIndex * this.lookup.pageSize;
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

  isOwnerOfAnimal(animal: Animal): boolean {
    // Check if the current shelter ID matches the animal's shelter ID
    return this.shelterId === animal.shelter?.id;
  }

  onAddAnimalClick() {
    // Navigate to add animals page
    this.router.navigate(['/animals/new']);
  }

  canAddAnimals(): boolean {
    // Only allow adding animals if user can edit animals and has a shelter
    return this.canEditAnimals && !!this.shelterId;
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
    this.lookup.ageFrom = undefined;
    this.lookup.ageTo = undefined;
    this.lookup.query = '';
    this.resetPagination();
    this.loadAnimals();
  }
}
