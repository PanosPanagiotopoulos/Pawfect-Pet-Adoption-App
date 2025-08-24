import { NotificationType } from '../common/enum/notification-type.enum';
import { Lookup } from './lookup';

export interface NotificationLookup extends Lookup {
  ids?: string[];
  userIds?: string[];
  notificationTypes?: NotificationType[]; 
  isRead?: boolean;
  createFrom?: Date;
  createdTill?: Date;
}
