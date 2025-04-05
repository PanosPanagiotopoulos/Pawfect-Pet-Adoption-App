import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LogService {
  separator = (key: string, value: any) => {
    if (value instanceof Error) {
      return {
        message: value.message,
        stack: value.stack,
        name: value.name
      };
    }
    return value ?? 'No value found to print';
  };

  logFormatted(log: any) {
    const timestamp = new Date().toISOString();
    const logWithTimestamp = {
      timestamp,
      ...log
    };

    const formatted: string = (typeof log).toLowerCase() === 'string' ? log : JSON.stringify(logWithTimestamp, this.separator, 2); 

    console.log(formatted);
  }
}