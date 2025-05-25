import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { AnimalService } from 'src/app/services/animal.service';
import { Animal } from 'src/app/models/animal/animal.model';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { ErrorDetails } from 'src/app/common/ui/error-message-banner.component';
import { takeUntil } from 'rxjs/operators';
import { nameof } from 'ts-simple-nameof';
import { AnimalType } from 'src/app/models/animal-type/animal-type.model';
import { Breed } from 'src/app/models/breed/breed.model';
import { Shelter } from 'src/app/models/shelter/shelter.model';
import { File } from 'src/app/models/file/file.model';

@Component({
  selector: 'app-adopt',
  template: `
    <div class="min-h-screen bg-gray-900">
      <!-- Background elements -->
      <div class="fixed inset-0 z-0">
        <div class="absolute inset-0 bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900"></div>
        <div class="absolute inset-0 bg-gradient-to-br from-primary-900/20 via-secondary-900/20 to-accent-900/20 animate-gradient"></div>
        <div class="absolute inset-0 bg-gradient-radial from-transparent via-primary-900/10 to-transparent"></div>
      </div>

      <!-- Content -->
      <div class="relative z-10 max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <!-- Loading State -->
        <div *ngIf="isLoading" class="flex justify-center items-center min-h-[60vh]">
          <div class="relative">
            <div class="w-20 h-20 rounded-full border-4 border-primary-500/30 border-t-primary-500 animate-spin"></div>
            <div class="absolute inset-0 flex items-center justify-center">
              <ng-icon name="lucidePawPrint" [size]="'32'" class="text-primary-500 animate-bounce"></ng-icon>
            </div>
          </div>
        </div>

        <!-- Error State -->
        <div *ngIf="error" class="flex flex-col items-center justify-center min-h-[60vh]">
          <app-error-message-banner [error]="error"></app-error-message-banner>
          <button 
            (click)="loadAnimal()"
            class="mt-6 px-6 py-2 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300"
          >
            Δοκιμάστε ξανά
          </button>
        </div>

        <!-- Main Content -->
        <div *ngIf="animal && !isLoading" class="grid grid-cols-1 lg:grid-cols-2 gap-8">
          <!-- Left Column: Animal Info & Shelter Info -->
          <div class="space-y-8">
            <app-shelter-info [shelter]="animal.shelter!"></app-shelter-info>
          </div>

          <!-- Right Column: Adoption Form -->
          <div>
            <app-adoption-form 
              [animal]="animal"
              (applicationSubmitted)="onApplicationSubmitted($event)"
            ></app-adoption-form>
          </div>
        </div>
      </div>
    </div>
  `
})
export class AdoptComponent extends BaseComponent implements OnInit {
  animal?: Animal;
  isLoading = true;
  error?: ErrorDetails;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private animalService: AnimalService,
    private errorHandler: ErrorHandlerService
  ) {
    super();
  }

  ngOnInit() {
    this.route.params.pipe(
      takeUntil(this._destroyed)
    ).subscribe(params => {
      const id = params['id'];
      if (id) {
        this.loadAnimal(id);
      } else {
        this.router.navigate(['/404']);
      }
    });
  }

  loadAnimal(id: string = '') {
    id = !id ? this.route.snapshot.params['id'] : id;
    if (!id) {
      this.error = {
        title: 'Δεν βρέθηκε το κατοικίδιο',
        message: 'Παρακαλώ ελέγξτε τον σύνδεσμο ή δοκιμάστε ξανά αργότερα.',
        type: 'error'
      };

      this.isLoading = false;
      return;
    }


    this.isLoading = true;
    this.error = undefined;

    this.animalService.getSingle(id, [
      nameof<Animal>(x => x.id),
      nameof<Animal>(x => x.name),
      nameof<Animal>(x => x.gender),
      nameof<Animal>(x => x.description),
      [nameof<Animal>(x => x.attachedPhotos), nameof<File>(x => x.sourceUrl)].join('.'),
      nameof<Animal>(x => x.adoptionStatus),
      nameof<Animal>(x => x.weight),
      nameof<Animal>(x => x.age),
      nameof<Animal>(x => x.healthStatus),
      [nameof<Animal>(x => x.animalType), nameof<AnimalType>(x => x.name)].join('.'),
      [nameof<Animal>(x => x.breed), nameof<Breed>(x => x.name)].join('.'),
      [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.shelterName)].join('.'),
      [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.description)].join('.'),
      [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.website)].join('.'),
      [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.socialMedia)].join('.'),
      [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.operatingHours)].join('.')
    ]).subscribe({
      next: (animal) => {
        this.animal = animal;
        this.isLoading = false;
      },
      error: (error) => {
        this.error = this.errorHandler.handleError(error);
        this.isLoading = false;
      }
    });
  }

  onApplicationSubmitted(success: boolean) {
    if (success) {
      this.router.navigate(['/search']);
    }
  }
}