import { AnimalType } from '../animal-type/animal-type.model';

// Breed Models
export interface Breed {
  Id?: string;
  Name?: string;
  AnimalType?: AnimalType;
  Description?: string;
  CreatedAt?: Date;
  UpdatedAt?: Date;
}

export interface BreedPersist {
  Id: string;
  Name: string;
  TypeId: string;
  Description: string;
}
