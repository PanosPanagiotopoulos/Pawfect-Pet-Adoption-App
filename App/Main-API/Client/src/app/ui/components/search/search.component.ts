import { Component, OnInit, ViewChild, ElementRef, OnDestroy } from '@angular/core';
import { FormControl, FormGroup, FormBuilder } from '@angular/forms';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { AnimalService } from 'src/app/services/animal.service';
import { AdoptionStatus, Animal } from 'src/app/models/animal/animal.model';
import { Breed } from 'src/app/models/breed/breed.model';
import { AnimalType } from 'src/app/models/animal-type/animal-type.model';
import { AnimalLookup } from 'src/app/lookup/animal-lookup';
import { takeUntil, debounceTime, distinctUntilChanged, tap, finalize, catchError } from 'rxjs/operators';
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

interface SearchSuggestion {
  text: string;
  query: string;
  icon: string;
}

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.css']
})
export class SearchComponent extends BaseComponent implements OnInit, OnDestroy {
  @ViewChild('swipeCardContainer', { static: false }) swipeCardContainer!: ElementRef;
  
  searchControl = new FormControl('');
  searchForm: FormGroup;

  animals: Animal[] = [];
  savedAnimals: Animal[] = [];
  currentIndex = 0;
  isLoading = false;
  isLoadingMore = false;
  error: ErrorDetails | null = null;

  // Search suggestions
  searchSuggestions: SearchSuggestion[] = [
    { 
      text: 'Φιλικό προς παιδιά',
      query: 'Ένα ήρεμο και φιλικό κατοικίδιο που αγαπάει τα παιδιά',
      icon: 'lucideHeart'
    },
    { 
      text: 'Μικρό μέγεθος',
      query: 'Ένα μικρόσωμο κατοικίδιο κατάλληλο για διαμέρισμα',
      icon: 'lucideDog'
    },
    { 
      text: 'Ενεργητικό',
      query: 'Ένα ενεργητικό κατοικίδιο για τρέξιμο και παιχνίδι',
      icon: 'lucideActivity'
    },
    { 
      text: 'Ήσυχο',
      query: 'Ένα ήσυχο και ήρεμο κατοικίδιο',
      icon: 'lucideMoon'
    }
  ];

  // Pagination related properties
  pageSize = 20;
  currentOffset = 0;
  loadThreshold = 0.55;
  hasMoreToLoad = true;

  isInitialLoad = true; 

  currentAnimalKey: string | null = null;

  showInstructionsModal = true;

  private readonly STORAGE_KEY = 'savedAnimals';

  constructor(
    private fb: FormBuilder,
    private animalService: AnimalService,
    private utilsService: UtilsService,
    private errorHandler: ErrorHandlerService,
    private route: ActivatedRoute,
    private router: Router,
    private secureStorage: SecureStorageService,
    private cdr: ChangeDetectorRef
  ) {
    super();
    this.searchForm = this.fb.group({
      searchQuery: this.searchControl
    });
  }

  ngOnInit() {
    // Load saved animals from storage
    this.loadSavedAnimals();

    // Handle search query from URL
    this.route.queryParams.pipe(
      takeUntil(this._destroyed)
    ).subscribe(params => {
      const query = params['query'] || '';
      this.searchControl.setValue(query);
      if (query) {
        this.onSearch();
      }
    });
  }

