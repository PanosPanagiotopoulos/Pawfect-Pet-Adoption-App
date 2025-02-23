import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AbstractControl } from '@angular/forms';

@Component({
  selector: 'app-validation-message',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div *ngIf="control?.invalid && (control?.dirty || control?.touched)"
         class="text-red-400 text-sm mt-1 transition-all duration-300">
      <div *ngIf="control?.errors?.['required']">{{ field }} is required</div>
      <div *ngIf="control?.errors?.['email']">Please enter a valid email address</div>
      <div *ngIf="control?.errors?.['minlength']">
        {{ field }} must be at least {{ control?.errors?.['minlength']?.requiredLength }} characters
      </div>
      <div *ngIf="control?.errors?.['pattern']">{{ field }} format is invalid</div>
      <div *ngIf="customError">{{ customError }}</div>
    </div>
  `
})
export class ValidationMessageComponent {
  @Input() control?: AbstractControl | null;
  @Input() field: string = 'This field';
  @Input() customError?: string;
}