import { ConversationType } from 'src/app/common/enum/conversation-type.enum';
import { Animal } from '../animal/animal.model';
import { User } from '../user/user.model';
import { Message } from '../message/message.model';

// Conversation Models
export interface Conversation {
  id?: string;
  type?: ConversationType;
  participants?: User[];
  animal?: Animal;
  lastMessagePreview?: Message;
  createdBy?: User;
  lastMessageAt?: Date;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface ConversationPersist {
  id?: string;
  type: ConversationType;
  participants: string[];
  createdBy: string;
}
