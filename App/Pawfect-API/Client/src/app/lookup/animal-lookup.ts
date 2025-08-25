import { Gender } from '../common/enum/gender';
import { AdoptionStatus } from '../models/animal/animal.model';
import { Lookup } from './lookup';

export interface AnimalLookup extends Lookup {
  ids?: string[];
  shelterIds?: string[];
  breedIds?: string[];
  animalTypeIds?: string[];
  adoptionStatuses?: AdoptionStatus[];
  genders?: Gender[];
  ageFrom?: number;
  ageTo?: number;
  createFrom?: Date;
  createdTill?: Date;
}
