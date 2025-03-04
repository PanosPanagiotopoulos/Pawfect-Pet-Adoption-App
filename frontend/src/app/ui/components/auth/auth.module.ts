import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgIconsModule } from '@ng-icons/core';
import {
  lucideHeart,
  lucideUser,
  lucideCheck,
  lucideX,
  lucideLogOut,
  lucideClock,
  lucideUpload,
  lucideFile,
  lucideTrash,
  lucideImage,
  lucideCamera,
} from '@ng-icons/lucide';

import { LoginComponent } from './login.component';
import { SignupComponent } from './signup.component';
import { ValidationMessageComponent } from './shared/validation-message/validation-message.component';
import { AuthButtonComponent } from './shared/auth-button/auth-button.component';
import { GoogleLoginButtonComponent } from './shared/google-login-button/google-login-button.component';
import { PersonalInfoComponent } from './sign-up-steps/personal-info/personal-info.component';
import { AccountDetailsComponent } from './sign-up-steps/account-details/account-details.component';
import { PreferencesComponent } from './sign-up-steps/preferences/preferences.component';
import { ShelterInfoComponent } from './sign-up-steps/shelter-info/shelter-info.component';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { FormFieldComponent } from 'src/app/common/ui/form-field.component';
import { PhoneInputComponent } from 'src/app/common/ui/phone-input.component';
import { TimeInputComponent } from 'src/app/common/ui/time-input.component';
import { ClockTimePickerComponent } from 'src/app/common/ui/clock-time-picker.component';
import { TextAreaInputComponent } from 'src/app/common/ui/text-area-input.component';
import { FileDropAreaComponent } from 'src/app/common/ui/file-drop-area.component';
import { OtpInputComponent } from 'src/app/common/ui/otp-input.component';

@NgModule({
  declarations: [LoginComponent, SignupComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      { path: 'login', component: LoginComponent },
      { path: 'sign-up', component: SignupComponent },
    ]),
    NgIconsModule.withIcons({
      lucideHeart,
      lucideUser,
      lucideCheck,
      lucideX,
      lucideLogOut,
      lucideClock,
      lucideUpload,
      lucideFile,
      lucideTrash,
      lucideImage,
      lucideCamera,
    }),
    FormInputComponent,
    FormFieldComponent,
    ValidationMessageComponent,
    PhoneInputComponent,
    TimeInputComponent,
    ClockTimePickerComponent,
    TextAreaInputComponent,
    FileDropAreaComponent,
    PreferencesComponent,
    PersonalInfoComponent,
    AccountDetailsComponent,
    ShelterInfoComponent,
    AuthButtonComponent,
    GoogleLoginButtonComponent,
    OtpInputComponent,
  ],
  exports: [LoginComponent, SignupComponent],
})
export class AuthModule {}