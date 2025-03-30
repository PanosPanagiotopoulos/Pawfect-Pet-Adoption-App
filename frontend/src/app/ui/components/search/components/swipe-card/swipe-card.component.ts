import { Component, Input, Output, EventEmitter, ElementRef, ViewChild, AfterViewInit, OnInit, SimpleChanges, OnChanges } from '@angular/core';
import { Animal } from 'src/app/models/animal/animal.model';
import { trigger, state, style, transition, animate } from '@angular/animations';

@Component({
  selector: 'app-swipe-card',
  template: `
    <div class="relative w-full max-w-sm mx-auto h-[600px] flex items-center justify-center" #container>
    <!-- Card -->
    <div
      *ngIf="animal"
      #card
      [@cardState]="getCardState()"
      class="absolute inset-0 bg-gradient-to-br from-gray-800/95 to-gray-900/95 backdrop-blur-lg rounded-3xl shadow-2xl overflow-hidden cursor-grab active:cursor-grabbing"
      [style.transform]="getTransform()"
      [style.opacity]="getOpacity()">
      
      <!-- Card Content -->
      <div class="relative h-full flex flex-col">
        <!-- Image Container - Adjusted height for more spacing -->
        <div class="h-3/6 relative overflow-hidden">
          <img
            [src]="currentImageUrl"
            [alt]="animal.name"
            class="w-full h-full object-cover transition-opacity duration-300"
            (error)="onImageError($event)" />
          <!-- Gradient Overlay -->
          <div class="absolute inset-0 bg-gradient-to-t from-gray-900 via-gray-900/50 to-transparent"></div>
          
          <!-- Type Badge -->
          <div class="absolute top-4 left-4 px-3 py-1.5 bg-primary-500/90 backdrop-blur-sm rounded-full text-white text-sm font-medium shadow-lg">
            {{ animal.animalType?.name || 'Άγνωστο' }}
          </div>
  
          <!-- Info Button -->
          <button 
            (click)="showDetails($event)"
            class="absolute top-4 right-4 p-2.5 bg-white/15 hover:bg-white/25 backdrop-blur-sm rounded-full transition-all duration-300 shadow-lg">
            <ng-icon name="lucideInfo" [size]="'28'" class="text-white"></ng-icon>
          </button>
          
          <!-- Image Navigation -->
          <div class="absolute bottom-4 left-0 right-0 flex justify-center gap-1.5" *ngIf="animal.photos && animal.photos.length > 0">
            <div *ngFor="let photo of animal.photos; let i = index"
                 [class.bg-white]="currentImageIndex === i"
                 [ngStyle]="{ 'background-color': currentImageIndex !== i ? 'rgba(255, 255, 255, 0.4)' : 'white' }"
                 class="w-2 h-2 rounded-full cursor-pointer transition-all"
                 (click)="changeImage(i, $event)">
            </div>
          </div>
        </div>
        
        <!-- Info Section - Enhanced with more details -->
        <div class="absolute bottom-0 left-0 right-0 p-6 text-white">
          <!-- Name, Age and Gender -->
          <div class="flex items-center justify-between mb-3">
            <div class="flex items-center space-x-2">
              <h3 class="text-2xl font-bold">{{ animal.name }}</h3>
              <div 
                class="w-7 h-7 rounded-full flex items-center justify-center shadow-md" 
                [ngClass]="{'bg-blue-500/25': animal.gender === 1, 'bg-pink-500/25': animal.gender === 2}">
                <ng-icon 
                  [name]="animal.gender === 1 ? 'lucideMars' : 'lucideVenus'" 
                  [size]="'16'" 
                  [class]="animal.gender === 1 ? 'text-blue-400' : 'text-pink-400'">
                </ng-icon>
              </div>
            </div>
            <div class="flex items-center gap-2">
              <span class="text-lg font-medium text-primary-400">{{ animal.age }} ετών</span>
              <span *ngIf="animal.adoptionStatus" 
                    class="px-2 py-0.5 text-xs font-medium rounded-full"
                    [ngClass]="{
                      'bg-green-500/20 text-green-300': animal.adoptionStatus === 1,
                      'bg-yellow-500/20 text-yellow-300': animal.adoptionStatus === 2,
                      'bg-red-500/20 text-red-300': animal.adoptionStatus === 3
                    }">
                {{ getAdoptionStatusLabel(animal.adoptionStatus) }}
              </span>
            </div>
          </div>
  
          <!-- Breed and Weight -->
          <div class="flex flex-wrap items-center gap-2 mb-3">
            <span class="px-3 py-1 bg-white/10 backdrop-blur-sm rounded-full text-sm font-medium shadow-sm hover:bg-white/15 transition-colors">
              {{ animal.breed?.name || 'Άγνωστη φυλή' }}
            </span>
            <span class="px-3 py-1 bg-white/10 backdrop-blur-sm rounded-full text-sm font-medium shadow-sm hover:bg-white/15 transition-colors" *ngIf="animal.weight">
              {{ animal.weight }} kg
            </span>
            <span class="px-3 py-1 bg-white/10 backdrop-blur-sm rounded-full text-sm font-medium shadow-sm hover:bg-white/15 transition-colors" *ngIf="animal.healthStatus">
              {{ animal.healthStatus }}
            </span>
            <span class="px-3 py-1 bg-white/10 backdrop-blur-sm rounded-full text-sm font-medium shadow-sm hover:bg-white/15 transition-colors" *ngIf="animal.shelter?.shelterName">
              <ng-icon name="lucideHome" [size]="'14'" class="text-gray-300 mr-1"></ng-icon>
              {{ animal.shelter?.shelterName }}
            </span>
          </div>
  
          <h4 class="text-lg font-medium mb-1 bg-gradient-to-r from-primary-400 to-accent-400 bg-clip-text text-transparent flex items-center">
            <ng-icon name="lucidePawPrint" [size]="'18'" class="text-primary-400 mr-1.5"></ng-icon>
            Πως είναι ο μικρός μας φίλος;
          </h4>

          <!-- Description -->
          <p class="text-sm text-gray-300 line-clamp-3 mb-5 leading-relaxed">
            {{ animal.description || 'Δεν υπάρχει διαθέσιμη περιγραφή.' }}
          </p>
  
          <!-- Action Buttons -->
          <div class="flex justify-center items-center space-x-6 mt-auto">
            <button
              (click)="onDislike()"
              class="p-4 bg-red-500/15 hover:bg-red-500/25 rounded-full transition-all duration-300 group shadow-lg">
              <ng-icon 
                name="lucideX" 
                [size]="'24'" 
                class="text-red-400 transform transition-transform group-hover:scale-110">
              </ng-icon>
            </button>
  
            <button
              (click)="onLike()"
              class="p-4 bg-green-500/15 hover:bg-green-500/25 rounded-full transition-all duration-300 group shadow-lg">
              <ng-icon 
                name="lucideHeart" 
                [size]="'24'" 
                class="text-green-400 transform transition-transform group-hover:scale-110">
              </ng-icon>
            </button>
          </div>
        </div>
  
        <!-- Removed red and green overlay elements (swipe indicators) -->
      </div>
    </div>
  
    <!-- No More Cards -->
    <div *ngIf="!animal && !hasMore" class="absolute inset-0 flex items-center justify-center">
      <div class="text-center space-y-4">
        <div class="w-20 h-20 mx-auto bg-gradient-to-br from-primary-500/20 to-accent-500/20 rounded-full flex items-center justify-center">
          <ng-icon name="lucideHeart" [size]="'40'" class="text-primary-400"></ng-icon>
        </div>
        <p class="text-lg text-gray-400">Δεν υπάρχουν άλλα ζώα</p>
        <p class="text-sm text-gray-500">Δοκιμάστε να αλλάξετε τα κριτήρια αναζήτησης</p>
      </div>
    </div>
  
    <!-- Loading Indicator -->
    <div *ngIf="!animal && hasMore" class="absolute inset-0 flex items-center justify-center">
      <div class="w-16 h-16 border-4 border-primary-500 border-t-transparent rounded-full animate-spin"></div>
    </div>
  </div>
  
  <!-- Enhanced Details Dialog -->
  <div *ngIf="showingDetails" class="fixed inset-0 z-50 flex items-center justify-center p-4" (click)="closeDetails($event)">
    <div class="absolute inset-0 bg-black/80 backdrop-blur-sm animate-fade-in"></div>
    <div 
      class="relative bg-gradient-to-br from-gray-800 to-gray-900 rounded-3xl overflow-hidden shadow-2xl w-full max-w-2xl max-h-[80vh] overflow-y-auto animate-slide-up"
      (click)="$event.stopPropagation()">
      <!-- Dialog Content -->
      <div *ngIf="animal" class="relative">
        <!-- Image Gallery -->
        <div class="h-72 sm:h-96 relative overflow-hidden">
          <img
            [src]="currentImageUrl"
            [alt]="animal.name"
            class="w-full h-full object-cover"
            (error)="onImageError($event)" />
          <!-- Gradient Overlay -->
          <div class="absolute inset-0 bg-gradient-to-t from-gray-900 via-gray-900/50 to-transparent"></div>
          
          <!-- Type Badge -->
          <div class="absolute top-4 left-4 px-3 py-1.5 bg-primary-500/90 backdrop-blur-sm rounded-full text-white text-sm font-medium shadow-lg">
            {{ animal.animalType?.name || 'Άγνωστο' }}
          </div>
  
          <!-- Close Button -->
          <button 
            (click)="closeDetails($event)"
            class="absolute top-4 right-4 p-2.5 bg-white/15 hover:bg-white/25 backdrop-blur-sm rounded-full transition-all duration-300 shadow-lg">
            <ng-icon name="lucideX" [size]="'22'" class="text-white"></ng-icon>
          </button>
          
          <!-- Image Navigation Controls -->
          <div class="absolute inset-x-0 bottom-4 flex justify-center items-center gap-2">
            <div class="flex justify-center gap-1.5 px-2 py-1 bg-black/40 backdrop-blur-sm rounded-full" *ngIf="animal.photos && animal.photos.length > 0">
              <div *ngFor="let photo of animal.photos; let i = index"
                   [class.bg-white]="currentImageIndex === i"
                   [ngStyle]="{ 'background-color': currentImageIndex !== i ? 'rgba(255, 255, 255, 0.4)' : 'white' }"
                   class="w-2 h-2 rounded-full cursor-pointer transition-all"
                   (click)="changeImage(i, $event)">
              </div>
            </div>
          </div>
        </div>
  
        <!-- Enhanced Info Content -->
        <div class="p-6 space-y-6">
          <!-- Header Section -->
          <div class="flex items-start justify-between">
            <div>
              <div class="flex items-center space-x-2 mb-1">
                <h2 class="text-2xl font-bold text-white">{{ animal.name }}</h2>
                <div 
                  class="w-7 h-7 rounded-full flex items-center justify-center shadow-md" 
                  [ngClass]="{'bg-blue-500/25': animal.gender === 1, 'bg-pink-500/25': animal.gender === 2}">
                  <ng-icon 
                    [name]="animal.gender === 1 ? 'lucideMars' : 'lucideVenus'" 
                    [size]="'16'" 
                    [class]="animal.gender === 1 ? 'text-blue-400' : 'text-pink-400'">
                  </ng-icon>
                </div>
              </div>
              <div class="flex flex-wrap items-center gap-x-2 gap-y-1">
                <span class="text-lg text-primary-400">{{ animal.age }} ετών</span>
                <span *ngIf="animal.weight" class="text-gray-400">•</span>
                <span *ngIf="animal.weight" class="text-gray-400">{{ animal.weight }} kg</span>
                <span *ngIf="animal.adoptionStatus" class="text-gray-400">•</span>
                <span *ngIf="animal.adoptionStatus" 
                      [ngClass]="{
                        'text-green-400': animal.adoptionStatus === 1,
                        'text-yellow-400': animal.adoptionStatus === 2,
                        'text-red-400': animal.adoptionStatus === 3
                      }">
                  {{ getAdoptionStatusLabel(animal.adoptionStatus) }}
                </span>
              </div>
            </div>
            
            <div class="flex space-x-2">
              <button
                (click)="onDislike(); closeDetails($event)"
                class="p-3 bg-red-500/15 hover:bg-red-500/25 rounded-full transition-all duration-300 shadow-lg">
                <ng-icon name="lucideX" [size]="'18'" class="text-red-400"></ng-icon>
              </button>
              <button
                (click)="onLike(); closeDetails($event)"
                class="p-3 bg-green-500/15 hover:bg-green-500/25 rounded-full transition-all duration-300 shadow-lg">
                <ng-icon name="lucideHeart" [size]="'18'" class="text-green-400"></ng-icon>
              </button>
            </div>
          </div>
  
          <!-- Details Cards Grid -->
          <div class="grid grid-cols-2 gap-4">
            <div class="bg-white/5 hover:bg-white/8 rounded-2xl p-4 transition-colors shadow-sm">
              <div class="text-sm text-gray-400 mb-1">Είδος</div>
              <div class="text-white font-medium">{{ animal.animalType?.name || 'Μη διαθέσιμο' }}</div>
            </div>
            <div class="bg-white/5 hover:bg-white/8 rounded-2xl p-4 transition-colors shadow-sm">
              <div class="text-sm text-gray-400 mb-1">Φυλή</div>
              <div class="text-white font-medium">{{ animal.breed?.name || 'Μη διαθέσιμο' }}</div>
            </div>
            <div class="bg-white/5 hover:bg-white/8 rounded-2xl p-4 transition-colors shadow-sm">
              <div class="text-sm text-gray-400 mb-1">Φύλο</div>
              <div class="text-white font-medium">
                {{ animal.gender === 1 ? 'Αρσενικό' : animal.gender === 2 ? 'Θηλυκό' : 'Μη διαθέσιμο' }}
              </div>
            </div>
            <div class="bg-white/5 hover:bg-white/8 rounded-2xl p-4 transition-colors shadow-sm">
              <div class="text-sm text-gray-400 mb-1">Ηλικία</div>
              <div class="text-white font-medium">
                {{ animal.age || 'Μη διαθέσιμη' }} ετών
              </div>
            </div>
            <div class="bg-white/5 hover:bg-white/8 rounded-2xl p-4 transition-colors shadow-sm">
              <div class="text-sm text-gray-400 mb-1">Βάρος</div>
              <div class="text-white font-medium">
                {{ animal.weight ? (animal.weight + ' kg') : 'Μη διαθέσιμο' }}
              </div>
            </div>
            <div class="bg-white/5 hover:bg-white/8 rounded-2xl p-4 transition-colors shadow-sm">
              <div class="text-sm text-gray-400 mb-1">Κατάσταση υγείας</div>
              <div class="text-white font-medium">
                {{ animal.healthStatus || 'Μη διαθέσιμη' }}
              </div>
            </div>
            <div *ngIf="animal.shelter" class="bg-white/5 hover:bg-white/8 rounded-2xl p-4 col-span-2 transition-colors shadow-sm">
              <div class="text-sm text-gray-400 mb-1">Καταφύγιο</div>
              <div class="text-white font-medium">
                {{ animal.shelter.shelterName || 'Μη διαθέσιμο' }}
              </div>
            </div>
            <div *ngIf="animal.createdAt" class="bg-white/5 hover:bg-white/8 rounded-2xl p-4 col-span-2 transition-colors shadow-sm">
              <div class="text-sm text-gray-400 mb-1">Καταχωρήθηκε</div>
              <div class="text-white font-medium">
                {{ animal.createdAt | date:'dd/MM/yyyy' }}
              </div>
            </div>
          </div>
  
          <!-- Description Section with Enhanced Styling -->
          <div class="bg-white/5 hover:bg-white/8 rounded-2xl p-4 transition-colors shadow-sm">
            <h3 class="text-lg font-medium text-white mb-2">Περιγραφή</h3>
            <p class="text-gray-300 leading-relaxed">
              {{ animal.description || 'Δεν υπάρχει διαθέσιμη περιγραφή.' }}
            </p>
          </div>
  
          <!-- More Photos if available -->
          <div *ngIf="animal.photos && animal.photos.length > 1" class="space-y-3">
            <h3 class="text-lg font-medium text-white">Περισσότερες φωτογραφίες</h3>
            <div class="grid grid-cols-3 gap-2">
              <div *ngFor="let photo of animal.photos; let i = index"
                   class="aspect-square rounded-xl overflow-hidden cursor-pointer relative group"
                   [class.ring-2]="currentImageIndex === i"
                   [class.ring-primary-500]="currentImageIndex === i"
                   (click)="changeImage(i, $event)">
                <img [src]="photo" [alt]="'Photo ' + (i + 1)" class="w-full h-full object-cover" />
                <div class="absolute inset-0 bg-black/30 opacity-0 group-hover:opacity-100 transition-opacity"></div>
              </div>
            </div>
          </div>
        </div> 
      </div> 
    </div> 
  </div> 
  `,
  styles: [`
    :host {
      display: block;
      touch-action: none;
    }

    .line-clamp-3 {
      display: -webkit-box;
      -webkit-line-clamp: 3;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }
    
    .animate-fade-in {
      animation: fadeIn 0.3s ease-out forwards;
    }
    
    .animate-slide-up {
      animation: slideUp 0.4s cubic-bezier(0.16, 1, 0.3, 1) forwards;
    }
    
    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }
    
    @keyframes slideUp {
      from { opacity: 0; transform: translateY(30px); }
      to { opacity: 1; transform: translateY(0); }
    }
  `],
  animations: [
    trigger('cardState', [
      state('default', style({
        transform: 'none'
      })),
      state('like', style({
        transform: 'translate(150%, -30px) rotate(30deg)',
        opacity: 0
      })),
      state('nope', style({
        transform: 'translate(-150%, -30px) rotate(-30deg)',
        opacity: 0
      })),
      transition('default => like', animate('400ms cubic-bezier(0.4, 0, 0.2, 1)')),
      transition('default => nope', animate('400ms cubic-bezier(0.4, 0, 0.2, 1)')),
      transition('* => default', animate('400ms cubic-bezier(0.4, 0, 0.2, 1)'))
    ])
  ]
})
export class SwipeCardComponent implements AfterViewInit, OnChanges {
 // Properties remain the same
 @Input() key: string | null = null;
 @Input() animal: Animal | undefined;
 @Input() hasMore = true;
 @Output() swipeLeft = new EventEmitter<void>();
 @Output() swipeRight = new EventEmitter<Animal>();

