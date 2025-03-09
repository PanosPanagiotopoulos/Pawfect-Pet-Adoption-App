import { Gender } from 'src/app/common/enum/gender';
import { AnimalType } from '../animal-type/animal-type.model';
import { Shelter } from '../shelter/shelter.model';
import { Breed } from '../breed/breed.model';

// Animal Models
export interface Animal {
  id?: string;
  name?: string;
  age?: number;
  gender?: Gender;
  description?: string;
  weight?: number;
  healthStatus?: string;
  shelter?: Shelter;
  breed?: Breed;
  animalType?: AnimalType;
  photos?: string[];
  adoptionStatus?: AdoptionStatus;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface AnimalPersist {
  id: string;
  name: string;
  age: number;
  gender: Gender;
  description: string;
  weight: number;
  healthStatus: string;
  shelterId: string;
  breedId: string;
  animalTypeId: string;
  //*TODO* Is type ok?
  attachedPhotos: File[];
  adoptionStatus: AdoptionStatus;
}

// Enum
export enum AdoptionStatus {
  Available = 1,
  Pending = 2,
  Adopted = 3,
}
