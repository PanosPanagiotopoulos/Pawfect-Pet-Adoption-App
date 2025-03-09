import { AnimalType } from '../animal-type/animal-type.model';

// Breed Models
export interface Breed {
  id?: string;
  name?: string;
  animalType?: AnimalType;
  description?: string;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface BreedPersist {
  id: string;
  name: string;
  typeId: string;
  description: string;
}
