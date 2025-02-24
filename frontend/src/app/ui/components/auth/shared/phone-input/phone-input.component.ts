import { Component, Input, OnInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { COUNTRY_CODES, CountryCode } from './country-codes';
import { ValidationMessageComponent } from '../validation-message/validation-message.component';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-phone-input',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    ValidationMessageComponent
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    trigger('dropdownAnimation', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-10px)' }),
        animate('200ms ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ]),
      transition(':leave', [
        animate('150ms ease-in', style({ opacity: 0, transform: 'translateY(-10px)' }))
      ])
    ])
  ],
  template: `
    <div [formGroup]="form" class="relative group mb-10">
      <div class="flex gap-2">
        <!-- Country Code Dropdown -->
        <div class="relative w-44 country-dropdown">
          <button
            type="button"
            (click)="toggleDropdown()"
            class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white
                   focus:border-primary-500/50 focus:ring-2 focus:ring-primary-500/20 focus:outline-none 
                   transition-all duration-300 flex items-center justify-between"
            [class.border-red-500]="form.get(countryCodeControl)?.invalid && form.get(countryCodeControl)?.touched"
          >
            <div class="flex items-center space-x-2">
              <span class="text-xl">{{ selectedCountry?.flag }}</span>
              <span class="text-sm">{{ selectedCountry?.code }}</span>
            </div>
            <svg 
              class="w-4 h-4 text-gray-400 transition-transform duration-300"
              [class.transform]="isOpen"
              [class.rotate-180]="isOpen"
              fill="none" 
              stroke="currentColor" 
              viewBox="0 0 24 24"
            >
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7"></path>
            </svg>
          </button>

          <!-- Dropdown Menu -->
          <div
            *ngIf="isOpen"
            class="absolute z-50 w-72 mt-2 py-2 bg-gray-800 rounded-xl shadow-lg border border-white/10
                   max-h-[300px] overflow-y-auto custom-scrollbar"
            [@dropdownAnimation]
          >
            <!-- Search Input -->
            <div class="px-3 pb-2">
              <input
                type="text"
                [(ngModel)]="searchQuery"
                [ngModelOptions]="{standalone: true}"
                placeholder="Search countries..."
                class="w-full px-3 py-2 bg-white/5 border border-white/10 rounded-lg text-white
                       placeholder-gray-500 focus:border-primary-500/50 focus:ring-2
                       focus:ring-primary-500/20 focus:outline-none text-sm"
                (input)="filterCountries()"
              />
            </div>

            <!-- Country List -->
            <div class="country-list">
              <button
                *ngFor="let country of filteredCountries"
                type="button"
                (click)="selectCountry(country)"
                class="w-full px-4 py-2.5 text-left hover:bg-white/5 transition-colors
                       flex items-center space-x-3 group"
              >
                <span class="text-xl group-hover:scale-110 transition-transform">{{ country.flag }}</span>
                <div class="flex flex-col">
                  <span class="text-white text-sm">{{ country.name }}</span>
                  <span class="text-gray-400 text-xs">{{ country.code }}</span>
                </div>
              </button>
            </div>
          </div>
        </div>

        <!-- Phone Number Input -->
        <div class="flex-1">
          <input
            type="tel"
            [formControlName]="phoneNumberControl"
            [id]="phoneNumberControl"
            [class.border-red-500]="form.get(phoneNumberControl)?.invalid && form.get(phoneNumberControl)?.touched"
            class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white 
                   placeholder-gray-500 focus:border-primary-500/50 focus:ring-2 
                   focus:ring-primary-500/20 focus:outline-none transition-all duration-300"
            placeholder="Phone number"
            (input)="onPhoneNumberInput($event)"
          />
        </div>
      </div>

      <!-- Background gradient effect -->
      <div
        class="absolute inset-0 rounded-xl bg-gradient-to-r from-primary-500/10 
               via-secondary-500/10 to-accent-500/10 opacity-0 
               group-hover:opacity-50 peer-focus:opacity-100 -z-10 
               transition-opacity duration-500">
      </div>

      <!-- Error messages -->
      <div class="mt-2 space-y-1">
        <app-validation-message
          [control]="form.get(countryCodeControl)"
          field="Country code">
        </app-validation-message>
        <app-validation-message
          [control]="form.get(phoneNumberControl)"
          field="Phone number">
        </app-validation-message>
      </div>
    </div>
  `,
  styles: [`
    .custom-scrollbar {
      scrollbar-width: thin;
      scrollbar-color: rgba(255, 255, 255, 0.2) rgba(255, 255, 255, 0.1);
    }

    .custom-scrollbar::-webkit-scrollbar {
      width: 6px;
    }

    .custom-scrollbar::-webkit-scrollbar-track {
      background: rgba(255, 255, 255, 0.1);
      border-radius: 3px;
    }

    .custom-scrollbar::-webkit-scrollbar-thumb {
      background: rgba(255, 255, 255, 0.2);
      border-radius: 3px;
    }

    .custom-scrollbar::-webkit-scrollbar-thumb:hover {
      background: rgba(255, 255, 255, 0.3);
    }

    .country-list {
      max-height: calc(300px - 50px);
      overflow-y: auto;
    }

    @media (max-width: 640px) {
      .flex {
        flex-direction: column;
      }

      .w-44 {
        width: 100%;
      }
    }
  `]
})
export class PhoneInputComponent implements OnInit, OnDestroy {
  @Input() form!: FormGroup;
  @Input() countryCodeControl: string = 'countryCode';
  @Input() phoneNumberControl: string = 'phoneNumber';

  isOpen = false;
  searchQuery = '';
  countryCodes = COUNTRY_CODES;
  filteredCountries = this.countryCodes;
  selectedCountry: CountryCode | null = null;
  private destroy$ = new Subject<void>();

  constructor() {
    // Close dropdown when clicking outside
    document.addEventListener('click', (e: Event) => {
      const target = e.target as HTMLElement;
      if (!target.closest('.country-dropdown')) {
        this.isOpen = false;
      }
    });
  }

  ngOnInit() {
    // Initialize selected country
    const initialCode = this.form.get(this.countryCodeControl)?.value;
    if (initialCode) {
      this.selectedCountry = this.countryCodes.find(c => c.code === initialCode) || null;
    }

    // Combine country code and phone number when either changes
    this.form.get(this.countryCodeControl)?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.updateCombinedValue());

    this.form.get(this.phoneNumberControl)?.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.updateCombinedValue());
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleDropdown() {
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.searchQuery = '';
      this.filteredCountries = this.countryCodes;
    }
  }

  filterCountries() {
    const query = this.searchQuery.toLowerCase();
    this.filteredCountries = this.countryCodes.filter(country =>
      country.name.toLowerCase().includes(query) ||
      country.code.includes(query)
    );
  }

  selectCountry(country: CountryCode) {
    this.selectedCountry = country;
    this.form.get(this.countryCodeControl)?.setValue(country.code);
    this.isOpen = false;
  }

  onPhoneNumberInput(event: Event) {
    const input = event.target as HTMLInputElement;
    input.value = input.value.replace(/\D/g, ''); // Remove non-numeric characters
    this.form.get(this.phoneNumberControl)?.setValue(input.value, { emitEvent: true });
  }

  private updateCombinedValue() {
    const countryCode = this.form.get(this.countryCodeControl)?.value || '';
    const phoneNumber = this.form.get(this.phoneNumberControl)?.value || '';
    
    if (countryCode && phoneNumber) {
      const combinedValue = `${countryCode}${phoneNumber}`;
      this.form.patchValue({ phone: combinedValue }, { emitEvent: false });
    }
  }
}