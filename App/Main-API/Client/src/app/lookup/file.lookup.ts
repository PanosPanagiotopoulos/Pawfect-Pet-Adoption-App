export interface FileLookup {
  ids?: string[];
  excludedIds?: string[];
  ownerIds?: string[];
  fileSaveStatuses?: FileSaveStatus[];
  createdFrom?: Date;
  createdTill?: Date;
  pageSize?: number;
  offset?: number;
  sortDescending?: boolean;
  sortBy?: string;
  fields?: string[];
  query?: string;
}

export enum FileSaveStatus {
  Temporary = 1,
  Permanent = 2,
}