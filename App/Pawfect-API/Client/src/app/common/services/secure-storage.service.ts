import { Injectable } from '@angular/core';
import * as CryptoJS from 'crypto-js';
import { InstallationConfigurationService } from './installation-configuration.service';

@Injectable({
  providedIn: 'root'
})
export class SecureStorageService {
  constructor(private readonly installationConfiguration: InstallationConfigurationService) {}

  /**
   * Encrypts and stores data in session storage
   * @param key The key to store the data under
   * @param data The data to encrypt and store
   */
  setItem(key: string, data: any): void {
    try {
      const jsonString = JSON.stringify(data);
      const encryptedData = CryptoJS.AES.encrypt(jsonString, this.installationConfiguration.encryptKey).toString();
      sessionStorage.setItem(key, encryptedData);
    } catch (error) {
      console.error('Error encrypting and storing data:', error);
      throw new Error('Failed to store data securely');
    }
  }

  /**
   * Retrieves and decrypts data from session storage
   * @param key The key to retrieve the data from
   * @returns The decrypted data or null if not found
   */
  getItem<T>(key: string): T | null {
    try {
      const encryptedData = sessionStorage.getItem(key);
      if (!encryptedData) return null;

      const decryptedData = CryptoJS.AES.decrypt(encryptedData, this.installationConfiguration.encryptKey).toString(CryptoJS.enc.Utf8);
      return JSON.parse(decryptedData) as T;
    } catch (error) {
      console.error('Error decrypting data:', error);
      return null;
    }
  }

  /**
   * Removes encrypted data from session storage
   * @param key The key to remove
   */
  removeItem(key: string): void {
    try {
      sessionStorage.removeItem(key);
    } catch (error) {
      console.error('Error removing data:', error);
      throw new Error('Failed to remove data');
    }
  }

  /**
   * Clears all encrypted data from session storage
   */
  clear(): void {
    try {
      sessionStorage.clear();
    } catch (error) {
      console.error('Error clearing storage:', error);
      throw new Error('Failed to clear storage');
    }
  }

  /**
   * Checks if a key exists in session storage
   * @param key The key to check
   * @returns boolean indicating if the key exists
   */
  hasItem(key: string): boolean {
    return sessionStorage.getItem(key) !== null;
  }
} 