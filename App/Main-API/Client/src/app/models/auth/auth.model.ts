import { UserRole } from 'src/app/common/enum/user-role.enum';
import { ShelterPersist } from '../shelter/shelter.model';
import { UserPersist } from '../user/user.model';
import { AuthProvider } from 'src/app/common/enum/auth-provider.enum';

export interface EmailPayload {
  id?: string;
  email: string;
  token?: string;
}

export interface OtpPayload {
  id?: string;
  email?: string;
  phone: string;
  otp?: number;
}

export interface LoggedAccount {
  userId: string;
  shelterId?: string;
  email: string;
  phone: string;
  roles: string[];
  permissions: string[];
  isPhoneVerified: boolean;
  isEmailVerified: boolean;
  isVerified: boolean;
  loggedAt: Date;
}

export interface LoginPayload {
  email?: string;
  password?: string;
  providerAccessCode?: string;
  loginProvider: AuthProvider;
}

export interface RegisterPayload {
  user: UserPersist;
  shelter?: ShelterPersist;
}

export interface ResetPasswordPayload {
  id?: string;
  email?: string;
  token?: string;
  newPassword?: string;
}
