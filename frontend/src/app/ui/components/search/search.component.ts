import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { AnimalService } from 'src/app/services/animal.service';
import { Animal } from 'src/app/models/animal/animal.model';
import { Breed } from 'src/app/models/breed/breed.model';
import { AnimalType } from 'src/app/models/animal-type/animal-type.model';
import { AnimalLookup } from 'src/app/lookup/animal-lookup';
import { takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { nameof } from 'ts-simple-nameof';
import { Shelter } from 'src/app/models/shelter/shelter.model';

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
  error: string | null = null;

  // Pagination related properties
  pageSize = 5;
  currentOffset = 0;
  loadThreshold = 0.7;
  hasMoreToLoad = true;

  // Key to force recreation of SwipeCardComponent
  currentAnimalKey: string | null = null;

  constructor(private animalService: AnimalService) {
    super();
  }

  ngOnInit() {
    this.loadAnimals();

    this.searchControl.valueChanges
      .pipe(
        takeUntil(this._destroyed),
        debounceTime(300),
        distinctUntilChanged()
      )
      .subscribe(() => {
        this.resetSearch();
      });
  }

  loadAnimals(append: boolean = false) {
    if (!this.hasMoreToLoad && append) {
      return;
    }

    if (!append) {
      this.isLoading = true;
      this.currentOffset = 0;
      this.hasMoreToLoad = true;
    } else {
      this.isLoadingMore = true;
    }
    
    const lookup: AnimalLookup = {
      offset: this.currentOffset,
      pageSize: this.pageSize,
      query: this.searchControl.value || '',
      excludedIds: this.animals.map(animal => animal.id!),
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
        nameof<Animal>(x => x.animalType) + "." + nameof<AnimalType>(x => x.name),
        nameof<Animal>(x => x.breed) + "." + nameof<Breed>(x => x.name),
        nameof<Animal>(x => x.shelter) + "." + nameof<Shelter>(x => x.shelterName),
      ],
      sortBy: ['createdAt'],
      sortDescending: true
    };

    this.animalService.query(lookup)
      .pipe(takeUntil(this._destroyed))
      .subscribe({
        next: (animals) => {
          if (animals.length < this.pageSize) {
            this.hasMoreToLoad = false;
          }

          if (append) {
            this.animals = [...this.animals, ...animals];
            this.isLoadingMore = false;
          } else {
            this.animals = animals;
            this.currentIndex = 0;
            this.isLoading = false;
          }
          this.error = null;
          this.currentOffset++;
          this.updateCurrentAnimalKey(); 
        },
        error: (error) => {
          console.error('Error loading animals:', error);
          this.error = 'Παρουσιάστηκε σφάλμα κατά τη φόρτωση των ζώων';
          this.isLoading = false;
          this.isLoadingMore = false;
        }
      });
  }

  checkLoadMore() {
    const viewedPercentage = this.currentIndex / this.animals.length;
    if (viewedPercentage >= this.loadThreshold && !this.isLoadingMore && this.hasMoreToLoad) {
      this.loadAnimals(true);
    }
  }

  onSwipeRight(animal: Animal) {
    this.savedAnimals = [...this.savedAnimals, animal];
    this.currentIndex++;
    this.updateCurrentAnimalKey(); // Recreate SwipeCardComponent
    this.checkLoadMore();
  }

  onSwipeLeft() {
    this.currentIndex++;
    this.updateCurrentAnimalKey(); // Recreate SwipeCardComponent
    this.checkLoadMore();
  }

  getCurrentAnimal(): Animal | undefined {
    return this.animals[this.currentIndex] || undefined;
  }

  hasMoreAnimals(): boolean {
    return this.currentIndex < this.animals.length || this.isLoadingMore;
  }

  resetSearch() {
    this.currentIndex = 0;
    this.animals = [];
    this.currentOffset = 0;
    this.loadAnimals();
  }

  // Helper method to generate a unique key for the current animal
  private updateCurrentAnimalKey() {
    const animal = this.getCurrentAnimal();
    if (animal) {
      // Generate a unique key with timestamp to force recreation
      this.currentAnimalKey = `animal-${animal.id}-${Date.now()}`;
    } else {
      this.currentAnimalKey = null;
    }
    console.log('Updated animal key:', this.currentAnimalKey);
  }
}