import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SearchComponent } from './search.component';
import { RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

@NgModule({
  declarations: [SearchComponent],
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forChild([{ path: '', component: SearchComponent }]),
  ],
})
export class SearchModule {}
