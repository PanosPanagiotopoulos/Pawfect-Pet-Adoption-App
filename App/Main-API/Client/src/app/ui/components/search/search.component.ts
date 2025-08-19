import {
  Component,
  OnInit,
  ViewChild,
  ElementRef,
  OnDestroy,
} from '@angular/core';
import { FormControl, FormGroup, FormBuilder } from '@angular/forms';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { AnimalService } from 'src/app/services/animal.service';
import { AdoptionStatus, Animal } from 'src/app/models/animal/animal.model';
import { Breed } from 'src/app/models/breed/breed.model';
import { AnimalType } from 'src/app/models/animal-type/animal-type.model';
import { AnimalLookup } from 'src/app/lookup/animal-lookup';
import { takeUntil, finalize, catchError } from 'rxjs/operators';
import { nameof } from 'ts-simple-nameof';
import { Shelter } from 'src/app/models/shelter/shelter.model';
import { UtilsService } from 'src/app/common/services/utils.service';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { ErrorDetails } from 'src/app/common/ui/error-message-banner.component';
import { of } from 'rxjs';
import { File } from 'src/app/models/file/file.model';
import { ActivatedRoute, Router } from '@angular/router';
import { SecureStorageService } from 'src/app/common/services/secure-storage.service';
import { ChangeDetectorRef } from '@angular/core';
import { TranslationService } from 'src/app/common/services/translation.service';
import { QueryResult } from 'src/app/common/models/query-result';

