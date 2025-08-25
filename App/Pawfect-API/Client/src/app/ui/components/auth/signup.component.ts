import { Component, OnInit, ViewChild, OnDestroy, HostListener } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
  FormControl,
} from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { AuthService } from 'src/app/services/auth.service';
import { RegisterPayload, OtpPayload } from 'src/app/models/auth/auth.model';
import { CustomValidators } from './validators/custom.validators';
import {
  trigger,
  transition,
  style,
  animate,
  state,
} from '@angular/animations';
import { OtpInputComponent } from 'src/app/common/ui/otp-input.component';
import { LogService } from 'src/app/common/services/log.service';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { ErrorDetails } from 'src/app/common/ui/error-message-banner.component';
import { AuthProvider } from 'src/app/common/enum/auth-provider.enum';
import { UserRole } from 'src/app/common/enum/user-role.enum';
import { User } from 'src/app/models/user/user.model';
import { SecureStorageService } from 'src/app/common/services/secure-storage.service';
import { TranslationService } from 'src/app/common/services/translation.service';
import { MatDialog } from '@angular/material/dialog';
import { FormLeaveConfirmationDialogComponent } from 'src/app/common/ui/form-leave-confirmation-dialog.component';
import { Observable } from 'rxjs';
import { CanComponentDeactivate } from 'src/app/common/guards/form.guard';
import { PersonalInfoComponent } from './sign-up-steps/personal-info/personal-info.component';
import { AccountDetailsComponent } from './sign-up-steps/account-details/account-details.component';
import { ShelterInfoComponent } from './sign-up-steps/shelter-info/shelter-info.component';
import { UserAvailabilityService } from 'src/app/services/user-availability.service';
import { SnackbarService } from 'src/app/common/services/snackbar.service';

interface LocationFormGroup extends FormGroup {
  controls: {
    city: AbstractControl;
    zipCode: AbstractControl;
    address: AbstractControl;
    number: AbstractControl;
  };
}

interface SocialMediaFormGroup extends FormGroup {
  controls: {
    facebook: AbstractControl;
    instagram: AbstractControl;
  };
}

interface OperatingHoursFormGroup extends FormGroup {
  controls: {
    monday: AbstractControl;
    tuesday: AbstractControl;
    wednesday: AbstractControl;
    thursday: AbstractControl;
    friday: AbstractControl;
    saturday: AbstractControl;
    sunday: AbstractControl;
  };
}

interface ShelterFormGroup extends FormGroup {
  controls: {
    shelterName: AbstractControl;
    description: AbstractControl;
    website: AbstractControl;
    socialMedia: SocialMediaFormGroup;
    operatingHours: OperatingHoursFormGroup;
  };
}

interface RegistrationFormGroup extends FormGroup {
  controls: {
    email: AbstractControl;
    password: AbstractControl;
    confirmPassword: AbstractControl;
    fullName: AbstractControl;
    phone: AbstractControl;
    countryCode: AbstractControl;
    phoneNumber: AbstractControl;
    role: AbstractControl;
    isShelter: AbstractControl;
    profilePhoto: AbstractControl;
    hasEmailVerified: AbstractControl;
    location: LocationFormGroup;
    shelter: ShelterFormGroup;
  };
}

interface OtpFormGroup extends FormGroup {
  controls: {
    otp: AbstractControl;
  };
}

