import { Lookup } from './lookup';

export interface MessageLookup extends Lookup {
  ids?: string[];
  conversationIds?: string[];
  senderIds?: string[];
  recipientIds?: string[];
  createFrom?: Date;
  createdTill?: Date;
}
