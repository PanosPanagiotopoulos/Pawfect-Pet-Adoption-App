import { VerificationStatus } from '../common/enum/verification-status';
import { Lookup } from './lookup';

export interface ShelterLookup extends Lookup {
  ids?: string[];
  userIds?: string[];
  verificationStatuses?: VerificationStatus[];
  verifiedBy?: string[];
}
