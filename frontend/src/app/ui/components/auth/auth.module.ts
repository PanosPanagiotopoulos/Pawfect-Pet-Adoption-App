import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgIconsModule } from '@ng-icons/core';
import {
  lucideInfo,
  lucideMapPin,
  lucideMenu,
  lucideX,
  lucideHeart,
  lucideUser,
  lucideSearch,
  lucideMessageCircle,
  lucideHouse,
  lucidePhone,
  lucideMail,
  lucideLogOut,
  lucideClock,
  lucideUpload,
  lucideFile,
  lucideCheck,
} from '@ng-icons/lucide';
// Auth components
import { LoginComponent } from './login.component';
import { SignupComponent } from './signup.component';

// Shared components (standalone)
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { TextAreaInputComponent } from 'src/app/common/ui/text-area-input.component';
import { AuthButtonComponent } from './shared/auth-button/auth-button.component';
import { OtpInputComponent } from 'src/app/common/ui/otp-input.component';
import { GoogleLoginButtonComponent } from './shared/google-login-button/google-login-button.component';
import { ValidationMessageComponent } from './shared/validation-message/validation-message.component';

// Signup step components
import { PersonalInfoComponent } from './sign-up-steps/personal-info/personal-info.component';
import { AccountDetailsComponent } from './sign-up-steps/account-details/account-details.component';
import { PreferencesComponent } from './sign-up-steps/preferences/preferences.component';
import { VerifiedComponent } from './sign-up-steps/verified.component';
import { ShelterInfoComponent } from './sign-up-steps/shelter-info/shelter-info.component';

@NgModule({
  declarations: [LoginComponent, SignupComponent, VerifiedComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      { path: 'login', component: LoginComponent },
      { path: 'sign-up', component: SignupComponent },
      {
        path: 'verified',
        component: VerifiedComponent,
      },
    ]),
    FormInputComponent,
    TextAreaInputComponent,
    AuthButtonComponent,
    OtpInputComponent,
    GoogleLoginButtonComponent,
    ValidationMessageComponent,
    PersonalInfoComponent,
    AccountDetailsComponent,
    PreferencesComponent,
    ShelterInfoComponent,
    NgIconsModule.withIcons({
      lucideHeart,
      lucideSearch,
      lucideMessageCircle,
      lucidePhone,
      lucideMail,
      lucideMenu,
      lucideUser,
      lucideX,
      lucideHouse,
      lucideInfo,
      lucideLogOut,
      lucideClock,
      lucideUpload,
      lucideFile,
      lucideCheck,
    }),
  ],
  exports: [LoginComponent, SignupComponent, VerifiedComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AuthModule {}
