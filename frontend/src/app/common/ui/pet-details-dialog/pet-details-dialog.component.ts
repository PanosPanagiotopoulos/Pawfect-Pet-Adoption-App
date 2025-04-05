import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, ChangeDetectorRef, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgIconsModule } from '@ng-icons/core';
import { Animal } from 'src/app/models/animal/animal.model';
import { UtilsService } from '../../services/utils.service';
import { animate, style, transition, trigger } from '@angular/animations';

@Component({
  selector: 'app-pet-details-dialog',
  templateUrl: './pet-details-dialog.component.html',
  styleUrls: ['./pet-details-dialog.component.scss'],
  standalone: true,
  imports: [CommonModule, NgIconsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  animations: [
    trigger('dialogAnimation', [
      transition(':enter', [
        style({ 
          opacity: 0,
          transform: 'scale(0.95) translateY(30px)'
        }),
        animate('400ms cubic-bezier(0.34, 1.56, 0.64, 1)', style({ 
          opacity: 1,
          transform: 'scale(1) translateY(0)'
        }))
      ]),
      transition(':leave', [
        animate('300ms cubic-bezier(0.4, 0, 0.2, 1)', style({ 
          opacity: 0,
          transform: 'scale(0.95) translateY(30px)'
        }))
      ])
    ]),
    trigger('backdropAnimation', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('200ms ease-out', style({ opacity: 1 }))
      ]),
      transition(':leave', [
        animate('150ms ease-in', style({ opacity: 0 }))
      ])
    ])
  ]
})
export class PetDetailsDialogComponent {
  @Input() animal!: Animal;
  @Input() isOpen = false;
  @Output() closeDialog = new EventEmitter<void>();

  currentImage: string = '';
  currentImageIndex: number = 0;

  constructor(
    private utilsService: UtilsService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnChanges(changes: SimpleChanges) {
    if (changes['isOpen']) {
      if (this.isOpen && this.animal) {
        // Dialog is opening - initialize
        this.currentImageIndex = 0;
        this.loadImage();
      } else if (!this.isOpen) {
        // Dialog is closing - reset all fields
        this.resetFields();
      }
    } else if (changes['animal'] && this.isOpen && this.animal) {
      // Animal changed while dialog is open
      this.currentImageIndex = 0;
      this.loadImage();
    }
  }

  async loadImage() {
    this.currentImage = await this.utilsService.tryLoadImages(this.animal);
    this.cdr.markForCheck();
  }

  onImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    img.src = '/assets/placeholder.jpg';
  }

  onClose() {
    this.closeDialog.emit();
  }

  getAdoptionStatusLabel(status: number): string {
    switch (status) {
      case 1: return 'Διαθέσιμο';
      case 2: return 'Σε αναμονή';
      case 3: return 'Υιοθετημένο';
      default: return 'Άγνωστο';
    }
  }

  changeImage(index: number) {
    this.currentImageIndex = index;
    if (this.animal?.photos?.length) {
      this.currentImage = this.animal.photos[index];
    } else {
      this.currentImage = 'assets/placeholder.jpg';
    }
  }

  private resetFields(): void {
    this.currentImage = '';
    this.currentImageIndex = 0;
    this.cdr.markForCheck();
  }

}
