import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorDetails } from '../ui/error-message-banner.component';
import { TranslationService } from './translation.service';

@Injectable({
  providedIn: 'root',
})
export class ErrorHandlerService {
  constructor(private translate: TranslationService) {}

  handleError(error: any): ErrorDetails {
    if (error instanceof HttpErrorResponse) {
      return this.handleHttpError(error);
    }

    return {
      title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.GENERAL_ERROR'),
      message: this.translate.translate('APP.SERVICES.ERROR_HANDLER.GENERAL_ERROR_MESSAGE'),
      type: 'error',
    };
  }

  private handleHttpError(error: HttpErrorResponse): ErrorDetails {
    switch (error.status) {
      case 400:
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.INVALID_DATA'),
          message:
            error.error?.message ||
            this.translate.translate('APP.SERVICES.ERROR_HANDLER.INVALID_DATA_MESSAGE'),
          type: 'warning',
        };

      case 401:
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.UNAUTHORIZED_ACCESS'),
          message: this.translate.translate('APP.SERVICES.ERROR_HANDLER.UNAUTHORIZED_MESSAGE'),
          type: 'warning',
        };

      case 403:
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.FORBIDDEN_ACCESS'),
          message: this.translate.translate('APP.SERVICES.ERROR_HANDLER.FORBIDDEN_MESSAGE'),
          type: 'error',
        };

      case 404:
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.NOT_FOUND'),
          message: this.translate.translate('APP.SERVICES.ERROR_HANDLER.NOT_FOUND_MESSAGE'),
          type: 'warning',
        };

      case 409:
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.DATA_CONFLICT'),
          message:
            error.error?.message || this.translate.translate('APP.SERVICES.ERROR_HANDLER.DATA_CONFLICT_MESSAGE'),
          type: 'warning',
        };

      case 422:
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.INVALID_DATA_SUBMISSION'),
          message:
            error.error?.message ||
            this.translate.translate('APP.SERVICES.ERROR_HANDLER.INVALID_DATA_SUBMISSION_MESSAGE'),
          type: 'warning',
        };

      case 500:
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.SERVER_ERROR'),
          message: this.translate.translate('APP.SERVICES.ERROR_HANDLER.SERVER_ERROR_MESSAGE'),
          type: 'error',
        };

      case 503:
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.SERVICE_UNAVAILABLE'),
          message: this.translate.translate('APP.SERVICES.ERROR_HANDLER.SERVICE_UNAVAILABLE_MESSAGE'),
          type: 'error',
        };

      case 0:
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.CONNECTION_ERROR'),
          message: this.translate.translate('APP.SERVICES.ERROR_HANDLER.CONNECTION_ERROR_MESSAGE'),
          type: 'warning',
        };

      default:
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.GENERAL_ERROR'),
          message: this.translate.translate('APP.SERVICES.ERROR_HANDLER.GENERAL_ERROR_MESSAGE'),
          type: 'error',
        };
    }
  }

  handleAuthError(error: HttpErrorResponse): ErrorDetails {
    switch (error.status) {
      case 401:
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.LOGIN_ERROR'),
          message: this.translate.translate('APP.SERVICES.ERROR_HANDLER.LOGIN_ERROR_MESSAGE'),
          type: 'error',
        };

      case 403:
        if (error.error?.isEmailVerified === false) {
          return {
            title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.EMAIL_NOT_VERIFIED'),
            message: this.translate.translate('APP.SERVICES.ERROR_HANDLER.EMAIL_NOT_VERIFIED_MESSAGE'),
            type: 'warning',
          };
        }
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.ACCESS_DENIED'),
          message: this.translate.translate('APP.SERVICES.ERROR_HANDLER.ACCESS_DENIED_MESSAGE'),
          type: 'error'
        };

      case 404:
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.ACCOUNT_NOT_FOUND'),
          message: this.translate.translate('APP.SERVICES.ERROR_HANDLER.ACCOUNT_NOT_FOUND_MESSAGE'),
          type: 'error',
        };

      case 429:
        return {
          title: this.translate.translate('APP.SERVICES.ERROR_HANDLER.TOO_MANY_ATTEMPTS'),
          message: this.translate.translate('APP.SERVICES.ERROR_HANDLER.TOO_MANY_ATTEMPTS_MESSAGE'),
          type: 'warning',
        };

      default:
        return this.handleError(error);
    }
  }
}