 @ViewChild('card') cardElement!: ElementRef;
 @ViewChild('container') containerElement!: ElementRef;

 private startX = 0;
 private startY = 0;
 deltaX = 0;
 deltaY = 0;
 private isDragging = false;
 cardState = 'default';
 showingDetails = false;
 currentImageIndex = 0;
 currentImageUrl = '';

 ngAfterViewInit() {
   if (this.cardElement) {
     this.setupTouchEvents();
     this.updateCurrentImageUrl(); // Runs on each creation
   }

   console.log('SwipeCardComponent initialized with key:', this.key);
 }

 ngOnChanges(changes: SimpleChanges) {
   if (changes['animal'] && changes['animal'].currentValue !== changes['animal'].previousValue) {
     this.currentImageIndex = 0; 
     this.currentImageUrl = '';
     this.updateCurrentImageUrl(); 
   }
 }

  private setupTouchEvents() {
    const element = this.cardElement.nativeElement;

    element.addEventListener('mousedown', this.onStart.bind(this));
    element.addEventListener('touchstart', this.onStart.bind(this));
    document.addEventListener('mousemove', this.onMove.bind(this));
    document.addEventListener('touchmove', this.onMove.bind(this));
    document.addEventListener('mouseup', this.onEnd.bind(this));
    document.addEventListener('touchend', this.onEnd.bind(this));
  }

