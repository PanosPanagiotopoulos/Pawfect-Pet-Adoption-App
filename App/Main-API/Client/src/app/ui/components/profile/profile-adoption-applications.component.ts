import { Component, Output, EventEmitter, ChangeDetectionStrategy, OnInit, Input, SimpleChanges, OnChanges, ChangeDetectorRef } from '@angular/core';
import { AdoptionApplication, ApplicationStatus } from 'src/app/models/adoption-application/adoption-application.model';
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
import { Subscription, Subject, of } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { TranslationService } from 'src/app/common/services/translation.service';

@Component({
  selector: 'app-profile-adoption-applications',
  templateUrl: './profile-adoption-applications.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileAdoptionApplicationsComponent implements OnInit, OnChanges {
  @Input() tabType: 'adoption-applications' | 'received-applications' = 'adoption-applications';
  applications: AdoptionApplication[] = [];
  totalApplications = 0;
  pageSize = 6;
  pageIndex = 0;
  isLoading = true;
  error: string | null = null;
  skeletonRows = Array(6);

  private lastLoadedTabType: string | null = null;
  private lastLoadedPageIndex: number | null = null;
  private lastLoadedPageSize: number | null = null;
  private dataSub?: Subscription;
  private translationSub?: Subscription;

  @Output() viewDetails = new EventEmitter<AdoptionApplication>();

  public ApplicationStatus = ApplicationStatus;

  constructor(
    private adoptionApplicationService: AdoptionApplicationService,
    private log: LogService,
    private errorHandler: ErrorHandlerService,
    private cdr: ChangeDetectorRef,
    public translationService: TranslationService
  ) {}

  ngOnInit() {
    this.loadApplications();
    this.translationSub = this.translationService.languageChanged$.subscribe(() => {
      this.cdr.markForCheck();
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['tabType'] && !changes['tabType'].firstChange) {
      this.pageIndex = 0;
      this.loadApplications();
    }
  }

  onPageSizeChange(event: Event) {
    const value = (event.target as HTMLSelectElement | null)?.value;
    if (value) {
      this.pageSize = +value;
      this.pageIndex = 0;
      this.loadApplications();
    }
  }

  loadApplications(event?: PageEvent) {
    const nextPageIndex = event ? event.pageIndex : this.pageIndex;
    const nextPageSize = event ? event.pageSize : this.pageSize;
    if (
      this.lastLoadedTabType === this.tabType &&
      this.lastLoadedPageIndex === nextPageIndex &&
      this.lastLoadedPageSize === nextPageSize && !this.error
    ) {
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
      this.pageSize = event.pageSize;
    }
    const fields = [
      nameof<AdoptionApplication>(x => x.id),
      [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.name)].join('.'),
      [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.breed), nameof<Breed>(x => x.name)].join('.'),
      [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.attachedPhotos), nameof<File>(x => x.sourceUrl)].join('.'),
      [nameof<AdoptionApplication>(x => x.user), nameof<User>(x => x.fullName)].join('.'),
      nameof<AdoptionApplication>(x => x.status),
      nameof<AdoptionApplication>(x => x.createdAt),
    ];
    const lookup: AdoptionApplicationLookup = {
      offset: this.pageIndex * this.pageSize,
      pageSize: this.pageSize,
      fields,
      sortBy: [nameof<AdoptionApplication>(x => x.createdAt)],
      sortDescending: true,
    };
    let serviceCall;
    if (this.tabType === 'received-applications') {
      serviceCall = this.adoptionApplicationService.queryMineReceived(lookup);
    } else {
      serviceCall = this.adoptionApplicationService.queryMineRequested(lookup);
    }
    this.dataSub = serviceCall.subscribe({
      next: (result) => {
        this.applications = result.items;
        this.totalApplications = result.count;
        this.isLoading = false;
        this.lastLoadedTabType = this.tabType;
        this.lastLoadedPageIndex = this.pageIndex;
        this.lastLoadedPageSize = this.pageSize;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = 'APP.PROFILE-PAGE.ADOPTION_APPLICATIONS.LOAD_ERROR';
        this.isLoading = false;
        this.errorHandler.handleError(err);
        this.log.logFormatted({ message: 'Failed to load adoption applications', error: err });
        this.cdr.markForCheck();
      }
    });
  }

  getStatusTranslationKey(status: ApplicationStatus | undefined): string {
    switch (status) {
      case ApplicationStatus.Pending:
        return 'APP.PROFILE-PAGE.APPLICATION_STATUS.PENDING';
      case ApplicationStatus.Available:
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
      case ApplicationStatus.Available:
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
      case ApplicationStatus.Available:
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
    this.viewDetails.emit(application);
  }

  onEditApplication(application: AdoptionApplication) {
    // TODO: Implement navigation to the edit page or open edit dialog for the adoption application
    // Example: this.router.navigate(['/adoption-applications', application.id, 'edit']);
    this.viewDetails.emit(application); // fallback to existing details logic for now
  }

  ngOnDestroy() {
    this.translationSub?.unsubscribe();
    if (this.dataSub) {
      this.dataSub.unsubscribe();
    }
  }
} 