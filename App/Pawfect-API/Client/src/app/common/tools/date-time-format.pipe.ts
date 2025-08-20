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
    if (!value) {
      return null;
    }

    // Convert value to Date if it's a string
    let dateValue: Date;
    if (typeof value === 'string') {
      dateValue = new Date(value);
    } else if (value instanceof Date) {
      dateValue = value;
    } else {
      return null;
    }

    // Check if date is valid
    if (isNaN(dateValue.getTime())) {
      return null;
    }

    // If no format is provided, use default
    if (!format) {
      format = 'medium';
    }

    // For our specific use case, handle the custom format directly
    if (format === 'dd/MM/yyyy : HH:mm') {
      const day = dateValue.getDate().toString().padStart(2, '0');
      const month = (dateValue.getMonth() + 1).toString().padStart(2, '0');
      const year = dateValue.getFullYear();
      const hours = dateValue.getHours().toString().padStart(2, '0');
      const minutes = dateValue.getMinutes().toString().padStart(2, '0');
      
      return `${day}/${month}/${year} : ${hours}:${minutes}`;
    }

    // For other formats, use Angular's DatePipe
    try {
      return this.datePipe.transform(value, format, timezone, locale);
    } catch (error) {
      console.error('DateTimeFormatPipe error:', error);
      return null;
    }
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
