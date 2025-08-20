import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ProfileComponent } from './profile.component';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgIconsModule } from '@ng-icons/core';
import { LucideAngularModule, LucideMapPin, LucideUser, LucideFileText, LucideCheck, LucideBuilding, LucidePawPrint, LucideAlertCircle, LucideInstagram, LucideFacebook, LucidePlus, LucideInbox } from 'lucide-angular';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { FormFieldComponent } from 'src/app/common/ui/form-field.component';
import { TextAreaInputComponent } from 'src/app/common/ui/text-area-input.component';
import { FileDropAreaComponent } from 'src/app/common/ui/file-drop-area.component';
import { FormErrorSummaryComponent } from 'src/app/common/ui/form-error-summary.component';
import { ValidationMessageComponent } from 'src/app/common/ui/validation-message.component';
import { PetDetailsDialogComponent } from 'src/app/common/ui/pet-details-dialog/pet-details-dialog.component';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatBadgeModule } from '@angular/material/badge';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatPaginatorIntl } from '@angular/material/paginator';
import { inject } from '@angular/core';
import { TranslationService } from 'src/app/common/services/translation.service';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatDialogModule } from '@angular/material/dialog';


import { ProfileAnimalsComponent } from './profile-animals.component';
import { ProfileAdoptionApplicationsComponent } from './profile-adoption-applications.component';
import { AuthGuard } from 'src/app/common/guards/auth.guard';
import { FormGuard } from 'src/app/common/guards/form.guard';
import { Permission } from 'src/app/common/enum/permission.enum';

@NgModule({
  declarations: [
    ProfileComponent,
    ProfileAdoptionApplicationsComponent,
    ProfileAnimalsComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    FormInputComponent,
    FormFieldComponent,
    TextAreaInputComponent,
    FileDropAreaComponent,
    FormErrorSummaryComponent,
    ValidationMessageComponent,
    PetDetailsDialogComponent,
    NgIconsModule,
    TranslatePipe,
    MatTabsModule,
    MatCardModule,
    MatListModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatSelectModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatDividerModule,
    MatBadgeModule,
    MatTooltipModule,
    MatMenuModule,
    MatPaginatorModule,
    MatCheckboxModule,
    MatDialogModule,
    RouterModule.forChild([
      { 
        path: '', 
        component: ProfileComponent,
        canActivate: [AuthGuard],
        canDeactivate: [FormGuard],
        data: {
          permissions: [Permission.BrowseUsers, Permission.BrowseShelters]
        }
      },
      {   
        path: ':id', 
        component: ProfileComponent,
        canActivate: [AuthGuard],
        canDeactivate: [FormGuard],
        data: {
          permissions: [Permission.BrowseUsers, Permission.BrowseShelters]
        }
      }
    ]),
  ],
  exports: [
    ProfileComponent,
  ],
  
})
export class ProfileModule {} 