import { Animal } from '../animal/animal.model';
import { Shelter } from '../shelter/shelter.model';
import { User } from '../user/user.model';

// AdoptionApplication Models
export interface AdoptionApplication {
  id?: string;
  user?: User;
  animal?: Animal;
  shelter?: Shelter;
  status?: ApplicationStatus;
  applicationDetails?: string;
  attachedFilesUrls?: string[];
  createdAt?: Date;
  updatedAt?: Date;
}

export interface AdoptionApplicationPersist {
  id: string;
  userId: string;
  animalId: string;
  shelterId: string;
  status: ApplicationStatus;
  applicationDetails: string;
  //*TODO* Is type ok?
  attachedFiles: File[];
}

// Enums
export enum ApplicationStatus {
  Available = 1,
  Pending = 2,
  Rejected = 3,
}
