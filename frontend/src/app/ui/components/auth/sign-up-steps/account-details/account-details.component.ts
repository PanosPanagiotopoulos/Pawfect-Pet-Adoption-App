import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  ViewChild,
  ElementRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';

@Component({
  selector: 'app-account-details',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormInputComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [formGroup]="form" class="space-y-6" #formContainer>
      <h2 class="text-2xl font-bold text-white mb-6">Στοιχεία Λογαριασμού</h2>

      <ng-container>
        <app-form-input
          [form]="form"
          controlName="password"
          type="password"
          placeholder="Κωδικός πρόσβασης"
        >
        </app-form-input>

        <app-form-input
          [form]="form"
          controlName="confirmPassword"
          type="password"
          placeholder="Επιβεβαίωση κωδικού"
        >
        </app-form-input>

        <!-- Password requirements info -->
        <div class="text-sm text-gray-400 space-y-1 mt-2">
          <p>Ο κωδικός πρέπει να περιέχει:</p>
          <ul class="list-disc list-inside pl-4">
            <li>Τουλάχιστον 8 χαρακτήρες</li>
            <li>Ένα κεφαλαίο γράμμα</li>
            <li>Ένα πεζό γράμμα</li>
            <li>Έναν αριθμό</li>
            <li>Έναν ειδικό χαρακτήρα</li>
          </ul>
        </div>
      </ng-container>

      <ng-template #externalProvider>
        <div class="text-center text-gray-400 py-8">
          <p>Ο λογαριασμός σας είναι συνδεδεμένος με το Google.</p>
          <p>Δεν απαιτείται κωδικός πρόσβασης.</p>
        </div>
      </ng-template>

      <!-- Navigation buttons -->
      <div class="flex justify-between pt-6">
        <button
          type="button"
          (click)="onBack()"
          class="px-6 py-2 border border-gray-600 text-gray-300 rounded-lg
                 hover:bg-white/5 transition-all duration-300"
        >
          Πίσω
        </button>

        <button
          type="button"
          (click)="onNext()"
          class="px-6 py-2 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg
                 hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 
                 transform hover:-translate-y-1"
        >
          Επόμενο
        </button>
      </div>
    </div>
  `,
})
export class AccountDetailsComponent {
  @Input() form!: FormGroup;
  @Output() next = new EventEmitter<void>();
  @Output() back = new EventEmitter<void>();
  @ViewChild('formContainer') formContainer!: ElementRef;

  onNext(): void {
    if (this.form.valid) {
      this.next.emit();
    }
  }

  onBack(): void {
    this.back.emit();
  }
}
