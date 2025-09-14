import { MessageType } from 'src/app/common/enum/message-type.enum';
import { Conversation } from '../conversation/conversation.model';
import { User } from '../user/user.model';
import { MessageStatus } from 'src/app/common/enum/message-status.enum';

// Message Models
export interface Message {
  id?: string;
  conversation?: Conversation;
  sender?: User;
  readBy?: User[];
  type?: MessageType;
  content?: string;
  status?: MessageStatus;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface MessagePersist {
  id?: string;
  conversationId: string;
  senderId: string;
  type: MessageType;
  content: string;
}

export interface MessageReadPersist {
  messageId: string;
  userIds: string[];
}