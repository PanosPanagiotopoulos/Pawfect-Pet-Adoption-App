import { Component, Input } from '@angular/core';
import { Animal } from 'src/app/models/animal/animal.model';

@Component({
  selector: 'app-saved-animals-grid',
  template: `
    <div *ngIf="animals && animals.length > 0" class="space-y-6">
      <div class="flex items-center justify-between">
        <h2 class="text-xl font-bold text-white">Αποθηκευμένα κατοικίδια</h2>
        <span class="px-2 py-1 bg-primary-500/20 rounded-full text-sm text-primary-400">{{ animals.length }}</span>
      </div>
      
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        <app-animal-card 
          *ngFor="let animal of animals" 
          [animal]="animal"
        ></app-animal-card>
      </div>
    </div>
  `,
  styles: []
})
export class SavedAnimalsGridComponent {
  @Input() animals: Animal[] = [];
}