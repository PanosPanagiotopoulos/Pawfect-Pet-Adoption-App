import { Lookup } from './lookup';

export interface ConversationLookup extends Lookup {
  ids?: string[];
  userIds?: string[];
  animalIds?: string[];
  createFrom?: Date;
  createdTill?: Date;
}
