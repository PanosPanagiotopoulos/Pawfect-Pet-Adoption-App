import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { UserService } from 'src/app/services/user.service';
import { AnimalService } from 'src/app/services/animal.service';
import { AdoptionApplicationService } from 'src/app/services/adoption-application.service';
import { AuthService } from 'src/app/services/auth.service';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { LogService } from 'src/app/common/services/log.service';
import { TranslationService } from 'src/app/common/services/translation.service';
import { takeUntil, catchError, finalize } from 'rxjs/operators';
import { of, Subscription, BehaviorSubject } from 'rxjs';
import { nameof } from 'ts-simple-nameof';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

import { User } from 'src/app/models/user/user.model';
import { Animal, AdoptionStatus } from 'src/app/models/animal/animal.model';
import { AdoptionApplication, ApplicationStatus } from 'src/app/models/adoption-application/adoption-application.model';
import { Shelter } from 'src/app/models/shelter/shelter.model';
import { AnimalType } from 'src/app/models/animal-type/animal-type.model';
import { Breed } from 'src/app/models/breed/breed.model';
import { File } from 'src/app/models/file/file.model';
import { Permission } from 'src/app/common/enum/permission.enum';
import { Gender } from 'src/app/common/enum/gender';
import { UserRole } from 'src/app/common/enum/user-role.enum';

interface ProfileTab {
  labelKey: string;
  icon: string;
  component: string;
  visible: boolean;
  permission?: Permission;
}

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.css']
})
export class ProfileComponent extends BaseComponent implements OnInit, OnDestroy {
  // Profile data
  profileUser: User | null = null;
  currentUser: User | null = null;
  isOwnProfile = false;
  isLoading = true;
  error: string | null = null;

  // Tabs configuration
  private tabsSubject = new BehaviorSubject<ProfileTab[]>([]);
  tabs$ = this.tabsSubject.asObservable();
  tabs: ProfileTab[] = [];
  selectedTabIndex = 0;

  // Data for different tabs
  animalTypes: AnimalType[] = [];
  breeds: Breed[] = [];

  // Add animal form
  addAnimalForm!: FormGroup;
  isSubmittingAnimal = false;

  // Filters
  applicationStatusFilter: ApplicationStatus | 'all' = 'all';

  // Field selectors for API calls
  private readonly userFields = [
    nameof<User>(x => x.id),
    nameof<User>(x => x.fullName),
    nameof<User>(x => x.email),
    nameof<User>(x => x.phone),
    nameof<User>(x => x.location),
    nameof<User>(x => x.roles),
    nameof<User>(x => x.isVerified),
    [nameof<User>(x => x.profilePhoto), nameof<File>(x => x.sourceUrl)].join('.'),
    nameof<User>(x => x.shelter)
  ];

  private readonly shelterFields = [
    nameof<Shelter>(x => x.id),
    nameof<Shelter>(x => x.shelterName),
    nameof<Shelter>(x => x.description),
    nameof<Shelter>(x => x.website),
    nameof<Shelter>(x => x.socialMedia),
    nameof<Shelter>(x => x.operatingHours),
    [nameof<Shelter>(x => x.user), nameof<User>(x => x.location)].join('.'),
    [nameof<Shelter>(x => x.user), nameof<User>(x => x.profilePhoto), nameof<File>(x => x.sourceUrl)].join('.')
  ];

