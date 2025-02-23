import { NotificationType } from '../models/notification/notification.model';
import { Lookup } from './lookup';

export interface NotificationLookup extends Lookup {
  ids?: string[];
  userIds?: string[];
  notificationTypes?: NotificationType[]; // e.g. IncomingMessage, AdoptionApplication, Report
  createFrom?: Date;
  createdTill?: Date;
}
