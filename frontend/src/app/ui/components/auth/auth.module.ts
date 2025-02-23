import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgIconsModule } from '@ng-icons/core';
import { lucideHeart, lucideUser, lucideCheck, lucideX } from '@ng-icons/lucide';

import { LoginComponent } from './login.component';
import { SignupComponent } from './signup.component';
import { FormInputComponent } from './shared/form-input/form-input.component';
import { AuthButtonComponent } from './shared/auth-button/auth-button.component';
import { TimeInputComponent } from './shared/time-input/time-input.component';
import { FormFieldComponent } from './shared/form-field/form-field.component';
import { ValidationMessageComponent } from './shared/validation-message/validation-message.component';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      { path: 'login', component: LoginComponent },
      { path: 'sign-up', component: SignupComponent }
    ]),
    NgIconsModule.withIcons({ 
      lucideHeart, 
      lucideUser,
      lucideCheck,
      lucideX
    }),
    FormInputComponent,
    AuthButtonComponent,
    TimeInputComponent,
    FormFieldComponent,
    ValidationMessageComponent
  ],
  declarations: [
    LoginComponent,
    SignupComponent
  ]
})
export class AuthModule { }