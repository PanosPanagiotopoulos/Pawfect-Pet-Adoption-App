import { ReportStatus, ReportType } from '../models/report/report.model';
import { Lookup } from './lookup';

export interface ReportLookup extends Lookup {
  ids?: string[];
  reporterIds?: string[];
  reportedIds?: string[];
  reportTypes?: ReportType[];
  reportStatus?: ReportStatus[];
  createFrom?: Date;
  createdTill?: Date;
}
