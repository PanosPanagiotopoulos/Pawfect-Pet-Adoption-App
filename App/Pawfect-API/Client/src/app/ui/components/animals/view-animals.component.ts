import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { trigger, transition, style, animate } from '@angular/animations';

import { AnimalService } from 'src/app/services/animal.service';
import { TranslationService } from 'src/app/common/services/translation.service';
import { LogService } from 'src/app/common/services/log.service';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { SnackbarService } from 'src/app/common/services/snackbar.service';

import { Animal, AdoptionStatus } from 'src/app/models/animal/animal.model';
import { AnimalType } from 'src/app/models/animal-type/animal-type.model';
import { Breed } from 'src/app/models/breed/breed.model';
import { Shelter, OperatingHours } from 'src/app/models/shelter/shelter.model';
import { File } from 'src/app/models/file/file.model';
import { User } from 'src/app/models/user/user.model';
import { Gender } from 'src/app/common/enum/gender';
import { nameof } from 'ts-simple-nameof';

@Component({
  selector: 'app-view-animals',
  templateUrl: './view-animals.component.html',
  styleUrls: ['./view-animals.component.scss'],
  animations: [
    trigger('fadeInLeft', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(-50px)' }),
        animate(
          '0.6s ease-out',
          style({ opacity: 1, transform: 'translateX(0)' })
        ),
      ]),
    ]),
    trigger('fadeInRight', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(50px)' }),
        animate(
          '0.6s ease-out',
          style({ opacity: 1, transform: 'translateX(0)' })
        ),
      ]),
    ]),
    trigger('slideDown', [
      transition(':enter', [
        style({ height: 0, opacity: 0 }),
        animate('0.3s ease-out', style({ height: '*', opacity: 1 })),
      ]),
      transition(':leave', [
        animate('0.3s ease-in', style({ height: 0, opacity: 0 })),
      ]),
    ]),
  ],
})
export class ViewAnimalsComponent implements OnInit, OnDestroy {
  AdoptionStatus = AdoptionStatus;

  animal: Animal | null = null;
  animalId: string = '';
  isLoading = true;
  error: string | null = null;

  // Image preview slider state
  currentImageIndex = 0;
  slideDirection: 'left' | 'right' | 'none' = 'none';
  isShelterInfoOpen = true;

  private subscriptions: Subscription[] = [];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private animalService: AnimalService,
    private translationService: TranslationService,
    private log: LogService,
    private errorHandler: ErrorHandlerService,
    private snackbarService: SnackbarService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.animalId = params['animalId'];