  private readonly animalFields = [
    nameof<Animal>(x => x.id),
    nameof<Animal>(x => x.name),
    nameof<Animal>(x => x.gender),
    nameof<Animal>(x => x.description),
    nameof<Animal>(x => x.adoptionStatus),
    nameof<Animal>(x => x.weight),
    nameof<Animal>(x => x.age),
    nameof<Animal>(x => x.healthStatus),
    [nameof<Animal>(x => x.attachedPhotos), nameof<File>(x => x.sourceUrl)].join('.'),
    [nameof<Animal>(x => x.attachedPhotos), nameof<File>(x => x.fileName)].join('.'),
    [nameof<Animal>(x => x.animalType), 'name'].join('.'),
    [nameof<Animal>(x => x.breed), 'name'].join('.'),
    [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.shelterName)].join('.'),
    [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.description)].join('.'),
    [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.website)].join('.'),
    [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.socialMedia)].join('.'),
    [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.operatingHours)].join('.'),
    [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.user), nameof<User>(x => x.location)].join('.'),
    [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.user), nameof<User>(x => x.profilePhoto), nameof<File>(x => x.sourceUrl)].join('.')
  ];

  private readonly animalTypeFields = [
    nameof<AnimalType>(x => x.id),
    nameof<AnimalType>(x => x.name),
  ];

  private readonly breedFields = [
    nameof<Breed>(x => x.id),
    nameof<Breed>(x => x.name),
  ];

  private readonly fileFields = [
    nameof<File>(x => x.id),
    nameof<File>(x => x.sourceUrl),
  ];

  private readonly applicationFields = [
    nameof<AdoptionApplication>(x => x.id),
    nameof<AdoptionApplication>(x => x.status),
    nameof<AdoptionApplication>(x => x.applicationDetails),
    nameof<AdoptionApplication>(x => x.createdAt),
    nameof<AdoptionApplication>(x => x.updatedAt),
    [nameof<AdoptionApplication>(x => x.attachedFiles), nameof<File>(x => x.sourceUrl)].join('.'),
    [nameof<AdoptionApplication>(x => x.attachedFiles), nameof<File>(x => x.fileName)].join('.'),
    [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.id)].join('.'),
    [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.name)].join('.'),
    [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.attachedPhotos), nameof<File>(x => x.sourceUrl)].join('.'),
    [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.animalType), 'name'].join('.'),
    [nameof<AdoptionApplication>(x => x.animal), nameof<Animal>(x => x.breed), 'name'].join('.'),
    [nameof<AdoptionApplication>(x => x.user), nameof<User>(x => x.id)].join('.'),
    [nameof<AdoptionApplication>(x => x.user), nameof<User>(x => x.fullName)].join('.'),
    [nameof<AdoptionApplication>(x => x.user), nameof<User>(x => x.profilePhoto), nameof<File>(x => x.sourceUrl)].join('.')
  ];

  UserRole = UserRole;
  ApplicationStatus = ApplicationStatus;
  AdoptionStatus = AdoptionStatus;
  Gender = Gender;

  showPersonalMap = true;
  showShelterMap = true;
  personalMapUrl: SafeResourceUrl | null = null;
  shelterMapUrl: SafeResourceUrl | null = null;

  private translationSub?: Subscription;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private animalService: AnimalService,
    private adoptionApplicationService: AdoptionApplicationService,
    private authService: AuthService,
    private errorHandler: ErrorHandlerService,
    private logService: LogService,
    private translationService: TranslationService,
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private sanitizer: DomSanitizer,
  ) {
    super();
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }


  public loadProfile(): void {
    const profileId = this.route.snapshot.paramMap.get('id');
    // If no profile ID is provided, load current user's profile
    if (!profileId) {
      this.isOwnProfile = true;
      this.loadCurrentUserProfile();
      // After profile and tabs are loaded, set tab from query param
      setTimeout(() => this.setTabFromQueryParams(), 0);
      return;
    }
    this.isOwnProfile = false;
    // Only fetch user and shelter data
    this.userService.getSingle(profileId, [
      ...this.userFields,
      ...this.shelterFields.map(field => `shelter.${field}`),
      ...this.fileFields.map(field => `profilePhoto.${field}`),
    ]).pipe(
      finalize(() => {
        this.isLoading = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: (user: User) => {
        this.profileUser = user;
        this.setupTabs();
        this.loadTabData();
        // Set map URLs for personal and shelter info tabs
        if (user.location) {
          this.personalMapUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.getGoogleMapsEmbedUrl(user.location));
        }
        if (user.shelter?.user?.location) {
          this.shelterMapUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.getGoogleMapsEmbedUrl(user.shelter.user.location));
        }
        // After tabs are set up, set tab from query param
        setTimeout(() => this.setTabFromQueryParams(), 0);
      },
      error: (error: any) => {
        this.error = this.translationService.translate('APP.PROFILE-PAGE.ERRORS.PROFILE_LOAD_ERROR');
        console.error('Failed to load profile', error);
      }
    });
  }

  private loadCurrentUserProfile(): void {
    // Only fetch user and shelter data
    const fields = [
      ...this.userFields,
      ...this.shelterFields.map(field => `shelter.${field}`),
      ...this.fileFields.map(field => `profilePhoto.${field}`),
    ];
    this.userService.getMe(fields).pipe(
      takeUntil(this._destroyed),
      finalize(() => {
        this.isLoading = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: (user: User) => {
        this.currentUser = user;
        this.profileUser = user;
        this.setupTabs();
        this.loadTabData();
        if (user.location) {
          this.personalMapUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.getGoogleMapsEmbedUrl(user.location));
        }
        if (user.shelter?.user?.location) {
          this.shelterMapUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.getGoogleMapsEmbedUrl(user.shelter.user.location));
        }
        setTimeout(() => this.setTabFromQueryParams(), 0);
      },
      error: (error: any) => {
        this.error = this.translationService.translate('APP.PROFILE-PAGE.ERRORS.PROFILE_LOAD_ERROR');
        console.error('Failed to load current user profile', error);
      }
    });
  }

  private setupTabs(): void {
    if (!this.profileUser) return;

    const isShelter = this.profileUser.roles?.includes(UserRole.Shelter) ?? false;
    const isOwnProfile = this.isOwnProfile ?? false;

    let tabs: ProfileTab[] = [
      {
        labelKey: 'APP.PROFILE-PAGE.TABS.PERSONAL_INFO',
        icon: 'lucideUser',
        component: 'personal-info',
        visible: true
      },
      {
        labelKey: 'APP.PROFILE-PAGE.TABS.SHELTER_INFO',
        icon: 'lucideBuilding',
        component: 'shelter-info',
        visible: isShelter,
        permission: Permission.BrowseShelters
      },
      {
        labelKey: 'APP.PROFILE-PAGE.TABS.ADOPTION_APPLICATIONS',
        icon: 'lucideFileText',
        component: 'adoption-applications',
        visible: isOwnProfile,
        permission: Permission.BrowseAdoptionApplications
      },
      {
        labelKey: 'APP.PROFILE-PAGE.TABS.RECEIVED_APPLICATIONS',
        icon: 'lucideInbox',
        component: 'received-applications',
        visible: isShelter && isOwnProfile,
        permission: Permission.BrowseAdoptionApplications
      },
      {
        labelKey: 'APP.PROFILE-PAGE.TABS.MY_ANIMALS',
        icon: 'lucidePawPrint',
        component: 'my-animals',
        visible: isShelter && isOwnProfile,
        permission: Permission.BrowseAnimals
      }
    ];

    // Filter tabs based on permissions
    tabs = tabs.filter(tab => {
      if (!tab.visible) return false;
      if (tab.permission && !this.hasPermission(tab.permission)) return false;
      return true;
    });

    this.tabs = tabs;
    this.tabsSubject.next(tabs);
  }

  private loadTabData(): void {
    if (!this.profileUser) return;
    // This method is now simplified.
    // All tab-specific data is fetched within the respective tab components.
    // This prevents duplicate requests and improves component encapsulation.
    this.cdr.markForCheck();
  }

  private hasPermission(permission: Permission): boolean {
    // This should check against the current user's permissions
    // For now, we'll assume the user has the permission if they're authenticated
    return !!this.currentUser;
  }

  private setTabFromQueryParams(): void {
    // Only set tab if tabs are available
    if (!this.tabs || this.tabs.length === 0) return;
    this.route.queryParams.subscribe(params => {
      const tabParam = params['tab'];
      if (tabParam) {
        const tabIndex = this.tabs.findIndex(tab => tab.component === tabParam);
        if (tabIndex !== -1) {
          this.selectedTabIndex = tabIndex;
          this.cdr.markForCheck();
          return;
        }
      }
      // Default to first tab if not found
      this.selectedTabIndex = 0;
      this.cdr.markForCheck();
    });
  }

  onTabChange(index: number): void {
    this.selectedTabIndex = index;
    const tabComponent = this.tabs[index]?.component;
    if (tabComponent) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: { tab: tabComponent },
        queryParamsHandling: 'merge',
      });
    }
  }

  onAddAnimal(): void {
    if (this.addAnimalForm?.valid && this.profileUser?.shelter?.id) {
      this.isSubmittingAnimal = true;
      
      const formValue = this.addAnimalForm.value;
      const animalData = {
        ...formValue,
        shelterId: this.profileUser.shelter.id,
        adoptionStatus: AdoptionStatus.Available
      };

      // Use proper field structure for animal creation
      const fields = [
        ...this.animalFields,
        ...this.animalTypeFields.map(field => `animalType.${field}`),
        ...this.breedFields.map(field => `breed.${field}`),
        ...this.fileFields.map(field => `attachedPhotos.${field}`),
      ];

      this.animalService.persist(animalData, fields).pipe(
        takeUntil(this._destroyed),
        finalize(() => {
          this.isSubmittingAnimal = false;
          this.cdr.markForCheck();
        })
      ).subscribe({
        next: (newAnimal: Animal) => {
          if (this.profileUser?.shelter) {
            if (!this.profileUser.shelter.animals) {
              this.profileUser.shelter.animals = [];
            }
            this.profileUser.shelter.animals.unshift(newAnimal);
          }
          this.addAnimalForm?.reset();
          console.log('Animal added successfully', { animalId: newAnimal.id });
        },
        error: (error: any) => {
          this.errorHandler.handleError(error);
        }
      });
    }
  }

  onApplicationStatusChange(status: ApplicationStatus | 'all'): void {
    this.applicationStatusFilter = status;
  }

  getFilteredApplications(): AdoptionApplication[] {
    const apps = this.profileUser?.shelter?.receivedAdoptionApplications || [];
    if (this.applicationStatusFilter === 'all') {
      return apps;
    }
    return apps.filter(app => app.status === this.applicationStatusFilter);
  }

  onViewApplication(application: AdoptionApplication): void {
    // Navigate to application details or open dialog
    this.router.navigate(['/adopt', application.animal?.id, 'edit']);
  }

  onViewAnimal(animal: Animal): void {
    // Open pet details dialog
    // This would be implemented with a dialog service
  }

  getApplicationStatusColor(status: ApplicationStatus): string {
    switch (status) {
      case ApplicationStatus.Pending:
        return 'bg-yellow-100 text-yellow-800';
      case ApplicationStatus.Available:
        return 'bg-green-100 text-green-800';
      case ApplicationStatus.Rejected:
        return 'bg-red-100 text-red-800';
      default:
        return 'bg-gray-100 text-gray-800';
    }
  }

  getGenderText(gender: Gender): string {
    return gender === Gender.Male 
      ? this.translationService.translate('APP.COMMONS.MALE')
      : this.translationService.translate('APP.COMMONS.FEMALE');
  }

  getApplicationStatusText(status: ApplicationStatus): string {
    switch (status) {
      case ApplicationStatus.Pending:
        return this.translationService.translate('APP.PROFILE-PAGE.APPLICATION_STATUS.PENDING');
      case ApplicationStatus.Available:
        return this.translationService.translate('APP.PROFILE-PAGE.APPLICATION_STATUS.APPROVED');
      case ApplicationStatus.Rejected:
        return this.translationService.translate('APP.PROFILE-PAGE.APPLICATION_STATUS.REJECTED');
      default:
        return this.translationService.translate('APP.PROFILE-PAGE.APPLICATION_STATUS.UNKNOWN');
    }
  }

  getOperatingHoursText(hours: any): string {
    if (!hours) return this.translationService.translate('APP.PROFILE-PAGE.SHELTER.NO_HOURS');
    
    const days = ['monday', 'tuesday', 'wednesday', 'thursday', 'friday', 'saturday', 'sunday'];
    const today = new Date().toLocaleDateString('en-US', { weekday: 'short' }).toLowerCase();
    const todayHours = hours[today];
    
    if (!todayHours || todayHours === 'Closed') {
      return this.translationService.translate('APP.PROFILE-PAGE.SHELTER.CLOSED_TODAY');
    }
    
    return `${this.translationService.translate('APP.PROFILE-PAGE.SHELTER.OPEN_TODAY')}: ${todayHours}`;
  }

  /**
   * Returns a Google Maps Static API URL for a given location object.
   */
  getMapImageUrl(location: any): string {
    if (!location) return '';
    const address = [location.address, location.number, location.city, location.zipCode]
      .filter(Boolean)
      .join(' ');
    const encoded = encodeURIComponent(address);
    // TODO: Replace with your actual API key
    const apiKey = 'YOUR_GOOGLE_MAPS_API_KEY';
    return `https://maps.googleapis.com/maps/api/staticmap?center=${encoded}&zoom=15&size=80x80&maptype=roadmap&markers=color:red%7C${encoded}&key=${apiKey}`;
  }

  /**
   * Handles image error event for map preview (hides the image).
   */
  hideImageOnError(event: Event): void {
    const target = event.target as HTMLImageElement | null;
    if (target) {
      target.style.display = 'none';
    }
  }

  /**
   * Safely gets a value from shelter.operatingHours by string key and formats as 'hh:mm - hh:mm' or '-' if closed.
   */
  getOperatingHourByDay(hours: any, day: string): string {
    if (!hours || typeof hours !== 'object') return '-';
    const value = hours[day];
    if (!value || value.toLowerCase() === 'closed') return '-';
    // Acceptable formats: '08:00,16:00' or '08:00 - 16:00' or '08:00-16:00'
    // Try to match 'hh:mm,hh:mm'
    const commaMatch = value.match(/^(\d{1,2}:\d{2}),(\d{1,2}:\d{2})$/);
    if (commaMatch) {
      return `${commaMatch[1]} - ${commaMatch[2]}`;
    }
    // Fallback to previous dash format
    const dashMatch = value.match(/(\d{1,2}:\d{2})\s*-\s*(\d{1,2}:\d{2})/);
    if (dashMatch) {
      return `${dashMatch[1]} - ${dashMatch[2]}`;
    }
    return value; // fallback to raw value if not matching expected format
  }

  /**
   * Returns a Google Maps embed URL for a given location object (no API key required).
   */
  getGoogleMapsEmbedUrl(location: any): string {
    if (!location) return '';
    const address = [location.address, location.number, location.city, location.zipCode]
      .filter(Boolean)
      .join(' ');
    const encoded = encodeURIComponent(address);
    return `https://www.google.com/maps?q=${encoded}&output=embed`;
  }

  togglePersonalMap(): void {
    this.showPersonalMap = !this.showPersonalMap;
    if (this.showPersonalMap && this.profileUser?.location) {
      this.personalMapUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.getGoogleMapsEmbedUrl(this.profileUser.location));
    } else {
      this.personalMapUrl = null;
    }
  }

  toggleShelterMap(): void {
    this.showShelterMap = !this.showShelterMap;
    if (this.showShelterMap && this.profileUser?.shelter?.user?.location) {
      this.shelterMapUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.getGoogleMapsEmbedUrl(this.profileUser.shelter.user.location));
    } else {
      this.shelterMapUrl = null;
    }
  }

  /**
   * Returns a Google Maps search URL for a given location object (for use in href).
   */
  getGoogleMapsLink(location: any): string {
    if (!location) return '';
    const address = [location.address, location.number, location.city, location.zipCode]
      .filter(Boolean)
      .join(' ');
    const encoded = encodeURIComponent(address);
    return `https://www.google.com/maps/search/?api=1&query=${encoded}`;
  }
} 