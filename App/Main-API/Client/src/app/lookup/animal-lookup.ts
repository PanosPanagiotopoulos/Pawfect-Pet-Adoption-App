import { AdoptionStatus } from '../models/animal/animal.model';
import { Lookup } from './lookup';

export interface AnimalLookup extends Lookup {
  ids?: string[];
  shelterIds?: string[];
  breedIds?: string[];
  typeIds?: string[];
  adoptionStatuses?: AdoptionStatus[];
  createFrom?: Date;
  createdTill?: Date;
}
