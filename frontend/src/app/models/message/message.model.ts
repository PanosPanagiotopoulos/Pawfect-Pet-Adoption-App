import { Conversation } from '../conversation/conversation.model';
import { User } from '../user/user.model';

// Message Models
export interface Message {
  Id?: string;
  Conversation?: Conversation;
  Sender?: User;
  Recipient?: User;
  Content?: string;
  IsRead?: boolean;
  CreatedAt?: Date;
}

export interface MessagePersist {
  Id: string;
  ConversationId: string;
  SenderId: string;
  RecipientId: string;
  Content: string;
  IsRead: boolean;
}
