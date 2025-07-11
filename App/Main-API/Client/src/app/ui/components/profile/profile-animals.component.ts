import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  OnInit,
  OnChanges,
  SimpleChanges,
  ChangeDetectorRef,
  OnDestroy
} from '@angular/core';
import { Animal, AdoptionStatus } from 'src/app/models/animal/animal.model';
import { Gender } from 'src/app/common/enum/gender';
import { AnimalService } from 'src/app/services/animal.service';
import { AnimalLookup } from 'src/app/lookup/animal-lookup';
import { PageEvent } from '@angular/material/paginator';
import { LogService } from 'src/app/common/services/log.service';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { Subscription } from 'rxjs';
import { TranslationService } from 'src/app/common/services/translation.service';

@Component({
  selector: 'app-profile-animals',
  templateUrl: './profile-animals.component.html',
  styleUrls: ['./profile-animals.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileAnimalsComponent implements OnInit, OnChanges, OnDestroy {
  @Input() shelterId: string | null = null;
  @Output() viewDetails = new EventEmitter<Animal>();
  @Output() addAnimal = new EventEmitter<void>();

  animals: Animal[] = [];
  totalAnimals = 0;
  pageSize = 6;
  pageIndex = 0;
  isLoading = false;
  error: string | null = null;
  skeletonRows = Array(6);

  private dataSub?: Subscription;
  private translationSub?: Subscription;

  constructor(
    private animalService: AnimalService,
    private log: LogService,
    private errorHandler: ErrorHandlerService,
    private cdr: ChangeDetectorRef,
    private translationService: TranslationService
  ) {}

  ngOnInit() {
    if (this.shelterId) {
      this.loadAnimals();
    }
    // Subscribe to language changes to trigger change detection for imperative translations
    this.translationSub = this.translationService.languageChanged$.subscribe(() => {
      this.cdr.markForCheck();
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['shelterId'] && !changes['shelterId'].firstChange && changes['shelterId'].currentValue) {
      this.pageIndex = 0;
      this.loadAnimals();
    }
  }

  ngOnDestroy() {
    this.translationSub?.unsubscribe();
    if (this.dataSub) {
      this.dataSub.unsubscribe();
    }
  }

  loadAnimals(event?: PageEvent) {
    if (!this.shelterId) {
      return;
    }

    if (this.dataSub) {
      this.dataSub.unsubscribe();
    }

    this.isLoading = true;
    this.error = null;
    this.cdr.markForCheck();

    if (event) {
      this.pageIndex = event.pageIndex;
      this.pageSize = event.pageSize;
    }

    const lookup: AnimalLookup = {
      shelterIds: [this.shelterId],
      offset: this.pageIndex * this.pageSize,
      pageSize: this.pageSize,
      fields: ['id', 'name', 'breed.name', 'attachedPhotos.sourceUrl', 'adoptionStatus'],
      sortBy: ['createdAt'],
      sortDescending: true,
    };

    this.dataSub = this.animalService.query(lookup).subscribe({
      next: (data) => {
        this.animals = data;
        // In a real app, the total count should come from the API.
        this.totalAnimals = data.length < this.pageSize ? data.length : 50; 
        this.isLoading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.error = 'APP.PROFILE-PAGE.ANIMALS.LOAD_ERROR';
        this.isLoading = false;
        this.errorHandler.handleError(err);
        this.log.logFormatted({ message: 'Failed to load shelter animals', error: err });
        this.cdr.markForCheck();
      }
    });
  }

  onImageError(event: Event) {
    const target = event.target as HTMLImageElement | null;
    if (target) {
      target.src = 'assets/placeholder.jpg';
    }
  }

  onCardClick(animal: Animal) {
    this.viewDetails.emit(animal);
  }

  onAddAnimalClick() {
    this.addAnimal.emit();
  }

  public AdoptionStatus = AdoptionStatus;

  getAdoptionStatusTranslationKey(status: AdoptionStatus | undefined): string {
    switch (status) {
      case AdoptionStatus.Available:
        return 'APP.PROFILE-PAGE.ADOPTION_STATUS.AVAILABLE';
      case AdoptionStatus.Pending:
        return 'APP.PROFILE-PAGE.ADOPTION_STATUS.PENDING';
      case AdoptionStatus.Adopted:
        return 'APP.PROFILE-PAGE.ADOPTION_STATUS.ADOPTED';
      default:
        return 'APP.PROFILE-PAGE.ADOPTION_STATUS.UNKNOWN';
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
} 