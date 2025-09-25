import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Shelter } from 'src/app/models/shelter/shelter.model';
import { TranslationService } from 'src/app/common/services/translation.service';

@Component({
  selector: 'app-shelter-info',
  standalone: false,
  template: `
    <div class="space-y-8">
      <!-- Shelter Header -->
      <div class="flex items-center space-x-4">
        <div class="relative">
          <img
            *ngIf="shelter.user?.profilePhoto?.sourceUrl"
            [src]="shelter.user?.profilePhoto?.sourceUrl"
            [alt]="shelter.shelterName"
            class="w-16 h-16 rounded-full object-cover border-4 border-white shadow-lg transform transition-transform duration-300 hover:scale-110"
            style="aspect-ratio: 1 / 1;"
          />
          <div
            *ngIf="!shelter.user?.profilePhoto?.sourceUrl"
            class="w-16 h-16 rounded-full bg-gradient-to-br from-primary-500/20 to-accent-500/20 border-4 border-white shadow-lg flex items-center justify-center"
          >
            <ng-icon
              name="lucideHouse"
              [size]="'32'"
              class="text-primary-400 stroke-[2.5px]"
            >
            </ng-icon>
          </div>
        </div>

        <div>
          <a
            *ngIf="shelter.user?.id"
            (click)="navigateToShelterProfile(shelter.user?.id)"
            class="text-xl font-semibold text-white hover:text-primary-400 transition-colors duration-200 cursor-pointer underline decoration-dotted underline-offset-2 hover:decoration-solid"
            [title]="'APP.ADOPT.VIEW_SHELTER_PROFILE' | translate"
          >
            {{ shelter.shelterName }}
          </a>
          <h3
            *ngIf="!shelter.user?.id"
            class="text-xl font-semibold text-white"
          >
            {{ shelter.shelterName }}
          </h3>
          <p class="text-gray-400 text-sm">{{ shelter.description }}</p>
        </div>
      </div>

      <!-- Location Information -->
      <div
        *ngIf="shelter.user?.location"
        class="bg-white/10 backdrop-blur-sm rounded-xl p-4"
      >
        <a
          [href]="getGoogleMapsUrl()"
          target="_blank"
          rel="noopener noreferrer"
          class="flex items-center text-gray-300 group hover:text-white transition-colors"
        >
          <div
            class="w-8 h-8 rounded-full bg-primary-500/10 flex items-center justify-center mr-3 group-hover:bg-primary-500/20 transition-colors"
          >
            <ng-icon
              name="lucideMapPin"
              [size]="'18'"
              class="text-primary-400 stroke-[2.5px]"
            ></ng-icon>
          </div>
          <span class="group-hover:underline whitespace-nowrap"
            >{{ shelter.user?.location?.address }}
            {{ shelter.user?.location?.number }},
            {{ shelter.user?.location?.city }}
            {{ shelter.user?.location?.zipCode }}</span
          >
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
          <div
            class="w-8 h-8 rounded-full bg-primary-500/10 flex items-center justify-center group-hover:bg-primary-500/20 transition-colors"
          >
            <ng-icon
              name="lucideGlobe"
              [size]="'18'"
              class="text-primary-400 stroke-[2.5px]"
            ></ng-icon>
          </div>
          <span
            class="text-gray-300 group-hover:text-white transition-colors"
            >{{ 'APP.ADOPT.WEBSITE' | translate }}</span
          >
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
            <div
              class="w-8 h-8 rounded-full bg-primary-500/10 flex items-center justify-center group-hover:bg-primary-500/20 transition-colors"
            >
              <ng-icon
                name="lucideFacebook"
                [size]="'18'"
                class="text-primary-400 stroke-[2.5px]"
              ></ng-icon>
            </div>
            <span class="text-gray-300 group-hover:text-white transition-colors"
              >Facebook</span
            >
          </a>

          <!-- Instagram -->
          <a
            *ngIf="shelter.socialMedia.instagram"
            [href]="shelter.socialMedia.instagram"
            target="_blank"
            rel="noopener noreferrer"
            class="flex items-center space-x-3 px-4 py-3 bg-white/10 backdrop-blur-sm rounded-xl hover:bg-white/15 transition-all duration-300 group"
          >
            <div
              class="w-8 h-8 rounded-full bg-primary-500/10 flex items-center justify-center group-hover:bg-primary-500/20 transition-colors"
            >
              <ng-icon
                name="lucideInstagram"
                [size]="'18'"
                class="text-primary-400 stroke-[2.5px]"
              ></ng-icon>
            </div>
            <span class="text-gray-300 group-hover:text-white transition-colors"
              >Instagram</span
            >
          </a>
        </ng-container>
      </div>

      <!-- Operating Hours -->
      <div class="space-y-3">
        <div class="flex items-center space-x-3">
          <ng-icon
            name="lucideClock"
            [size]="'24'"
            class="text-primary-400 stroke-[2.5px]"
          ></ng-icon>
          <h4 class="text-lg font-medium text-white">
            {{ 'APP.ADOPT.OPERATING_HOURS' | translate }}
          </h4>
        </div>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
          <ng-container *ngFor="let day of days">
            <div
              class="px-4 py-3 bg-white/10 backdrop-blur-sm rounded-xl hover:bg-white/15 transition-all duration-300"
            >
              <div class="flex justify-between items-center">
                <div class="flex items-center space-x-2">
                  <ng-icon
                    name="lucideClock1"
                    [size]="'16'"
                    class="text-primary-400 stroke-[2.5px]"
                  ></ng-icon>
                  <span class="text-gray-300">{{
                    day.translationKey | translate
                  }}</span>
                </div>
                <span class="text-gray-300 ml-2">
                  <ng-container
                    *ngIf="
                      getOperatingHoursByKey(day.key) &&
                        getOperatingHoursByKey(day.key) !== 'closed';
                      else closedTemplate
                    "
                  >
                    {{ formatOpenHours(getOperatingHoursByKey(day.key)) }}
                  </ng-container>
                  <ng-template #closedTemplate>
                    {{ 'APP.ADOPT.CLOSED' | translate }}
                  </ng-template>
                </span>
              </div>
            </div>
          </ng-container>
        </div>
      </div>
    </div>
  `,
})
export class ShelterInfoComponent implements OnInit {
  @Input() shelter!: Shelter;

  days: { key: string; translationKey: string }[] = [
    { key: 'monday', translationKey: 'APP.ADOPT.DAYS.MONDAY' },
    { key: 'tuesday', translationKey: 'APP.ADOPT.DAYS.TUESDAY' },
    { key: 'wednesday', translationKey: 'APP.ADOPT.DAYS.WEDNESDAY' },
    { key: 'thursday', translationKey: 'APP.ADOPT.DAYS.THURSDAY' },
    { key: 'friday', translationKey: 'APP.ADOPT.DAYS.FRIDAY' },
    { key: 'saturday', translationKey: 'APP.ADOPT.DAYS.SATURDAY' },
    { key: 'sunday', translationKey: 'APP.ADOPT.DAYS.SUNDAY' },
  ];

  constructor(
    private translationService: TranslationService,
    private router: Router
  ) {}

  ngOnInit() {
    // No need for language subscription since translate pipe handles live updates
  }

  formatOpenHours(hours: string): string {
    if (!hours || hours === 'closed') {
      return '';
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
    const query = encodeURIComponent(
      `${address} ${number}, ${city} ${zipCode}`
    );
    return `https://www.google.com/maps/search/?api=1&query=${query}`;
  }

  navigateToShelterProfile(userId?: string): void {
    if (userId) {
      this.router.navigate(['/profile', userId]);
    }
  }
}
