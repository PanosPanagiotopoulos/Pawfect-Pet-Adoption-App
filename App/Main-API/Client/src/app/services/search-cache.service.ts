import { Injectable } from '@angular/core';
import { Shelter } from '../models/shelter/shelter.model';
import { SecureStorageService } from '../common/services/secure-storage.service';

interface CachedSearchResult {
  query: string;
  results: Shelter[];
  timestamp: number;
  expiresAt: number;
  wasClicked?: boolean; // Track if user clicked on a result from this search
}

@Injectable({
  providedIn: 'root',
})
export class SearchCacheService {
  private readonly CACHE_DURATION = 20 * 60 * 1000; // 20 minutes in milliseconds
  private readonly STORAGE_KEY = 'pawfect_search_cache';
  private cache: Map<string, CachedSearchResult> = new Map();

  constructor
  (
    private secureStorageService: SecureStorageService,
  ) {
    this.loadCacheFromStorage();
    this.startCleanupInterval();
  }

  /**
   * Get cached search results for a query
   */
  getCachedResults(query: string): Shelter[] | null {
    const normalizedQuery = this.normalizeQuery(query);
    const cached = this.cache.get(normalizedQuery);

    if (!cached) {
      return null;
    }

    // Check if cache has expired
    if (Date.now() > cached.expiresAt) {
      this.cache.delete(normalizedQuery);
      this.saveCacheToStorage();
      return null;
    }

    return cached.results;
  }

  /**
   * Cache search results for a query
   */
  cacheResults(query: string, results: Shelter[]): void {
    const normalizedQuery = this.normalizeQuery(query);
    const now = Date.now();

    const cachedResult: CachedSearchResult = {
      query: normalizedQuery,
      results: results,
      timestamp: now,
      expiresAt: now + this.CACHE_DURATION,
      wasClicked: false, // Initialize as not clicked
    };

    this.cache.set(normalizedQuery, cachedResult);
    this.saveCacheToStorage();
  }

  /**
   * Get all distinct cached queries that resulted in clicks (for recent searches feature)
   */
  getRecentQueries(limit: number = 5): string[] {
    const validEntries = Array.from(this.cache.values())
      .filter((entry) => Date.now() <= entry.expiresAt && entry.wasClicked === true)
      .sort((a, b) => b.timestamp - a.timestamp)
      .slice(0, limit);

    return validEntries.map((entry) => entry.query);
  }

  /**
   * Mark a search query as clicked (successful)
   */
  markQueryAsClicked(query: string): void {
    const normalizedQuery = this.normalizeQuery(query);
    const cached = this.cache.get(normalizedQuery);
    
    if (cached) {
      cached.wasClicked = true;
      cached.timestamp = Date.now(); // Update timestamp to make it most recent
      this.cache.set(normalizedQuery, cached);
      this.saveCacheToStorage();
    }
  }

  /**
   * Delete a specific recent search query
   */
  deleteRecentQuery(query: string): void {
    const normalizedQuery = this.normalizeQuery(query);
    this.cache.delete(normalizedQuery);
    this.saveCacheToStorage();
  }

  /**
   * Clear all cached results
   */
  clearCache(): void {
    this.cache.clear();
    this.saveCacheToStorage();
  }

  /**
   * Clear expired entries from cache
   */
  private cleanupExpiredEntries(): void {
    const now = Date.now();
    let hasChanges = false;

    for (const [key, value] of this.cache.entries()) {
      if (now > value.expiresAt) {
        this.cache.delete(key);
        hasChanges = true;
      }
    }

    if (hasChanges) {
      this.saveCacheToStorage();
    }
  }

  /**
   * Normalize query for consistent caching
   */
  private normalizeQuery(query: string): string {
    return query.trim().toLowerCase();
  }

  /**
   * Save cache 
   */
  private saveCacheToStorage(): void {
    try {
      const cacheArray = Array.from(this.cache.entries());
      this.secureStorageService.setItem(this.STORAGE_KEY, JSON.stringify(cacheArray));
    } catch (error) {
      console.warn('Failed to save search cache ', error);
    }
  }

  /**
   * Load cache 
   */
  private loadCacheFromStorage(): void {
    try {
      const stored: any = this.secureStorageService.getItem<any>(this.STORAGE_KEY);
      if (stored) {
        const cacheArray: [string, CachedSearchResult][] = JSON.parse(stored);
        this.cache = new Map(cacheArray);

        // Ensure backward compatibility - add wasClicked property to existing entries
        for (const [key, value] of this.cache.entries()) {
          if (value.wasClicked === undefined) {
            value.wasClicked = false;
            this.cache.set(key, value);
          }
        }

        // Clean up expired entries on load
        this.cleanupExpiredEntries();
      }
    } catch (error) {
      console.warn('Failed to load search cache ', error);
      this.cache = new Map();
    }
  }

  /**
   * Start periodic cleanup of expired entries
   */
  private startCleanupInterval(): void {
    // Clean up expired entries every 5 minutes
    setInterval(() => {
      this.cleanupExpiredEntries();
    }, 5 * 60 * 1000);
  }
}
