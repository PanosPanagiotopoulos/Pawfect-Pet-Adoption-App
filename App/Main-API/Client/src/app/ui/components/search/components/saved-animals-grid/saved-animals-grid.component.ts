import { Component, Input, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Animal } from 'src/app/models/animal/animal.model';
import { AnimalCardComponent } from '../animal-card/animal-card.component';
import { PetDetailsDialogComponent } from 'src/app/common/ui/pet-details-dialog/pet-details-dialog.component';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-saved-animals-grid',
  template: `
    <div class="space-y-8 lg:space-y-10">
      <h2 *ngIf="animals?.length" class="text-2xl lg:text-3xl font-bold text-white">{{ 'APP.SEARCH.SAVED_PETS' | translate }}</h2>
      
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-8 lg:gap-10 max-w-[1600px] mx-auto">
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
  changeDetection: ChangeDetectionStrategy.OnPush,
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