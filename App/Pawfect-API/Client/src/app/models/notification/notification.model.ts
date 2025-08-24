import { NotificationType } from 'src/app/common/enum/notification-type.enum';
import { User } from '../user/user.model';

// Notification Models
export interface Notification {
  id?: string;
  user?: User;
  type?: NotificationType;
  // HTML Text
  title?: string;
  // HTML Text
  content?: string;
  isRead?: boolean;
  createdAt?: Date;
  readAt?: Date;
}