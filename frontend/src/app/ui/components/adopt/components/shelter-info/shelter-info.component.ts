import { Component, Input } from '@angular/core';
import { Shelter } from 'src/app/models/shelter/shelter.model';

@Component({
  selector: 'app-shelter-info',
  template: `
    <div class="bg-white/5 backdrop-blur-lg rounded-2xl p-6 border border-white/10">
      <h2 class="text-2xl font-bold text-white mb-6">Πληροφορίες Καταφυγίου</h2>
      
      <!-- Shelter Name and Description -->
      <div class="mb-6">
        <h3 class="text-xl font-semibold text-white mb-2">{{ shelter.shelterName }}</h3>
        <p class="text-gray-300">{{ shelter.description }}</p>
      </div>

      <!-- Contact Information -->
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-6">
        <!-- Website -->
        <a
          *ngIf="shelter.website"
          [href]="shelter.website"
          target="_blank"
          rel="noopener noreferrer"
          class="flex items-center space-x-2 px-4 py-3 bg-white/5 rounded-xl hover:bg-white/10 transition-colors"
        >
          <ng-icon name="lucideGlobe" [size]="'20'" class="text-primary-400"></ng-icon>
          <span class="text-gray-300">Ιστοσελίδα</span>
        </a>

        <!-- Social Media -->
        <ng-container *ngIf="shelter.socialMedia">
          <!-- Facebook -->
          <a
            *ngIf="shelter.socialMedia.facebook"
            [href]="shelter.socialMedia.facebook"
            target="_blank"
            rel="noopener noreferrer"
            class="flex items-center space-x-2 px-4 py-3 bg-white/5 rounded-xl hover:bg-white/10 transition-colors"
          >
            <ng-icon name="lucideFacebook" [size]="'20'" class="text-primary-400"></ng-icon>
            <span class="text-gray-300">Facebook</span>
          </a>

          <!-- Instagram -->
          <a
            *ngIf="shelter.socialMedia.instagram"
            [href]="shelter.socialMedia.instagram"
            target="_blank"
            rel="noopener noreferrer"
            class="flex items-center space-x-2 px-4 py-3 bg-white/5 rounded-xl hover:bg-white/10 transition-colors"
          >
            <ng-icon name="lucideInstagram" [size]="'20'" class="text-primary-400"></ng-icon>
            <span class="text-gray-300">Instagram</span>
          </a>
        </ng-container>
      </div>

      <!-- Operating Hours -->
      <div class="space-y-4">
        <h4 class="text-lg font-medium text-white">Ώρες Λειτουργίας</h4>
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <ng-container *ngFor="let day of getDays()">
            <div class="px-4 py-3 bg-white/5 rounded-xl">
              <div class="flex justify-between items-center">
                <span class="text-gray-300">{{ day.label }}</span>
                <span class="text-gray-300">{{ formatHours(getOperatingHoursByKey(day.key)) }}</span>
              </div>
            </div>
          </ng-container>
        </div>
      </div>
    </div>
  `
})
export class ShelterInfoComponent {
  @Input() shelter!: Shelter;
  getDays() {
    return [
      { key: 'monday', label: 'Δευτέρα' },
      { key: 'tuesday', label: 'Τρίτη' },
      { key: 'wednesday', label: 'Τετάρτη' },
      { key: 'thursday', label: 'Πέμπτη' },
      { key: 'friday', label: 'Παρασκευή' },
      { key: 'saturday', label: 'Σάββατο' },
      { key: 'sunday', label: 'Κυριακή' }
    ];
  }

  formatHours(hours: string): string {
    if (!hours || hours === 'closed') {
      return 'Κλειστό';
    }
    const [open, close] = hours.split(',');
    return `${open} - ${close}`;
  }

  getOperatingHoursByKey(key: string): string 
  {
     const operatingHours = this.shelter?.operatingHours as any;
     return operatingHours && operatingHours[key] ? operatingHours[key] : '';
  }
}