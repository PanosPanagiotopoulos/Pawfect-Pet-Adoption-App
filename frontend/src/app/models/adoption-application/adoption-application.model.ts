import { Animal } from '../animal/animal.model';
import { Shelter } from '../shelter/shelter.model';
import { User } from '../user/user.model';

// AdoptionApplication Models
export interface AdoptionApplication {
  Id?: string;
  User?: User;
  Animal?: Animal;
  Shelter?: Shelter;
  Status?: ApplicationStatus;
  ApplicationDetails?: string;
  AttachedFilesUrls?: string[];
  CreatedAt?: Date;
  UpdatedAt?: Date;
}

export interface AdoptionApplicationPersist {
  Id: string;
  UserId: string;
  AnimalId: string;
  ShelterId: string;
  Status: ApplicationStatus;
  ApplicationDetails: string;
  //*TODO* Is type ok?
  AttachedFiles: File[];
}

// Enums
export enum ApplicationStatus {
  Available = 1,
  Pending = 2,
  Rejected = 3,
}
