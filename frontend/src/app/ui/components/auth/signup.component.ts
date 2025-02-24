import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
  FormControl,
} from '@angular/forms';
import { Router } from '@angular/router';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { AuthService } from 'src/app/services/auth.service';
import { RegisterPayload } from 'src/app/models/auth/auth.model';
import { UserRole } from 'src/app/models/user/user.model';
import { takeUntil } from 'rxjs';
import { CustomValidators } from './validators/custom.validators';

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
    fullName: AbstractControl;
    phone: AbstractControl;
    countryCode: AbstractControl;
    phoneNumber: AbstractControl;
    role: AbstractControl;
    hasEmailVerified: AbstractControl;
    isShelter: AbstractControl;
    location: LocationFormGroup;
    shelter: ShelterFormGroup;
  };
}

interface OtpFormGroup extends FormGroup {
  controls: {
    otp: AbstractControl;
  };
}

enum SignupStep {
  Registration = 1,
  OtpVerification = 2,
  EmailConfirmation = 3,
}

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css'],
})
export class SignupComponent extends BaseComponent implements OnInit {
  currentStep = SignupStep.Registration;
  SignupStep = SignupStep;
  isLoading = false;
  userId: string = '';
  resendOtpTimer = 0;
  resendOtpInterval: any;

  registrationForm!: RegistrationFormGroup;
  otpForm!: OtpFormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {
    super();
    this.initializeForms();
  }

  private initializeForms(): void {
    const operatingHoursGroup = this.fb.group({
      monday: ['09:00,17:00'],
      tuesday: ['09:00,17:00'],
      wednesday: ['09:00,17:00'],
      thursday: ['09:00,17:00'],
      friday: ['09:00,17:00'],
      saturday: ['09:00,17:00'],
      sunday: ['09:00,17:00'],
    });

    this.registrationForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: [
        '',
        [Validators.required, CustomValidators.passwordValidator()],
      ],
      fullName: ['', [Validators.required, Validators.minLength(5)]],
      phone: [''],
      countryCode: ['+30', Validators.required],
      phoneNumber: [
        '',
        [Validators.required, Validators.pattern(/^\d{1,14}$/)],
      ],
      role: [
        UserRole.User,
        [Validators.required, CustomValidators.roleValidator()],
      ],
      hasEmailVerified: [false],
      isShelter: [false],
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
        description: [''],
        website: [''],
        socialMedia: this.fb.group({
          facebook: [''],
          instagram: [''],
        }) as SocialMediaFormGroup,
        operatingHours: operatingHoursGroup,
      }) as ShelterFormGroup,
    }) as RegistrationFormGroup;

    // Subscribe to isShelter changes to update validations
    this.registrationForm
      .get('isShelter')
      ?.valueChanges.subscribe((isShelter) => {
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
              ?.setValidators([
                Validators.required,
                CustomValidators.operatingHoursValidator(),
              ]);
          });

          // Update social media validators
          shelterForm
            .get('socialMedia.facebook')
            ?.setValidators(CustomValidators.socialMediaValidator('facebook'));
          shelterForm
            .get('socialMedia.instagram')
            ?.setValidators(CustomValidators.socialMediaValidator('instagram'));
        } else {
          // Clear all shelter-related validators when not a shelter
          shelterForm.get('shelterName')?.clearValidators();
          shelterForm.get('description')?.clearValidators();

          Object.keys(operatingHoursGroup.controls).forEach((key) => {
            operatingHoursGroup.get(key)?.clearValidators();
          });

          shelterForm.get('socialMedia.facebook')?.clearValidators();
          shelterForm.get('socialMedia.instagram')?.clearValidators();

          // Reset shelter form values
          shelterForm.reset();
        }

        // Update validation state for all shelter form controls
        shelterForm.get('shelterName')?.updateValueAndValidity();
        shelterForm.get('description')?.updateValueAndValidity();
        shelterForm.get('socialMedia.facebook')?.updateValueAndValidity();
        shelterForm.get('socialMedia.instagram')?.updateValueAndValidity();
        operatingHoursGroup.updateValueAndValidity();
      });

    this.otpForm = this.fb.group({
      otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]],
    }) as OtpFormGroup;
  }

  // *TODO* Implement register with Google
  registerWithGoogle(): void {}

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

  ngOnInit(): void {
    history.pushState(null, '', location.href);
    window.onpopstate = () => {
      history.pushState(null, '', location.href);
    };
  }

  onSubmitRegistration(): void {
    const formValue = this.registrationForm.value;

    const separator = (key: string, value: any) => {
      return value ? value : 'Not set';
    };
    console.log(JSON.stringify(formValue, separator, 2));
    if (this.registrationForm.valid) {
      this.isLoading = true;
      const formValue = this.registrationForm.value;

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
          OperatingHours: formValue.shelter.operatingHours,
          VerificationStatus: 1,
          VerifiedBy: '',
        };
      }

      this.userId = 'userId';
      this.startOtpTimer();
      this.currentStep = SignupStep.OtpVerification;
      this.isLoading = false;
      // this.authService
      //   .register(payload)
      //   .pipe(takeUntil(this._destroyed))
      //   .subscribe({
      //     next: (userId) => {
      // this.userId = userId;
      // this.startOtpTimer();
      // this.currentStep = SignupStep.OtpVerification;
      // this.isLoading = false;
      //     },
      //     error: (error) => {
      //       console.error('Registration error:', error);
      //       this.isLoading = false;
      //     },
      // });
    } else {
      Object.keys(this.registrationForm.controls).forEach((key) => {
        const control = this.registrationForm.get(key);
        if (control?.invalid) {
          control.markAsTouched();
        }
      });
    }
  }

  onSubmitOtp(): void {
    if (this.otpForm.valid) {
      this.isLoading = true;
      const { otp } = this.otpForm.value;
      const { phone, email } = this.registrationForm.value;

      this.currentStep = SignupStep.EmailConfirmation;
      this.isLoading = false;
      // this.authService
      //   .verifyOtp(phone, otp, this.userId, email)
      //   .pipe(takeUntil(this._destroyed))
      //   .subscribe({
      //     next: () => {
      //       this.currentStep = SignupStep.EmailConfirmation;
      //       this.isLoading = false;
      //     },
      //     error: (error) => {
      //       console.error('OTP verification error:', error);
      //       this.isLoading = false;
      //     },
      //   });
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

  override ngOnDestroy(): void {
    super.ngOnDestroy();
    clearInterval(this.resendOtpInterval);
    window.onpopstate = null;
  }
}
