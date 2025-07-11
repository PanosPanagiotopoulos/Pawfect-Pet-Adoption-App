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

function createProfilePaginatorIntl(): MatPaginatorIntl {
  const intl = new MatPaginatorIntl();
  const translationService = inject(TranslationService);
  const setLabels = () => {
    const translate = (key: string) => translationService.translate ? translationService.translate(key) : key;
    intl.itemsPerPageLabel = translate('APP.COMMONS.ITEMS_PER_PAGE');
    intl.nextPageLabel = translate('APP.COMMONS.NEXT_PAGE');
    intl.previousPageLabel = translate('APP.COMMONS.PREVIOUS_PAGE');
    intl.firstPageLabel = translate('APP.COMMONS.FIRST_PAGE') || 'First page';
    intl.lastPageLabel = translate('APP.COMMONS.LAST_PAGE') || 'Last page';
    intl.getRangeLabel = (page: number, pageSize: number, length: number) => {
      if (length === 0 || pageSize === 0) {
        return `0 ${translate('APP.COMMONS.OF')} ${length}`;
      }
      const startIndex = page * pageSize;
      const endIndex = startIndex < length ? Math.min(startIndex + pageSize, length) : startIndex + pageSize;
      return `${startIndex + 1} - ${endIndex} ${translate('APP.COMMONS.OF')} ${length}`;
    };
  };
  setLabels();
  // Subscribe to language changes and update labels live
  if (translationService.languageChanged$) {
    translationService.languageChanged$.subscribe(() => {
      setLabels();
      intl.changes.next();
    });
  }
  return intl;
}
import { ProfileAnimalsComponent } from './profile-animals.component';
import { ProfileAdoptionApplicationsComponent } from './profile-adoption-applications.component';

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
    LucideAngularModule.pick({
      mapPin: LucideMapPin,
      user: LucideUser,
      fileText: LucideFileText,
      check: LucideCheck,
      building: LucideBuilding,
      pawPrint: LucidePawPrint,
      alertCircle: LucideAlertCircle,
      instagram: LucideInstagram,
      facebook: LucideFacebook,
      plus: LucidePlus,
      inbox: LucideInbox,
    }),
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
    RouterModule.forChild([
      { 
        path: '', 
        component: ProfileComponent
      }
    ]),
  ],
  exports: [
    ProfileComponent,
  ],
  providers: [
    {
      provide: MatPaginatorIntl,
      useFactory: createProfilePaginatorIntl
    }
  ]
})
export class ProfileModule {} 