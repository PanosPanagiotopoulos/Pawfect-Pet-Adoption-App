import { Shelter } from '../shelter/shelter.model';

export interface User {
  id?: string;
  email?: string;
  fullName?: string;
  role?: UserRole;
  phone?: string;
  location?: Location;
  shelter?: Shelter;
  authProvider?: AuthProvider;
  authProviderId?: string;
  profilePhoto?: string;
  isVerified?: boolean;
  hasPhoneVerified?: boolean;
  hasEmailVerified?: boolean;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface UserPersist {
  id: string;
  email: string;
  password: string;
  fullName: string;
  role: UserRole;
  phone: string;
  location: Location;
  authProvider: AuthProvider;
  authProviderId?: string;
  attachedPhoto?: File;
  hasPhoneVerified: boolean;
  hasEmailVerified: boolean;
}

// Shared Models
export interface Location {
  city?: string;
  zipCode?: string;
  address?: string;
  number?: string;
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
