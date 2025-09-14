import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import {
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { UserService } from 'src/app/services/user.service';
import { ShelterService } from 'src/app/services/shelter.service';
import { AuthService } from 'src/app/services/auth.service';
import { TranslationService } from 'src/app/common/services/translation.service';
import { SnackbarService } from 'src/app/common/services/snackbar.service';
import { takeUntil, finalize } from 'rxjs/operators';
import { BehaviorSubject } from 'rxjs';
import { nameof } from 'ts-simple-nameof';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { CustomValidators } from 'src/app/ui/components/auth/validators/custom.validators';
import { CanComponentDeactivate } from 'src/app/common/guards/form.guard';
import {
  ProfilePhotoDialogComponent,
  ProfilePhotoDialogData,
  ProfilePhotoDialogResult,
} from './profile-photo-dialog.component';

import { User, UserPersist, UserUpdate } from 'src/app/models/user/user.model';
import { Animal, AdoptionStatus } from 'src/app/models/animal/animal.model';
import {
  AdoptionApplication,
  ApplicationStatus,
} from 'src/app/models/adoption-application/adoption-application.model';
import { Shelter, ShelterPersist } from 'src/app/models/shelter/shelter.model';
import { File, FileItem } from 'src/app/models/file/file.model';
import { Permission } from 'src/app/common/enum/permission.enum';
import { Gender } from 'src/app/common/enum/gender';
import { UserRole } from 'src/app/common/enum/user-role.enum';
import { AuthProvider } from 'src/app/common/enum/auth-provider.enum';
import { VerificationStatus } from 'src/app/common/enum/verification-status';

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
  styleUrls: ['./profile.component.css'],
})
export class ProfileComponent
  extends BaseComponent
  implements OnInit, OnDestroy, CanComponentDeactivate
{
  // Profile data
  profileUser: User | null = null;
  currentUser: User | null = null;
  isOwnProfile = false;
  isLoading = true;
  error: string | null = null;

  // Edit state
  isEditMode = false;
  isSaving = false;
  editForm: any = null;
  private formSaved = false;

  // Operating hours properties (from shelter-info component)
  days = [
    'Δευτέρα',
    'Τρίτη',
    'Τετάρτη',
    'Πέμπτη',
    'Παρασκευή',
    'Σάββατο',
    'Κυριακή',
  ];

  closedDays: { [key: string]: boolean } = {};
  openTimes: { [key: string]: string } = {};
  closeTimes: { [key: string]: string } = {};
  timeErrors: { [key: string]: string | null } = {};
  operatingHoursModified: boolean = false;

  // Profile photo dialog data
  profilePhotoPreview: string | null = null;
  profilePhotoFile: File | null = null;
  localProfilePhotoPreview: string | null = null; // For local preview before save

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
    nameof<User>((x) => x.id),
    nameof<User>((x) => x.fullName),
    nameof<User>((x) => x.email),
    nameof<User>((x) => x.phone),
    nameof<User>((x) => x.location),
    nameof<User>((x) => x.roles),
    nameof<User>((x) => x.isVerified),
    [
      nameof<User>((x) => x.profilePhoto),
      nameof<File>((x) => x.sourceUrl),
    ].join('.'),
    nameof<User>((x) => x.shelter),
  ];

  private readonly shelterFields = [
    nameof<Shelter>((x) => x.id),
    nameof<Shelter>((x) => x.shelterName),
    nameof<Shelter>((x) => x.description),
    nameof<Shelter>((x) => x.website),
    nameof<Shelter>((x) => x.socialMedia),
    nameof<Shelter>((x) => x.operatingHours),
    [nameof<Shelter>((x) => x.user), nameof<User>((x) => x.location)].join('.'),
    [
      nameof<Shelter>((x) => x.user),
      nameof<User>((x) => x.profilePhoto),
      nameof<File>((x) => x.sourceUrl),
    ].join('.'),
  ];

  private readonly fileFields = [
    nameof<File>((x) => x.id),
    nameof<File>((x) => x.sourceUrl),
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private userService: UserService,
    private shelterService: ShelterService,
    private authService: AuthService,
    private translationService: TranslationService,
    private snackbarService: SnackbarService,
    private cdr: ChangeDetectorRef,
    private sanitizer: DomSanitizer,
    private dialog: MatDialog
  ) {
    super();

    // Initialize operating hours properties
    this.days.forEach((day) => {
      this.closedDays[day] = false;
      this.openTimes[day] = '';
      this.closeTimes[day] = '';
      this.timeErrors[day] = null;
    });
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    // Clean up local profile photo preview to prevent memory leaks
    this.clearLocalProfilePhotoPreview();
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
    this.userService
      .getSingle(profileId, this.getProfileFields())
      .pipe(
        finalize(() => {
          this.isLoading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (user: User) => {
          this.profileUser = user;
          this.setupTabs();
          this.loadTabData();
          this.setupMapUrls(user);
          setTimeout(() => this.setTabFromQueryParams(), 0);
        },
        error: (error: any) => {
          this.error = this.translationService.translate(
            'APP.PROFILE-PAGE.ERRORS.PROFILE_LOAD_ERROR'
          );
        },
      });
  }

  private loadCurrentUserProfile(): void {
    this.userService
      .getMe(this.getProfileFields())
      .pipe(
        takeUntil(this._destroyed),
        finalize(() => {
          this.isLoading = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (user: User) => {
          this.currentUser = user;
          this.profileUser = user;
          this.setupTabs();
          this.loadTabData();
          this.setupMapUrls(user);
          setTimeout(() => this.setTabFromQueryParams(), 0);
        },
        error: (error: any) => {
          this.error = this.translationService.translate(
            'APP.PROFILE-PAGE.ERRORS.PROFILE_LOAD_ERROR'
          );
        },
      });
  }

  private getProfileFields(): string[] {
    return [
      ...this.userFields,
      ...this.shelterFields.map((field) => `shelter.${field}`),
      ...this.fileFields.map((field) => `profilePhoto.${field}`),
    ];
  }

  private getUpdateFields(): string[] {
    // Fields needed for update operations to get complete data back
    return [
      nameof<User>((x) => x.id),
      nameof<User>((x) => x.fullName),
      nameof<User>((x) => x.email),
      nameof<User>((x) => x.phone),
      nameof<User>((x) => x.location),
      nameof<User>((x) => x.roles),
      nameof<User>((x) => x.isVerified),
      [nameof<User>((x) => x.profilePhoto), nameof<File>((x) => x.id)].join(
        '.'
      ),
      [
        nameof<User>((x) => x.profilePhoto),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),
    ];
  }

  private getShelterUpdateFields(): string[] {
    // Fields needed for shelter update operations to get complete data back
    return [
      nameof<Shelter>((x) => x.id),
      nameof<Shelter>((x) => x.shelterName),
      nameof<Shelter>((x) => x.description),
      nameof<Shelter>((x) => x.website),
      nameof<Shelter>((x) => x.socialMedia),
      nameof<Shelter>((x) => x.operatingHours),
      nameof<Shelter>((x) => x.verificationStatus),
      nameof<Shelter>((x) => x.verifiedBy),
      [nameof<Shelter>((x) => x.user), nameof<User>((x) => x.id)].join('.'),
      [nameof<Shelter>((x) => x.user), nameof<User>((x) => x.location)].join(
        '.'
      ),
      [
        nameof<Shelter>((x) => x.user),
        nameof<User>((x) => x.profilePhoto),
        nameof<File>((x) => x.id),
      ].join('.'),
      [
        nameof<Shelter>((x) => x.user),
        nameof<User>((x) => x.profilePhoto),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),
    ];
  }

  private setupMapUrls(user: User | null): void {
    if (user?.location) {
      this.personalMapUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
        this.getGoogleMapsEmbedUrl(user.location)
      );
    }
    if (user?.shelter?.user?.location) {
      this.shelterMapUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
        this.getGoogleMapsEmbedUrl(user.shelter.user.location)
      );
    }
  }

  private setupTabs(): void {
    if (!this.profileUser) return;

    const isShelter =
      this.profileUser.roles?.includes(UserRole.Shelter) ?? false;
    const isOwnProfile = this.isOwnProfile ?? false;

    // New merged user-info tab
    let tabs: ProfileTab[] = [
      {
        labelKey: 'APP.PROFILE-PAGE.TABS.USER_INFO',
        icon: 'lucideUser',
        component: 'user-info',
        visible:
          this.authService.hasPermission(Permission.BrowseUsers) ||
          (isShelter &&
            this.authService.hasPermission(Permission.BrowseShelters)),
        permission: isShelter
          ? Permission.BrowseShelters
          : Permission.BrowseUsers,
      },
      {
        labelKey: 'APP.PROFILE-PAGE.TABS.ADOPTION_APPLICATIONS',
        icon: 'lucideFileText',
        component: 'adoption-applications',
        visible: isOwnProfile && !isShelter, // Hide from shelters since they cannot create adoption applications
        permission: Permission.BrowseAdoptionApplications,
      },
      {
        labelKey: 'APP.PROFILE-PAGE.TABS.RECEIVED_APPLICATIONS',
        icon: 'lucideInbox',
        component: 'received-applications',
        visible: isShelter && isOwnProfile,
        permission: Permission.BrowseAdoptionApplications,
      },
      {
        labelKey: 'APP.PROFILE-PAGE.TABS.MY_ANIMALS',
        icon: 'lucidePawPrint',
        component: 'my-animals',
        visible: isShelter,
      },
    ];

    // Filter tabs based on permissions (only for tabs with a permission property)
    tabs = tabs.filter((tab) => {
      if (!tab.visible) return false;
      if (tab.permission && !this.authService.hasPermission(tab.permission))
        return false;
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

    this.route.queryParams.subscribe((params) => {
      const tabParam = params['tab'];
      if (tabParam) {
        const tabIndex = this.tabs.findIndex(
          (tab) => tab.component === tabParam
        );
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
    return apps.filter((app) => app.status === this.applicationStatusFilter);
  }

  onViewApplication(application: AdoptionApplication): void {
    // Navigate to application details or open dialog
    this.router.navigate(['/adopt/edit', application.id]);
  }

  getApplicationStatusColor(status: ApplicationStatus): string {
    switch (status) {
      case ApplicationStatus.Pending:
        return 'bg-yellow-100 text-yellow-800';
      case ApplicationStatus.Approved:
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

  // Edit functionality
  toggleEditMode(): void {
    if (this.isEditMode) {
      this.cancelEdit();
    } else {
      this.enterEditMode();
    }
  }

  private enterEditMode(): void {
    if (!this.profileUser || !this.isOwnProfile) return;

    this.isEditMode = true;
    this.formSaved = false; // Reset form saved state when entering edit mode
    this.createEditForm();
    this.initializeOperatingHours();
    this.cdr.markForCheck();
  }

  private initializeOperatingHours(): void {
    if (!this.profileUser?.shelter) return;

    this.days.forEach((day) => {
      const dayKey = this.getDayKey(day);
      const value =
        this.profileUser?.shelter?.operatingHours?.[
          dayKey as keyof typeof this.profileUser.shelter.operatingHours
        ];

      if (value === 'closed') {
        this.closedDays[day] = true;
        this.operatingHoursModified = true;
      } else if (value && value.includes(',')) {
        const [open, close] = value.split(',');
        if (open) {
          this.openTimes[day] = open;
          this.operatingHoursModified = true;
        }
        if (close) {
          this.closeTimes[day] = close;
          this.operatingHoursModified = true;
        }
      } else {
        this.openTimes[day] = '';
        this.closeTimes[day] = '';
      }
    });
  }

  private initializeOperatingHoursDisplay(): void {
    if (!this.profileUser?.shelter) return;

    // Reset operating hours display state
    this.days.forEach((day) => {
      this.closedDays[day] = false;
      this.openTimes[day] = '';
      this.closeTimes[day] = '';
      this.timeErrors[day] = null;
    });

    this.operatingHoursModified = false;
  }

  private createEditForm(): void {
    if (!this.profileUser) return;

    const isShelter =
      this.profileUser.roles?.includes(UserRole.Shelter) ?? false;

    this.editForm = this.fb.group({
      // Profile photo for personal info
      profilePhoto: [this.profileUser.profilePhoto?.id || null],
      deletePhoto: [false], // Flag to indicate if photo should be deleted
      // Shelter info (if user is a shelter)
      ...(isShelter && this.profileUser.shelter
        ? {
            shelter: this.fb.group({
              shelterName: [
                this.profileUser.shelter.shelterName || '',
                [Validators.required, Validators.minLength(3)],
              ],
              website: [
                this.profileUser.shelter.website || '',
                [Validators.pattern(/^https?:\/\/.+\..+$/)],
              ],
              description: [
                this.profileUser.shelter.description || '',
                [Validators.required, Validators.minLength(10)],
              ],
              socialMedia: this.fb.group({
                facebook: [
                  this.profileUser.shelter.socialMedia?.facebook || '',
                  [this.createOptionalSocialMediaValidator('facebook')],
                ],
                instagram: [
                  this.profileUser.shelter.socialMedia?.instagram || '',
                  [this.createOptionalSocialMediaValidator('instagram')],
                ],
              }),
              operatingHours: this.fb.group({
                monday: [
                  this.profileUser.shelter.operatingHours?.monday || '',
                  [CustomValidators.operatingHoursValidator()],
                ],
                tuesday: [
                  this.profileUser.shelter.operatingHours?.tuesday || '',
                  [CustomValidators.operatingHoursValidator()],
                ],
                wednesday: [
                  this.profileUser.shelter.operatingHours?.wednesday || '',
                  [CustomValidators.operatingHoursValidator()],
                ],
                thursday: [
                  this.profileUser.shelter.operatingHours?.thursday || '',
                  [CustomValidators.operatingHoursValidator()],
                ],
                friday: [
                  this.profileUser.shelter.operatingHours?.friday || '',
                  [CustomValidators.operatingHoursValidator()],
                ],
                saturday: [
                  this.profileUser.shelter.operatingHours?.saturday || '',
                  [CustomValidators.operatingHoursValidator()],
                ],
                sunday: [
                  this.profileUser.shelter.operatingHours?.sunday || '',
                  [CustomValidators.operatingHoursValidator()],
                ],
              }),
            }),
          }
        : {}),
    });
  }

  private createOptionalSocialMediaValidator(
    platform: 'facebook' | 'instagram'
  ): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      // If no value is provided, it's valid (optional field)
      if (!control.value || control.value.trim() === '') {
        return null;
      }

      // If value is provided, validate it contains the platform name
      const containsPlatform = control.value.toLowerCase().includes(platform);
      return containsPlatform ? null : { invalidSocialMedia: true };
    };
  }

  cancelEdit(): void {
    this.isEditMode = false;
    this.editForm = null;
    this.formSaved = false; // Reset form saved state
    // Clean up local profile photo preview
    this.clearLocalProfilePhotoPreview();
    this.cdr.markForCheck();
  }

  async saveChanges(): Promise<void> {
    if (!this.editForm || !this.profileUser || this.isSaving) return;

    // Mark all fields as touched to show validation errors
    this.markFormGroupTouched(this.editForm);

    // Validate operating hours if shelter has any hours set
    const isShelter =
      this.profileUser.roles?.includes(UserRole.Shelter) ?? false;
    if (isShelter && this.hasAnyOperatingHoursSet()) {
      const hasInvalidHours = this.validateOperatingHours();
      if (hasInvalidHours) {
        this.snackbarService.showError({
          message: this.translationService.translate(
            'APP.PROFILE-PAGE.EDIT.VALIDATION_ERROR'
          ),
          subMessage: this.translationService.translate(
            'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.ALL_DAYS_REQUIRED'
          ),
        });
        return;
      }
    }

    // Check if form is valid
    if (!this.editForm.valid) {
      this.snackbarService.showError({
        message: this.translationService.translate(
          'APP.PROFILE-PAGE.EDIT.VALIDATION_ERROR'
        ),
        subMessage: this.translationService.translate(
          'APP.COMMONS.CHECK_FORM_ERRORS'
        ),
      });
      return;
    }

    this.isSaving = true;
    this.cdr.markForCheck();

    try {
      const isShelter =
        this.profileUser.roles?.includes(UserRole.Shelter) ?? false;

      // Handle profile photo changes
      const profilePhotoId = this.editForm.get('profilePhoto')?.value;
      const shouldDeletePhoto = this.editForm.get('deletePhoto')?.value;

      // Only proceed if we have the required user data
      if (!this.profileUser.id) {
        this.snackbarService.showError({
          message: this.translationService.translate(
            'APP.PROFILE-PAGE.EDIT.UPDATE_ERROR'
          ),
        });
        return;
      }

      // Prepare promises for simultaneous execution
      const promises: Promise<any>[] = [];

      // Handle profile photo update if needed
      if (profilePhotoId || shouldDeletePhoto) {
        const userUpdate: UserUpdate = {
          id: this.profileUser.id,
          fullName: this.profileUser.fullName || '',
          email: this.profileUser.email || '',
          phone: this.profileUser.phone || '',
          profilePhotoId: shouldDeletePhoto ? undefined : profilePhotoId,
        };

        promises.push(
          this.userService
            .update(userUpdate, this.getUpdateFields())
            .toPromise()
        );
      }

      // Handle shelter changes (if user is a shelter)
      if (
        isShelter &&
        this.profileUser.shelter &&
        this.editForm.get('shelter')
      ) {
        const shelterForm = this.editForm.get('shelter');
        const socialMediaForm = shelterForm?.get('socialMedia');
        const operatingHoursForm = shelterForm?.get('operatingHours');

        const shelterPersist: ShelterPersist = {
          id: this.profileUser.shelter.id!,
          userId: this.profileUser.id!,
          shelterName:
            shelterForm?.get('shelterName')?.value ||
            this.profileUser.shelter.shelterName!,
          description:
            shelterForm?.get('description')?.value ||
            this.profileUser.shelter.description!,
          website: shelterForm?.get('website')?.value || null,
          socialMedia: this.getSocialMediaPayload(socialMediaForm?.value),
          operatingHours: this.getOperatingHoursPayload(
            operatingHoursForm?.value
          ),
          verificationStatus:
            this.profileUser.shelter.verificationStatus ||
            VerificationStatus.Pending,
          verifiedBy: this.profileUser.shelter.verifiedBy,
        };

        promises.push(
          this.shelterService
            .persist(shelterPersist, this.getShelterUpdateFields())
            .toPromise()
        );
      }

      // Execute all promises simultaneously
      if (promises.length > 0) {
        const results = await Promise.all(promises);

        // Update local data based on results
        let resultIndex = 0;

        // Handle user update result
        if (profilePhotoId || shouldDeletePhoto) {
          const updatedUser = results[resultIndex++];
          if (updatedUser) {
            // Update user data with response from server
            this.profileUser = {
              ...this.profileUser,
              ...updatedUser,
              // Preserve nested objects that might not be fully returned
              location: updatedUser.location || this.profileUser.location,
              shelter: this.profileUser.shelter, // Keep existing shelter data
            };

            // Update current user if this is own profile
            if (this.isOwnProfile) {
              this.currentUser = this.profileUser;
            }
          }
        }

        // Handle shelter update result
        if (
          isShelter &&
          this.profileUser?.shelter &&
          this.editForm.get('shelter')
        ) {
          const updatedShelter = results[resultIndex++];
          if (updatedShelter && this.profileUser?.shelter) {
            // Update shelter data with response from server
            this.profileUser.shelter = {
              ...this.profileUser.shelter,
              ...updatedShelter,
              // Ensure user reference is maintained
              user: updatedShelter.user || this.profileUser.shelter.user,
            };
          }
        }
      }

      this.snackbarService.showSuccess({
        message: this.translationService.translate(
          'APP.PROFILE-PAGE.EDIT.SAVE_SUCCESS'
        ),
      });

      this.formSaved = true; // Mark form as saved to prevent guard dialog
      this.isEditMode = false;
      this.editForm = null;
      // Clear local preview after successful save
      this.clearLocalProfilePhotoPreview();

      // Update map URLs with the updated profile data
      this.setupMapUrls(this.profileUser);

      // Re-initialize operating hours display with updated data
      if (this.profileUser?.shelter) {
        this.initializeOperatingHoursDisplay();
      }

      // Force change detection to update the UI
      this.cdr.detectChanges();
    } catch (error) {
      this.snackbarService.showError({
        message: this.translationService.translate(
          'APP.PROFILE-PAGE.EDIT.SAVE_ERROR'
        ),
        subMessage: this.translationService.translate('APP.COMMONS.TRY_AGAIN'),
      });
    } finally {
      this.isSaving = false;
      this.cdr.markForCheck();
    }
  }

  openProfilePhotoDialog(): void {
    if (!this.profileUser) return;

    const dialogData: ProfilePhotoDialogData = {
      currentPhotoUrl:
        this.localProfilePhotoPreview ||
        this.profileUser.profilePhoto?.sourceUrl,
      userName: this.profileUser.fullName || 'User',
    };

    const dialogRef = this.dialog.open(ProfilePhotoDialogComponent, {
      width: '480px',
      maxWidth: '90vw',
      maxHeight: '90vh',
      data: dialogData,
      disableClose: false,
      autoFocus: false,
      hasBackdrop: true,
      panelClass: ['profile-photo-dialog-panel'],
      backdropClass: 'profile-photo-backdrop',
    });

    dialogRef.afterClosed().subscribe({
      next: (result: ProfilePhotoDialogResult) => {
        // Ensure body scrolling is re-enabled even if dialog didn't handle it
        document.body.classList.remove('no-scroll');

        if (result) {
          switch (result.action) {
            case 'upload':
              if (result.fileId && result.file) {
                this.editForm?.get('profilePhoto')?.setValue(result.fileId);
                this.editForm?.get('deletePhoto')?.setValue(false);
                // Create local preview immediately
                this.createLocalProfilePhotoPreview(result.file);
                this.snackbarService.showSuccess({
                  message: this.translationService.translate(
                    'APP.PROFILE-PAGE.EDIT.PHOTO_SELECTED'
                  ),
                });
              }
              break;
            case 'delete':
              this.editForm?.get('profilePhoto')?.setValue(null);
              this.editForm?.get('deletePhoto')?.setValue(true);
              // Clear the local preview
              this.clearLocalProfilePhotoPreview();
              this.snackbarService.showSuccess({
                message: this.translationService.translate(
                  'APP.PROFILE-PAGE.EDIT.PHOTO_WILL_BE_DELETED'
                ),
              });
              break;
            case 'cancel':
              // Do nothing
              break;
          }
          // Use setTimeout to avoid ExpressionChangedAfterItHasBeenCheckedError
          setTimeout(() => {
            this.cdr.markForCheck();
          });
        }
      },
      error: () => {
        // Ensure body scrolling is re-enabled even on error
        document.body.classList.remove('no-scroll');
      },
    });
  }

  private updateProfilePhotoPreview(fileId: string): void {
    // Store the new file ID for later use
    // The actual photo will be updated when the form is saved
    if (this.profileUser) {
      // Mark that we have a pending photo update
      this.profileUser.profilePhoto = {
        ...this.profileUser.profilePhoto,
        id: fileId,
        sourceUrl:
          this.profileUser.profilePhoto?.sourceUrl || 'assets/placeholder.jpg',
      };
    }
  }

  private createLocalProfilePhotoPreview(file: File): void {
    // Clean up previous blob URL to prevent memory leaks
    if (
      this.localProfilePhotoPreview &&
      this.localProfilePhotoPreview.startsWith('blob:')
    ) {
      URL.revokeObjectURL(this.localProfilePhotoPreview);
    }

    // Create new blob URL for local preview
    this.localProfilePhotoPreview = URL.createObjectURL(file as Blob);
  }

  private clearLocalProfilePhotoPreview(): void {
    // Clean up blob URL to prevent memory leaks
    if (
      this.localProfilePhotoPreview &&
      this.localProfilePhotoPreview.startsWith('blob:')
    ) {
      URL.revokeObjectURL(this.localProfilePhotoPreview);
    }
    this.localProfilePhotoPreview = null;
  }

  private clearProfilePhotoPreview(): void {
    if (this.profileUser) {
      // Mark for deletion but keep the current photo until save
      this.profileUser.profilePhoto = undefined;
    }
  }

  private getSocialMediaPayload(socialMedia: any) {
    if (!socialMedia) {
      return null;
    }

    const facebook = socialMedia.facebook;
    const instagram = socialMedia.instagram;

    return facebook || instagram ? socialMedia : null;
  }

  private getOperatingHoursPayload(operatingHours: any): any {
    if (!operatingHours) {
      return null;
    }

    const days = Object.keys(operatingHours);

    return days.every((day) => {
      const value = operatingHours[day];
      return !value || value === '';
    })
      ? null
      : operatingHours;
  }

  getDayKey(day: string): string {
    const dayMap: { [key: string]: string } = {
      Δευτέρα: 'monday',
      Τρίτη: 'tuesday',
      Τετάρτη: 'wednesday',
      Πέμπτη: 'thursday',
      Παρασκευή: 'friday',
      Σάββατο: 'saturday',
      Κυριακή: 'sunday',
    };
    return dayMap[day] || day.toLowerCase();
  }

  private markFormGroupTouched(formGroup: FormGroup) {
    Object.keys(formGroup.controls).forEach((key) => {
      const control = formGroup.get(key);
      if (control instanceof FormGroup) {
        this.markFormGroupTouched(control);
      } else if (control) {
        control.markAsTouched();
        control.markAsDirty();
        control.updateValueAndValidity();
      }
    });
  }

  getApplicationStatusText(status: ApplicationStatus): string {
    switch (status) {
      case ApplicationStatus.Pending:
        return this.translationService.translate(
          'APP.PROFILE-PAGE.APPLICATION_STATUS.PENDING'
        );
      case ApplicationStatus.Approved:
        return this.translationService.translate(
          'APP.PROFILE-PAGE.APPLICATION_STATUS.APPROVED'
        );
      case ApplicationStatus.Rejected:
        return this.translationService.translate(
          'APP.PROFILE-PAGE.APPLICATION_STATUS.REJECTED'
        );
      default:
        return this.translationService.translate(
          'APP.PROFILE-PAGE.APPLICATION_STATUS.UNKNOWN'
        );
    }
  }

  getOperatingHoursText(hours: any): string {
    if (!hours)
      return this.translationService.translate(
        'APP.PROFILE-PAGE.SHELTER.NO_HOURS'
      );

    const days = [
      'monday',
      'tuesday',
      'wednesday',
      'thursday',
      'friday',
      'saturday',
      'sunday',
    ];
    const today = new Date()
      .toLocaleDateString('en-US', { weekday: 'short' })
      .toLowerCase();
    const todayHours = hours[today];

    if (!todayHours || todayHours === 'Closed') {
      return this.translationService.translate(
        'APP.PROFILE-PAGE.SHELTER.CLOSED_TODAY'
      );
    }

    return `${this.translationService.translate(
      'APP.PROFILE-PAGE.SHELTER.OPEN_TODAY'
    )}: ${todayHours}`;
  }

  /**
   * Returns a Google Maps Static API URL for a given location object.
   */
  getMapImageUrl(location: any): string {
    if (!location) return '';
    const address = [
      location.address,
      location.number,
      location.city,
      location.zipCode,
    ]
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
    const address = [
      location.address,
      location.number,
      location.city,
      location.zipCode,
    ]
      .filter(Boolean)
      .join(' ');
    const encoded = encodeURIComponent(address);
    return `https://www.google.com/maps?q=${encoded}&output=embed`;
  }

  togglePersonalMap(): void {
    this.showPersonalMap = !this.showPersonalMap;
    if (this.showPersonalMap && this.profileUser?.location) {
      this.personalMapUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
        this.getGoogleMapsEmbedUrl(this.profileUser.location)
      );
    } else {
      this.personalMapUrl = null;
    }
  }

  toggleShelterMap(): void {
    this.showShelterMap = !this.showShelterMap;
    if (this.showShelterMap && this.profileUser?.shelter?.user?.location) {
      this.shelterMapUrl = this.sanitizer.bypassSecurityTrustResourceUrl(
        this.getGoogleMapsEmbedUrl(this.profileUser.shelter.user.location)
      );
    } else {
      this.shelterMapUrl = null;
    }
  }

  /**
   * Returns a Google Maps search URL for a given location object (for use in href).
   */
  getGoogleMapsLink(location: any): string {
    if (!location) return '';
    const address = [
      location.address,
      location.number,
      location.city,
      location.zipCode,
    ]
      .filter(Boolean)
      .join(' ');
    const encoded = encodeURIComponent(address);
    return `https://www.google.com/maps/search/?api=1&query=${encoded}`;
  }

  // Operating hours methods (from shelter-info component)

  onClosedChange(day: string): void {
    const dayKey = this.getDayKey(day);
    const operatingHours = this.editForm?.get('shelter.operatingHours');

    this.operatingHoursModified = true;

    if (this.closedDays[day]) {
      operatingHours?.get(dayKey)?.setValue('closed');
      this.timeErrors[day] = null;
    } else {
      this.openTimes[day] = '';
      this.closeTimes[day] = '';
      operatingHours?.get(dayKey)?.setValue('');
    }

    this.updateOperatingHoursValidators();
    this.cdr.markForCheck();
  }

  onTimeInput(event: Event, day: string, type: 'open' | 'close'): void {
    const input = event.target as HTMLInputElement;
    let value = input.value;

    this.operatingHoursModified = true;
    value = value.replace(/[^\d:]/g, '');

    if (value.length > 0) {
      if (!value.includes(':') && value.length > 2) {
        value = value.substring(0, 2) + ':' + value.substring(2);
      }

      if (value.length > 5) {
        value = value.substring(0, 5);
      }

      if (value.includes(':')) {
        const hours = value.split(':')[0];
        if (hours.length === 2) {
          const hoursNum = parseInt(hours, 10);
          if (hoursNum > 23) {
            value = '23' + value.substring(2);
          }
        }
      }

      if (value.includes(':') && value.length > 3) {
        const minutes = value.split(':')[1];
        if (minutes.length === 2) {
          const minutesNum = parseInt(minutes, 10);
          if (minutesNum > 59) {
            value = value.split(':')[0] + ':59';
          }
        }
      }
    }

    if (type === 'open') {
      this.openTimes[day] = value;
    } else {
      this.closeTimes[day] = value;
    }

    input.value = value;

    if (this.openTimes[day] && this.closeTimes[day]) {
      this.updateFormValue(day);
      this.validateTimeRange(day);
    }

    this.updateOperatingHoursValidators();
  }

  formatTime(day: string, type: 'open' | 'close'): void {
    let time = type === 'open' ? this.openTimes[day] : this.closeTimes[day];

    if (time) {
      time = time.replace(/[^\d:]/g, '');

      if (!time.includes(':') && time.length > 0) {
        if (time.length === 1) {
          time = '0' + time + ':00';
        } else if (time.length === 2) {
          time = time + ':00';
        } else {
          time = time.substring(0, 2) + ':' + time.substring(2);
        }
      }

      let [hours, minutes] = time.split(':');

      let hoursNum = parseInt(hours || '0', 10);
      if (isNaN(hoursNum) || hoursNum > 23) hoursNum = 0;
      hours = hoursNum.toString().padStart(2, '0');

      let minutesNum = parseInt(minutes || '0', 10);
      if (isNaN(minutesNum) || minutesNum > 59) minutesNum = 0;
      minutes = minutesNum.toString().padStart(2, '0');

      time = `${hours}:${minutes}`;

      if (type === 'open') {
        this.openTimes[day] = time;
      } else {
        this.closeTimes[day] = time;
      }

      this.updateFormValue(day);
      this.validateTimeRange(day);
      this.cdr.markForCheck();
    }
  }

  updateFormValue(day: string): void {
    const dayKey = this.getDayKey(day);
    const operatingHours = this.editForm?.get('shelter.operatingHours');

    if (!this.closedDays[day]) {
      if (
        this.isValidTimeFormat(this.openTimes[day]) &&
        this.isValidTimeFormat(this.closeTimes[day])
      ) {
        const timeValue = `${this.openTimes[day]},${this.closeTimes[day]}`;
        operatingHours?.get(dayKey)?.setValue(timeValue);
        operatingHours?.get(dayKey)?.markAsTouched();
        operatingHours?.get(dayKey)?.markAsDirty();
      } else {
        operatingHours?.get(dayKey)?.setValue('');
      }
    }
  }

  isValidTimeFormat(time: string): boolean {
    return /^([01]\d|2[0-3]):([0-5]\d)$/.test(time);
  }

  validateTimeRange(day: string): void {
    const openTime = this.openTimes[day];
    const closeTime = this.closeTimes[day];

    this.timeErrors[day] = null;

    if (openTime && closeTime) {
      if (!this.isValidTimeFormat(openTime)) {
        this.timeErrors[day] =
          'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.OPENING_TIME_FORMAT';
        this.setTimeRangeError(day);
        return;
      }

      if (!this.isValidTimeFormat(closeTime)) {
        this.timeErrors[day] =
          'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.CLOSING_TIME_FORMAT';
        this.setTimeRangeError(day);
        return;
      }

      if (openTime >= closeTime) {
        this.timeErrors[day] =
          'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.TIME_RANGE_INVALID';
        this.setTimeRangeError(day);
        return;
      }

      const dayKey = this.getDayKey(day);
      const operatingHours = this.editForm?.get('shelter.operatingHours');
      const control = operatingHours?.get(dayKey);
      if (control?.errors?.['invalidTimeRange']) {
        const errors = { ...control.errors };
        delete errors['invalidTimeRange'];
        if (Object.keys(errors).length === 0) {
          control.setErrors(null);
        } else {
          control.setErrors(errors);
        }
      }
    } else if ((openTime && !closeTime) || (!openTime && closeTime)) {
      this.timeErrors[day] =
        'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.BOTH_TIMES_REQUIRED';
      this.setTimeRangeError(day);
    }
  }

  setTimeRangeError(day: string): void {
    const dayKey = this.getDayKey(day);
    const operatingHours = this.editForm?.get('shelter.operatingHours');
    operatingHours?.get(dayKey)?.setErrors({ invalidTimeRange: true });
  }

  hasAnyOperatingHoursSet(): boolean {
    return this.days.some((day) => {
      if (this.closedDays[day]) return true;

      const dayKey = this.getDayKey(day);
      const operatingHours = this.editForm?.get('shelter.operatingHours');
      const value = operatingHours?.get(dayKey)?.value;
      return value && value !== '' && value !== 'closed';
    });
  }

  updateOperatingHoursValidators(): void {
    const hasAnyHoursSet = this.hasAnyOperatingHoursSet();
    const operatingHours = this.editForm?.get('shelter.operatingHours');

    this.days.forEach((day) => {
      const dayKey = this.getDayKey(day);
      const control = operatingHours?.get(dayKey);

      if (hasAnyHoursSet) {
        if (this.closedDays[day]) {
          control?.setErrors(null);
        } else if (!this.openTimes[day] || !this.closeTimes[day]) {
          this.timeErrors[day] =
            'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.HOURS_OR_CLOSED_REQUIRED';
          control?.setErrors({ required: true });
        } else {
          this.validateTimeRange(day);
        }
      } else {
        control?.setErrors(null);
        this.timeErrors[day] = null;
      }
    });

    this.cdr.markForCheck();
  }

  hasAnyNonClosedDay(): boolean {
    return this.days.some((day) => {
      if (!this.closedDays[day]) {
        const dayKey = this.getDayKey(day);
        const operatingHours = this.editForm?.get('shelter.operatingHours');
        const value = operatingHours?.get(dayKey)?.value;
        return value && value !== '' && value !== 'closed';
      }
      return false;
    });
  }

  validateOperatingHours(): boolean {
    let hasInvalidDay = false;

    this.days.forEach((day) => {
      const dayKey = this.getDayKey(day);
      const operatingHours = this.editForm?.get('shelter.operatingHours');
      const control = operatingHours?.get(dayKey);
      const value = control?.value;

      if (this.closedDays[day]) {
        control?.setErrors(null);
        this.timeErrors[day] = null;
        return;
      }

      if (!value || value === '') {
        this.timeErrors[day] =
          'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.HOURS_OR_CLOSED_REQUIRED';
        control?.setErrors({ required: true });
        hasInvalidDay = true;
      } else if (value !== 'closed') {
        if (control?.errors?.['invalidTimeRange']) {
          hasInvalidDay = true;
        }
      }
    });

    return hasInvalidDay;
  }

  reloadPage(): void {
    window.location.reload();
  }

  // CanComponentDeactivate implementation
  canDeactivate(): boolean {
    return !this.hasUnsavedChanges();
  }

  hasUnsavedChanges(): boolean {
    // Don't show guard dialog if form was saved or not in edit mode
    if (this.formSaved || !this.isEditMode || !this.editForm) {
      return false;
    }

    // Check if the edit form has unsaved changes
    return this.editForm.dirty;
  }

  // Form validation helpers
  isFieldInvalid(fieldPath: string): boolean {
    const field = this.editForm?.get(fieldPath);
    return !!(field?.invalid && (field?.touched || field?.dirty));
  }

  getFieldError(fieldPath: string): string | null {
    const field = this.editForm?.get(fieldPath);
    if (!field?.errors || !field?.touched) return null;

    const errors = field.errors;
    if (errors['required']) return 'REQUIRED';
    if (errors['email']) return 'EMAIL_INVALID';
    if (errors['minlength']) return 'MINLENGTH';
    if (errors['pattern']) return 'PATTERN_INVALID';
    if (errors['invalidSocialMedia']) return 'SOCIAL_MEDIA_INVALID';
    if (errors['invalidTimeRange']) return 'TIME_RANGE_INVALID';

    return 'INVALID';
  }
}
