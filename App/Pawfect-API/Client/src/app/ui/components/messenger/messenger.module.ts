import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NgIconsModule } from '@ng-icons/core';
import { TranslatePipe } from '../../../common/tools/translate.pipe';
import { MessengerComponent } from './messenger.component';

@NgModule({
  declarations: [
    MessengerComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    NgIconsModule,
    TranslatePipe
  ],
  exports: [
    MessengerComponent
  ]
})
export class MessengerModule { }