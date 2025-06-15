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
import { User } from 'src/app/models/user/user.model';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-adopt',
  template: `
    <div class="min-h-screen bg-gray-900">
      <!-- Enhanced Background elements -->
      <div class="fixed inset-0 z-0">
        <div class="absolute inset-0 bg-gradient-to-br from-gray-900 via-gray-800 to-gray-900"></div>
        <div class="absolute inset-0 bg-gradient-to-br from-primary-900/20 via-secondary-900/20 to-accent-900/20 animate-gradient"></div>
        <div class="absolute inset-0 bg-gradient-radial from-transparent via-primary-900/10 to-transparent"></div>
        
        <!-- Animated Pet Paw Prints -->
        <div class="absolute inset-0 overflow-hidden pointer-events-none">
          <div class="absolute top-1/4 left-1/4 w-32 h-32 text-primary-500/5 transform rotate-45 animate-float-1">
            <ng-icon name="lucidePawPrint" [size]="'128'"></ng-icon>
          </div>
          <div class="absolute bottom-1/3 right-1/3 w-24 h-24 text-secondary-500/5 transform -rotate-12 animate-float-2">
            <ng-icon name="lucideHeart" [size]="'96'"></ng-icon>
          </div>
          <div class="absolute top-1/3 right-1/4 w-20 h-20 text-accent-500/5 transform rotate-12 animate-float-3">
            <ng-icon name="lucideDog" [size]="'80'"></ng-icon>
          </div>
          <div class="absolute bottom-1/4 left-1/3 w-16 h-16 text-primary-500/5 transform -rotate-45 animate-float-4">
            <ng-icon name="lucideCake" [size]="'64'"></ng-icon>
          </div>
          <div class="absolute top-1/2 left-1/2 w-40 h-40 text-primary-500/5 transform -rotate-12 animate-float-5">
            <ng-icon name="lucideHeartPulse" [size]="'160'"></ng-icon>
          </div>
        </div>

        <!-- Animated Gradient Orbs -->
        <div class="absolute top-0 left-0 w-96 h-96 bg-primary-500/10 rounded-full filter blur-3xl animate-pulse"></div>
        <div class="absolute bottom-0 right-0 w-96 h-96 bg-accent-500/10 rounded-full filter blur-3xl animate-pulse delay-1000"></div>
        <div class="absolute top-1/2 left-1/2 w-96 h-96 bg-secondary-500/10 rounded-full filter blur-3xl animate-pulse delay-500"></div>
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
          <div @fadeInLeft class="space-y-8">
            <!-- Adoption Header -->
            <div class="text-center mb-8">
              <h1 class="text-3xl font-bold text-white mb-2">Ώρα για Υιοθεσία!</h1>
              <p class="text-gray-400">Ένα νέο μέλος της οικογένειάς σας σας περιμένει</p>
            </div>

            <!-- Animal Info Card -->
            <div class="relative rounded-2xl overflow-hidden group">
              <!-- Background Image with Overlay -->
              <div class="absolute inset-0">
                <img 
                  [src]="animal.attachedPhotos?.[0]?.sourceUrl || '/assets/placeholder.jpg'" 
                  [alt]="animal.name"
                  class="w-full h-full object-cover transform transition-transform duration-500 group-hover:scale-110"
                />
                <div class="absolute inset-0 bg-gradient-to-br from-gray-900/95 via-gray-900/90 to-gray-900/95"></div>
              </div>

              <!-- Content -->
              <div class="relative p-6 space-y-6">
                <div class="flex justify-between items-start">
                  <div class="flex items-center space-x-3">
                    <ng-icon name="lucideHeartPulse" [size]="'24'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
                    <h2 class="text-2xl font-bold text-white">Στοιχεία Κατοικιδίου</h2>
                  </div>
                  <button 
                    (click)="openDialog()"
                    class="p-2 bg-white/10 hover:bg-white/20 rounded-lg transition-all duration-300 hover:scale-110"
                  >
                    <ng-icon name="lucideInfo" [size]="'24'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
                  </button>
                </div>
                
                <!-- Animal Details -->
                <div class="space-y-4">
                  <!-- Name and Type -->
                  <div class="flex items-center justify-between">
                    <div>
                      <h3 class="text-2xl font-semibold text-white">{{ animal.name }}</h3>
                      <div class="flex items-center space-x-2 mt-1">
                        <ng-icon name="lucidePawPrint" [size]="'16'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
                        <p class="text-gray-400">{{ animal.animalType?.name }} • {{ animal.breed?.name }}</p>
                      </div>
                    </div>
                    <div 
                      class="w-10 h-10 rounded-full flex items-center justify-center shadow-lg" 
                      [ngClass]="{'bg-blue-500/25': animal.gender === 1, 'bg-pink-500/25': animal.gender === 2}">
                      <ng-icon 
                        [name]="animal.gender === 1 ? 'lucideMars' : 'lucideVenus'" 
                        [size]="'20'" 
                        [class]="animal.gender === 1 ? 'text-blue-400 stroke-[2.5px]' : 'text-pink-400 stroke-[2.5px]'">
                      </ng-icon>
                    </div>
                  </div>

                  <!-- Quick Info Grid -->
                  <div class="grid grid-cols-2 gap-4">
                    <div class="bg-white/10 backdrop-blur-sm rounded-xl p-3 flex items-center space-x-2 group-hover:bg-white/15 transition-all duration-300">
                      <ng-icon name="lucideCake" [size]="'18'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
                      <span class="text-gray-300">{{ animal.age }} ετών</span>
                    </div>
                    <div class="bg-white/10 backdrop-blur-sm rounded-xl p-3 flex items-center space-x-2 group-hover:bg-white/15 transition-all duration-300">
                      <ng-icon name="lucideScale" [size]="'18'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
                      <span class="text-gray-300">{{ animal.weight }} kg</span>
                    </div>
                    <div class="bg-white/10 backdrop-blur-sm rounded-xl p-3 flex items-center space-x-2 group-hover:bg-white/15 transition-all duration-300">
                      <ng-icon name="lucideActivity" [size]="'18'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
                      <span class="text-gray-300">{{ animal.healthStatus || 'Καλή κατάσταση' }}</span>
                    </div>
                    <div class="bg-white/10 backdrop-blur-sm rounded-xl p-3 flex items-center space-x-2 group-hover:bg-white/15 transition-all duration-300">
                      <ng-icon name="lucideHeart" [size]="'18'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
                      <span class="text-gray-300">{{ getAdoptionStatusLabel(animal.adoptionStatus) }}</span>
                    </div>
                  </div>

                  <!-- Description -->
                  <div class="bg-white/10 backdrop-blur-sm rounded-xl p-4">
                    <p class="text-gray-300 text-sm leading-relaxed">{{ animal.description || 'Δεν υπάρχει διαθέσιμη περιγραφή.' }}</p>
                  </div>
                </div>
              </div>
            </div>

            <!-- Shelter Info Dropdown -->
            <div class="bg-white/5 backdrop-blur-lg rounded-2xl border border-white/10 overflow-hidden hover:border-primary-500/30 transition-all duration-300">
              <button 
                (click)="toggleShelterInfo()"
                role="button"
                [attr.aria-expanded]="isShelterInfoOpen"
                class="w-full px-6 py-4 flex justify-between items-center hover:bg-white/5 transition-all duration-300"
              >
                <div class="flex items-center space-x-3">
                  <ng-icon name="lucideHouse" [size]="'24'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
                  <h2 class="text-2xl font-bold text-white">Στοιχεία Καταφυγίου</h2>
                </div>
                <ng-icon 
                  [name]="isShelterInfoOpen ? 'lucideChevronUp' : 'lucideChevronDown'" 
                  [size]="'24'" 
                  class="text-primary-400 transition-transform duration-300"
                  [class.rotate-180]="isShelterInfoOpen"
                ></ng-icon>
              </button>

              <div 
                *ngIf="isShelterInfoOpen"
                [@slideDown]
                class="p-6"
              >
                <app-shelter-info [shelter]="animal.shelter!"></app-shelter-info>
              </div>
            </div>
          </div>

          <div @fadeInRight>
            <!-- Adoption Form Card -->
            <div class="bg-white/5 backdrop-blur-lg rounded-2xl p-6 border border-white/10 hover:border-primary-500/30 transition-all duration-300">
              <div class="flex items-center space-x-3 mb-6">
                <ng-icon name="lucideHeart" [size]="'24'" class="text-primary-400 stroke-[2.5px]"></ng-icon>
                <h2 class="text-2xl font-bold text-white">Αίτηση Υιοθεσίας</h2>
              </div>
              <app-adoption-form 
                [animal]="animal"
                (applicationSubmitted)="onApplicationSubmitted($event)"
              ></app-adoption-form>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Pet Details Dialog -->
    <app-pet-details-dialog 
      [animal]="animal!" 
      [isOpen]="isDialogOpen" 
      (closeDialog)="closeDialog()">
    </app-pet-details-dialog>
  `,
  animations: [
    trigger('fadeInLeft', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(-20px)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ]),
    trigger('fadeInRight', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(20px)' }),
        animate('300ms ease-out', style({ opacity: 1, transform: 'translateX(0)' }))
      ])
    ]),
    trigger('slideDown', [
      transition(':enter', [
        style({ height: 0, opacity: 0 }),
        animate('300ms cubic-bezier(0.4, 0, 0.2, 1)', 
          style({ height: '*', opacity: 1 }))
      ]),
      transition(':leave', [
        style({ height: '*', opacity: 1 }),
        animate('300ms cubic-bezier(0.4, 0, 0.2, 1)', 
          style({ height: 0, opacity: 0 }))
      ])
    ])
  ]
})
export class AdoptComponent extends BaseComponent implements OnInit {
  animal?: Animal;
  isLoading = true;
  error?: ErrorDetails;
  isDialogOpen = false;
  isShelterInfoOpen = true;

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

  /** Loads animal data based on the provided ID */
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
      [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.operatingHours)].join('.'),
      [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.user), nameof<User>(x => x.location)].join('.'),
      [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.user), nameof<User>(x => x.profilePhoto), nameof<File>(x => x.sourceUrl)].join('.')
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

  onApplicationSubmitted(id: string) {
    if (id) {
      this.router.navigate([`/profile/adoption-applications/${id}`]);
    }
  }

  openDialog(): void {
    this.isDialogOpen = true;
  }

  closeDialog(): void {
    this.isDialogOpen = false;
  }

  /** Toggles the shelter info dropdown visibility */
  toggleShelterInfo(): void {
    this.isShelterInfoOpen = !this.isShelterInfoOpen;
  }

  getAdoptionStatusLabel(status?: number): string {
    switch (status) {
      case 1:
        return 'Διαθέσιμο';
      case 2:
        return 'Σε εξέλιξη';
      case 3:
        return 'Υιοθετημένο';
      default:
        return 'Διαθέσιμο';
    }
  }
}