interface SearchSuggestion {
  text: string;
  query: string;
  icon: string;
}

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css'],
})
export class SearchComponent
  extends BaseComponent
  implements OnInit, OnDestroy
{
  @ViewChild('swipeCardContainer', { static: false })
  swipeCardContainer!: ElementRef;

  searchControl = new FormControl('');
  searchForm: FormGroup;

  animals: Animal[] = [];
  savedAnimals: Animal[] = [];
  currentIndex = 0;
  isLoading = false;
  isLoadingMore = false;
  error: ErrorDetails | null = null;

  // Search suggestions
  searchSuggestions: SearchSuggestion[] = [];

  // Pagination related properties
  pageSize = 20;
  currentOffset = 0;
  loadThreshold = 0.8; // Changed to 80% as requested
  hasMoreToLoad = true;
  totalAnimalsCount = 0; // Track total count from query response

  isInitialLoad = true;

  currentAnimalKey: string | null = null;

  showInstructionsModal = true;

  // Track animals that have been explicitly saved or declined by user
  private savedAnimalIds: Set<string> = new Set();
  private declinedAnimalIds: Set<string> = new Set();
  private currentSearchQuery = '';
  private currentFilters: Partial<AnimalLookup> = {};

  private readonly STORAGE_KEY = 'savedAnimals';
  private hasHandledInitialQuery = false;

  constructor(
    private fb: FormBuilder,
    private animalService: AnimalService,
    private utilsService: UtilsService,
    private errorHandler: ErrorHandlerService,
    private route: ActivatedRoute,
    private router: Router,
    private secureStorage: SecureStorageService,
    private cdr: ChangeDetectorRef,
    private translationService: TranslationService
  ) {
    super();
    this.searchForm = this.fb.group({
      searchQuery: this.searchControl,
    });

    // Initialize search suggestions with translated text
    this.searchSuggestions = [
      {
        text: this.translationService.translate(
          'APP.SEARCH.SUGGESTIONS.CHILD_FRIENDLY'
        ),
        query: this.translationService.translate(
          'APP.SEARCH.SUGGESTIONS.CHILD_FRIENDLY_QUERY'
        ),
        icon: 'lucideHeart',
      },
      {
        text: this.translationService.translate(
          'APP.SEARCH.SUGGESTIONS.SMALL_SIZE'
        ),
        query: this.translationService.translate(
          'APP.SEARCH.SUGGESTIONS.SMALL_SIZE_QUERY'
        ),
        icon: 'lucideDog',
      },
      {
        text: this.translationService.translate(
          'APP.SEARCH.SUGGESTIONS.ACTIVE'
        ),
        query: this.translationService.translate(
          'APP.SEARCH.SUGGESTIONS.ACTIVE_QUERY'
        ),
        icon: 'lucideActivity',
      },
      {
        text: this.translationService.translate('APP.SEARCH.SUGGESTIONS.QUIET'),
        query: this.translationService.translate(
          'APP.SEARCH.SUGGESTIONS.QUIET_QUERY'
        ),
        icon: 'lucideMoon',
      },
    ];
  }

  ngOnInit() {
    // Load saved animals from storage
    this.loadSavedAnimals();

    // Handle search query from URL
    this.route.queryParams
      .pipe(takeUntil(this._destroyed))
      .subscribe((params) => {
        const query = params['query'] || '';
        if (
          !this.hasHandledInitialQuery &&
          query !== this.searchControl.value
        ) {
          this.searchControl.setValue(query);
          if (query) {
            this.onSearch();
          }
          this.hasHandledInitialQuery = true;
        }
      });
  }

  override ngOnDestroy(): void {
    this.clearSavedAnimals();
  }

  private loadSavedAnimals() {
    const savedData = this.secureStorage.getItem<{
      savedAnimals: Animal[];
      timestamp: number;
    }>(this.STORAGE_KEY);
    if (savedData) {
      const now = new Date().getTime();
      const expirationTime = 20 * 60 * 1000; // 20 minutes
      if (now - savedData.timestamp < expirationTime) {
        this.savedAnimals = savedData.savedAnimals || [];

        // Add saved animals to saved IDs set
        this.savedAnimals.forEach((animal) => {
          if (animal.id) {
            this.savedAnimalIds.add(animal.id);
          }
        });

        this.cdr.markForCheck();
      } else {
        this.secureStorage.removeItem(this.STORAGE_KEY);
      }
    }
  }

  onSearch(): void {
    if (this.searchForm.valid) {
      const query = this.searchControl.value || '';
      this.updateQueryParams(query);

      // Reset for new search query
      this.currentOffset = 0;
      this.currentSearchQuery = query;
      this.isLoading = true;
      this.isInitialLoad = false;
      this.error = null;
      this.currentIndex = 0;
      this.animals = [];
      this.currentAnimalKey = null;
      this.hasMoreToLoad = true;

      // Clear declined animals for new search (allow previously declined animals to appear again)
      this.declinedAnimalIds.clear();

      // Store current filters for pagination
      this.currentFilters = {
        adoptionStatuses: [AdoptionStatus.Available, AdoptionStatus.Pending],
        // Add other filters here as needed
      };

      this.loadAnimalsWithCurrentSettings();
    } else {
      // Handle form validation errors if needed
      this.error = {
        title: this.translationService.translate(
          'APP.SEARCH.ERRORS.SEARCH_ERROR_TITLE'
        ),
        message: this.translationService.translate(
          'APP.SEARCH.ERRORS.SEARCH_ERROR_MESSAGE'
        ),
      };
      this.cdr.markForCheck();
    }
  }

  applySearchSuggestion(suggestion: SearchSuggestion) {
    this.searchControl.setValue(suggestion.query);
    this.onSearch();
  }

  private loadAnimalsWithCurrentSettings(append: boolean = false) {
    if (!this.hasMoreToLoad && append) {
      return;
    }

    if (append) {
      this.isLoadingMore = true;
    }

    // Only exclude animals that have been explicitly saved or declined
    const excludedIds = Array.from(
      new Set([...this.savedAnimalIds, ...this.declinedAnimalIds])
    );

    const lookup: AnimalLookup = {
      offset: this.currentOffset,
      pageSize: this.pageSize,
      query: this.currentSearchQuery,
      excludedIds: excludedIds.length > 0 ? excludedIds : undefined,
      ...this.currentFilters,
      fields: [
        nameof<Animal>((x) => x.id),
        nameof<Animal>((x) => x.name),
        nameof<Animal>((x) => x.gender),
        nameof<Animal>((x) => x.description),
        [
          nameof<Animal>((x) => x.attachedPhotos),
          nameof<File>((x) => x.sourceUrl),
        ].join('.'),
        nameof<Animal>((x) => x.adoptionStatus),
        nameof<Animal>((x) => x.weight),
        nameof<Animal>((x) => x.age),
        nameof<Animal>((x) => x.healthStatus),
        [
          nameof<Animal>((x) => x.animalType),
          nameof<AnimalType>((x) => x.name),
        ].join('.'),
        [nameof<Animal>((x) => x.breed), nameof<Breed>((x) => x.name)].join(
          '.'
        ),
        [
          nameof<Animal>((x) => x.shelter),
          nameof<Shelter>((x) => x.shelterName),
        ].join('.'),
      ],
      sortBy: [],
      sortDescending: false,
    };

    this.animalService
      .queryFreeView(lookup)
      .pipe(
        takeUntil(this._destroyed),
        catchError((error) => {
          this.error = this.errorHandler.handleError(error);
          return of({ items: [], count: 0 });
        }),
        finalize(() => {
          this.isLoading = false;
          this.isLoadingMore = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe((response: QueryResult<Animal>) => {
        // Update total count from response
        this.totalAnimalsCount = response.count || 0;

        if (response.items.length < this.pageSize) {
          this.hasMoreToLoad = false;
        }

        if (append) {
          this.animals = this.utilsService.combineDistinct(
            this.animals,
            response.items
          );
        } else {
          this.animals = response.items;

          if (this.animals.length > 0) {
            this.updateCurrentAnimalKey();
            setTimeout(() => {
              if (this.swipeCardContainer?.nativeElement) {
                const containerRect =
                  this.swipeCardContainer.nativeElement.getBoundingClientRect();
                const scrollPosition = containerRect.top - 350;
                window.scrollTo({
                  top: scrollPosition,
                  behavior: 'smooth',
                });
              }
            }, 100);
          }
        }

        this.currentOffset++;
        this.updateCurrentAnimalKey();
        this.cdr.markForCheck();
      });
  }

  loadAnimals(append: boolean = false) {
    this.loadAnimalsWithCurrentSettings(append);
  }

  checkLoadMore() {
    // Calculate viewed percentage based on total animals count from query response
    const viewedPercentage =
      this.totalAnimalsCount > 0
        ? this.currentIndex / this.totalAnimalsCount
        : 0;
    if (
      viewedPercentage >= this.loadThreshold &&
      !this.isLoadingMore &&
      this.hasMoreToLoad
    ) {
      this.loadAnimals(true);
    }
  }

  onSwipeRight(animal: Animal) {
    this.savedAnimals = this.utilsService.combineDistinct(this.savedAnimals, [
      animal,
    ]);

    // Add to saved animals IDs set
    if (animal.id) {
      this.savedAnimalIds.add(animal.id);
    }

    this.currentIndex++;
    this.updateCurrentAnimalKey();
    this.checkLoadMore();
    this.cdr.markForCheck();
    this.saveSavedAnimals();
  }

  onSwipeLeft() {
    const currentAnimal = this.getCurrentAnimal();

    // Add to declined animals list
    if (currentAnimal?.id) {
      this.declinedAnimalIds.add(currentAnimal.id);
    }

    if (this.hasMoreAnimals()) {
      this.currentIndex++;
      this.updateCurrentAnimalKey();
    }
    this.checkLoadMore();
    this.cdr.markForCheck();
  }

  getCurrentAnimal(): Animal | null {
    return this.animals[this.currentIndex] || null;
  }

  hasMoreAnimals(): boolean {
    return this.currentIndex < this.animals.length - 1;
  }

  resetSearch() {
    this.currentOffset = 0;
    this.currentSearchQuery = '';
    this.currentFilters = {};
    this.declinedAnimalIds.clear(); // Only clear declined animals, keep saved ones
    this.totalAnimalsCount = 0;
    this.error = null;
    this.currentIndex = 0;
    this.animals = [];
    this.currentAnimalKey = null;
    this.isInitialLoad = true;
    this.hasMoreToLoad = true;
    this.searchControl.setValue('');
    this.updateQueryParams('');
    this.cdr.markForCheck();
  }

  clearSavedAnimals() {
    this.savedAnimals = [];
    this.savedAnimalIds.clear();
    this.declinedAnimalIds.clear();
    this.secureStorage.removeItem(this.STORAGE_KEY);
    this.cdr.markForCheck();
  }

  removeSavedAnimal(animalToRemove: Animal) {
    this.savedAnimals = this.savedAnimals.filter(
      (animal) => animal.id !== animalToRemove.id
    );

    // Remove from saved animals IDs set so it can appear again in search
    if (animalToRemove.id) {
      this.savedAnimalIds.delete(animalToRemove.id);
    }

    this.saveSavedAnimals();
    this.cdr.markForCheck();
  }

  private updateCurrentAnimalKey() {
    const currentAnimal = this.getCurrentAnimal();
    this.currentAnimalKey = currentAnimal
      ? `${currentAnimal.id}-${this.currentIndex}`
      : null;
  }

  private updateQueryParams(query: string) {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { query: query || null },
      queryParamsHandling: 'merge',
    });
  }

  private saveSavedAnimals() {
    if (this.savedAnimals.length > 0) {
      const data = {
        savedAnimals: this.savedAnimals,
        timestamp: new Date().getTime(),
      };
      this.secureStorage.setItem(this.STORAGE_KEY, data);
    }
  }

  onCloseInstructionsModal() {
    this.showInstructionsModal = false;
  }

  openInstructionsModal() {
    this.showInstructionsModal = true;
  }
}
