import { Component, Input, Output, EventEmitter, ChangeDetectorRef } from '@angular/core';
import { Animal } from 'src/app/models/animal/animal.model';
import { Router } from '@angular/router';
import { trigger, transition, style, animate } from '@angular/animations';
import { UtilsService } from 'src/app/common/services/utils.service';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-animal-card',
  standalone: false,
  template: `
    <div 
      [@fadeIn]
      class="relative group overflow-hidden rounded-xl bg-gradient-to-br from-gray-800/95 to-gray-900/95 transition-all duration-300 hover:-translate-y-1 hover:shadow-xl hover:shadow-primary-500/10 overflow-x-hidden"
    >
      <!-- Image -->
      <div class="aspect-video overflow-hidden relative">
        <img
          [src]="currentImageUrl"
          [alt]="animal.name"
          class="w-full h-full object-cover transform transition-transform duration-300 group-hover:scale-110"
          (error)="onImageError($event)"
        />
        <!-- Gradient Overlay -->
        <div class="absolute inset-0 bg-gradient-to-t from-gray-900 via-gray-900/50 to-transparent"></div>
        
        <!-- Type Badge -->
        <div class="absolute top-3 left-3 px-3 py-1.5 bg-primary-500/90 backdrop-blur-sm rounded-full text-white text-sm font-medium">
          {{ animal.animalType?.name }}
        </div>

        <!-- Info Button -->
        <button 
          (click)="onInfoClick()"
          class="action-button absolute top-3 right-3 p-2.5 bg-white/15 hover:bg-white/25 backdrop-blur-sm rounded-full transition-all duration-300 shadow-lg">
          <ng-icon name="lucideInfo" [size]="'22'" class="text-white stroke-[2.5px]"></ng-icon>
        </button>
      </div>

      <!-- Content -->
      <div class="p-5">
        <!-- Name and Age -->
        <div class="flex items-center justify-between mb-3">
          <h3 *ngIf="animal.name" class="text-xl font-semibold text-white">{{ animal.name }}</h3>
          <span *ngIf="animal.age" class="text-base font-medium text-primary-400">{{ animal.age }} {{ 'APP.SEARCH.YEARS_OLD' | translate }}</span>
        </div>
        
        <!-- Details -->
        <div class="flex flex-wrap gap-3">
          <span class="px-3 py-1 bg-white/10 rounded-full text-sm text-gray-300" *ngIf="animal.breed">
            {{ animal.breed.name }}
          </span>
          <span class="px-3 py-1 bg-white/10 rounded-full text-sm text-gray-300" *ngIf="animal.weight">
            {{ animal.weight + ' kg' }} 
          </span>
        </div>

        <!-- Description Preview -->
        <p class="mt-3 text-sm text-gray-400 line-clamp-2">{{ animal.description || ('APP.SEARCH.NO_DESCRIPTION' | translate) }}</p>

        <!-- Adopt Button (visible on hover) -->
        <div class="absolute inset-x-0 bottom-0 p-5 transform translate-y-full group-hover:translate-y-0 transition-transform duration-300 bg-gradient-to-t from-gray-900 via-gray-900/95 to-transparent">
          <button 
            (click)="navigateToAdoption($event)"
            class="w-full py-3 bg-gradient-to-r from-primary-600 to-accent-600 text-white text-base font-medium rounded-lg hover:shadow-lg hover:shadow-primary-500/20 transition-all duration-300 transform hover:-translate-y-1"
          >
            {{ 'APP.SEARCH.ADOPT_ME_NOW' | translate }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .line-clamp-2 {
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .action-button {
      @apply flex items-center justify-center;
      svg {
        @apply stroke-[2.5px] stroke-current;
      }
    }
  `],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(20px)' }),
        animate('400ms cubic-bezier(0.4, 0, 0.2, 1)', 
          style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ],
})
export class AnimalCardComponent {
  @Input() animal!: Animal;
  @Output() showDetails = new EventEmitter<Animal>();
  
  currentImageUrl: string = '';
  
  constructor(
    private router: Router,
    private utilsService: UtilsService,
    private cdr: ChangeDetectorRef
  ) {}
  
  ngOnInit() {
    this.loadImage();
  }

  async loadImage() {
    if (this.animal) {
      this.currentImageUrl = await this.utilsService.tryLoadImages(this.animal);
      this.cdr.markForCheck();
    }
  }
  
  onImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    img.src = '/assets/placeholder.jpg';
  }

  navigateToAdoption(event: Event) {
    event.stopPropagation();
    this.router.navigate(['/adopt', this.animal.id]);
  }
  
  onInfoClick() {
    this.showDetails.emit(this.animal);
  }
}