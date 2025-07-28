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
      <div *ngIf="control?.errors?.['required']">
        {{
          ('APP.UI_COMPONENTS.VALIDATION_MESSAGE.REQUIRED' | translate).replace(
            '{field}',
            field | translate
          )
        }}
      </div>

      <!-- Email validation -->
      <div *ngIf="control?.errors?.['email']">
        {{ 'APP.UI_COMPONENTS.VALIDATION_MESSAGE.EMAIL_INVALID' | translate }}
      </div>

      <!-- Min length -->
      <div *ngIf="control?.errors?.['minlength']">
        {{ ('APP.UI_COMPONENTS.VALIDATION_MESSAGE.MIN_LENGTH' | translate).replace('{field}', field | translate).replace('{length}', control?.errors?.['minlength']?.requiredLength) }}
      </div>

      <!-- Max length -->
      <div *ngIf="control?.errors?.['maxlength']">
        {{ ('APP.UI_COMPONENTS.VALIDATION_MESSAGE.MAX_LENGTH' | translate).replace('{field}', field | translate).replace('{length}', control?.errors?.['maxlength']?.requiredLength) }}
      </div>

      <!-- Min value -->
      <div *ngIf="control?.errors?.['min']">
        {{ ('APP.UI_COMPONENTS.VALIDATION_MESSAGE.MIN_VALUE' | translate).replace('{field}', field | translate).replace('{min}', control?.errors?.['min']?.min) }}
      </div>

      <!-- Max value -->
      <div *ngIf="control?.errors?.['max']">
        {{ ('APP.UI_COMPONENTS.VALIDATION_MESSAGE.MAX_VALUE' | translate).replace('{field}', field | translate).replace('{max}', control?.errors?.['max']?.max) }}
      </div>

      <!-- Pattern validation -->
      <div *ngIf="control?.errors?.['pattern']">
        <ng-container [ngSwitch]="field">
          <!-- Phone number -->
          <ng-container
            *ngSwitchCase="
              'APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.PHONE'
            "
          >
            {{
              'APP.UI_COMPONENTS.VALIDATION_MESSAGE.PHONE_PATTERN' | translate
            }}
          </ng-container>

          <!-- ZIP code -->
          <ng-container
            *ngSwitchCase="
              'APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.ZIP'
            "
          >
            {{ 'APP.UI_COMPONENTS.VALIDATION_MESSAGE.ZIP_PATTERN' | translate }}
          </ng-container>

          <!-- Operating hours -->
          <ng-container
            *ngSwitchCase="
              'APP.UI_COMPONENTS.FORM_ERROR_TRACKER.FIELD_NAMES.OPERATING_HOURS'
            "
          >
            {{
              'APP.UI_COMPONENTS.VALIDATION_MESSAGE.HOURS_PATTERN' | translate
            }}
          </ng-container>

          <!-- Default pattern message -->
          <ng-container *ngSwitchDefault>
            {{
              (
                'APP.UI_COMPONENTS.VALIDATION_MESSAGE.DEFAULT_PATTERN'
                | translate
              ).replace('{field}', field | translate)
            }}
          </ng-container>
        </ng-container>
      </div>

      <!-- Password validation -->
      <div *ngIf="control?.errors?.['uppercase']">
        {{
          'APP.UI_COMPONENTS.VALIDATION_MESSAGE.PASSWORD_UPPERCASE' | translate
        }}
      </div>
      <div *ngIf="control?.errors?.['lowercase']">
        {{
          'APP.UI_COMPONENTS.VALIDATION_MESSAGE.PASSWORD_LOWERCASE' | translate
        }}
      </div>
      <div *ngIf="control?.errors?.['number']">
        {{ 'APP.UI_COMPONENTS.VALIDATION_MESSAGE.PASSWORD_NUMBER' | translate }}
      </div>
      <div *ngIf="control?.errors?.['specialChar']">
        {{
          'APP.UI_COMPONENTS.VALIDATION_MESSAGE.PASSWORD_SPECIAL_CHAR'
            | translate
        }}
      </div>

      <!-- Password match validation -->
      <div *ngIf="control?.errors?.['mismatch']">
        {{
          'APP.UI_COMPONENTS.VALIDATION_MESSAGE.PASSWORD_MISMATCH' | translate
        }}
      </div>

      <!-- Social media validation -->
      <div *ngIf="control?.errors?.['invalidSocialMedia']">
        {{
          (
            'APP.UI_COMPONENTS.VALIDATION_MESSAGE.INVALID_SOCIAL' | translate
          ).replace('{field}', field | translate)
        }}
      </div>

      <!-- Operating hours validation -->
      <div *ngIf="control?.errors?.['invalidTimeRange']">
        {{
          'APP.UI_COMPONENTS.VALIDATION_MESSAGE.INVALID_TIME_RANGE' | translate
        }}
      </div>

      <!-- Role validation -->
      <div *ngIf="control?.errors?.['invalidRole']">
        {{ 'APP.UI_COMPONENTS.VALIDATION_MESSAGE.INVALID_ROLE' | translate }}
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
  @Input() field: string = 'APP.UI_COMPONENTS.VALIDATION_MESSAGE.DEFAULT_FIELD';
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
