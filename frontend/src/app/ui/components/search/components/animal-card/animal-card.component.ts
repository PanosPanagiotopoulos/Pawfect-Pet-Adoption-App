import { Component, Input, OnInit } from '@angular/core';
import { Animal } from 'src/app/models/animal/animal.model';
import { Router } from '@angular/router';

@Component({
  selector: 'app-animal-card',
  template: `
    <div 
      class="relative group overflow-hidden rounded-xl bg-gradient-to-br from-gray-800/95 to-gray-900/95 transition-all duration-300 hover:-translate-y-1 hover:shadow-xl hover:shadow-primary-500/10 cursor-pointer"
      (click)="navigateToDetail()"
    >
      <!-- Image -->
      <div class="aspect-video overflow-hidden relative">
        <img
          [src]="animal.photos?.[0] || 'assets/placeholder.jpg'"
          [alt]="animal.name"
          class="w-full h-full object-cover transform transition-transform duration-300 group-hover:scale-110"
          (error)="onImageError($event)"
        />
        <!-- Gradient Overlay -->
        <div class="absolute inset-0 bg-gradient-to-t from-gray-900 via-gray-900/50 to-transparent"></div>
        
        <!-- Type Badge -->
        <div class="absolute top-2 left-2 px-2 py-1 bg-primary-500/90 backdrop-blur-sm rounded-full text-white text-xs font-medium">
          {{ animal.animalType?.name }}
        </div>
      </div>

      <!-- Content -->
      <div class="p-4">
        <!-- Name and Age -->
        <div class="flex items-center justify-between mb-2">
          <h3 class="text-lg font-semibold text-white">{{ animal.name }}</h3>
          <span class="text-sm font-medium text-primary-400">{{ animal.age }} ετών</span>
        </div>
        
        <!-- Details -->
        <div class="flex flex-wrap gap-2">
          <span class="px-2 py-0.5 bg-white/10 rounded-full text-xs text-gray-300">
            {{ animal.breed?.name }}
          </span>
          <span class="px-2 py-0.5 bg-white/10 rounded-full text-xs text-gray-300" *ngIf="animal.weight">
            {{ animal.weight }} kg
          </span>
        </div>

        <!-- Description Preview -->
        <p class="mt-2 text-xs text-gray-400 line-clamp-2">{{ animal.description }}</p>
        
        <!-- View Button (appears on hover) -->
        <div class="mt-3 overflow-hidden h-0 group-hover:h-8 transition-all duration-300 ease-out">
          <button 
            class="w-full py-1 bg-primary-600/90 hover:bg-primary-600 text-white text-xs rounded-lg transition-colors"
          >
            Περισσότερα
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
  `]
})
export class AnimalCardComponent implements OnInit {
  @Input() animal!: Animal;
  
  // Add missing properties
  private currentImageIndex = 0;
  
  constructor(private router: Router) {}
  
  ngOnInit(): void {
    // Initialize with first photo if available
    this.currentImageIndex = 0;
  }
  
  navigateToDetail() {
    this.router.navigate(['/animals', this.animal.id]);
  }
  
  onImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    
    if (this.animal && this.animal.photos && this.animal.photos.length > 0) {
      // Try next image if available
      if (this.currentImageIndex < this.animal.photos.length - 1) {
        this.currentImageIndex++;
        img.src = this.animal.photos[this.currentImageIndex];
      } else {
        // If we've tried all photos, use placeholder
        img.src = 'assets/placeholder.jpg';
      }
    } else {
      // Fallback to placeholder if no photos
      img.src = 'assets/placeholder.jpg';
    }
  }
}