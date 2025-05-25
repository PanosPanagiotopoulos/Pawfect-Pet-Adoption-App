import { Component, Input, Output, EventEmitter, ElementRef, ViewChild, AfterViewInit, OnChanges, SimpleChanges, ChangeDetectorRef } from '@angular/core';
import { Animal } from 'src/app/models/animal/animal.model';
import { trigger, state, style, transition, animate } from '@angular/animations';
import { UtilsService } from 'src/app/common/services/utils.service';

@Component({
  selector: 'app-swipe-card',
  templateUrl: './swipe-card.component.html',
  styles: [`
    :host {
      display: block;
      width: 100%;
      height: 100%;
    }
    
    .action-button {
      @apply flex items-center justify-center;
      svg {
        @apply stroke-[2.5px] stroke-current;
      }
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
      transition('default => like', [
        animate('400ms cubic-bezier(0.4, 0, 0.2, 1)', style({
          transform: 'translate(150%, -30px) rotate(30deg)',
          opacity: 0
        }))
      ]),
      transition('default => nope', [
        animate('400ms cubic-bezier(0.4, 0, 0.2, 1)', style({
          transform: 'translate(-150%, -30px) rotate(-30deg)',
          opacity: 0
        }))
      ]),
      transition('* => default', [
        animate('400ms cubic-bezier(0.4, 0, 0.2, 1)', style({
          transform: 'none',
          opacity: 1
        }))
      ])
    ]),
    trigger('fadeInOut', [
      state('in', style({ opacity: 1 })),
      transition(':enter', [
        style({ opacity: 0 }),
        animate('300ms ease-out')
      ]),
      transition(':leave', [
        animate('300ms ease-in', style({ opacity: 0 }))
      ])
    ])
  ]
})
export class SwipeCardComponent implements AfterViewInit, OnChanges {
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
  isDialogOpen = false;
  isLoading = false;

  currentImageIndex = 0;
  currentImageUrl = '';
  
  private readonly SWIPE_THRESHOLD = 30;
  private readonly MAX_ROTATION = 15;
  private readonly SWIPE_VELOCITY_THRESHOLD = 0.5;
  private lastMoveTime = 0;
  private lastMoveX = 0;
  private velocity = 0;

  constructor(
    private utilsService: UtilsService,
    private cdr: ChangeDetectorRef
  ) {}

  ngAfterViewInit() {
    if (this.cardElement) {
      this.setupTouchEvents();
      this.updateCurrentImageUrl();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['animal'] && changes['animal'].currentValue !== changes['animal'].previousValue) {
      this.currentImageIndex = 0;
      this.currentImageUrl = '';
      this.isLoading = true;
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
    this.lastMoveTime = Date.now();
    this.lastMoveX = point.x;
    this.velocity = 0;
  }

  private onMove(event: MouseEvent | TouchEvent) {
    if (!this.isDragging) return;

    const point = this.getPoint(event);
    const currentTime = Date.now();
    const timeDiff = currentTime - this.lastMoveTime;
    
    if (timeDiff > 0) {
      this.velocity = (point.x - this.lastMoveX) / timeDiff;
    }
    
    this.lastMoveX = point.x;
    this.lastMoveTime = currentTime;

    const rawDeltaX = point.x - this.startX;
    this.deltaX = Math.sign(rawDeltaX) * Math.min(Math.abs(rawDeltaX), 150);
    this.deltaY = 0;
  }

  private onEnd() {
    if (!this.isDragging) return;
    this.isDragging = false;

    const absVelocity = Math.abs(this.velocity);
    const direction = Math.sign(this.velocity);

    if (Math.abs(this.deltaX) > this.SWIPE_THRESHOLD || absVelocity > this.SWIPE_VELOCITY_THRESHOLD) {
      if (this.deltaX > 0 || (direction > 0 && absVelocity > this.SWIPE_VELOCITY_THRESHOLD)) {
        this.onLike();
      } else {
        this.onDislike();
      }
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
    this.velocity = 0;
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

  getTransform(): string {
    if (!this.isDragging && this.cardState === 'default') return '';
    
    const rotate = (this.deltaX / 100) * this.MAX_ROTATION;
    const clampedRotation = Math.max(Math.min(rotate, this.MAX_ROTATION), -this.MAX_ROTATION);
    
    return `translate(${this.deltaX}px, ${this.deltaY}px) rotate(${clampedRotation}deg)`;
  }

  getOpacity(): number {
    return Math.max(1 - Math.abs(this.deltaX) / 400, 0);
  }

  getCardState(): string {
    return this.cardState;
  }

  openDialog(): void {
    this.isDialogOpen = true;
  }

  closeDialog(): void {
    this.isDialogOpen = false;
  }

  onImageError(event: Event) {
    const img = event.target as HTMLImageElement;
    img.src = '/assets/placeholder.jpg';
  }
  
  private updateCurrentImageUrl() {
    this.isLoading = true;
    if (this.animal?.attachedPhotos?.length) {
      this.loadImage().finally(() => {
        this.isLoading = false; 
        this.cdr.markForCheck();
      });
    } else {
      this.currentImageUrl = 'assets/placeholder.jpg';
      this.isLoading = false;
      this.cdr.markForCheck();
    }
  }

  async loadImage() {
    if (this.animal) {
      this.currentImageUrl = await this.utilsService.tryLoadImages(this.animal);
      this.cdr.markForCheck();
    }
  }

  changeImage(index: number, event: Event) {
    event.stopPropagation();
    this.currentImageIndex = index;
    this.updateCurrentImageUrl();
  }

  getAdoptionStatusLabel(status: number): string {
    switch(status) {
      case 1: return 'Διαθέσιμο';
      case 2: return 'Σε αναμονή';
      case 3: return 'Υιοθετημένο';
      default: return 'Άγνωστο';
    }
  }
}