  private onStart(event: MouseEvent | TouchEvent) {
    if (!this.animal) return;

    this.isDragging = true;
    const point = this.getPoint(event);
    this.startX = point.x - this.deltaX;
    this.startY = point.y - this.deltaY;
  }

  private onMove(event: MouseEvent | TouchEvent) {
    if (!this.isDragging) return;

    const point = this.getPoint(event);
    // Calculate raw movement
    const rawDeltaX = point.x - this.startX;
    const rawDeltaY = point.y - this.startY;
    
    // Restrict to horizontal movement only
    this.deltaX = rawDeltaX;
    this.deltaY = 0; // Remove vertical movement completely
  }

  private onEnd() {
    if (!this.isDragging) return;

    this.isDragging = false;

    // Reduced threshold to 50px for easier swiping
    if (this.deltaX > 50) {
      this.onLike();
    } else if (this.deltaX < -50) {
      this.onDislike();
    } else {
      this.resetPosition();
    }
  }

  onLike() {
    if (!this.animal) return;
    this.cardState = 'like';
    setTimeout(() => {
      this.swipeRight.emit(this.animal);
      this.resetPosition();
    }, 300);
  }

  onDislike() {
    if (!this.animal) return;
    this.cardState = 'nope';
    setTimeout(() => {
      this.swipeLeft.emit();
      this.resetPosition();
    }, 300);
  }

