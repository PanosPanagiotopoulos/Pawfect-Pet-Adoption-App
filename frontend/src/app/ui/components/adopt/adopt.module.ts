import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { NgIconsModule } from '@ng-icons/core';
import { AdoptionFormComponent } from './components/adoption-form/adoption-form.component';
import { ShelterInfoComponent } from './components/shelter-info/shelter-info.component';
import { AdoptComponent } from './adopt.component';
import { NotFoundComponent } from '../not-found/not-found.component';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { TextAreaInputComponent } from 'src/app/common/ui/text-area-input.component';
import { FileDropAreaComponent } from 'src/app/common/ui/file-drop-area.component';
import { ErrorMessageBannerComponent } from 'src/app/common/ui/error-message-banner.component';
import { FormErrorSummaryComponent } from 'src/app/common/ui/form-error-summary.component';

@NgModule({
  declarations: [
    AdoptComponent,
    AdoptionFormComponent,
    ShelterInfoComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    NgIconsModule,
    FormInputComponent,
    TextAreaInputComponent,
    FileDropAreaComponent,
    ErrorMessageBannerComponent,
    FormErrorSummaryComponent,
    RouterModule.forChild([
      { path: ':id', component: AdoptComponent },
      { path: '**', component: NotFoundComponent }
    ])
  ]
})
export class AdoptModule { }