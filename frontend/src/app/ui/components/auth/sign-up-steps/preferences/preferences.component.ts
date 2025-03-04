import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { ShelterInfoComponent } from '../shelter-info/shelter-info.component';

@Component({
  selector: 'app-preferences',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ShelterInfoComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="space-y-6">
      <app-shelter-info
        [form]="form"
        (back)="back.emit()"
        (submit)="submit.emit()"
      ></app-shelter-info>
    </div>
  `
})
export class PreferencesComponent {
  @Input() form!: FormGroup;
  @Output() back = new EventEmitter<void>();
  @Output() submit = new EventEmitter<void>();
}