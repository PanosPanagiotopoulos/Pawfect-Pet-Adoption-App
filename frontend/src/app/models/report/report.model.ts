import { User } from '../user/user.model';

// Report Models
export interface Report {
  Id?: string;
  Reporter?: User;
  Reported?: User;
  Type?: ReportType;
  Reason?: string;
  Status?: ReportStatus;
  CreatedAt?: Date;
  UpdatedAt?: Date;
}

export interface ReportPersist {
  Id: string;
  ReporterId: string;
  ReportedId: string;
  Type: ReportType;
  Reason: string;
  Status: ReportStatus;
}

// Enums
export enum ReportType {
  InAppropriateMessage = 1,
  Other = 2,
}

export enum ReportStatus {
  Pending = 1,
  Resolved = 2,
}
