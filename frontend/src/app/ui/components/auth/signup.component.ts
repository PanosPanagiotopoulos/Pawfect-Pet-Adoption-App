import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, AbstractControl, FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { AuthService } from 'src/app/services/auth.service';
import { RegisterPayload } from 'src/app/models/auth/auth.model';
import { UserRole } from 'src/app/models/user/user.model';
import { takeUntil } from 'rxjs';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgIconsModule } from '@ng-icons/core';
import { TimeInputComponent } from './shared/time-input/time-input.component';
import { FormFieldComponent } from './shared/form-field/form-field.component';
import { ValidationMessageComponent } from './shared/validation-message/validation-message.component';
import { FormInputComponent } from './shared/form-input/form-input.component';
import { AuthButtonComponent } from './shared/auth-button/auth-button.component';

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
    role: AbstractControl;
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
  EmailConfirmation = 3
}

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css'],
  standalone: false
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
      monday: [''],
      tuesday: [''],
      wednesday: [''],
      thursday: [''],
      friday: [''],
      saturday: [''],
      sunday: ['']
    });

    operatingHoursGroup.valueChanges.subscribe(hours => {
      const hasAnyHours = Object.values(hours).some(time => time);
      if (hasAnyHours) {
        Object.keys(hours).forEach(day => {
          const control = operatingHoursGroup.get(day);
          control?.setValidators([
            Validators.required,
            Validators.pattern(/^([0-1]?[0-9]|2[0-3]):[0-5][0-9],([0-1]?[0-9]|2[0-3]):[0-5][0-9]$/)
          ]);
          control?.updateValueAndValidity({ emitEvent: false });
        });
      } else {
        Object.keys(hours).forEach(day => {
          const control = operatingHoursGroup.get(day);
          control?.clearValidators();
          control?.updateValueAndValidity({ emitEvent: false });
        });
      }
    });

    this.registrationForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      fullName: ['', Validators.required],
      phone: ['', [Validators.required, Validators.pattern(/^\+?[1-9]\d{1,14}$/)]],
      role: [UserRole.User],
      isShelter: [false],
      location: this.fb.group({
        city: ['', Validators.required],
        zipCode: ['', Validators.required],
        address: ['', Validators.required],
        number: ['', Validators.required]
      }) as LocationFormGroup,
      shelter: this.fb.group({
        shelterName: ['', Validators.required],
        description: [''],
        website: [''],
        socialMedia: this.fb.group({
          facebook: [''],
          instagram: ['']
        }) as SocialMediaFormGroup,
        operatingHours: operatingHoursGroup
      }) as ShelterFormGroup
    }) as RegistrationFormGroup;

    this.otpForm = this.fb.group({
      otp: ['', [Validators.required, Validators.pattern(/^\d{6}$/)]]
    }) as OtpFormGroup;

    this.registrationForm.get('isShelter')?.valueChanges.subscribe(isShelter => {
      const role = isShelter ? UserRole.Shelter : UserRole.User;
      this.registrationForm.patchValue({ role });
      
      const shelterForm = this.getShelterForm();
      if (shelterForm) {
        if (isShelter) {
          shelterForm.get('shelterName')?.setValidators(Validators.required);
          shelterForm.get('description')?.setValidators(Validators.required);
        } else {
          shelterForm.get('shelterName')?.clearValidators();
          shelterForm.get('description')?.clearValidators();
        }
        shelterForm.updateValueAndValidity();
      }
    });
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
    return this.getShelterForm().get('operatingHours') as OperatingHoursFormGroup;
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
          HasEmailVerified: false
        }
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
          VerifiedBy: ''
        };
      }

      this.authService.register(payload)
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
          }
        });
    } else {
      Object.keys(this.registrationForm.controls).forEach(key => {
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

      this.authService.verifyOtp(phone, otp, this.userId, email)
        .pipe(takeUntil(this._destroyed))
        .subscribe({
          next: () => {
            this.currentStep = SignupStep.EmailConfirmation;
            this.isLoading = false;
          },
          error: (error) => {
            console.error('OTP verification error:', error);
            this.isLoading = false;
          }
        });
    }
  }

  resendOtp(): void {
    if (this.resendOtpTimer === 0) {
      const { phone } = this.registrationForm.value;
      this.authService.sendOtp(phone)
        .pipe(takeUntil(this._destroyed))
        .subscribe({
          next: () => {
            this.startOtpTimer();
          },
          error: (error) => {
            console.error('Resend OTP error:', error);
          }
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