  private resetPosition() {
    this.deltaX = 0;
    this.deltaY = 0;
    this.cardState = 'default';
  }

  private getPoint(event: MouseEvent | TouchEvent) {
    if (event instanceof MouseEvent) {
      return { x: event.clientX, y: event.clientY };
    } else {
      return {
        x: event.touches[0].clientX,
        y: event.touches[0].clientY
      };
    }
  }

  getSwipeIndicatorClass(deltaX: number, threshold: number): string {
    return deltaX > threshold 
      ? 'opacity-100 scale-100' 
      : 'opacity-0 scale-90';
  }
  

  getTransform(): string {
    if (!this.isDragging) return '';
    const rotate = this.deltaX * 0.1;
    return `translate(${this.deltaX}px, ${this.deltaY}px) rotate(${rotate}deg)`;
  }

  getOpacity(): number {
    return Math.max(1 - Math.abs(this.deltaX) / 400, 0);
  }

  getCardState(): string {
    return this.cardState;
  }
  
  showDetails(event: Event) {
    event.stopPropagation();
    this.showingDetails = true;
  }
  
  closeDetails(event: Event) {
    event.stopPropagation();
    this.showingDetails = false;
  }

  changeImage(index: number, event: Event) {
    event.stopPropagation();
    this.currentImageIndex = index;
    this.updateCurrentImageUrl();
  }

