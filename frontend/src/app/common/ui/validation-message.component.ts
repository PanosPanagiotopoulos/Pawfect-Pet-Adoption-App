import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl } from '@angular/forms';

@Component({
  selector: 'app-validation-message',
  standalone: true,
  imports: [CommonModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div
      *ngIf="shouldShowErrors()"
      [id]="id"
      role="alert"
      class="absolute -bottom-6 left-0 text-sm text-red-400 transition-all duration-300"
    >
      <!-- Required field -->
      <div *ngIf="control?.errors?.['required']">{{ field }} απαιτείται</div>

      <!-- Email validation -->
      <div *ngIf="control?.errors?.['email']">
        Παρακαλώ εισάγετε έναν έγκυρο email (π.χ., user&#64;domain.com)
      </div>

      <!-- Min length -->
      <div *ngIf="control?.errors?.['minlength']">
        {{ field }} πρέπει να έχει τουλάχιστον
        {{ control?.errors?.['minlength']?.requiredLength }} χαρακτήρες
      </div>

      <!-- Max length -->
      <div *ngIf="control?.errors?.['maxlength']">
        {{ field }} δεν μπορεί να υπερβαίνει τους
        {{ control?.errors?.['maxlength']?.requiredLength }} χαρακτήρες
      </div>

      <!-- Pattern validation -->
      <div *ngIf="control?.errors?.['pattern']">
        <ng-container [ngSwitch]="field.toLowerCase()">
          <!-- Phone number -->
          <ng-container *ngSwitchCase="'αριθμός τηλεφώνου'">
            Παρακαλώ εισάγετε έναν έγκυρο αριθμό τηλεφώνου (μόνο αριθμοί)
          </ng-container>

          <!-- ZIP code -->
          <ng-container *ngSwitchCase="'ταχυδρομικός κώδικας'">
            Παρακαλώ εισάγετε έναν έγκυρο ταχυδρομικό κώδικα (π.χ., 12345)
          </ng-container>

          <!-- Operating hours -->
          <ng-container *ngSwitchCase="'ώρες λειτουργίας'">
            Παρακαλώ εισάγετε έγκυρες ώρες λειτουργίας σε μορφή 24ώρου (π.χ.,
            09:00,17:00)
          </ng-container>

          <!-- Default pattern message -->
          <ng-container *ngSwitchDefault>
            Μη έγκυρη μορφή για {{ field }}
          </ng-container>
        </ng-container>
      </div>

      <!-- Password validation -->
      <div *ngIf="control?.errors?.['uppercase']">
        Ο κωδικός πρέπει να περιέχει τουλάχιστον ένα κεφαλαίο γράμμα
      </div>
      <div *ngIf="control?.errors?.['lowercase']">
        Ο κωδικός πρέπει να περιέχει τουλάχιστον ένα πεζό γράμμα
      </div>
      <div *ngIf="control?.errors?.['number']">
        Ο κωδικός πρέπει να περιέχει τουλάχιστον έναν αριθμό
      </div>
      <div *ngIf="control?.errors?.['specialChar']">
        Ο κωδικός πρέπει να περιέχει τουλάχιστον έναν ειδικό χαρακτήρα
      </div>

      <!-- Password match validation -->
      <div *ngIf="control?.errors?.['mismatch']">Οι κωδικοί δεν ταιριάζουν</div>

      <!-- Social media validation -->
      <div *ngIf="control?.errors?.['invalidSocialMedia']">
        Παρακαλώ εισάγετε έγκυρο link {{ field }}
      </div>

      <!-- Operating hours validation -->
      <div *ngIf="control?.errors?.['invalidTimeRange']">
        Η ώρα κλεισίματος πρέπει να είναι μετά την ώρα ανοίγματος
      </div>

      <!-- Role validation -->
      <div *ngIf="control?.errors?.['invalidRole']">
        Ο ρόλος του χρήστη δεν είναι έγκυρος
      </div>

      <!-- Custom error message -->
      <div *ngIf="control?.errors?.['custom']">
        {{ control?.errors?.['custom'] }}
      </div>
    </div>
  `,
})
export class ValidationMessageComponent {
  @Input() control?: AbstractControl | null;
  @Input() field: string = 'Αυτό το πεδίο';
  @Input() customError?: string;
  @Input() id?: string;
  @Input() showImmediately: boolean = false;

  shouldShowErrors(): boolean {
    if (!this.control) return false;

    // Show errors immediately when control is invalid and dirty
    if (this.showImmediately && this.control.invalid && this.control.dirty) {
      return true;
    }

    // Traditional approach - show when touched
    return this.control.invalid && (this.control.touched || this.control.dirty);
  }
}
