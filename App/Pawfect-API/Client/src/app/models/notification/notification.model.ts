import { User } from '../user/user.model';

// Notification Models
export interface Notification {
  id?: string;
  user?: User;
  type?: NotificationType;
  content?: string;
  isRead?: boolean;
  createdAt?: Date;
}

export interface NotificationPersist {
  id: string;
  userId: string;
  type: NotificationType;
  content: string;
  isRead: boolean;
}

// Enum
export enum NotificationType {
  IncomingMessage = 1,
  AdoptionApplication = 2,
  Report = 3,
}
