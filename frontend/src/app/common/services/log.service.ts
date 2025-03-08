import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class LogService {
  logFormatted(log: any) {
    const separator = (key: string, value: any) => {
      return value ?? 'No value found to print';
    };

    console.log(JSON.stringify(log, separator, 2));
  }
}
