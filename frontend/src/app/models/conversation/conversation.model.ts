import { Animal } from '../animal/animal.model';
import { User } from '../user/user.model';

// Conversation Models
export interface Conversation {
  Id?: string;
  Users?: User[];
  Animal?: Animal;
  CreatedAt?: Date;
  UpdatedAt?: Date;
}

export interface ConversationPersist {
  Id: string;
  UserIds: string[];
  AnimalId: string;
}
