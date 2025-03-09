import { Animal } from '../animal/animal.model';
import { User } from '../user/user.model';

// Conversation Models
export interface Conversation {
  id?: string;
  users?: User[];
  animal?: Animal;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface ConversationPersist {
  id: string;
  userIds: string[];
  animalId: string;
}
