import { Shelter } from '../shelter/shelter.model';

export interface User {
  Id?: string;
  Email?: string;
  FullName?: string;
  Role?: UserRole;
  Phone?: string;
  Location?: Location;
  Shelter?: Shelter;
  AuthProvider?: AuthProvider;
  AuthProviderId?: string;
  ProfilePhoto?: string;
  IsVerified?: boolean;
  HasPhoneVerified?: boolean;
  HasEmailVerified?: boolean;
  CreatedAt?: Date;
  UpdatedAt?: Date;
}

export interface UserPersist {
  Id: string;
  Email: string;
  Password: string;
  FullName: string;
  Role: UserRole;
  Phone: string;
  Location: Location;
  AuthProvider: AuthProvider;
  AuthProviderId?: string;
  AttachedPhoto?: File;
  HasPhoneVerified: boolean;
  HasEmailVerified: boolean;
}

// Shared Models
export interface Location {
  City?: string;
  ZipCode?: string;
  Address?: string;
  Number?: string;
}

// Enums
export enum UserRole {
  User = 1,
  Shelter = 2,
  Admin = 3,
}

export enum AuthProvider {
  Local = 1,
  Google = 2,
}
