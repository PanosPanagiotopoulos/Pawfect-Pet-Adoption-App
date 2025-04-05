import { Component, Input, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Animal } from 'src/app/models/animal/animal.model';
import { AnimalCardComponent } from '../animal-card/animal-card.component';
import { PetDetailsDialogComponent } from 'src/app/common/ui/pet-details-dialog/pet-details-dialog.component';

@Component({
  selector: 'app-saved-animals-grid',
  template: `
    <div class="space-y-6">
      <h2 *ngIf="animals?.length" class="text-2xl font-bold text-white">Αποθηκευμένα Κατοικίδια</h2>
      
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        <app-animal-card
          *ngFor="let animal of animals"
          [animal]="animal!"
          (showDetails)="openDialog($event)"
        ></app-animal-card>
      </div>

      <app-pet-details-dialog
        [animal]="selectedAnimal!"
        [isOpen]="isDialogOpen"
        (closeDialog)="closeDialog()"
      ></app-pet-details-dialog>
    </div>
  `,
  standalone: false,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SavedAnimalsGridComponent {
  @Input() animals: Animal[] = [];
  
  isDialogOpen = false;
  selectedAnimal?: Animal;

  constructor(private cdr: ChangeDetectorRef) {}

  openDialog(animal: Animal) {
    this.selectedAnimal = animal;
    this.isDialogOpen = true;
    this.cdr.markForCheck();
  }

  closeDialog() {
    this.isDialogOpen = false;
    this.cdr.markForCheck();
  }
}