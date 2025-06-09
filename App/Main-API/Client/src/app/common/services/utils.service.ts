import { Injectable } from '@angular/core';
import { Animal } from '../../models/animal/animal.model';
import { LogService } from './log.service';

@Injectable({
  providedIn: 'root'
})
export class UtilsService {
  constructor(private logService: LogService) {}

  // Combine arrays while removing duplicates based on id
  combineDistinct<T extends { id?: string }>(arr1: T[], arr2: T[]): T[] {
    const combined = [...arr1]; 
    arr2.forEach(item => {
      if (!combined.some(existingItem => existingItem.id! === item.id!)) {
        combined.push(item); 
      }
    });
    return combined;
  }

  // Try to load each image URL until one succeeds or fall back to placeholder
  async tryLoadImages(animal: Animal): Promise<string> {
    if (!animal.attachedPhotos || animal.attachedPhotos.length === 0) {
      return '/assets/placeholder.jpg';
    }

    for (const photoUrl of animal.attachedPhotos.map(photo => photo.sourceUrl)) {
      try {
        await this.loadImage(photoUrl!);
        return photoUrl!;
      } catch (error) {
        continue;
      }
    }

    // If all photos fail, return placeholder
    return '/assets/placeholder.jpg';
  }

  private loadImage(url: string): Promise<void> {
    return new Promise((resolve, reject) => {
      const img = new Image();
      img.onload = () => resolve();
      img.onerror = () => reject();
      img.src = url;
    });
  }
}