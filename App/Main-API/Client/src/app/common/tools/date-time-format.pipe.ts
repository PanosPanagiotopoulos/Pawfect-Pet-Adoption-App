import { DatePipe } from '@angular/common';
import { Pipe, PipeTransform } from '@angular/core';
import * as moment from 'moment';
import * as momentTimezone from 'moment-timezone';
import { TimezoneService } from '../services/time-zone.service';

@Pipe({
  name: 'dateTimeFormatter',
})
export class DateTimeFormatPipe implements PipeTransform {
  constructor(
    private datePipe: DatePipe,
    private timezoneService: TimezoneService
  ) {}

  transform(
    value: any,
    format?: string,
    timezone?: string,
    locale?: string
  ): string | null {
    // using timezone set in timezoneService by default. can be overwritten with pipe arguments
    const timezoneToUse = timezone
      ? timezone
      : momentTimezone(value)
          .tz(this.timezoneService.getCurrentTimezone())
          .format('Z');
    return this.datePipe.transform(value, format, timezoneToUse, locale);
  }
}

@Pipe({
  name: 'dataTableDateTimeFormatter',
})
// This is only used for the DataTable Column definition.
// It's a hacky way to apply format to the pipe because it only supports passing a pipe instance and calls transform in it without params.
export class DataTableDateTimeFormatPipe
  extends DateTimeFormatPipe
  implements PipeTransform
{
  format: string = '';
  timezone: string = '';

  constructor(
    private _datePipe: DatePipe,
    private _timezoneService: TimezoneService
  ) {
    super(_datePipe, _timezoneService);
  }

  public withTimezone(timezone: string): DataTableDateTimeFormatPipe {
    this.timezone = timezone;
    return this;
  }

  public withFormat(format: string): DataTableDateTimeFormatPipe {
    this.format = format;
    return this;
  }

  override transform(value: any): string | null {
    return super.transform(value, this.format, this.timezone);
  }
}
