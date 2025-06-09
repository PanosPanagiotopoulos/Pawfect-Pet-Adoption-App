import { Shelter } from '../shelter/shelter.model';
import { File } from '../file/file.model';
import { AuthProvider } from 'src/app/common/enum/auth-provider.enum';
import { UserRole } from 'src/app/common/enum/user-role.enum';

export interface User {
  id?: string;
  email?: string;
  fullName?: string;
  roles?: UserRole[];
  phone?: string;
  location?: Location;
  shelter?: Shelter;
  authProvider?: AuthProvider;
  authProviderId?: string;
  profilePhoto?: File;
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
  profilePhotoId?: string;
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
