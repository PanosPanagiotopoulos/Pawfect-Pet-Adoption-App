import { ConversationType } from '../common/enum/conversation-type.enum';
import { Lookup } from './lookup';

export interface ConversationLookup extends Lookup {
  ids?: string[];
  excludedIds?: string[];
  participants?: string[];
  types?: ConversationType[];
  createFrom?: Date;
  createdTill?: Date;
}
