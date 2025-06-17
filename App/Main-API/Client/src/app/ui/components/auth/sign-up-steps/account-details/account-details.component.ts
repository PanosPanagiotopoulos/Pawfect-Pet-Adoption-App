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
import { PasswordInputComponent } from 'src/app/common/ui/password-input.component';
import { NgIconsModule } from '@ng-icons/core';

@Component({
  selector: 'app-account-details',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, PasswordInputComponent, NgIconsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [formGroup]="form" class="space-y-6" #formContainer>
      <h2 class="text-2xl font-bold text-white mb-6">Στοιχεία Λογαριασμού</h2>

      <ng-container>
        <app-password-input
          [form]="form"
          controlName="password"
          placeholder="Κωδικός πρόσβασης"
        >
        </app-password-input>

        <app-password-input
          [form]="form"
          controlName="confirmPassword"
          placeholder="Επιβεβαίωση κωδικού"
        >
        </app-password-input>

        <!-- Password requirements info -->
        <div class="bg-white/5 backdrop-blur-sm rounded-xl p-4 space-y-3">
          <h4 class="text-sm font-medium text-white">Απαιτήσεις κωδικού:</h4>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
            <div class="flex items-center space-x-2">
              <ng-icon 
                [name]="isPasswordRequirementMet('minlength') ? 'lucideCheck' : 'lucideX'" 
                [size]="'16'" 
                [class]="isPasswordRequirementMet('minlength') ? 'text-green-400' : 'text-red-400'"
                class="stroke-[2.5px]">
              </ng-icon>
              <span class="text-sm text-gray-300">Τουλάχιστον 8 χαρακτήρες</span>
            </div>
            <div class="flex items-center space-x-2">
              <ng-icon 
                [name]="isPasswordRequirementMet('uppercase') ? 'lucideCheck' : 'lucideX'" 
                [size]="'16'" 
                [class]="isPasswordRequirementMet('uppercase') ? 'text-green-400' : 'text-red-400'"
                class="stroke-[2.5px]">
              </ng-icon>
              <span class="text-sm text-gray-300">Ένα κεφαλαίο γράμμα</span>
            </div>
            <div class="flex items-center space-x-2">
              <ng-icon 
                [name]="isPasswordRequirementMet('lowercase') ? 'lucideCheck' : 'lucideX'" 
                [size]="'16'" 
                [class]="isPasswordRequirementMet('lowercase') ? 'text-green-400' : 'text-red-400'"
                class="stroke-[2.5px]">
              </ng-icon>
              <span class="text-sm text-gray-300">Ένα πεζό γράμμα</span>
            </div>
            <div class="flex items-center space-x-2">
              <ng-icon 
                [name]="isPasswordRequirementMet('number') ? 'lucideCheck' : 'lucideX'" 
                [size]="'16'" 
                [class]="isPasswordRequirementMet('number') ? 'text-green-400' : 'text-red-400'"
                class="stroke-[2.5px]">
              </ng-icon>
              <span class="text-sm text-gray-300">Έναν αριθμό</span>
            </div>
            <div class="flex items-center space-x-2">
              <ng-icon 
                [name]="isPasswordRequirementMet('specialChar') ? 'lucideCheck' : 'lucideX'" 
                [size]="'16'" 
                [class]="isPasswordRequirementMet('specialChar') ? 'text-green-400' : 'text-red-400'"
                class="stroke-[2.5px]">
              </ng-icon>
              <span class="text-sm text-gray-300">Έναν ειδικό χαρακτήρα</span>
            </div>
            <div class="flex items-center space-x-2">
              <ng-icon 
                [name]="isPasswordRequirementMet('mismatch') ? 'lucideCheck' : 'lucideX'" 
                [size]="'16'" 
                [class]="isPasswordRequirementMet('mismatch') ? 'text-green-400' : 'text-red-400'"
                class="stroke-[2.5px]">
              </ng-icon>
              <span class="text-sm text-gray-300">Οι κωδικοί ταιριάζουν</span>
            </div>
          </div>
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
          [disabled]="!form.valid"
          class="px-6 py-2 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg
                 hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 
                 transform hover:-translate-y-1 disabled:opacity-50 disabled:cursor-not-allowed
                 disabled:transform-none disabled:shadow-none"
        >
          Επόμενο
        </button>
      </div>
    </div>
  `
})
export class AccountDetailsComponent {
  @Input() form!: FormGroup;
  @Output() next = new EventEmitter<void>();
  @Output() back = new EventEmitter<void>();
  @ViewChild('formContainer') formContainer!: ElementRef;

  isPasswordRequirementMet(requirement: string): boolean {
    const passwordControl = this.form.get('password');
    const confirmPasswordControl = this.form.get('confirmPassword');
    
    if (!passwordControl?.value) {
      return false;
    }

    switch (requirement) {
      case 'minlength':
        return passwordControl.value.length >= 8;
      case 'uppercase':
        return /[A-Z]/.test(passwordControl.value);
      case 'lowercase':
        return /[a-z]/.test(passwordControl.value);
      case 'number':
        return /\d/.test(passwordControl.value);
      case 'specialChar':
        return /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(passwordControl.value);
      case 'mismatch':
        return confirmPasswordControl?.value && passwordControl.value === confirmPasswordControl.value;
      default:
        return false;
    }
  }

  onNext(): void {
    if (this.form.valid) {
      this.next.emit();
    }
  }

  onBack(): void {
    this.back.emit();
  }
}
