import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { catchError, map, switchMap } from 'rxjs/operators';
import { SecureStorageService } from './secure-storage.service';

export interface LanguageOption {
  code: string;
  label: string;
  flag?: string; // optional, for future use
}

export type SupportedLanguage = string;

@Injectable({ providedIn: 'root' })
export class TranslationService {
  private readonly LANG_STORAGE_KEY = 'pawfect-language';
  public readonly supportedLanguages: LanguageOption[] = [
    { code: 'en', label: 'English', flag: '🇬🇧' },
    { code: 'gr', label: 'Ελληνικά', flag: '🇬🇷' }
  ];
  private language$ = new BehaviorSubject<SupportedLanguage>(this.getInitialLanguage());
  private translations: Record<string, any> = {};
  private isInitialized = false;

  constructor(private http: HttpClient, private secureStorageService: SecureStorageService) {
    this.language$.subscribe(lang => this.loadTranslations(lang));
  }

  private getInitialLanguage(): SupportedLanguage {
    const stored = this.secureStorageService.getItem(this.LANG_STORAGE_KEY);
    const found = this.supportedLanguages.find(l => l.code === stored);
    return found ? found.code : this.supportedLanguages[0].code;
  }

  // Initialize translations at app startup
  initialize(): Observable<boolean> {
    if (this.isInitialized) {
      return of(true);
    }

    const currentLang = this.getLanguage();
    const path = `assets/${currentLang}.json`;
    
    console.log(`[TranslationService] Initializing with language: ${currentLang}, path: ${path}`);
    
    return this.http.get<Record<string, any>>(path).pipe(
      map(translations => {
        this.translations = translations;
        this.isInitialized = true;
        console.log(`[TranslationService] Initialized successfully with ${Object.keys(translations).length} translation keys`);
        return true;
      }),
      catchError((error) => {
        console.error(`[TranslationService] Failed to initialize:`, error);
        this.translations = {};
        this.isInitialized = true;
        return of(false);
      })
    );
  }

  setLanguage(lang: SupportedLanguage): void {
    if (lang !== this.language$.value) {
      this.secureStorageService.setItem(this.LANG_STORAGE_KEY, lang);
      this.language$.next(lang);
    }
  }

  getLanguage(): SupportedLanguage {
    return this.language$.value;
  }

  getLanguage$(): Observable<SupportedLanguage> {
    return this.language$.asObservable();
  }

  private loadTranslations(lang: SupportedLanguage): void {
    const path = `assets/${lang}.json`;
    this.http.get<Record<string, any>>(path).pipe(
      catchError(() => of({}))
    ).subscribe(translations => {
      this.translations = translations;
    });
  }

  translate(key: string): string {
    if (!key) return '';
    
    // If not initialized yet, return the key
    if (!this.isInitialized) {
      console.warn(`[TranslationService] Not initialized yet, returning key: ${key}`);
      return key;
    }
    
    const parts = key.split('.');
    let value: any = this.translations;
    for (const part of parts) {
      if (value && typeof value === 'object' && part in value) {
        value = value[part];
      } else {
        console.warn(`[TranslationService] Key not found: ${key}`);
        return key; // fallback to key if not found
      }
    }
    return typeof value === 'string' ? value : key;
  }

  // Check if translations are loaded
  isReady(): boolean {
    return this.isInitialized && Object.keys(this.translations).length > 0;
  }

  // Get initialization status for debugging
  getStatus(): { isInitialized: boolean; translationCount: number; currentLanguage: string } {
    return {
      isInitialized: this.isInitialized,
      translationCount: Object.keys(this.translations).length,
      currentLanguage: this.getLanguage()
    };
  }
} 