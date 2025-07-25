import { UserRole } from '../common/enum/user-role.enum';
import { Lookup } from './lookup';

export interface UserLookup extends Lookup {
  ids?: string[];
  fullNames?: string[];
  roles?: UserRole[];
  cities?: string[];
  zipcodes?: string[];
  createdFrom?: Date;
  createdTill?: Date;
}
