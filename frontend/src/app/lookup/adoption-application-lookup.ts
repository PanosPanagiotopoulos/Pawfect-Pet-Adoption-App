import { ApplicationStatus } from '../models/adoption-application/adoption-application.model';
import { Lookup } from './lookup';

export interface AdoptionApplicationLookup extends Lookup {
  ids?: string[];
  userIds?: string[];
  animalIds?: string[];
  shelterIds?: string[];
  status?: ApplicationStatus[]; // e.g. Available, Pending, Rejected
  createdFrom?: Date;
  createdTill?: Date;
}
