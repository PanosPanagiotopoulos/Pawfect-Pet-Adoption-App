import { Component, OnInit, ViewChild, OnDestroy } from '@angular/core';
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
import { RegisterPayload } from 'src/app/models/auth/auth.model';
import { UserRole } from 'src/app/models/user/user.model';
import { takeUntil } from 'rxjs';
import { CustomValidators } from './validators/custom.validators';
import {
  trigger,
  transition,
  style,
  animate,
  state,
} from '@angular/animations';
import { OtpInputComponent } from 'src/app/common/ui/otp-input.component';

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
  implements OnInit, OnDestroy
{
  @ViewChild(OtpInputComponent) otpInputComponent?: OtpInputComponent;

  currentStep = SignupStep.PersonalInfo;
  SignupStep = SignupStep;
  isLoading = false;
  userId: string = '';
  resendOtpTimer = 0;
  resendOtpInterval: any;
  showShelterInfo = false;
  stepDirection: 'next' | 'prev' = 'next';
  fromLogin = false;

  registrationForm!: RegistrationFormGroup;
  otpForm!: OtpFormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute
  ) {
    super();
    this.initializeForms();
  }

  ngOnInit(): void {
    // Check if we're coming from login with an unverified email
    const navigation = this.router.getCurrentNavigation();
    if (navigation?.extras.state) {
      const state = navigation.extras.state as {
        step: SignupStep;
        fromLogin: boolean;
      };

      if (state.step === SignupStep.EmailConfirmation && state.fromLogin) {
        this.fromLogin = true;
        this.currentStep = SignupStep.EmailConfirmation;

        this.registrationForm.get('hasEmailVerified')?.setValue(true);
      }
    }

    history.pushState(null, '', location.href);
    window.onpopstate = () => {
      history.pushState(null, '', location.href);
    };
  }

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    clearInterval(this.resendOtpInterval);
    window.onpopstate = null;
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
        [Validators.required, CustomValidators.passwordValidator()],
      ],
      confirmPassword: ['', [Validators.required]],
      fullName: ['', [Validators.required, Validators.minLength(5)]],
      phone: [''],
      countryCode: ['+30', Validators.required],
      phoneNumber: [
        '',
        [Validators.required, Validators.pattern(/^\d{1,14}$/)],
      ],
      role: [UserRole.User, [Validators.required]],
      isShelter: [false],
      profilePhoto: [null],
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

    // Add password confirmation validator
    this.registrationForm
      .get('confirmPassword')
      ?.setValidators([
        Validators.required,
        CustomValidators.matchValidator('password'),
      ]);

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

  loginWithGoogle(): void {
    this.isLoading = true;

    // For testing without backend
    setTimeout(() => {
      this.isLoading = false;
      this.router.navigate(['/']);
    }, 1500);

    // Uncomment this when ready to connect to backend
    // this.authService
    //   .loginWithGoogle('')
    //   .pipe(takeUntil(this._destroyed))
    //   .subscribe({
    //     next: () => {
    //       this.router.navigate(['/']);
    //     },
    //     error: (error) => {
    //       console.error('Google login error:', error);
    //     },
    //   });
  }

  getStepsToShow() {
    // Base steps that are always shown
    const steps = [
      {
        value: SignupStep.PersonalInfo,
        displayNumber: 1,
        label: 'Προσωπικά Στοιχεία',
      },
      {
        value: SignupStep.AccountDetails,
        displayNumber: 2,
        label: 'Λογαριασμός',
      },
    ];

    // Conditionally add shelter info step
    if (this.showShelterInfo) {
      steps.push({
        value: SignupStep.ShelterInfo,
        displayNumber: 3,
        label: 'Καταφύγιο',
      });
    }

    steps.push({
      value: SignupStep.OtpVerification,
      displayNumber: this.showShelterInfo ? 4 : 3,
      label: 'Επαλήθευση',
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
    const personalInfoForm = this.fb.group({
      fullName: this.registrationForm.get('fullName'),
      email: this.registrationForm.get('email'),
      countryCode: this.registrationForm.get('countryCode'),
      phoneNumber: this.registrationForm.get('phoneNumber'),
      phone: this.registrationForm.get('phone'),
      location: this.registrationForm.get('location'),
      profilePhoto: this.registrationForm.get('profilePhoto'),
    });
    return personalInfoForm;
  }

  getAccountDetailsForm(): FormGroup {
    const accountDetailsForm = this.fb.group({
      password: this.registrationForm.get('password'),
      confirmPassword: this.registrationForm.get('confirmPassword'),
    });
    return accountDetailsForm;
  }

  getFileUploadForm(): FormGroup {
    const fileUploadForm = this.fb.group({
      profilePhoto: this.registrationForm.get('profilePhoto'),
    });
    return fileUploadForm;
  }

  onPersonalInfoNext(): void {
    // Set direction for animation
    this.stepDirection = 'next';
    // Always proceed to next step, validation will be handled in the personal-info component
    this.currentStep = SignupStep.AccountDetails;
  }

  onAccountDetailsNext(): void {
    // Set direction for animation
    this.stepDirection = 'next';

    // Check if passwords match
    const password = this.registrationForm.get('password')?.value;
    const confirmPassword = this.registrationForm.get('confirmPassword')?.value;

    if (password !== confirmPassword) {
      this.registrationForm
        .get('confirmPassword')
        ?.setErrors({ mismatch: true });
      // Mark as touched to show validation errors
      this.registrationForm.get('confirmPassword')?.markAsTouched();
      return;
    }

    // If form is valid, proceed to next step
    if (this.getAccountDetailsForm().valid) {
      if (this.showShelterInfo) {
        this.currentStep = SignupStep.ShelterInfo;
      } else {
        this.onSubmitRegistration();
        this.currentStep = SignupStep.OtpVerification;
      }
    } else {
      // Mark all fields as touched to show validation errors
      this.markFormGroupTouched(this.getAccountDetailsForm());
    }
  }

  onAccountDetailsBack(): void {
    // Set direction for animation
    this.stepDirection = 'prev';
    this.currentStep = SignupStep.PersonalInfo;
  }

  onShelterInfoBack(): void {
    // Set direction for animation
    this.stepDirection = 'prev';
    this.currentStep = SignupStep.AccountDetails;
  }

  onSubmitRegistration(): void {
    // Mark all fields as touched to trigger validation messages immediately
    this.markFormGroupTouched(this.registrationForm);
    if (this.registrationForm.valid) {
      this.isLoading = true;
      const formValue = this.registrationForm.value;
      // Log the complete form data
      console.log('Registration Form Data:', {
        personalInfo: {
          email: formValue.email,
          fullName: formValue.fullName,
          phone: formValue.phone,
          location: formValue.location,
        },
        accountDetails: {
          password: formValue.password,
          role: formValue.role,
        },
        profilePhoto: formValue.profilePhoto
          ? {
              name: formValue.profilePhoto.name || 'unknown',
              size: formValue.profilePhoto.size
                ? this.formatFileSize(formValue.profilePhoto.size)
                : 'unknown',
              type: formValue.profilePhoto.type || 'unknown',
            }
          : 'No profile photo uploaded',
        shelterInfo: formValue.isShelter
          ? {
              shelterName: formValue.shelter.shelterName,
              description: formValue.shelter.description,
              website: formValue.shelter.website,
              socialMedia: formValue.shelter.socialMedia,
              operatingHours: formValue.shelter.operatingHours,
            }
          : null,
      });

      const payload: RegisterPayload = {
        user: {
          Id: '',
          Email: formValue.email,
          Password: formValue.password,
          FullName: formValue.fullName,
          Role: formValue.role,
          Phone: formValue.phone,
          Location: formValue.location,
          AuthProvider: 1,
          AttachedPhoto: formValue.profilePhoto,
          HasPhoneVerified: false,
          HasEmailVerified: formValue.hasEmailVerified || false,
        },
      };

      if (formValue.isShelter) {
        payload.shelter = {
          Id: '',
          UserId: '',
          ShelterName: formValue.shelter.shelterName,
          Description: formValue.shelter.description,
          Website: formValue.shelter.website,
          SocialMedia: formValue.shelter.socialMedia,
          OperatingHours: this.getOperatingHoursPayload(
            formValue.shelter.operatingHours
          ),
          VerificationStatus: 1,
          VerifiedBy: '',
        };
      }

      const separator = (key: string, value: any) => {
        return value ? value : 'No value found to print';
      };
      console.log(JSON.stringify(payload, separator, 2));

      this.authService
        .register(payload)
        .pipe(takeUntil(this._destroyed))
        .subscribe({
          next: (userId) => {
            this.userId = userId;
            this.startOtpTimer();
            this.currentStep = SignupStep.OtpVerification;
            this.isLoading = false;
          },
          error: (error) => {
            console.error('Registration error:', error);
            this.isLoading = false;
          },
        });
    }
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
    // Mark all fields as touched to trigger validation messages immediately
    this.markFormGroupTouched(this.otpForm);

    if (this.otpForm.valid) {
      this.isLoading = true;
      const { otp } = this.otpForm.value;
      const { phone, email } = this.registrationForm.value;

      this.authService
        .verifyOtp(phone, otp, this.userId, email)
        .pipe(takeUntil(this._destroyed))
        .subscribe({
          next: () => {
            this.currentStep = SignupStep.EmailConfirmation;
            this.isLoading = false;
          },
          error: (error) => {
            console.error('OTP verification error:', error);
            this.isLoading = false;
          },
        });
    }
  }

  onOtpCompleted(otp: string): void {
    // Auto-submit when all digits are filled
    if (otp.length === 6) {
      this.onSubmitOtp();
    }
  }

  resendOtp(): void {
    if (this.resendOtpTimer === 0) {
      const { phone } = this.registrationForm.value;

      this.authService
        .sendOtp(phone)
        .pipe(takeUntil(this._destroyed))
        .subscribe({
          next: () => {
            this.startOtpTimer();
          },
          error: (error) => {
            console.error('Resend OTP error:', error);
          },
        });

      // For testing without backend
      this.startOtpTimer();
    }
  }

  // Method to resend email verification
  resendEmailVerification(): void {
    const email = this.registrationForm.get('email')?.value;
    if (email) {
      this.isLoading = true;

      this.authService
        .sendVerificationEmail(email)
        .pipe(takeUntil(this._destroyed))
        .subscribe({
          next: () => {
            this.isLoading = false;
            // Show success message
          },
          error: (error) => {
            this.isLoading = false;
            console.error('Resend email verification error:', error);
          },
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
