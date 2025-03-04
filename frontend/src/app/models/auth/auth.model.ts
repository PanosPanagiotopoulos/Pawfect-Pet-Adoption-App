import { ShelterPersist } from '../shelter/shelter.model';
import { AuthProvider, UserPersist, UserRole } from '../user/user.model';

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
  token: string;
  role: UserRole;
  loggedAt: Date;
  isEmailVerified?: boolean;
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