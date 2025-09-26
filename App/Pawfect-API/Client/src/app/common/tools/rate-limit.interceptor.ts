import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { SnackbarService } from 'src/app/common/services/snackbar.service';
import { SecureStorageService } from '../services/secure-storage.service';

@Injectable()
export class RateLimitInterceptor implements HttpInterceptor {
  private readonly LANG_STORAGE_KEY = 'pawfect-language';

  private readonly fallbackMessages = {
    tooManyAttempts: {
      en: 'Too Many Attempts',
      gr: 'Πάρα Πολλές Προσπάθειες',
    },
    tooManyAttemptsMessage: {
      en: 'You have made too many attempts. Please try again later.',
      gr: 'Έχετε κάνει υπερβολικά πολλές προσπάθειες. Παρακαλούμε δοκιμάστε ξανά αργότερα.',
    },
  };
  
  constructor(
    private snackbarService: SnackbarService,
    private secureStorageService: SecureStorageService
  ) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 429) {
          this.snackbarService.showWarning({
            message: this.getFallbackMessage('tooManyAttempts'),
            subMessage: this.getFallbackMessage('tooManyAttemptsMessage')
          });
        }
        return throwError(() => error);
      })
    );
  }

  private getFallbackMessage(
    messageType: 'tooManyAttempts' | 'tooManyAttemptsMessage'
  ): string {
    const currentLang = this.getCurrentLanguage();
    return (
      this.fallbackMessages[messageType][currentLang] ||
      this.fallbackMessages[messageType].gr
    );
  }

  private getCurrentLanguage(): 'en' | 'gr' {
    const stored = this.secureStorageService.getItem<string>(
      this.LANG_STORAGE_KEY
    );
    return stored === 'en' || stored === 'gr' ? stored : 'en';
  }
}