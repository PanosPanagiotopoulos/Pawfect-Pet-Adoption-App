import { Injectable, ApplicationRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, Subject } from 'rxjs';
import { catchError, map, switchMap, takeUntil } from 'rxjs/operators';
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
  public readonly languageChanged$ = this.language$.asObservable();
  private translations: Record<string, any> = {};
  private isInitialized = false;
  private translationsLoadedSubject = new BehaviorSubject<boolean>(false);
  public translationsLoaded$ = this.translationsLoadedSubject.asObservable();
  private destroy$ = new Subject<void>();
  private currentLanguage: SupportedLanguage = this.language$.value;

  constructor(
    private http: HttpClient, 
    private appRef: ApplicationRef,
    private secureStorageService: SecureStorageService
  ) {
    // Use switchMap to cancel previous requests and only use the latest
    this.language$
      .pipe(
        switchMap(lang => {
          const path = `assets/${lang}.json`;
          return this.http.get<Record<string, any>>(path).pipe(
            catchError(() => of({})),
            map(translations => ({ lang, translations }))
          );
        }),
        takeUntil(this.destroy$)
      )
      .subscribe(({ lang, translations }) => {
        this.translations = translations;
        this.isInitialized = true;
        this.currentLanguage = lang;
        this.translationsLoadedSubject.next(true);
        this.appRef.tick();
      });
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
    
    
    return this.http.get<Record<string, any>>(path).pipe(
      map(translations => {
        this.translations = translations;
        this.isInitialized = true;
        return true;
      }),
      catchError((error) => {
        this.translations = {};
        this.isInitialized = true;
        return of(false);
      })
    );
  }

  setLanguage(lang: SupportedLanguage): void {
    if (lang !== this.language$.value) {
      this.secureStorageService.setItem(this.LANG_STORAGE_KEY, lang);
      this.translationsLoadedSubject.next(false);
      this.language$.next(lang);
    }
  }

  getLanguage(): SupportedLanguage {
    return this.currentLanguage;
  }

  getLanguage$(): Observable<SupportedLanguage> {
    return this.language$.asObservable();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private loadTranslations(lang: SupportedLanguage): void {
    const path = `assets/${lang}.json`;
    this.http.get<Record<string, any>>(path).pipe(
      catchError(() => of({}))
    ).subscribe(translations => {
      this.translations = translations;
      this.isInitialized = true; // Ensure translate() works after language change
      this.translationsLoadedSubject.next(true);
      this.appRef.tick(); // Force global change detection
    });
  }

  translate(key: string): string {
    if (!key) return '';
    
    // If not initialized yet, return the key
    if (!this.isInitialized) {
      return key;
    }
    
    const parts = key.split('.');
    let value: any = this.translations;
    for (const part of parts) {
      if (value && typeof value === 'object' && part in value) {
        value = value[part];
      } else {
        return key; // fallback to key if not found
      }
    }
    return typeof value === 'string' ? value : key;
  }

  /**
   * Synchronous translation for imperative use (no logging, no async, just returns the value or key)
   */
  instant(key: string): string {
    if (!key) return '';
    const parts = key.split('.');
    let value: any = this.translations;
    for (const part of parts) {
      if (value && typeof value === 'object' && part in value) {
        value = value[part];
      } else {
        return key;
      }
    }
    return typeof value === 'string' ? value : key;
  }

  /**
   * Returns an observable that emits the translation for the given key and updates reactively on language changes.
   * Usage: this.translationService.instant$(key).subscribe(...)
   */
  instant$(key: string): Observable<string> {
    if (!key) return of('');
    return this.languageChanged$.pipe(
      switchMap(() => {
        // Always emit the latest translation for the key
        return of(this.instant(key));
      })
    );
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