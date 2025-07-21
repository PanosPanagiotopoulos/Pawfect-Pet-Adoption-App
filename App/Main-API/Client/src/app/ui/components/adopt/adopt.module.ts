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
import { PetDetailsDialogComponent } from 'src/app/common/ui/pet-details-dialog/pet-details-dialog.component';
import { AuthGuard } from 'src/app/common/guards/auth.guard';
import { Permission } from 'src/app/common/enum/permission.enum';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { DateTimeFormatPipe } from 'src/app/common/tools/date-time-format.pipe';
import { DatePipe } from '@angular/common';
import { TimezoneService } from 'src/app/common/services/time-zone.service';

@NgModule({
  declarations: [
    AdoptComponent,
    AdoptionFormComponent,
    ShelterInfoComponent,
    DateTimeFormatPipe,
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
    PetDetailsDialogComponent,
    TranslatePipe,
    RouterModule.forChild([
      {
        path: 'edit/:applicationId',
        component: AdoptComponent,
        canActivate: [AuthGuard],
        data: {
          permissions: [Permission.EditAdoptionApplications],
        },
      },
      {
        path: ':id',
        component: AdoptComponent,
        canActivate: [AuthGuard],
        data: {
          permissions: [Permission.CreateAdoptionApplications],
        },
      },
      { path: '**', component: NotFoundComponent },
    ]),
  ],
  providers: [DatePipe, TimezoneService],
})
export class AdoptModule {}
