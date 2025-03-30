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


@NgModule({
  declarations: [
    SearchComponent,
    SwipeCardComponent,
    AnimalCardComponent,
    SavedAnimalsGridComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    FormInputComponent,
    LucideAngularModule,
    NgIconsModule,
    RouterModule,
    RouterModule.forChild([
      { path: '', component: SearchComponent },
      { path: '**', component: NotFoundComponent },
    ]),
  ],
})
export class SearchModule {}