import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LoginComponent } from './login.component';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { SignupComponent } from './signup.component';

@NgModule({
  declarations: [LoginComponent, SignupComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild([{ path: 'login', component: LoginComponent }]),
    RouterModule.forChild([{ path: 'sign-up', component: SignupComponent }])
  ],
})
export class AuthModule {}