      if (this.animalId) {
        this.loadAnimal();
      } else {
        this.router.navigate(['/404']);
      }
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((sub) => sub.unsubscribe());
  }

  loadAnimal(): void {
    this.isLoading = true;
    this.error = null;

    const fields = [
      nameof<Animal>((x) => x.id),
      nameof<Animal>((x) => x.name),
      nameof<Animal>((x) => x.age),
      nameof<Animal>((x) => x.gender),
      nameof<Animal>((x) => x.description),
      nameof<Animal>((x) => x.weight),
      nameof<Animal>((x) => x.healthStatus),
      nameof<Animal>((x) => x.adoptionStatus),
      nameof<Animal>((x) => x.createdAt),
      nameof<Animal>((x) => x.updatedAt),
      [
        nameof<Animal>((x) => x.animalType),
        nameof<AnimalType>((x) => x.id),
      ].join('.'),
      [
        nameof<Animal>((x) => x.animalType),
        nameof<AnimalType>((x) => x.name),
      ].join('.'),
      [
        nameof<Animal>((x) => x.animalType),
        nameof<AnimalType>((x) => x.description),
      ].join('.'),
      [nameof<Animal>((x) => x.breed), nameof<Breed>((x) => x.id)].join('.'),
      [nameof<Animal>((x) => x.breed), nameof<Breed>((x) => x.name)].join('.'),
      [
        nameof<Animal>((x) => x.breed),
        nameof<Breed>((x) => x.description),
      ].join('.'),
      [nameof<Animal>((x) => x.shelter), nameof<Shelter>((x) => x.id)].join(
        '.'
      ),
      [
        nameof<Animal>((x) => x.shelter),
        nameof<Shelter>((x) => x.shelterName),
      ].join('.'),
      [
        nameof<Animal>((x) => x.shelter),
        nameof<Shelter>((x) => x.description),
      ].join('.'),
      [
        nameof<Animal>((x) => x.shelter),
        nameof<Shelter>((x) => x.website),
      ].join('.'),
      [
        nameof<Animal>((x) => x.shelter),
        nameof<Shelter>((x) => x.socialMedia),
      ].join('.'),
      [
        nameof<Animal>((x) => x.shelter),
        nameof<Shelter>((x) => x.operatingHours),
      ].join('.'),
      [
        nameof<Animal>((x) => x.shelter),
        nameof<Shelter>((x) => x.user),
        nameof<User>((x) => x.id),
      ].join('.'),
      [
        nameof<Animal>((x) => x.shelter),
        nameof<Shelter>((x) => x.user),
        nameof<User>((x) => x.location),
      ].join('.'),
      [
        nameof<Animal>((x) => x.shelter),
        nameof<Shelter>((x) => x.user),
        nameof<User>((x) => x.profilePhoto),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),
      [nameof<Animal>((x) => x.attachedPhotos), nameof<File>((x) => x.id)].join(
        '.'
      ),
      [
        nameof<Animal>((x) => x.attachedPhotos),
        nameof<File>((x) => x.sourceUrl),
      ].join('.'),
      [
        nameof<Animal>((x) => x.attachedPhotos),
        nameof<File>((x) => x.filename),
      ].join('.'),
      [
        nameof<Animal>((x) => x.attachedPhotos),
        nameof<File>((x) => x.size),
      ].join('.'),
      [
        nameof<Animal>((x) => x.attachedPhotos),
        nameof<File>((x) => x.mimeType),
      ].join('.'),
    ];

    const animalSub = this.animalService
      .getSingle(this.animalId, fields)
      .subscribe({
        next: (animal) => {
          this.animal = animal;
          this.isLoading = false;
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.isLoading = false;
          this.error = 'APP.ANIMALS.VIEW.LOAD_ERROR';
          this.errorHandler.handleError(err);
          this.log.logFormatted({
            message: 'Failed to load animal',
            error: err,
            data: { animalId: this.animalId },
          });
          this.cdr.markForCheck();
        },
      });

    this.subscriptions.push(animalSub);
  }

  // Image preview slider methods
  getImageFiles(): File[] {
    return this.animal?.attachedPhotos || [];
  }

  getCurrentImageIndex(): number {
    return this.currentImageIndex || 0;
  }

  getCurrentImageUrl(): string {
    return this.getImageUrlByIndex(this.getCurrentImageIndex());
  }

  getImageUrlByIndex(index: number): string {
    const files = this.getImageFiles();
    const file = files[index];
    return file?.sourceUrl || '/assets/placeholder.jpg';
  }

  nextImage(totalImages: number): void {
    if (totalImages <= 0) return;
    const current = this.getCurrentImageIndex();
    this.slideDirection = 'left';
    this.currentImageIndex = (current + 1) % totalImages;
    this.cdr.markForCheck();

    setTimeout(() => {
      this.slideDirection = 'none';
      this.cdr.markForCheck();
    }, 300);
  }

  previousImage(totalImages: number): void {
    if (totalImages <= 0) return;
    const current = this.getCurrentImageIndex();
    this.slideDirection = 'right';
    this.currentImageIndex = current === 0 ? totalImages - 1 : current - 1;
    this.cdr.markForCheck();

    setTimeout(() => {
      this.slideDirection = 'none';
      this.cdr.markForCheck();
    }, 300);
  }

  goToImage(index: number): void {
    if (index < 0) return;
    const current = this.getCurrentImageIndex();

    if (index > current) {
      this.slideDirection = 'left';
    } else if (index < current) {
      this.slideDirection = 'right';
    } else {
      this.slideDirection = 'none';
    }

    this.currentImageIndex = index;
    this.cdr.markForCheck();

    setTimeout(() => {
      this.slideDirection = 'none';
      this.cdr.markForCheck();
    }, 300);
  }

  getSlideDirection(): string {
    return this.slideDirection || 'none';
  }

  onImageError(event: Event) {
    const target = event.target as HTMLImageElement | null;
    if (target) {
      target.src = '/assets/placeholder.jpg';
    }
  }

  getShelterProfilePicture(): string {
    return (
      this.animal?.shelter?.user?.profilePhoto?.sourceUrl ||
      'assets/placeholder.jpg'
    );
  }

  onShelterImageError(event: Event) {
    const target = event.target as HTMLImageElement | null;
    if (target) {
      target.src = 'assets/placeholder.jpg';
    }
  }

  toggleShelterInfo(): void {
    this.isShelterInfoOpen = !this.isShelterInfoOpen;
  }

  getAdoptionStatusTranslationKey(status: AdoptionStatus | undefined): string {
    switch (status) {
      case AdoptionStatus.Available:
        return 'APP.PROFILE-PAGE.ADOPTION_STATUS.AVAILABLE';
      case AdoptionStatus.Adopted:
        return 'APP.PROFILE-PAGE.ADOPTION_STATUS.ADOPTED';
      default:
        return 'APP.PROFILE-PAGE.ADOPTION_STATUS.UNKNOWN';
    }
  }

  getAdoptionStatusClass(status: AdoptionStatus | undefined): string {
    switch (status) {
      case AdoptionStatus.Available:
        return 'bg-green-500/25 text-green-400';
      case AdoptionStatus.Adopted:
        return 'bg-blue-500/25 text-blue-400';
      default:
        return 'bg-gray-500/25 text-gray-400';
    }
  }

  getAdoptionStatusIcon(status: AdoptionStatus | undefined): string {
    switch (status) {
      case AdoptionStatus.Available:
        return 'lucideCheck';
      case AdoptionStatus.Adopted:
        return 'lucideHeart';
      default:
        return 'lucideCircleHelp';
    }
  }

  getGenderTranslationKey(gender: Gender | undefined): string {
    switch (gender) {
      case Gender.Male:
        return 'APP.COMMONS.MALE';
      case Gender.Female:
        return 'APP.COMMONS.FEMALE';
      default:
        return 'APP.COMMONS.UNKNOWN';
    }
  }

  getGenderIcon(gender: Gender | undefined): string {
    switch (gender) {
      case Gender.Male:
        return 'lucideShield';
      case Gender.Female:
        return 'lucideHeart';
      default:
        return 'lucideCircleHelp';
    }
  }

  getGenderClass(gender: Gender | undefined): string {
    switch (gender) {
      case Gender.Male:
        return 'bg-blue-500/25 text-blue-400';
      case Gender.Female:
        return 'bg-pink-500/25 text-pink-400';
      default:
        return 'bg-gray-500/25 text-gray-400';
    }
  }

  navigateToAdopt(): void {
    if (this.animal) {
      this.router.navigate(['/adopt', this.animal.id]);
    }
  }

  navigateToShelterProfile(): void {
    if (this.animal?.shelter?.user?.id) {
      this.router.navigate(['/profile', this.animal.shelter.user.id]);
    }
  }

  goBack(): void {
    window.history.back();
  }

  reloadPage(): void {
    window.location.reload();
  }

  getOperatingHours(day: string): string {
    if (!this.animal?.shelter?.operatingHours) {
      return this.translationService.translate('APP.ANIMALS.VIEW.CLOSED');
    }

    const operatingHours = this.animal.shelter.operatingHours as OperatingHours;
    const hours = operatingHours[day as keyof OperatingHours];
    return (
      hours || this.translationService.translate('APP.ANIMALS.VIEW.CLOSED')
    );
  }
}
