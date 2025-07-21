import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { UserService } from 'src/app/services/user.service';
import { AuthService } from 'src/app/services/auth.service';
import { TranslationService } from 'src/app/common/services/translation.service';
import { takeUntil, finalize } from 'rxjs/operators';
import { BehaviorSubject } from 'rxjs';
import { nameof } from 'ts-simple-nameof';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

import { User } from 'src/app/models/user/user.model';
import { Animal, AdoptionStatus } from 'src/app/models/animal/animal.model';
import { AdoptionApplication, ApplicationStatus } from 'src/app/models/adoption-application/adoption-application.model';
import { Shelter } from 'src/app/models/shelter/shelter.model';
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

  applicationStatusFilter: ApplicationStatus | 'all' = 'all';
  showPersonalMap = true;
  showShelterMap = true;
  personalMapUrl: SafeResourceUrl | null = null;
  shelterMapUrl: SafeResourceUrl | null = null;

  UserRole = UserRole;
  ApplicationStatus = ApplicationStatus;
  AdoptionStatus = AdoptionStatus;
  Gender = Gender;

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

  private readonly fileFields = [
    nameof<File>(x => x.id),
    nameof<File>(x => x.sourceUrl)
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private userService: UserService,
    private authService: AuthService,
    private translationService: TranslationService,
    private cdr: ChangeDetectorRef,
    private sanitizer: DomSanitizer
  ) {
    super();
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
  }

  loadProfile(): void {
    const profileId = this.route.snapshot.paramMap.get('id');
    
    if (!profileId) {
      this.isOwnProfile = true;
      this.loadCurrentUserProfile();
      setTimeout(() => this.setTabFromQueryParams(), 0);
      return;
    }
    
    this.isOwnProfile = false;
    this.userService.getSingle(profileId, this.getProfileFields()).pipe(
      finalize(() => {
        this.isLoading = false;
        this.cdr.markForCheck();
      })
    ).subscribe({
      next: (user: User) => {
        this.profileUser = user;
        this.setupTabs();
        this.loadTabData();
        this.setupMapUrls(user);
        setTimeout(() => this.setTabFromQueryParams(), 0);
      },
      error: (error: any) => {
        this.error = this.translationService.translate('APP.PROFILE-PAGE.ERRORS.PROFILE_LOAD_ERROR');
        console.error('Failed to load profile', error);
      }
    });
  }

  private loadCurrentUserProfile(): void {
    this.userService.getMe(this.getProfileFields()).pipe(
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
        this.setupMapUrls(user);
        setTimeout(() => this.setTabFromQueryParams(), 0);
      },
      error: (error: any) => {
        this.error = this.translationService.translate('APP.PROFILE-PAGE.ERRORS.PROFILE_LOAD_ERROR');
        console.error('Failed to load current user profile', error);
      }
    });
  }

  private getProfileFields(): string[] {
    return [
      ...this.userFields,
      ...this.shelterFields.map(field => `shelter.${field}`),
      ...this.fileFields.map(field => `profilePhoto.${field}`)
    ];
  }

  private setupMapUrls(user: User): void {
    if (user.location) {
      this.personalMapUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
        this.getGoogleMapsEmbedUrl(user.location)
      );
    }
    if (user.shelter?.user?.location) {
      this.shelterMapUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
        this.getGoogleMapsEmbedUrl(user.shelter.user.location)
      );
    }
  }

  private setupTabs(): void {
    if (!this.profileUser) return;

    const isShelter = this.profileUser.roles?.includes(UserRole.Shelter) ?? false;
    const isOwnProfile = this.isOwnProfile ?? false;

    // New merged user-info tab
    let tabs: ProfileTab[] = [
      {
        labelKey: 'APP.PROFILE-PAGE.TABS.USER_INFO',
        icon: 'lucideUser',
        component: 'user-info',
        visible: this.authService.hasPermission(Permission.BrowseUsers) || (isShelter && this.authService.hasPermission(Permission.BrowseShelters)),
        permission: isShelter ? Permission.BrowseShelters : Permission.BrowseUsers
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
        visible: isShelter
      }
    ];

    // Filter tabs based on permissions (only for tabs with a permission property)
    tabs = tabs.filter(tab => {
      if (!tab.visible) return false;
      if (tab.permission && !this.authService.hasPermission(tab.permission)) return false;
      return true;
    });

    this.tabs = tabs;
    this.tabsSubject.next(tabs);
  }

  private loadTabData(): void {
    if (!this.profileUser) return;
    this.cdr.markForCheck();
  }

  private setTabFromQueryParams(): void {
    if (!this.tabs?.length) return;
    
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
    this.router.navigate(['/adopt/edit', application.id]);
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

  reloadPage(): void {
    window.location.reload();
  }
} 