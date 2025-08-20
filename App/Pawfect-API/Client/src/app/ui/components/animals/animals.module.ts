import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatDialogModule } from '@angular/material/dialog';
import { NgIconsModule } from '@ng-icons/core';

import { AddAnimalsComponent } from './add-animals.component';
import { EditAnimalsComponent } from './edit-animals.component';
import { ViewAnimalsComponent } from './view-animals.component';
import { AuthGuard } from 'src/app/common/guards/auth.guard';
import { FormGuard } from 'src/app/common/guards/form.guard';
import { Permission } from 'src/app/common/enum/permission.enum';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { FileDropAreaComponent } from 'src/app/common/ui/file-drop-area.component';
import { ValidationMessageComponent } from 'src/app/common/ui/validation-message.component';
import { FileToUrlPipe } from 'src/app/common/tools/file-to-url.pipe';

const routes: Routes = [
  {
    path: 'new',
    component: AddAnimalsComponent,
    canActivate: [AuthGuard],
    canDeactivate: [FormGuard],
    data: {
      permissions: [Permission.CreateAnimals],
    },
  },
  {
    path: 'edit/:animalId',
    component: EditAnimalsComponent,
    canActivate: [AuthGuard],
    canDeactivate: [FormGuard],
  },
  {
    path: 'view/:animalId',
    component: ViewAnimalsComponent,
    canActivate: [AuthGuard],
    data: {
      permissions: [Permission.BrowseAnimals],
    },
  },
];

@NgModule({
  declarations: [
    AddAnimalsComponent,
    EditAnimalsComponent,
    ViewAnimalsComponent,
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    NgIconsModule,
    MatPaginatorModule,
    MatDialogModule,
    TranslatePipe,
    FileDropAreaComponent,
    ValidationMessageComponent,
    FileToUrlPipe,
    RouterModule.forChild(routes),
  ],
  exports: [RouterModule],
})
export class AnimalsModule {}
