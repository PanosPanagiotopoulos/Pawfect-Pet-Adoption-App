import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { AnimalService } from 'src/app/services/animal.service';
import { Animal } from 'src/app/models/animal/animal.model';
import { Breed } from 'src/app/models/breed/breed.model';
import { AnimalType } from 'src/app/models/animal-type/animal-type.model';
import { AnimalLookup } from 'src/app/lookup/animal-lookup';
import { takeUntil, debounceTime, distinctUntilChanged, tap, finalize, catchError } from 'rxjs/operators';
import { nameof } from 'ts-simple-nameof';
import { Shelter } from 'src/app/models/shelter/shelter.model';
import { UtilsService } from 'src/app/common/services/utils.service';
import { LogService } from 'src/app/common/services/log.service';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { ErrorDetails } from 'src/app/common/ui/error-message-banner.component';
import { of } from 'rxjs';

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
export class SearchComponent extends BaseComponent implements OnInit {
  searchControl = new FormControl('');
  searchForm = new FormGroup({
    searchQuery: this.searchControl
  });

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
  pageSize = 2;
  currentOffset = 1;
  loadThreshold = 0.55;
  hasMoreToLoad = true;

  // Initial state
  isInitialLoad = true; 

  // Key to force recreation of SwipeCardComponent
  currentAnimalKey: string | null = null;

  constructor(
    private animalService: AnimalService,
    private utilsService: UtilsService,
    private logService: LogService,
    private errorHandler: ErrorHandlerService
  ) {
    super();
  }

  ngOnInit() { }

  onSearch() {
    this.isInitialLoad = false;
    this.resetSearch();
    this.loadAnimals();
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
        nameof<Animal>(x => x.photos),
        nameof<Animal>(x => x.adoptionStatus),
        nameof<Animal>(x => x.weight),
        nameof<Animal>(x => x.age),
        nameof<Animal>(x => x.healthStatus),
        [nameof<Animal>(x => x.animalType), nameof<AnimalType>(x => x.name)].join('.'),
        [nameof<Animal>(x => x.breed), nameof<Breed>(x => x.name)].join('.'),
        [nameof<Animal>(x => x.shelter), nameof<Shelter>(x => x.shelterName)].join('.'),
      ],
      sortBy: [],
      sortDescending: true
    };
  
    this.animalService.query(lookup)
      .pipe(
        takeUntil(this._destroyed),
        catchError(error => {
          this.error = this.errorHandler.handleError(error);
          return of([]);
        }),
        finalize(() => {
          this.isLoading = false;
          this.isLoadingMore = false;
        })
      )
      .subscribe(animals => {
        if (animals.length < this.pageSize) {
          this.hasMoreToLoad = false;
        }

        this.animals = this.utilsService.combineDistinct(this.utilsService.combineDistinct(this.animals, animals), this.savedAnimals);
        
        this.currentOffset++;
        this.updateCurrentAnimalKey();
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
  }

  onSwipeLeft() {
    this.currentIndex++;
    this.updateCurrentAnimalKey();
    this.checkLoadMore();
  }

  getCurrentAnimal(): Animal | undefined {
    return this.animals[this.currentIndex];
  }

  hasMoreAnimals(): boolean {
    return this.currentIndex < this.animals.length || this.isLoadingMore;
  }

  resetSearch() {
    this.currentOffset = 1;
    this.error = null;
  }

  private updateCurrentAnimalKey() {
    const animal = this.getCurrentAnimal();
    if (animal) {
      this.currentAnimalKey = `animal-${animal.id}-${Date.now()}`;
    } else {
      this.currentAnimalKey = null;
    }
  }
}