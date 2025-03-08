import { VerificationStatus } from 'src/app/common/enum/verification-status';
import { Animal } from '../animal/animal.model';
import { User } from '../user/user.model';

// Shelter Models
export interface Shelter {
  Id?: string;
  User?: User;
  ShelterName?: string;
  Description?: string;
  Website?: string;
  SocialMedia?: SocialMedia;
  OperatingHours?: OperatingHours;
  VerificationStatus?: VerificationStatus;
  VerifiedBy?: string;
  Animals?: Animal[];
}

export interface ShelterPersist {
  Id: string;
  UserId: string;
  ShelterName: string;
  Description: string;
  Website: string;
  SocialMedia: SocialMedia;
  OperatingHours: OperatingHours;
  VerificationStatus: VerificationStatus;
  VerifiedBy?: string;
}

export interface OperatingHours {
  Monday: string;
  Tuesday: string;
  Wednesday: string;
  Thursday: string;
  Friday: string;
  Saturday: string;
  Sunday: string;
}

export interface SocialMedia {
  Facebook?: string;
  Instagram?: string;
}
