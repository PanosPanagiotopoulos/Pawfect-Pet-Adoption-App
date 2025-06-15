import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SearchComponent } from './search.component';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NotFoundComponent } from '../not-found/not-found.component';
import { NgIconsModule } from '@ng-icons/core';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { SwipeCardComponent } from './components/swipe-card/swipe-card.component';
import { AnimalCardComponent } from './components/animal-card/animal-card.component';
import { SavedAnimalsGridComponent } from './components/saved-animals-grid/saved-animals-grid.component';
import { LucideAngularModule } from 'lucide-angular';
import { PetDetailsDialogComponent } from 'src/app/common/ui/pet-details-dialog/pet-details-dialog.component';
import { AuthGuard } from 'src/app/common/guards/auth.guard';
import { Permission } from 'src/app/common/enum/permission.enum';
import { InstructionsModalComponent } from 'src/app/common/ui/instructions-modal/instructions-modal.component';

@NgModule({
  declarations: [
    SearchComponent,
    SwipeCardComponent,
    SavedAnimalsGridComponent,
    AnimalCardComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    FormInputComponent,
    LucideAngularModule,
    PetDetailsDialogComponent,
    NgIconsModule,
    RouterModule,
    InstructionsModalComponent,
    RouterModule.forChild([
      { 
        path: '', 
        component: SearchComponent
      }
    ]),
  ],
  exports: [
    SearchComponent,
    SwipeCardComponent,
    SavedAnimalsGridComponent,
    AnimalCardComponent,
  ]
})
export class SearchModule {}