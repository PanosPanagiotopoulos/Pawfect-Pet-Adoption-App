import { Conversation } from '../conversation/conversation.model';
import { User } from '../user/user.model';

// Message Models
export interface Message {
  id?: string;
  conversation?: Conversation;
  sender?: User;
  recipient?: User;
  content?: string;
  isRead?: boolean;
  createdAt?: Date;
}

export interface MessagePersist {
  id: string;
  conversationId: string;
  senderId: string;
  recipientId: string;
  content: string;
  isRead: boolean;
}
