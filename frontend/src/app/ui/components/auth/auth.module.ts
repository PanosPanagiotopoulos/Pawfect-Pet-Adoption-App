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
} from '@ng-icons/lucide';

import { LoginComponent } from './login.component';
import { SignupComponent } from './signup.component';
import { AuthButtonComponent } from './shared/auth-button/auth-button.component';
import { ValidationMessageComponent } from '../../../common/ui/validation-message.component';
import { GoogleLoginButtonComponent } from './shared/google-login-button/google-login-button.component';
import { PersonalInfoComponent } from './sign-up-steps/personal-info/personal-info.component';
import { AccountDetailsComponent } from './sign-up-steps/account-details/account-details.component';
import { ShelterInfoComponent } from './sign-up-steps/shelter-info/shelter-info.component';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { TimeInputComponent } from 'src/app/common/ui/time-input.component';
import { PhoneInputComponent } from 'src/app/common/ui/phone-input.component';
import { TextAreaInputComponent } from 'src/app/common/ui/text-area-input.component';
import { FileDropAreaComponent } from 'src/app/common/ui/file-drop-area.component';

const COMPONENTS = [LoginComponent, SignupComponent];

@NgModule({
  declarations: [...COMPONENTS],
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
    }),
    FormInputComponent,
    AuthButtonComponent,
    TimeInputComponent,
    ValidationMessageComponent,
    PhoneInputComponent,
    GoogleLoginButtonComponent,
    PersonalInfoComponent,
    AccountDetailsComponent,
    ShelterInfoComponent,
    TextAreaInputComponent,
    FileDropAreaComponent,
  ],
})
export class AuthModule {}
