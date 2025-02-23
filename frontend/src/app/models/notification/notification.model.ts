import { User } from '../user/user.model';

// Notification Models
export interface Notification {
  Id?: string;
  User?: User;
  Type?: NotificationType;
  Content?: string;
  IsRead?: boolean;
  CreatedAt?: Date;
}

export interface NotificationPersist {
  Id: string;
  UserId: string;
  Type: NotificationType;
  Content: string;
  IsRead: boolean;
}

// Enum
export enum NotificationType {
  IncomingMessage = 1,
  AdoptionApplication = 2,
  Report = 3,
}
