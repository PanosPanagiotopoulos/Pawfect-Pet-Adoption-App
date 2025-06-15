import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgIconsModule } from '@ng-icons/core';
import { animate, style, transition, trigger } from '@angular/animations';

@Component({
  selector: 'app-instructions-modal',
  standalone: true,
  imports: [CommonModule, NgIconsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div *ngIf="isOpen" class="fixed inset-0 z-[9999] flex items-center justify-center p-4 top-20" (click)="onClose()">
      <!-- Backdrop with blur animation -->
      <div 
        class="absolute inset-0 bg-white/5 backdrop-blur-md"
        [@backdropAnimation]>
      </div>

      <!-- Dialog Content with animation and scroll -->
      <div 
        [@dialogAnimation]
        class="relative bg-gradient-to-br from-gray-800 to-gray-900 rounded-3xl overflow-y-auto max-h-[85vh] shadow-2xl w-full max-w-2xl transform-gpu"
        (click)="$event.stopPropagation()">
        
        <!-- Close Button -->
        <button 
          (click)="onClose()"
          class="absolute top-4 right-4 p-2.5 bg-red-600 hover:bg-red-700 rounded-full transition-all duration-300 group shadow-lg flex items-center justify-center hover:scale-110">
          <ng-icon 
            name="lucideX" 
            [size]="'24'" 
            class="text-white transform transition-transform group-hover:scale-110">
          </ng-icon>
        </button>

        <!-- Content -->
        <div class="p-6 sm:p-8">
          <h2 class="text-2xl sm:text-3xl font-bold text-white mb-4 sm:mb-6 bg-gradient-to-r from-primary-400 to-accent-400 bg-clip-text text-transparent">Καλώς ήρθατε!</h2>
          
          <div class="space-y-3 sm:space-y-4">
            <!-- Search Instructions -->
            <div class="flex items-start space-x-3 group hover:bg-gray-800/50 p-3 rounded-xl transition-all duration-300">
              <div class="p-2 bg-primary-500/20 rounded-lg group-hover:bg-primary-500/30 transition-all duration-300">
                <ng-icon name="lucideSearch" [size]="'20'" class="text-primary-400 sm:scale-110"></ng-icon>
              </div>
              <div>
                <h3 class="text-lg sm:text-xl font-semibold text-white mb-1 group-hover:text-primary-400 transition-colors duration-300">Αναζήτηση</h3>
                <p class="text-sm sm:text-base text-gray-300 group-hover:text-gray-200 transition-colors duration-300">Περιγράψτε το ιδανικό σας κατοικίδιο στο πεδίο αναζήτησης.</p>
              </div>
            </div>

            <!-- Swipe Instructions -->
            <div class="flex items-start space-x-3 group hover:bg-gray-800/50 p-3 rounded-xl transition-all duration-300">
              <div class="p-2 bg-secondary-500/20 rounded-lg group-hover:bg-secondary-500/30 transition-all duration-300">
                <ng-icon name="lucidePawPrint" [size]="'20'" class="text-secondary-400 sm:scale-110"></ng-icon>
              </div>
              <div>
                <h3 class="text-lg sm:text-xl font-semibold text-white mb-1 group-hover:text-secondary-400 transition-colors duration-300">Σαρώστε</h3>
                <p class="text-sm sm:text-base text-gray-300 group-hover:text-gray-200 transition-colors duration-300">Σαρώστε δεξιά για να αποθηκεύσετε τα αγαπημένα σας ή αριστερά για να δείτε το επόμενο.</p>
              </div>
            </div>

            <!-- Saved Animals Instructions -->
            <div class="flex items-start space-x-3 group hover:bg-gray-800/50 p-3 rounded-xl transition-all duration-300">
              <div class="p-2 bg-accent-500/20 rounded-lg group-hover:bg-accent-500/30 transition-all duration-300">
                <ng-icon name="lucideHeart" [size]="'20'" class="text-accent-400 sm:scale-110"></ng-icon>
              </div>
              <div>
                <h3 class="text-lg sm:text-xl font-semibold text-white mb-1 group-hover:text-accent-400 transition-colors duration-300">Αποθηκευμένα Κατοικίδια</h3>
                <p class="text-sm sm:text-base text-gray-300 group-hover:text-gray-200 transition-colors duration-300">Τα αποθηκευμένα κατοικίδια εμφανίζονται στο κάτω μέρος της σελίδας. Μπορείτε να τα δείτε όλα μαζί.</p>
              </div>
            </div>

            <!-- Adoption Instructions -->
            <div class="flex items-start space-x-3 group hover:bg-gray-800/50 p-3 rounded-xl transition-all duration-300">
              <div class="p-2 bg-green-500/20 rounded-lg group-hover:bg-green-500/30 transition-all duration-300">
                <ng-icon name="lucideHouse" [size]="'20'" class="text-green-400 sm:scale-110"></ng-icon>
              </div>
              <div>
                <h3 class="text-lg sm:text-xl font-semibold text-white mb-1 group-hover:text-green-400 transition-colors duration-300">Υιοθέτηση</h3>
                <p class="text-sm sm:text-base text-gray-300 group-hover:text-gray-200 transition-colors duration-300">Πατήστε το κουμπί "Υιοθέτησε με τώρα!" για να ξεκινήσετε τη διαδικασία υιοθέτησης του κατοικιδίου σας.</p>
              </div>
            </div>

            <!-- Visual Indicators -->
            <div class="flex justify-center space-x-8 sm:space-x-12 mt-6 sm:mt-8">
              <div class="flex flex-col items-center group">
                <div class="w-12 h-12 sm:w-16 sm:h-16 bg-red-500/20 rounded-full flex items-center justify-center mb-2 group-hover:bg-red-500/30 transition-all duration-300 transform group-hover:scale-110">
                  <ng-icon name="lucideX" [size]="'24'" class="text-red-400 sm:scale-110"></ng-icon>
                </div>
                <span class="text-xs sm:text-sm text-gray-400 group-hover:text-gray-300 transition-colors duration-300">Παράλειψη</span>
              </div>
              <div class="flex flex-col items-center group">
                <div class="w-12 h-12 sm:w-16 sm:h-16 bg-green-500/20 rounded-full flex items-center justify-center mb-2 group-hover:bg-green-500/30 transition-all duration-300 transform group-hover:scale-110">
                  <ng-icon name="lucideHeart" [size]="'24'" class="text-green-400 sm:scale-110"></ng-icon>
                </div>
                <span class="text-xs sm:text-sm text-gray-400 group-hover:text-gray-300 transition-colors duration-300">Αποθήκευση</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  animations: [
    trigger('dialogAnimation', [
      transition(':enter', [
        style({ 
          opacity: 0,
          transform: 'translateY(-20px) translateZ(-100px) scale(0.95)'
        }),
        animate('400ms cubic-bezier(0.34, 1.56, 0.64, 1)', style({ 
          opacity: 1,
          transform: 'translateY(0) translateZ(0px) scale(1)'
        }))
      ]),
      transition(':leave', [
        animate('300ms cubic-bezier(0.4, 0, 0.2, 1)', style({ 
          opacity: 0,
          transform: 'translateY(-20px) translateZ(-100px) scale(0.95)'
        }))
      ])
    ]),
    trigger('backdropAnimation', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms cubic-bezier(0.4, 0, 0.2, 1)', style({ opacity: 1 }))
      ]),
      transition(':leave', [
        animate('200ms cubic-bezier(0.4, 0, 0.2, 1)', style({ opacity: 0 }))
      ])
    ])
  ]
})
export class InstructionsModalComponent implements OnChanges {
  @Input() isOpen = false;
  @Output() close = new EventEmitter<void>();

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['isOpen']) {
      if (this.isOpen) {
        document.body.style.overflow = 'hidden';
      } else {
        document.body.style.overflow = '';
      }
    }
  }

  onClose(): void {
    this.close.emit();
  }
}