  private loadSavedAnimals() {
    const savedData = this.secureStorage.getItem<{ savedAnimals: Animal[], timestamp: number }>(this.STORAGE_KEY);
    if (savedData) {
      const now = new Date().getTime();
      const expirationTime = 20 * 60 * 1000; // 20 minutes
      if (now - savedData.timestamp < expirationTime) {
        this.savedAnimals = savedData.savedAnimals || [];
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

      this.isLoading = true;
      this.isInitialLoad = false;
      this.error = null;
      this.currentIndex = 0;
      this.animals = [];
      this.currentAnimalKey = null;

      const lookup: AnimalLookup = {
        offset: this.currentOffset, 
        pageSize: this.pageSize,
        adoptionStatuses: [AdoptionStatus.Available, AdoptionStatus.Pending],
        query: query,
        fields: [
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
        ],
        sortBy: [],
        sortDescending: false
      };
      
      this.animalService.queryFreeView(lookup).subscribe({
        next: (response: Animal[]) => {
          this.animals = response;
          this.isLoading = false;
          if (this.animals.length > 0) {
            this.updateCurrentAnimalKey();
            setTimeout(() => {
              if (this.swipeCardContainer?.nativeElement) {
                const containerRect = this.swipeCardContainer.nativeElement.getBoundingClientRect();
                const scrollPosition = containerRect.top - 350; 
                window.scrollTo({
                  top: scrollPosition,
                  behavior: 'smooth'
                });
              }
            }, 100);
          }
          this.cdr.markForCheck();
        },
        error: (error: any) => {
          this.isLoading = false;
          this.error = {
            title: 'Σφάλμα Αναζήτησης',
            message: 'Δεν ήταν δυνατή η αναζήτηση κατοικιδίων. Παρακαλώ δοκιμάστε ξανά.'
          };
          this.cdr.markForCheck();
        }
      });
    }
  }

  applySearchSuggestion(suggestion: SearchSuggestion) {
    this.searchControl.setValue(suggestion.query);
    this.onSearch();
  }

  loadAnimals(append: boolean = false) {
    if (!this.hasMoreToLoad && append) {
      return;
    }
  
    if (!append) {
      this.isLoading = true;
      this.currentOffset = 1;
      this.hasMoreToLoad = true;
      this.error = null;
    } else {
      this.isLoadingMore = true;
    }
    
    const lookup: AnimalLookup = {
      offset: this.currentOffset, 
      pageSize: this.pageSize,
      query: this.searchControl.value || '',
      fields: [
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
      ],
      sortBy: [],
      sortDescending: false
    };
  
    this.animalService.queryFreeView(lookup)
      .pipe(
        takeUntil(this._destroyed),
        catchError(error => {
          this.error = this.errorHandler.handleError(error);
          return of([]);
        }),
        finalize(() => {
          this.isLoading = false;
          this.isLoadingMore = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe(animals => {
        if (animals.length < this.pageSize) {
          this.hasMoreToLoad = false;
        }

        this.animals = this.utilsService.combineDistinct(this.animals, animals);
        
        this.currentOffset++;
        this.updateCurrentAnimalKey();
        this.cdr.markForCheck();
      });
  }

  checkLoadMore() {
    const viewedPercentage = this.currentIndex / this.animals.length;
    if (viewedPercentage >= this.loadThreshold && !this.isLoadingMore && this.hasMoreToLoad) {
      this.loadAnimals(true);
    }
  }

  onSwipeRight(animal: Animal) {
    this.savedAnimals = this.utilsService.combineDistinct(this.savedAnimals, [animal]);
    this.currentIndex++;
    this.updateCurrentAnimalKey();
    this.checkLoadMore();
    this.cdr.markForCheck();
    this.saveSavedAnimals();
  }

  onSwipeLeft() {
    if (this.hasMoreAnimals()) {
      this.currentIndex++;
      this.updateCurrentAnimalKey();
    }
    this.cdr.markForCheck();
  }

  getCurrentAnimal(): Animal | null {
    return this.animals[this.currentIndex] || null;
  }

  hasMoreAnimals(): boolean {
    return this.currentIndex < this.animals.length - 1;
  }

  resetSearch() {
    this.currentOffset = 1;
    this.error = null;
    this.currentIndex = 0;
    this.animals = [];
    this.currentAnimalKey = null;
    this.isInitialLoad = true;
    this.searchControl.setValue('');
    this.updateQueryParams('');
    this.cdr.markForCheck();
  }

  private updateCurrentAnimalKey() {
    const currentAnimal = this.getCurrentAnimal();
    this.currentAnimalKey = currentAnimal ? `${currentAnimal.id}-${this.currentIndex}` : null;
  }

  private updateQueryParams(query: string) {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { query: query || null },
      queryParamsHandling: 'merge'
    });
  }

  private saveSavedAnimals() {
    if (this.savedAnimals.length > 0) {
      const data = {
        savedAnimals: this.savedAnimals,
        timestamp: new Date().getTime()
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