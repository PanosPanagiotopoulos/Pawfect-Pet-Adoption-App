import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-form-input',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div [formGroup]="form" class="relative group mb-10">
      <!-- Input field -->
      <input
        [type]="type"
        [formControlName]="controlName"
        [id]="controlName"
        [class.border-red-500]="form.get(controlName)?.invalid && form.get(controlName)?.touched"
        class="peer w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white 
               placeholder-transparent
               focus:border-primary-500/50 focus:ring-2 focus:ring-primary-500/20 focus:outline-none 
               transition-all duration-300"
        [placeholder]="placeholder"
      />

      <!-- Floating label -->
      <label
        [for]="controlName"
        [class.text-red-400]="form.get(controlName)?.invalid && form.get(controlName)?.touched"
        class="absolute text-sm text-gray-400 duration-300 transform 
               -translate-y-4 scale-75 top-2 z-10 origin-[0] bg-transparent
               px-2 peer-focus:px-2 peer-focus:text-primary-500 
               peer-placeholder-shown:scale-100 peer-placeholder-shown:-translate-y-1/2 
               peer-placeholder-shown:top-1/2 peer-focus:top-2 peer-focus:-translate-y-4 
               peer-focus:scale-75 peer-focus:text-primary-400 left-1"
      >
        {{ placeholder }}
      </label>

      <!-- Background gradient effect -->
      <div
        class="absolute inset-0 rounded-xl bg-gradient-to-r from-primary-500/10 
               via-secondary-500/10 to-accent-500/10 opacity-0 
               group-hover:opacity-50 peer-focus:opacity-100 -z-10 
               transition-opacity duration-500"
      ></div>

      <!-- Error message -->
      <div
        *ngIf="form.get(controlName)?.invalid && form.get(controlName)?.touched"
        class="absolute -bottom-6 left-0 text-sm text-red-400 transition-all duration-300"
      >
        <span *ngIf="form.get(controlName)?.errors?.['required']">
          {{ placeholder }} is required
        </span>
        <span *ngIf="form.get(controlName)?.errors?.['email']">
          Please enter a valid email
        </span>
        <span *ngIf="form.get(controlName)?.errors?.['pattern']">
          Please enter a valid format
        </span>
        <span *ngIf="form.get(controlName)?.errors?.['minlength']">
          Must be at least {{ form.get(controlName)?.errors?.['minlength'].requiredLength }} characters
        </span>
      </div>
    </div>
  `
})
export class FormInputComponent {
  @Input() form!: FormGroup;
  @Input() controlName!: string;
  @Input() type: string = 'text';
  @Input() placeholder: string = '';
}