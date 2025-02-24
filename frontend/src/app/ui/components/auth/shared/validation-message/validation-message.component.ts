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
      *ngIf="control?.invalid && (control?.dirty || control?.touched)"
      [id]="id"
      role="alert"
      class="absolute -bottom-6 left-0 text-sm text-red-400 transition-all duration-300">
      <!-- Required field -->
      <div *ngIf="control?.errors?.['required']">
        {{ field }} is required
      </div>

      <!-- Email validation -->
      <div *ngIf="control?.errors?.['email']">
        Please enter a valid email address (e.g., user&#64;domain.com)
      </div>

      <!-- Min length -->
      <div *ngIf="control?.errors?.['minlength']">
        {{ field }} must be at least {{ control?.errors?.['minlength']?.requiredLength }} characters
      </div>

      <!-- Max length -->
      <div *ngIf="control?.errors?.['maxlength']">
        {{ field }} cannot exceed {{ control?.errors?.['maxlength']?.requiredLength }} characters
      </div>

      <!-- Pattern validation -->
      <div *ngIf="control?.errors?.['pattern']">
        <ng-container [ngSwitch]="field.toLowerCase()">
          <!-- Phone number -->
          <ng-container *ngSwitchCase="'phone number'">
            Please enter a valid phone number (numbers only)
          </ng-container>
          
          <!-- ZIP code -->
          <ng-container *ngSwitchCase="'zip code'">
            Please enter a valid ZIP code (e.g., 12345 or 12345-6789)
          </ng-container>
          
          <!-- Operating hours -->
          <ng-container *ngSwitchCase="'operating hours'">
            Please enter valid operating hours in 24-hour format (e.g., 09:00,17:00)
          </ng-container>
          
          <!-- Default pattern message -->
          <ng-container *ngSwitchDefault>
            Please enter a valid format for {{ field }}
          </ng-container>
        </ng-container>
      </div>

      <!-- Password validation -->
      <div *ngIf="control?.errors?.['uppercase']">
        Password must include at least one uppercase letter
      </div>
      <div *ngIf="control?.errors?.['lowercase']">
        Password must include at least one lowercase letter
      </div>
      <div *ngIf="control?.errors?.['number']">
        Password must include at least one number
      </div>
      <div *ngIf="control?.errors?.['specialChar']">
        Password must include at least one special character
      </div>

      <!-- Social media validation -->
      <div *ngIf="control?.errors?.['invalidSocialMedia']">
        Please enter a valid {{ field }} URL
      </div>

      <!-- Operating hours validation -->
      <div *ngIf="control?.errors?.['invalidTimeRange']">
        Closing time must be after opening time
      </div>

      <!-- Custom error message -->
      <div *ngIf="customError">{{ customError }}</div>
    </div>
  `
})
export class ValidationMessageComponent {
  @Input() control?: AbstractControl | null;
  @Input() field: string = 'This field';
  @Input() customError?: string;
  @Input() id?: string;
}