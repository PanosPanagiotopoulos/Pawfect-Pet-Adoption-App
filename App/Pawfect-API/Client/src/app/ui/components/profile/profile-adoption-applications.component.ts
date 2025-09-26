import {
  Component,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  OnInit,
  Input,
  OnChanges,
  ChangeDetectorRef,
  OnDestroy,
  SimpleChanges,
} from '@angular/core';
import {
  AdoptionApplication,
  ApplicationStatus,
} from 'src/app/models/adoption-application/adoption-application.model';
import { AdoptionApplicationService } from 'src/app/services/adoption-application.service';
import { AdoptionApplicationLookup } from 'src/app/lookup/adoption-application-lookup';
import { PageEvent } from '@angular/material/paginator';
import { LogService } from 'src/app/common/services/log.service';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { nameof } from 'ts-simple-nameof';
import { Animal } from 'src/app/models/animal/animal.model';
import { User } from 'src/app/models/user/user.model';
import { File } from 'src/app/models/file/file.model';
import { Breed } from 'src/app/models/breed/breed.model';
import { Shelter } from 'src/app/models/shelter/shelter.model';
import { Subscription } from 'rxjs';
import { TranslationService } from 'src/app/common/services/translation.service';
import { Permission } from 'src/app/common/enum/permission.enum';
import { AuthService } from 'src/app/services/auth.service';
import { Router } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-profile-adoption-applications',
  templateUrl: './profile-adoption-applications.component.html',
  styleUrls: ['./profile-adoption-applications.component.css'],
  animations: [
    trigger('slideDown', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-96px)' }),
        animate(
          '250ms cubic-bezier(0.4,0,0.2,1)',
          style({ opacity: 1, transform: 'translateY(0)' })
        ),
      ]),
      transition(':leave', [
        animate(
          '200ms cubic-bezier(0.4,0,0.2,1)',
          style({ opacity: 0, transform: 'translateY(-96px)' })
        ),
      ]),
    ]),
  ],
})
export class ProfileAdoptionApplicationsComponent
  implements OnInit, OnChanges, OnDestroy
{
  @Input() tabType: 'adoption-applications' | 'received-applications' =
    'adoption-applications';
  @Output() viewDetails = new EventEmitter<AdoptionApplication>();

  applications: AdoptionApplication[] = [];
  totalApplications = 0;
  isLoading = true;
  error: string | null = null;
  skeletonRows = Array(6);
  canEditApplications = false;
  filterPanelVisible = false;
  pageIndex = 0;

  lookup: AdoptionApplicationLookup = {
    offset: 0,
    pageSize: 6,
    fields: [],
    sortBy: [],
    sortDescending: true,
    status: undefined,
    query: '', // Add search query field
    searchShelters: false, // Initialize searchShelters flag
  };

  readonly sortFields = [
    {
      value: nameof<AdoptionApplication>((x) => x.createdAt),
      label: 'APP.PROFILE-PAGE.SORT.CREATED_AT',
    },
    {
      value: nameof<AdoptionApplication>((x) => x.updatedAt),
      label: 'APP.PROFILE-PAGE.SORT.UPDATED_AT',
    },
    {
      value: nameof<AdoptionApplication>((x) => x.status),
      label: 'APP.PROFILE-PAGE.SORT.STATUS',
    },
  ];

  readonly statusOptions = [
    {
      value: ApplicationStatus.Pending,
      label: 'APP.PROFILE-PAGE.APPLICATION_STATUS.PENDING',
    },
    {
      value: ApplicationStatus.Approved,
      label: 'APP.PROFILE-PAGE.APPLICATION_STATUS.APPROVED',
    },
    {
      value: ApplicationStatus.Rejected,
      label: 'APP.PROFILE-PAGE.APPLICATION_STATUS.REJECTED',
    },
  ];

  ApplicationStatus = ApplicationStatus;
  statusDropdownOpen = false;
  sortDropdownOpen = false;

  private dataSub?: Subscription;
  private translationSub?: Subscription;

  get statusFilter(): ApplicationStatus[] {
    return this.lookup.status ?? [];
  }

  get sortField(): string | undefined {
    return this.lookup.sortBy?.length ? this.lookup.sortBy[0] : undefined;
  }

  get sortDescending(): boolean {
    return !!this.lookup.sortDescending;
  }

  get pageSize(): number {
    return this.lookup.pageSize;
  }

  set pageSize(val: number) {
    this.lookup.pageSize = val;
  }

  get searchQuery(): string {
    return this.lookup.query ?? '';
  }

  set searchQuery(val: string) {
    this.lookup.query = val;
  }

  get searchLabel(): string {
    return this.tabType === 'received-applications' 
      ? 'APP.PROFILE-PAGE.ADOPTION_APPLICATIONS.SEARCH_ADOPTER_LABEL'
      : 'APP.PROFILE-PAGE.ADOPTION_APPLICATIONS.SEARCH_SHELTER_LABEL';
  }

  get searchPlaceholder(): string {
    return this.tabType === 'received-applications'
      ? 'APP.PROFILE-PAGE.ADOPTION_APPLICATIONS.SEARCH_ADOPTER_PLACEHOLDER'
      : 'APP.PROFILE-PAGE.ADOPTION_APPLICATIONS.SEARCH_SHELTER_PLACEHOLDER';
  }

  constructor(
    private adoptionApplicationService: AdoptionApplicationService,
    private log: LogService,
    private errorHandler: ErrorHandlerService,
    private cdr: ChangeDetectorRef,
    public translationService: TranslationService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit() {
    this.canEditApplications = this.authService.hasPermission(
      Permission.EditAdoptionApplications
    );
    // Initialize searchShelters flag based on initial tab type
    this.lookup.searchShelters = this.tabType === 'adoption-applications';
    this.loadApplications();
    this.translationSub = this.translationService.languageChanged$.subscribe(
      () => {
        this.cdr.markForCheck();
      }
    );
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['tabType'] && !changes['tabType'].firstChange) {
      // Update searchShelters flag when tab type changes
      this.lookup.searchShelters = this.tabType === 'adoption-applications';
      this.resetPagination();
      this.loadApplications();
    }
  }

  onPageSizeChange(event: Event) {
    const value = (event.target as HTMLSelectElement | null)?.value;
    if (value) {
      this.lookup.pageSize = +value;
      this.resetPagination();
      this.loadApplications();
    }
  }

  loadApplications(event?: PageEvent) {
    this.updatePagination(event);
    this.setLookupFields();
    this.isLoading = true;
    this.error = null;
    this.cdr.markForCheck();

    const serviceCall =
      this.tabType === 'received-applications'
        ? this.adoptionApplicationService.queryMineReceived(this.lookup)
        : this.adoptionApplicationService.queryMineRequested(this.lookup);

    this.dataSub?.unsubscribe();
    this.dataSub = serviceCall.subscribe({
      next: (result) => {
        this.applications = result.items;
        this.totalApplications = result.count;
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = 'APP.PROFILE-PAGE.ADOPTION_APPLICATIONS.LOAD_ERROR';
        this.isLoading = false;
        this.errorHandler.handleError(err);
        this.log.logFormatted({
          message: 'Failed to load adoption applications',
          error: err,
        });
        this.cdr.markForCheck();
      },
    });
  }

  private updatePagination(event?: PageEvent): void {
    if (event) {
      this.pageIndex = event.pageIndex;
      this.lookup.offset = event.pageIndex;
      this.lookup.pageSize = event.pageSize;
    }
  }

  private resetPagination(): void {
    this.pageIndex = 0;
    this.lookup.offset = 0;
  }

  private setLookupFields(): void {
    const baseFields = [
      nameof<AdoptionApplication>((x) => x.id),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.name),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.breed),
        nameof<Breed>((x) => x.name),
      ].join('.'),
      [
        nameof<AdoptionApplication>((x) => x.animal),
        nameof<Animal>((x) => x.attachedPhotos),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),
      nameof<AdoptionApplication>((x) => x.status),
      nameof<AdoptionApplication>((x) => x.createdAt),
    ];

    if (this.tabType === 'received-applications') {
      // For received applications, include user (applicant) information
      baseFields.push(
        [
          nameof<AdoptionApplication>((x) => x.user),
          nameof<User>((x) => x.fullName),
        ].join('.')
      );
      // Set searchShelters to false for received applications (searching adopters)
      this.lookup.searchShelters = false;
    } else {
      // For requested applications, include shelter information
      baseFields.push(
        [
          nameof<AdoptionApplication>((x) => x.animal),
          nameof<Animal>((x) => x.shelter),
          nameof<Shelter>((x) => x.shelterName),
        ].join('.')
      );
      // Set searchShelters to true for adoption applications (searching shelters)
      this.lookup.searchShelters = true;
    }

    this.lookup.fields = baseFields;
  }

  reloadPage(): void {
    window.location.reload();
  }

  getStatusTranslationKey(status: ApplicationStatus | undefined): string {
    switch (status) {
      case ApplicationStatus.Pending:
        return 'APP.PROFILE-PAGE.APPLICATION_STATUS.PENDING';
      case ApplicationStatus.Approved:
        return 'APP.PROFILE-PAGE.APPLICATION_STATUS.APPROVED';
      case ApplicationStatus.Rejected:
        return 'APP.PROFILE-PAGE.APPLICATION_STATUS.REJECTED';
      default:
        return 'APP.PROFILE-PAGE.APPLICATION_STATUS.UNKNOWN';
    }
  }

  getStatusChipClass(status: ApplicationStatus | undefined): string {
    switch (status) {
      case ApplicationStatus.Pending:
        return 'bg-amber-200/80 text-amber-900';
      case ApplicationStatus.Approved:
        return 'bg-green-200/80 text-green-900';
      case ApplicationStatus.Rejected:
        return 'bg-red-200/80 text-red-900';
      default:
        return 'bg-gray-200/80 text-gray-900';
    }
  }

  getStatusIcon(status: ApplicationStatus | undefined): string {
    switch (status) {
      case ApplicationStatus.Pending:
        return 'lucideClock';
      case ApplicationStatus.Approved:
        return 'lucideCheck';
      case ApplicationStatus.Rejected:
        return 'lucideX';
      default:
        return 'lucideCircleHelp';
    }
  }

  onImageError(event: Event) {
    const target = event.target as HTMLImageElement | null;
    if (target) {
      target.src = 'assets/placeholder.jpg';
    }
  }

  onRowClick(application: AdoptionApplication) {
    if (this.tabType === 'adoption-applications') {
      // For user's own applications, navigate directly to edit page
      this.router.navigate(['/adopt/edit', application.id]);
    } else if (this.canEditApplications) {
      // For received applications, emit event to parent (shelter management)
      this.viewDetails.emit(application);
    }
  }

  onEditApplication(application: AdoptionApplication) {
    if (this.tabType === 'adoption-applications') {
      // For user's own applications, navigate directly to edit page
      this.router.navigate(['/adopt/edit', application.id]);
    } else {
      // For received applications, emit event to parent (shelter management)
      this.viewDetails.emit(application);
    }
  }

  toggleFilterPanel() {
    this.filterPanelVisible = !this.filterPanelVisible;
    this.cdr.markForCheck();
  }

  toggleStatus(status: ApplicationStatus) {
    if (!this.lookup.status) this.lookup.status = [];
    if (this.lookup.status.includes(status)) {
      this.lookup.status = this.lookup.status.filter((s) => s !== status);
    } else {
      this.lookup.status = [...this.lookup.status, status];
    }
    this.resetPagination();
    this.loadApplications();
  }

  onSortFieldChange(value: string) {
    this.lookup.sortBy = value ? [value] : [];
    this.resetPagination();
    this.loadApplications();
  }

  toggleSortDirection() {
    this.lookup.sortDescending = !this.lookup.sortDescending;
    this.resetPagination();
    this.loadApplications();
  }

  onSearchQueryChange(value: string) {
    this.lookup.query = value;
    // Set searchShelters flag based on tab type
    this.lookup.searchShelters = this.tabType === 'adoption-applications';
    this.resetPagination();
    this.loadApplications();
  }

  clearFilters() {
    this.lookup.status = [];
    this.lookup.sortBy = [];
    this.lookup.sortDescending = true;
    this.lookup.query = ''; // Clear search query
    // Reset searchShelters flag based on tab type
    this.lookup.searchShelters = this.tabType === 'adoption-applications';
    this.resetPagination();
    this.loadApplications();
  }

  statusOptionsFiltered() {
    return this.statusOptions.filter((option) =>
      this.lookup.status?.includes(option.value)
    );
  }

  ngOnDestroy() {
    if (this.dataSub) {
      this.dataSub.unsubscribe();
    }
    if (this.translationSub) {
      this.translationSub.unsubscribe();
    }
  }
}
