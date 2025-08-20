import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl } from '@angular/forms';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-validation-message',
  standalone: true,
  imports: [CommonModule, TranslatePipe],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div
      *ngIf="shouldShowErrors()"
      [id]="id"
      role="alert"
      class="absolute -bottom-6 left-0 text-sm text-red-400 transition-all duration-300"
    >
      <!-- Required field -->
      <div *ngIf="control?.errors?.['required']">{{ 'APP.COMMONS.VALIDATION.REQUIRED' | translate }}</div>

      <!-- Email validation -->
      <div *ngIf="control?.errors?.['email']">{{ 'APP.COMMONS.VALIDATION.EMAIL' | translate }}</div>

      <!-- Min length -->
      <div *ngIf="control?.errors?.['minlength']">{{ 'APP.COMMONS.VALIDATION.MIN_LENGTH' | translate }}</div>

      <!-- Max length -->
      <div *ngIf="control?.errors?.['maxlength']">{{ 'APP.COMMONS.VALIDATION.MAX_LENGTH' | translate }}</div>

      <!-- Pattern validation -->
      <div *ngIf="control?.errors?.['pattern']">
        <ng-container [ngSwitch]="field.toLowerCase()">
          <!-- Phone number -->
          <ng-container *ngSwitchCase="'αριθμός τηλεφώνου'">{{ 'APP.COMMONS.VALIDATION.PATTERN_PHONE' | translate }}</ng-container>

          <!-- ZIP code -->
          <ng-container *ngSwitchCase="'ταχυδρομικός κώδικας'">{{ 'APP.COMMONS.VALIDATION.PATTERN_ZIP' | translate }}</ng-container>

          <!-- Operating hours -->
          <ng-container *ngSwitchCase="'ώρες λειτουργίας'">{{ 'APP.COMMONS.VALIDATION.PATTERN_HOURS' | translate }}</ng-container>

          <!-- Default pattern message -->
          <ng-container *ngSwitchDefault>{{ 'APP.COMMONS.VALIDATION.PATTERN_DEFAULT' | translate }}</ng-container>
        </ng-container>
      </div>

      <!-- Password validation -->
      <div *ngIf="control?.errors?.['uppercase']">{{ 'APP.COMMONS.VALIDATION.UPPERCASE' | translate }}</div>
      <div *ngIf="control?.errors?.['lowercase']">{{ 'APP.COMMONS.VALIDATION.LOWERCASE' | translate }}</div>
      <div *ngIf="control?.errors?.['number']">{{ 'APP.COMMONS.VALIDATION.NUMBER' | translate }}</div>
      <div *ngIf="control?.errors?.['specialChar']">{{ 'APP.COMMONS.VALIDATION.SPECIAL_CHAR' | translate }}</div>

      <!-- Password match validation -->
      <div *ngIf="control?.errors?.['mismatch']">{{ 'APP.COMMONS.VALIDATION.MISMATCH' | translate }}</div>

      <!-- Social media validation -->
      <div *ngIf="control?.errors?.['invalidSocialMedia']">{{ 'APP.COMMONS.VALIDATION.INVALID_SOCIAL' | translate }}</div>

      <!-- Operating hours validation -->
      <div *ngIf="control?.errors?.['invalidTimeRange']">{{ 'APP.COMMONS.VALIDATION.INVALID_TIME_RANGE' | translate }}</div>

      <!-- Role validation -->
      <div *ngIf="control?.errors?.['invalidRole']">{{ 'APP.COMMONS.VALIDATION.INVALID_ROLE' | translate }}</div>

      <!-- Custom error message -->
      <div *ngIf="control?.errors?.['custom']">{{ control?.errors?.['custom'] }}</div>
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
