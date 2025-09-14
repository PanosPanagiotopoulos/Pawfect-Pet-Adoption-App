import { Lookup } from './lookup';
import { MessageType } from '../common/enum/message-type.enum';
import { MessageStatus } from '../common/enum/message-status.enum';

export interface MessageLookup extends Lookup {
  ids?: string[];
  excludedIds?: string[];
  conversationIds?: string[];
  senderIds?: string[];
  types?: MessageType[];
  readBy?: string[];
  statuses?: MessageStatus[];
  createFrom?: Date;
  createdTill?: Date;
}
