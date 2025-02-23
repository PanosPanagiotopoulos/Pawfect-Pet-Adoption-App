import { Lookup } from './lookup';

export interface BreedLookup extends Lookup {
  ids?: string[];
  typeIds?: string[];
  createdFrom?: Date;
  createdTill?: Date;
}
