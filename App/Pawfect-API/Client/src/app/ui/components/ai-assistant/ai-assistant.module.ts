import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { NgIconsModule } from '@ng-icons/core';
import { AiAssistantComponent } from './ai-assistant.component';
import { TranslatePipe } from '../../../common/tools/translate.pipe';

@NgModule({
  declarations: [
    AiAssistantComponent
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    NgIconsModule,
    TranslatePipe
  ],
  exports: [
    AiAssistantComponent
  ]
})
export class AiAssistantModule { }