export enum SignupStep {
  PersonalInfo = 1,
  AccountDetails = 2,
  ShelterInfo = 3,
  OtpVerification = 4,
  EmailConfirmation = 5,
}

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css'],
  animations: [
    trigger('stepTransition', [
      state('next', style({ transform: 'translateX(0)', opacity: 1 })),
      state('prev', style({ transform: 'translateX(0)', opacity: 1 })),

      // Slide from right to left (next)
      transition('void => next', [
        style({ transform: 'translateX(100%)', opacity: 0 }),
        animate(
          '400ms cubic-bezier(0.35, 0, 0.25, 1)',
          style({ transform: 'translateX(0)', opacity: 1 })
        ),
      ]),

      // Slide from left to right (back)
      transition('void => prev', [
        style({ transform: 'translateX(-100%)', opacity: 0 }),
        animate(
          '400ms cubic-bezier(0.35, 0, 0.25, 1)',
          style({ transform: 'translateX(0)', opacity: 1 })
        ),
      ]),
    ]),
  ],
})
export class SignupComponent
  extends BaseComponent
  implements OnInit, OnDestroy, CanComponentDeactivate
{
  @ViewChild(OtpInputComponent) otpInputComponent?: OtpInputComponent;
  @ViewChild(PersonalInfoComponent) personalInfoCmp?: PersonalInfoComponent;
  @ViewChild(AccountDetailsComponent) accountDetailsCmp?: AccountDetailsComponent;
  @ViewChild(ShelterInfoComponent) shelterInfoCmp?: ShelterInfoComponent;
  currentStep = SignupStep.PersonalInfo;
  SignupStep = SignupStep;
  stepDirection: 'next' | 'prev' = 'next';
  fromLogin = false;

  isLoading = false;
  isExternalProviderLoading = false;

  userId: string = '';
  resendOtpTimer = 0;
  resendOtpInterval: any;

  error?: ErrorDetails;

  showShelterInfo = false;

  googlePopulatedFields: string[] = [];
  hasGoogleData = false;

  registrationForm!: RegistrationFormGroup;
  otpForm!: OtpFormGroup;
  personalInfoForm!: FormGroup;
  accountDetailsForm!: FormGroup;

  private hasUnsavedChangesFlag = false;
  private isSubmitting = false;
  private suppressFormGuard = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly logService: LogService,
    private readonly errorHandler: ErrorHandlerService,
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly secureStorageService: SecureStorageService,
    public readonly translationService: TranslationService,
    private readonly snackbarService: SnackbarService,
    private readonly dialog: MatDialog
  ) {
    super();
    this.initializeForms();
  }

  ngOnInit(): void {
    // Handle Google mode first to prevent race conditions
    this.route.queryParams.subscribe((params: any) => {
      if (params['mode'] === 'google') {
        
        // Get the Google auth data from session storage
        const googleAuthData = this.secureStorageService.getItem<any>('googleAuthData');
  
        if (googleAuthData && googleAuthData.code) {
          // Check if the data is not too old (5 minutes max)
          const dataAge = Date.now() - googleAuthData.timestamp;
          const maxAge = 5 * 60 * 1000; // 5 minutes
  
          if (dataAge > maxAge) {
            console.error('Google auth data is too old');
            this.error = {
              title: 'Authentication Expired',
              message: 'The Google authentication session has expired. Please try again.',
              type: 'error'
            };
            this.secureStorageService.removeItem('googleAuthData');
            // Don't return here, let the normal flow continue
          } else {
            // Clear the data from session storage to prevent reuse
            this.secureStorageService.removeItem('googleAuthData');
            this.secureStorageService.removeItem('googleAuthOrigin');
  
            // Call the login with Google method
            this.isExternalProviderLoading = true;
            this.error = undefined;
            
            // Add a small delay to ensure UI updates
            setTimeout(() => {
              this.processGoogleSignUp(googleAuthData.code);
            }, 100);
          }
        } else {
          console.error('No Google auth data found or invalid data');
          this.error = {
            title: 'Authentication Error',
            message: 'Google authentication data not found. Please try signing up with Google again.',
            type: 'error'
          };
        }
      }
    });
  
    // Handle other initialization after a delay to prevent conflicts
    setTimeout(() => {
      const state = history.state;
  
      // Handle verification flow from login
      const existingPhone: string | null =
        this.secureStorageService.getItem<string>('unverifiedPhone');
      const existingEmail: string | null =
        this.secureStorageService.getItem<string>('unverifiedEmail');
      const fromGoogleLogin = this.secureStorageService.getItem<string>('fromGoogleLogin') === 'true';
  
      if (existingPhone) {
        this.registrationForm.get('phone')?.setValue(existingPhone);
      }
      if (existingEmail) {
        this.registrationForm.get('email')?.setValue(existingEmail);
      }
  
      if (state && state.step === SignupStep.OtpVerification && state.fromLogin) {
        this.fromLogin = true;
        this.currentStep = SignupStep.OtpVerification;
        this.resendOtp();
      }
  
      if (
        state &&
        state.step === SignupStep.EmailConfirmation &&
        state.fromLogin
      ) {
        this.fromLogin = true;
        if (!fromGoogleLogin) {
          this.currentStep = SignupStep.EmailConfirmation;
          this.resendEmailVerification();
        } else {
          // Skip email verification step entirely for Google-origin login
          this.router.navigate(['/home']);
          this.secureStorageService.removeItem('fromGoogleLogin');
        }
      }
  
      // Track form changes for unsaved changes detection
      this.setupFormChangeTracking();
    }, 50);
  }
  
  // Add this method to handle Google mode disable from child component
  onGoogleModeDisabled(): void {
    // Reset auth provider in main form
    this.registrationForm.get('authProvider')?.setValue(AuthProvider.Local);
    this.registrationForm.get('authProviderId')?.setValue(null);
    
    // Re-enable password fields
    this.registrationForm.get('password')?.enable();
    this.registrationForm.get('confirmPassword')?.enable();
    
    // Reset password validators
    const passwordControl = this.registrationForm.get('password');
    const confirmPasswordControl = this.registrationForm.get('confirmPassword');
    
    passwordControl?.setValidators([
      Validators.required,
      CustomValidators.passwordValidator(this.translationService),
    ]);
    
    confirmPasswordControl?.setValidators([
      Validators.required,
      CustomValidators.matchValidator('password', this.translationService),
    ]);
    
    passwordControl?.updateValueAndValidity();
    confirmPasswordControl?.updateValueAndValidity();
    
    // Clear Google data flags
    this.hasGoogleData = false;
    this.googlePopulatedFields = [];
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    clearInterval(this.resendOtpInterval);
    window.onpopstate = null;
  }

  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any): void {
    if (this.hasUnsavedChanges()) {
      $event.returnValue = this.translationService.translate('APP.COMMONS.FORM_GUARD.MESSAGE');
    }
  }

  private setupFormChangeTracking(): void {
    // Track changes in registration form
    this.registrationForm.valueChanges.subscribe(() => {
      if (!this.isSubmitting) {
        this.hasUnsavedChangesFlag = true;
      }
    });

    // Track changes in OTP form
    this.otpForm.valueChanges.subscribe(() => {
      if (!this.isSubmitting) {
        this.hasUnsavedChangesFlag = true;
      }
    });

    // Track changes in step forms
    if (this.personalInfoForm) {
      this.personalInfoForm.valueChanges.subscribe(() => {
        if (!this.isSubmitting) {
          this.hasUnsavedChangesFlag = true;
        }
      });
    }
    if (this.accountDetailsForm) {
      this.accountDetailsForm.valueChanges.subscribe(() => {
        if (!this.isSubmitting) {
          this.hasUnsavedChangesFlag = true;
        }
      });
    }
  }

  canDeactivate(): Observable<boolean> | boolean {
    if (!this.hasUnsavedChanges()) {
      return true;
    }

    const dialogRef = this.dialog.open(FormLeaveConfirmationDialogComponent, {
      data: {
        title: this.translationService.translate('APP.COMMONS.FORM_GUARD.TITLE'),
        message: this.translationService.translate('APP.COMMONS.FORM_GUARD.MESSAGE'),
        confirmText: this.translationService.translate('APP.COMMONS.FORM_GUARD.LEAVE'),
        cancelText: this.translationService.translate('APP.COMMONS.FORM_GUARD.STAY')
      },
      disableClose: false,
      width: '28rem',
      panelClass: 'form-guard-panel',
      backdropClass: 'form-guard-backdrop',
      autoFocus: false,
      hasBackdrop: true
    });

    return dialogRef.afterClosed();
  }

  hasUnsavedChanges(): boolean {
    if (this.isSubmitting) {
      return false;
    }
    if ((this as any).suppressFormGuard) {
      return false;
    }
    const formDirty = !!this.registrationForm && this.registrationForm.dirty;
    const otpDirty = !!this.otpForm && this.otpForm.dirty;

    const stepDirty = !!(
      (this.personalInfoCmp && this.personalInfoCmp.hasUnsavedChanges && this.personalInfoCmp.hasUnsavedChanges()) ||
      (this.accountDetailsCmp && this.accountDetailsCmp.hasUnsavedChanges && this.accountDetailsCmp.hasUnsavedChanges()) ||
      (this.shelterInfoCmp && this.shelterInfoCmp.hasUnsavedChanges && this.shelterInfoCmp.hasUnsavedChanges())
    );

    return this.hasUnsavedChangesFlag || formDirty || otpDirty || stepDirty;
  }

  private initializeForms(): void {
    // Change this in the initializeForms() method:
    const operatingHoursGroup = this.fb.group({
      monday: [''],
      tuesday: [''],
      wednesday: [''],
      thursday: [''],
      friday: [''],
      saturday: [''],
      sunday: [''],
    });

    this.registrationForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: [
        '',
        [
          Validators.required,
          CustomValidators.passwordValidator(this.translationService),
        ],
      ],
      confirmPassword: ['', [Validators.required]],
      fullName: ['', [Validators.required, Validators.minLength(5)]],
      phone: [''],
      countryCode: ['+30', Validators.required],
      phoneNumber: [
        '',
        [Validators.required, Validators.pattern(/^\d{1,14}$/)],
      ],
      authProvider: [AuthProvider.Local],
      authProviderId: [null],
      role: [UserRole.User, [Validators.required]],
      isShelter: [false],
      profilePhoto: [null],
      hasPhoneVerified: [false],
      hasEmailVerified: [false],
      location: this.fb.group({
        city: [
          '',
          [
            Validators.required,
            Validators.minLength(2),
            Validators.maxLength(50),
          ],
        ],
        zipCode: [
          '',
          [Validators.required, Validators.pattern(/^\d{5}(-\d{4})?$/)],
        ],
        address: [
          '',
          [
            Validators.required,
            Validators.minLength(3),
            Validators.maxLength(100),
          ],
        ],
        number: [
          '',
          [
            Validators.required,
            Validators.pattern(/^\d+$/),
            Validators.minLength(1),
            Validators.maxLength(5),
          ],
        ],
      }) as LocationFormGroup,
      shelter: this.fb.group({
        shelterName: [''],
        description: [''], // Empty string instead of whitespace
        website: [''],
        socialMedia: this.fb.group({
          facebook: [''],
          instagram: [''],
        }) as SocialMediaFormGroup,
        operatingHours: operatingHoursGroup,
      }) as ShelterFormGroup,
    }) as RegistrationFormGroup;

    // Create stable step forms referencing registration controls
    this.personalInfoForm = this.fb.group({
      fullName: this.registrationForm.get('fullName'),
      email: this.registrationForm.get('email'),
      countryCode: this.registrationForm.get('countryCode'),
      phoneNumber: this.registrationForm.get('phoneNumber'),
      phone: this.registrationForm.get('phone'),
      location: this.registrationForm.get('location'),
      profilePhoto: this.registrationForm.get('profilePhoto'),
    });

    this.accountDetailsForm = this.fb.group({
      password: this.registrationForm.get('password'),
      confirmPassword: this.registrationForm.get('confirmPassword'),
    });

    // Add password confirmation validator
    this.registrationForm
      .get('confirmPassword')
      ?.setValidators([
        Validators.required,
        CustomValidators.matchValidator('password', this.translationService),
      ]);

    // Subscribe to password changes to update confirm password validation
    this.registrationForm.get('password')?.valueChanges.subscribe(() => {
      const confirmPasswordControl =
        this.registrationForm.get('confirmPassword');
      if (confirmPasswordControl) {
        confirmPasswordControl.updateValueAndValidity();
      }
    });

    // Subscribe to isShelter changes to update validations
    this.registrationForm
      .get('isShelter')
      ?.valueChanges.subscribe((isShelter) => {
        this.showShelterInfo = isShelter;
        const role = isShelter ? UserRole.Shelter : UserRole.User;
        this.registrationForm.patchValue({ role });

        const shelterForm = this.getShelterForm();

        // Update shelter form validators
        if (isShelter) {
          shelterForm
            .get('shelterName')
            ?.setValidators([Validators.required, Validators.minLength(3)]);
          shelterForm
            .get('description')
            ?.setValidators([Validators.required, Validators.minLength(10)]);

          // Update operating hours validators
          Object.keys(operatingHoursGroup.controls).forEach((key) => {
            operatingHoursGroup
              .get(key)
              ?.setValidators([CustomValidators.operatingHoursValidator()]);
          });

          // Update social media validators - only validate if values are provided
          shelterForm
            .get('socialMedia.facebook')
            ?.setValidators(CustomValidators.socialMediaValidator('facebook'));
          shelterForm
            .get('socialMedia.instagram')
            ?.setValidators(CustomValidators.socialMediaValidator('instagram'));

          // Optional website field - only validate if value is provided
          shelterForm
            .get('website')
            ?.setValidators(Validators.pattern(/^https?:\/\/.+\..+$/));
        } else {
          // Clear all shelter-related validators when not a shelter
          shelterForm.get('shelterName')?.clearValidators();
          shelterForm.get('description')?.clearValidators();

          Object.keys(operatingHoursGroup.controls).forEach((key) => {
            operatingHoursGroup.get(key)?.clearValidators();
          });

          shelterForm.get('socialMedia.facebook')?.clearValidators();
          shelterForm.get('socialMedia.instagram')?.clearValidators();
          shelterForm.get('website')?.clearValidators();

          // Reset shelter form values
          shelterForm.reset();
        }

        // Update validation state for all shelter form controls
        shelterForm.get('shelterName')?.updateValueAndValidity();
        shelterForm.get('description')?.updateValueAndValidity();
        shelterForm.get('socialMedia.facebook')?.updateValueAndValidity();
        shelterForm.get('socialMedia.instagram')?.updateValueAndValidity();
        shelterForm.get('website')?.updateValueAndValidity();
        operatingHoursGroup.updateValueAndValidity();
      });

    this.otpForm = this.fb.group({
      otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
    }) as OtpFormGroup;
  }

  getStepsToShow() {
    // Base steps that are always shown
    const steps = [
      {
        value: SignupStep.PersonalInfo,
        displayNumber: 1,
        label: this.translationService.translate(
          'APP.AUTH.SIGNUP.PERSONAL_INFO_STEP'
        ),
      },
      {
        value: SignupStep.AccountDetails,
        displayNumber: 2,
        label: this.translationService.translate(
          'APP.AUTH.SIGNUP.ACCOUNT_DETAILS_STEP'
        ),
      },
    ];

    // Conditionally add shelter info step
    if (this.showShelterInfo) {
      steps.push({
        value: SignupStep.ShelterInfo,
        displayNumber: 3,
        label: this.translationService.translate(
          'APP.AUTH.SIGNUP.SHELTER_INFO_STEP'
        ),
      });
    }

    steps.push({
      value: SignupStep.OtpVerification,
      displayNumber: this.showShelterInfo ? 4 : 3,
      label: this.translationService.translate(
        'APP.AUTH.SIGNUP.OTP_VERIFICATION'
      ),
    });

    return steps;
  }

  getShelterForm(): ShelterFormGroup {
    return this.registrationForm.get('shelter') as ShelterFormGroup;
  }

  getLocationForm(): LocationFormGroup {
    return this.registrationForm.get('location') as LocationFormGroup;
  }

  getSocialMediaForm(): SocialMediaFormGroup {
    return this.getShelterForm().get('socialMedia') as SocialMediaFormGroup;
  }

  getOperatingHoursForm(): OperatingHoursFormGroup {
    return this.getShelterForm().get(
      'operatingHours'
    ) as OperatingHoursFormGroup;
  }

  getShelterDescriptionControl(): FormControl {
    const control = this.getShelterForm().get('description');
    if (!(control instanceof FormControl)) {
      throw new Error('Description control not found or invalid');
    }
    return control;
  }

  getPersonalInfoForm(): FormGroup {
    return this.personalInfoForm;
  }

  getAccountDetailsForm(): FormGroup {
    return this.accountDetailsForm;
  }

  getFileUploadForm(): FormGroup {
    const fileUploadForm = this.fb.group({
      profilePhoto: this.registrationForm.get('profilePhoto'),
    });
    return fileUploadForm;
  }

  private processGoogleSignUp(authCode: string): void {
    this.authService.registerWithGoogle(authCode).subscribe({
      next: (response: User) => {
        const googlePopulatedFields: string[] = [];
  
        if (response.email) {
          this.registrationForm.get('email')?.setValue(response.email);
          this.registrationForm.get('email')?.disable();
          this.registrationForm.get('hasEmailVerified')?.setValue(true);
          googlePopulatedFields.push('email');
        }
  
        if (response.fullName) {
          this.registrationForm.get('fullName')?.setValue(response.fullName);
          this.registrationForm.get('fullName')?.disable();
          googlePopulatedFields.push('fullName');
        }
  
        if (response.location) {
          const locationForm = this.getLocationForm();
  
          // Handle each location field individually
          if (response.location.city) {
            locationForm.get('city')?.setValue(response.location.city);
            locationForm.get('city')?.disable();
            googlePopulatedFields.push('location.city');
          }
  
          if (response.location.zipCode) {
            locationForm.get('zipCode')?.setValue(response.location.zipCode);
            locationForm.get('zipCode')?.disable();
            googlePopulatedFields.push('location.zipCode');
          }
  
          if (response.location.address) {
            locationForm.get('address')?.setValue(response.location.address);
            locationForm.get('address')?.disable();
            googlePopulatedFields.push('location.address');
          }
  
          if (response.location.number) {
            locationForm.get('number')?.setValue(response.location.number);
            locationForm.get('number')?.disable();
            googlePopulatedFields.push('location.number');
          }
        }
  
        if (response.phone) {
          const phoneNumbers = response.phone.split(' ');
          if (phoneNumbers.length > 1) {
            this.registrationForm.get('countryCode')?.setValue(phoneNumbers[0]);
            this.registrationForm.get('phoneNumber')?.setValue(phoneNumbers[1]);
            this.registrationForm.get('countryCode')?.disable();
            this.registrationForm.get('phoneNumber')?.disable();
          } else {
            this.registrationForm.get('phoneNumber')?.setValue(response.phone);
            this.registrationForm.get('phoneNumber')?.disable();
          }
  
          this.registrationForm.get('phone')?.setValue(response.phone);
          this.registrationForm.get('phone')?.disable();
          this.registrationForm.get('hasPhoneVerified')?.setValue(true);
          googlePopulatedFields.push('phone');
        }
  
        // Set Google as auth provider
        this.registrationForm
          .get('authProvider')
          ?.setValue(AuthProvider.Google);
  
        // Set auth provider ID from response
        if (response.authProviderId) {
          this.registrationForm.get('authProviderId')?.setValue(response.authProviderId);
        }
  
        // Disable password fields for Google authentication
        this.registrationForm.get('password')?.disable();
        this.registrationForm.get('confirmPassword')?.disable();
        this.registrationForm.get('password')?.clearValidators();
        this.registrationForm.get('confirmPassword')?.clearValidators();
        this.registrationForm.get('password')?.updateValueAndValidity();
        this.registrationForm.get('confirmPassword')?.updateValueAndValidity();
  
        this.googlePopulatedFields = googlePopulatedFields;
        this.hasGoogleData = true;
        this.hasUnsavedChangesFlag = true; 
  
        this.isExternalProviderLoading = false;
        this.error = undefined;
      },
      error: (error: HttpErrorResponse) => {
        console.error('Google signup error:', error);
        this.isExternalProviderLoading = false;
        this.error = this.errorHandler.handleAuthError(error);
      },
    });
  }

  isGooglePopulated(fieldName: string): boolean {
    return this.googlePopulatedFields.includes(fieldName);
  }

  isGoogleAuthenticated(): boolean {
    return (
      this.registrationForm.get('authProvider')?.value === AuthProvider.Google
    );
  }

  onPersonalInfoNext(): void {
    this.stepDirection = 'next';
    if (this.isGoogleAuthenticated()) {
      this.onAccountDetailsNext(true);
      return;
    }

    this.currentStep = SignupStep.AccountDetails;
  }

  onAccountDetailsNext(isExternalProvided: boolean = false): void {
    this.stepDirection = 'next';

    // Handle Google authentication - no password required
    if (this.isGoogleAuthenticated() || isExternalProvided) {
      this.registrationForm
        .get('password')
        ?.setValue('AuthenticatedExternally2025!');
      this.registrationForm
        .get('confirmPassword')
        ?.setValue('AuthenticatedExternally2025!');
    } else {
      // For local authentication, validate passwords
      const password = this.registrationForm.get('password')?.value;
      const confirmPassword =
        this.registrationForm.get('confirmPassword')?.value;

      if (password !== confirmPassword) {
        this.registrationForm
          .get('confirmPassword')
          ?.setErrors({ mismatch: true });
        this.registrationForm.get('confirmPassword')?.markAsTouched();
        return;
      }
    }

    // For Google auth, skip account details validation since passwords are disabled
    const isFormValid =
      this.isGoogleAuthenticated() || this.getAccountDetailsForm().valid;

    if (isFormValid) {
      if (this.showShelterInfo) {
        this.currentStep = SignupStep.ShelterInfo;
      } else {
        this.onSubmitRegistration();
      }
    } else {
      this.markFormGroupTouched(this.getAccountDetailsForm());
    }
  }

  onAccountDetailsBack(): void {
    // Set direction for animation
    this.stepDirection = 'prev';
    this.currentStep = SignupStep.PersonalInfo;
  }

  onShelterInfoBack(): void {
    this.stepDirection = 'prev';
    if (this.isGoogleAuthenticated()) {
      this.onAccountDetailsBack();
      return;
    }

    this.currentStep = SignupStep.AccountDetails;
  }

  onSubmitRegistration(): void {
    this.markFormGroupTouched(this.registrationForm);
    this.logService.logFormatted(this.registrationForm.getRawValue());
    if (this.registrationForm.valid) {
      this.isLoading = true;
      this.isSubmitting = true;
      const formValue = this.registrationForm.getRawValue();

      const payload: RegisterPayload = {
        user: {
          id: '',
          email: formValue.email,
          password:
            formValue.authProvider === AuthProvider.Local
              ? formValue.password
              : '',
          fullName: formValue.fullName,
          role: formValue.role,
          phone: formValue.phone,
          location: formValue.location,
          authProvider: formValue.authProvider,
          authProviderId: formValue.authProviderId,
          profilePhotoId: formValue.profilePhoto,
          hasPhoneVerified: formValue.hasPhoneVerified,
          hasEmailVerified: formValue.hasEmailVerified,
        },
      };

      if (formValue.isShelter) {
        payload.shelter = {
          id: '',
          userId: '',
          shelterName: formValue.shelter.shelterName,
          description: formValue.shelter.description,
          website: formValue.shelter.website ? formValue.shelter.website : null,
          socialMedia: this.getSocialMediaPayload(
            formValue.shelter.socialMedia
          ),
          operatingHours: this.getOperatingHoursPayload(
            formValue.shelter.operatingHours
          ),
          verificationStatus: 1,
          verifiedBy: undefined,
        };
      }

      this.authService.register(payload).subscribe({
        next: (user: User) => {
          this.userId = user.id!;
          if (!user.hasPhoneVerified) {
            this.resendOtp();
            this.currentStep = SignupStep.OtpVerification;
          } else if (!user.hasEmailVerified) {
            this.resendEmailVerification();
            this.currentStep = SignupStep.EmailConfirmation;
          } else {
            this.router.navigate(['/auth/verified/user'], {
              queryParams: {
                complete: formValue.role,
                identification: this.userId,
              },
            });
          }

          this.isLoading = false;
          this.isSubmitting = false;
          this.hasUnsavedChangesFlag = false;
          this.error = undefined;
        },
        error: (error) => {
          console.error('Registration error:', error);
          this.isLoading = false;
          this.isSubmitting = false;
          this.error = this.errorHandler.handleAuthError(error);
        },
      });
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
      return !value;
    })
      ? null
      : operatingHours;
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

  onSubmitOtp(): void {
    if (this.isLoading || this.isSubmitting) {
      return;
    }
    this.markFormGroupTouched(this.otpForm);
    this.error = undefined;

    if (this.otpForm.valid) {
      this.isLoading = true;
      this.isSubmitting = true;
      const { otp } = this.otpForm.value;
      const { phone, email, role } = this.registrationForm.getRawValue();

      this.authService
        .verifyOtp({
          phone: phone,
          otp: +otp,
          id: this.userId,
          email: email,
        } as OtpPayload)
        .subscribe({
          next: () => {
            this.isLoading = false;
            this.isSubmitting = false;
            this.hasUnsavedChangesFlag = false;
            this.suppressFormGuard = true;
            if (this.otpForm) {
              this.otpForm.markAsPristine();
            }
            this.error = undefined;

            // Clear phone verification from storage since it's now verified
            this.secureStorageService.removeItem('unverifiedPhone');

            // Check if coming from login and if email verification is still needed
            if (this.fromLogin) {
             const fromGoogleLogin = this.secureStorageService.getItem<string>('fromGoogleLogin') === 'true';
             if (fromGoogleLogin) {
               this.secureStorageService.removeItem('fromGoogleLogin');
               this.router.navigate(['/home']);
             } else {
               this.authService.me().subscribe({
                 next: (account) => {
                   if (account && account.isEmailVerified) {
                     this.router.navigate(['/home']);
                   } else {
                     this.currentStep = SignupStep.EmailConfirmation;
                     this.resendEmailVerification();
                   }
                 },
                 error: () => {
                   const unverifiedEmail = this.secureStorageService.getItem<string>('unverifiedEmail');
                   if (unverifiedEmail) {
                     this.currentStep = SignupStep.EmailConfirmation;
                     this.resendEmailVerification();
                   } else {
                     this.router.navigate(['/home']);
                   }
                 }
               });
             }
            } else {
              // Regular signup flow
              if (!this.registrationForm.get('hasEmailVerified')?.value) {
                this.currentStep = SignupStep.EmailConfirmation;
                this.resendEmailVerification();
              } else {
                this.router.navigate(['/auth/login']);
                this.snackbarService.showSuccess({
                  message: this.translationService.translate(
                    'APP.AUTH.VERIFICATION_SUCCESS'
                  ),
                  subMessage: this.translationService.translate(
                    'APP.AUTH.VERIFICATION_SUCCESS_SUBMESSAGE'
                  ),
                });
              }
            }
          },
          error: (error: HttpErrorResponse) => {
            this.isLoading = false;
            this.isSubmitting = false;
            this.error = {
              title: this.translationService.translate(
                'APP.AUTH.SIGNUP.OTP_ERROR_TITLE'
              ),
              message: this.translationService.translate(
                'APP.AUTH.SIGNUP.OTP_ERROR_MESSAGE'
              ),
              type: 'error',
            };
            console.error('OTP verification error:', error);
          },
        });
    }
  }

  onOtpCompleted(otp: string): void {
    if (otp.length === 6 && !this.isLoading) {
      this.onSubmitOtp();
    }
  }

  resendOtp(): void {
    const { phone, email } = this.registrationForm.getRawValue();

    // For login flow, we might not have userId, so we'll use the phone/email for OTP
    const otpPayload: OtpPayload = {
      phone: phone,
      id: this.userId || '', // Empty string if no userId (login flow)
      email: email,
    };

    this.authService.sendOtp(otpPayload).subscribe({
      next: () => {
        this.startOtpTimer();
        this.error = undefined;
      },
      error: (error: HttpErrorResponse) => {
        this.error = {
          title: this.translationService.translate(
            'APP.AUTH.SIGNUP.RESEND_OTP_ERROR_TITLE'
          ),
          message: this.translationService.translate(
            'APP.AUTH.SIGNUP.RESEND_OTP_ERROR_MESSAGE'
          ),
          type: 'error',
        };
        console.error('Resend OTP error:', error);
      },
    });
  }

  resendEmailVerification(): void {
    const { email } = this.registrationForm.getRawValue();
    this.isLoading = true;
    this.error = undefined;

    this.authService.sendVerificationEmail(email).subscribe({
      next: () => {
        this.isLoading = false;
        this.error = {
          title: this.translationService.translate(
            'APP.AUTH.SIGNUP.EMAIL_VERIFICATION_SENT_TITLE'
          ),
          message: this.translationService.translate(
            'APP.AUTH.SIGNUP.EMAIL_VERIFICATION_SENT_MESSAGE'
          ),
          type: 'info',
        };
      },
      error: (error: HttpErrorResponse) => {
        this.isLoading = false;
        this.error = {
          title: this.translationService.translate(
            'APP.AUTH.SIGNUP.EMAIL_VERIFICATION_RESEND_ERROR_TITLE'
          ),
          message: this.translationService.translate(
            'APP.AUTH.SIGNUP.EMAIL_VERIFICATION_RESEND_ERROR_MESSAGE'
          ),
          type: 'error',
        };
        console.error('Resend email verification error:', error);
      },
    });
  }

  onEmailVerificationComplete(): void {
    // Clear email verification from storage since it's now complete
    this.secureStorageService.removeItem('unverifiedEmail');
    this.hasUnsavedChangesFlag = false;

    if (this.fromLogin) {
      // Coming from login, redirect to home
      this.router.navigate(['/home']);
    } else {
      // Regular signup flow, redirect to verification complete page
      const { role } = this.registrationForm.getRawValue();
      this.router.navigate(['/auth/verified/user'], {
        queryParams: { complete: role, identification: this.userId },
      });
    }
  }

  private startOtpTimer(): void {
    this.resendOtpTimer = 30;
    clearInterval(this.resendOtpInterval);
    this.resendOtpInterval = setInterval(() => {
      if (this.resendOtpTimer > 0) {
        this.resendOtpTimer--;
      } else {
        clearInterval(this.resendOtpInterval);
      }
    }, 1000);
  }

  private formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}
