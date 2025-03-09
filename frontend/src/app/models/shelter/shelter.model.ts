import { VerificationStatus } from 'src/app/common/enum/verification-status';
import { Animal } from '../animal/animal.model';
import { User } from '../user/user.model';

// Shelter Models
export interface Shelter {
  id?: string;
  user?: User;
  shelterName?: string;
  description?: string;
  website?: string;
  socialMedia?: SocialMedia;
  operatingHoours?: OperatingHours;
  verificationStatus?: VerificationStatus;
  verifiedBy?: string;
  animals?: Animal[];
}

export interface ShelterPersist {
  id: string;
  userId: string;
  shelterName: string;
  description: string;
  website: string;
  socialMedia: SocialMedia;
  operatingHours: OperatingHours;
  verificationStatus: VerificationStatus;
  verifiedBy?: string;
}

export interface OperatingHours {
  monday: string;
  tuesday: string;
  wednesday: string;
  thursday: string;
  friday: string;
  saturday: string;
  sunday: string;
}

export interface SocialMedia {
  facebook?: string;
  instagram?: string;
}
