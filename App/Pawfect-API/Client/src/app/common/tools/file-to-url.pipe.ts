import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'fileToUrl',
  standalone: true
})
export class FileToUrlPipe implements PipeTransform {
  private urlCache = new Map<File, string>();

  transform(file: File | undefined | null): string {
    if (!file) {
      return '';
    }

    // Check if we already have a URL for this file
    if (this.urlCache.has(file)) {
      return this.urlCache.get(file)!;
    }

    // Create object URL for the file
    const url = URL.createObjectURL(file);
    this.urlCache.set(file, url);

    return url;
  }

  // Clean up method to revoke URLs when component is destroyed
  static revokeUrls(files: File[]): void {
    files.forEach(file => {
      const url = URL.createObjectURL(file);
      URL.revokeObjectURL(url);
    });
  }
}