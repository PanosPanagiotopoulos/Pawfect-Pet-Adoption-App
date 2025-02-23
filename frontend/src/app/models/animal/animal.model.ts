import { Gender } from 'src/app/common/enum/gender';
import { AnimalType } from '../animal-type/animal-type.model';
import { Shelter } from '../shelter/shelter.model';
import { Breed } from '../breed/breed.model';

// Animal Models
export interface Animal {
  Id?: string;
  Name?: string;
  Age?: number;
  Gender?: Gender;
  Description?: string;
  Weight?: number;
  HealthStatus?: string;
  Shelter?: Shelter;
  Breed?: Breed;
  AnimalType?: AnimalType;
  Photos?: string[];
  AdoptionStatus?: AdoptionStatus;
  CreatedAt?: Date;
  UpdatedAt?: Date;
}

export interface AnimalIndexModel {
  Id: string;
  Text: string;
}

export interface AnimalPersist {
  Id: string;
  Name: string;
  Age: number;
  Gender: Gender;
  Description: string;
  Weight: number;
  HealthStatus: string;
  ShelterId: string;
  BreedId: string;
  AnimalTypeId: string;
  //*TODO* Is type ok?
  AttachedPhotos: File[];
  AdoptionStatus: AdoptionStatus;
}

// Enum
export enum AdoptionStatus {
  Available = 1,
  Pending = 2,
  Adopted = 3,
}
