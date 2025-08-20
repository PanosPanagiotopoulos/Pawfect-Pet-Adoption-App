import { User } from '../user/user.model';

// Report Models
export interface Report {
  id?: string;
  reporter?: User;
  reported?: User;
  type?: ReportType;
  reason?: string;
  status?: ReportStatus;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface ReportPersist {
  id: string;
  reporterId: string;
  reportedId: string;
  type: ReportType;
  reason: string;
  status: ReportStatus;
}

// Enums
export enum ReportType {
  InappropriateMessage = 1,
  Other = 2,
}

export enum ReportStatus {
  Pending = 1,
  Resolved = 2,
}
