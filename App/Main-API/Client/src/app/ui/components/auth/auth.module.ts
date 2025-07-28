import { NgModule, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { NgIconsModule } from '@ng-icons/core';
import { LoggedInGuard } from 'src/app/common/guards/logged-in.guard';
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
  lucideTriangle,
} from '@ng-icons/lucide';

// Auth components
import { LoginComponent } from './login.component';
import { SignupComponent } from './signup.component';
import { ResetPasswordRequestComponent } from './reset-password/reset-password-request.component';
import { ResetPasswordComponent } from './reset-password/reset-password.component';
import { NotFoundComponent } from '../not-found/not-found.component';
import { GoogleCallbackComponent } from './google-callback.component';

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
import { VerifiedComponent } from './sign-up-steps/verified.component';
import { ShelterInfoComponent } from './sign-up-steps/shelter-info/shelter-info.component';
import { ErrorMessageBannerComponent } from 'src/app/common/ui/error-message-banner.component';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@NgModule({
  declarations: [LoginComponent, SignupComponent, VerifiedComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      { path: 'login', component: LoginComponent, canActivate: [LoggedInGuard] },
      { path: 'sign-up', component: SignupComponent, canActivate: [LoggedInGuard] },
      { path: 'verified', component: VerifiedComponent },
      { path: 'google/callback-page', component: GoogleCallbackComponent },
      {
        path: 'reset-password-request',
        component: ResetPasswordRequestComponent,
        canActivate: [LoggedInGuard]
      },
      { path: 'reset-password', component: ResetPasswordComponent, canActivate: [LoggedInGuard] },
    ]),
    FormInputComponent,
    TextAreaInputComponent,
    AuthButtonComponent,
    OtpInputComponent,
    GoogleLoginButtonComponent,
    ValidationMessageComponent,
    PersonalInfoComponent,
    AccountDetailsComponent,
    ShelterInfoComponent,
    ErrorMessageBannerComponent,
    TranslatePipe,
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
      lucideTriangle,
    }),
  ],
  exports: [LoginComponent, SignupComponent, VerifiedComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class AuthModule {}
