import { User } from "../user/user.model";

export interface File {
    id?: string;
    filename?: string;
    mimeType?: string;
    fileType?: string;
    owner?: User;
    size?: number;
    sourceUrl?: string;
    createdAt?: Date;
    updatedAt?: Date;
}

export interface FilePersist {
    id: string;
    fileName: string;
    mimeType: string;
    fileType: string;
    ownerId: string;
    size: number;
    sourceUrl: string;
}

export interface FileItem {
  file: globalThis.File;
  addedAt: number;
  persistedId?: string;
  isPersisting: boolean;
  uploadFailed: boolean;
  isExisting?: boolean; 
  sourceUrl?: string; 
}