  private updateCurrentImageUrl() {
    if (this.animal && this.animal.photos && this.animal.photos.length > 0) {
      this.currentImageUrl = this.animal.photos[this.currentImageIndex];
    } else {
      this.currentImageUrl = 'assets/placeholder.jpg';
    }
  }

  // Helper method to get adoption status label
  getAdoptionStatusLabel(status: number): string {
    switch(status) {
      case 1: return 'Διαθέσιμο';
      case 2: return 'Σε αναμονή';
      case 3: return 'Υιοθετημένο';
      default: return 'Άγνωστο';
    }
  }

  // Enhanced image error handling
  onImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    
    if (this.animal && this.animal.photos && this.animal.photos.length > 0) {
      // If on the last image, use placeholder
      if (this.currentImageIndex === this.animal.photos.length - 1) {
        img.src = 'assets/placeholder.jpg';
        return;
      }
      
      // Try next image
      this.currentImageIndex = (this.currentImageIndex + 1) % this.animal.photos.length;
      this.updateCurrentImageUrl();
    } else {
      // Fallback to placeholder if no photos
      img.src = 'assets/placeholder.jpg';
    }
  }
  
  private tryNextImage(): number {
    if (!this.animal || !this.animal.photos || this.animal.photos.length === 0) {
      return -1;
    }
    
    // Start from current index + 1
    const startIdx = this.currentImageIndex;
    let nextIdx = (startIdx + 1) % this.animal.photos.length;
    
    // Try each image in the array until we've tried them all
    while (nextIdx !== startIdx) {
      this.currentImageIndex = nextIdx;
      this.updateCurrentImageUrl();
      
      // If we've gone through all images, return -1
      if (nextIdx === startIdx) {
        return -1;
      }
      
      nextIdx = (nextIdx + 1) % this.animal.photos.length;
    }
    
    return nextIdx;
  }
}
