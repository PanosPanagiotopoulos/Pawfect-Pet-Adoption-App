import { Component, Input, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Shelter } from 'src/app/models/shelter/shelter.model';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { TranslationService } from 'src/app/common/services/translation.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-shelter-info',
  standalone: false,
  template: `
    <div class="space-y-8">
      <!-- Shelter Header -->
      <div class="flex items-center space-x-4">
        <div class="relative w-16 h-16 rounded-full overflow-hidden bg-gradient-to-br from-primary-500/20 to-accent-500/20 group">
          <img 
            *ngIf="shelter.user?.profilePhoto?.sourceUrl"
            [src]="shelter.user?.profilePhoto?.sourceUrl"
            [alt]="shelter.shelterName"
            class="w-full h-full object-cover transform transition-transform duration-300 group-hover:scale-110"
          />
          <ng-icon 
            *ngIf="!shelter.user?.profilePhoto?.sourceUrl"
            name="lucideHouse" 
            [size]="'32'" 
            class="absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 text-primary-400 stroke-[2.5px]">
          </ng-icon>
        </div>
        
        <div>
          <h3 class="text-xl font-semibold text-white">{{ shelter.shelterName }}</h3>
          <p class="text-gray-400 text-sm">{{ shelter.description }}</p>
        </div>
      </div>

      <!-- Location Information -->
      <div *ngIf="shelter.user?.location" class="bg-white/10 backdrop-blur-sm rounded-xl p-4">
        <a 
          [href]="getGoogleMapsUrl()"
          target="_blank"
          rel="noopener noreferrer"
          class="flex items-center text-gray-300 group hover:text-white transition-colors"
        >
          <div class="w-8 h-8 rounded-full bg-primary-500/10 flex items-center justify-center mr-3 group-hover:bg-primary-500/20 transition-colors">
            <ng-icon name="lucideMapPin" [size]="'18'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
          </div>
          <span class="group-hover:underline whitespace-nowrap">{{ shelter.user?.location?.address }} {{ shelter.user?.location?.number }}, {{ shelter.user?.location?.city }} {{ shelter.user?.location?.zipCode }}</span>
        </a>
      </div>

      <!-- Contact Links -->
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <!-- Website -->
        <a
          *ngIf="shelter.website"
          [href]="shelter.website"
          target="_blank"
          rel="noopener noreferrer"
          class="flex items-center space-x-3 px-4 py-3 bg-white/10 backdrop-blur-sm rounded-xl hover:bg-white/15 transition-all duration-300 group"
        >
          <div class="w-8 h-8 rounded-full bg-primary-500/10 flex items-center justify-center group-hover:bg-primary-500/20 transition-colors">
            <ng-icon name="lucideGlobe" [size]="'18'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
          </div>
          <span class="text-gray-300 group-hover:text-white transition-colors">{{ 'APP.ADOPT.WEBSITE' | translate }}</span>
        </a>

        <!-- Social Media -->
        <ng-container *ngIf="shelter.socialMedia">
          <!-- Facebook -->
          <a
            *ngIf="shelter.socialMedia.facebook"
            [href]="shelter.socialMedia.facebook"
            target="_blank"
            rel="noopener noreferrer"
            class="flex items-center space-x-3 px-4 py-3 bg-white/10 backdrop-blur-sm rounded-xl hover:bg-white/15 transition-all duration-300 group"
          >
            <div class="w-8 h-8 rounded-full bg-primary-500/10 flex items-center justify-center group-hover:bg-primary-500/20 transition-colors">
              <ng-icon name="lucideFacebook" [size]="'18'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
            </div>
            <span class="text-gray-300 group-hover:text-white transition-colors">Facebook</span>
          </a>

          <!-- Instagram -->
          <a
            *ngIf="shelter.socialMedia.instagram"
            [href]="shelter.socialMedia.instagram"
            target="_blank"
            rel="noopener noreferrer"
            class="flex items-center space-x-3 px-4 py-3 bg-white/10 backdrop-blur-sm rounded-xl hover:bg-white/15 transition-all duration-300 group"
          >
            <div class="w-8 h-8 rounded-full bg-primary-500/10 flex items-center justify-center group-hover:bg-primary-500/20 transition-colors">
              <ng-icon name="lucideInstagram" [size]="'18'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
            </div>
            <span class="text-gray-300 group-hover:text-white transition-colors">Instagram</span>
          </a>
        </ng-container>
      </div>

      <!-- Operating Hours -->
      <div class="space-y-3">
        <div class="flex items-center space-x-3">
          <ng-icon name="lucideClock" [size]="'24'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
          <h4 class="text-lg font-medium text-white">{{ 'APP.ADOPT.OPERATING_HOURS' | translate }}</h4>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
          <ng-container *ngFor="let day of days">
            <div class="px-4 py-3 bg-white/10 backdrop-blur-sm rounded-xl hover:bg-white/15 transition-all duration-300">
              <div class="flex justify-between items-center">
                <div class="flex items-center space-x-2">
                  <ng-icon name="lucideClock1" [size]="'16'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
                  <span class="text-gray-300">{{ day.label }}</span>
                </div>
                <span class="text-gray-300 ml-2">{{ formatHours(getOperatingHoursByKey(day.key)) }}</span>
              </div>
            </div>
          </ng-container>
        </div>
      </div>
    </div>
  `
})
export class ShelterInfoComponent implements OnInit, OnDestroy {
  @Input() shelter!: Shelter;

  days: { key: string, label: string }[] = [];
  private langSub?: Subscription;

  constructor(
    private translationService: TranslationService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit() {
    this.updateDays();
    this.langSub = this.translationService.languageChanged$.subscribe(() => {
      this.updateDays();
      this.cdr.markForCheck();
    });
  }

  ngOnDestroy() {
    this.langSub?.unsubscribe();
  }

  private updateDays() {
    this.days = [
      { key: 'monday', label: this.translationService.translate('APP.ADOPT.DAYS.MONDAY') },
      { key: 'tuesday', label: this.translationService.translate('APP.ADOPT.DAYS.TUESDAY') },
      { key: 'wednesday', label: this.translationService.translate('APP.ADOPT.DAYS.WEDNESDAY') },
      { key: 'thursday', label: this.translationService.translate('APP.ADOPT.DAYS.THURSDAY') },
      { key: 'friday', label: this.translationService.translate('APP.ADOPT.DAYS.FRIDAY') },
      { key: 'saturday', label: this.translationService.translate('APP.ADOPT.DAYS.SATURDAY') },
      { key: 'sunday', label: this.translationService.translate('APP.ADOPT.DAYS.SUNDAY') }
    ];
  }

  formatHours(hours: string): string {
    if (!hours || hours === 'closed') {
      return this.translationService.translate('APP.ADOPT.CLOSED');
    }
    const [open, close] = hours.split(',');
    return `${open} - ${close}`;
  }

  getOperatingHoursByKey(key: string): string {
    const operatingHours = this.shelter?.operatingHours as any;
    return operatingHours && operatingHours[key] ? operatingHours[key] : '';
  }

  getGoogleMapsUrl(): string {
    if (!this.shelter.user?.location) return '';
    const { address, number, city, zipCode } = this.shelter.user.location;
    const query = encodeURIComponent(`${address} ${number}, ${city} ${zipCode}`);
    return `https://www.google.com/maps/search/?api=1&query=${query}`;
  }
}