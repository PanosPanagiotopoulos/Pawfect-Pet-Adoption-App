import { Lookup } from './lookup';

export interface AnimalTypeLookup extends Lookup {
  ids?: string[];
  name?: string;
}
