import { Injectable } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ErrorDetails } from '../ui/error-message-banner.component';

@Injectable({
  providedIn: 'root',
})
export class ErrorHandlerService {
  handleError(error: any): ErrorDetails {
    if (error instanceof HttpErrorResponse) {
      return this.handleHttpError(error);
    }

    return {
      title: 'Σφάλμα',
      message: 'Παρουσιάστηκε ένα απρόσμενο σφάλμα. Παρακαλώ δοκιμάστε ξανά.',
      type: 'error',
    };
  }

  private handleHttpError(error: HttpErrorResponse): ErrorDetails {
    switch (error.status) {
      case 400:
        return {
          title: 'Μη έγκυρα δεδομένα',
          message:
            error.error?.message ||
            'Παρακαλώ ελέγξτε τα στοιχεία σας και δοκιμάστε ξανά.',
          type: 'warning',
        };

      case 401:
        return {
          title: 'Μη εξουσιοδοτημένη πρόσβαση',
          message: 'Παρακαλώ συνδεθείτε για να συνεχίσετε.',
          type: 'warning',
        };

      case 403:
        return {
          title: 'Απαγορευμένη πρόσβαση',
          message: 'Δεν έχετε δικαίωμα πρόσβασης σε αυτό το περιεχόμενο.',
          type: 'error',
        };

      case 404:
        return {
          title: 'Δεν βρέθηκε',
          message: 'Το περιεχόμενο που αναζητάτε δεν είναι διαθέσιμο.',
          type: 'warning',
        };

      case 409:
        return {
          title: 'Διένεξη δεδομένων',
          message:
            error.error?.message || 'Υπάρχει ήδη εγγραφή με αυτά τα στοιχεία.',
          type: 'warning',
        };

      case 422:
        return {
          title: 'Μη έγκυρα δεδομένα',
          message:
            error.error?.message ||
            'Τα δεδομένα που υποβάλατε δεν είναι έγκυρα.',
          type: 'warning',
        };

      case 500:
        return {
          title: 'Σφάλμα διακομιστή',
          message:
            'Παρουσιάστηκε ένα εσωτερικό σφάλμα. Παρακαλώ δοκιμάστε ξανά αργότερα.',
          type: 'error',
        };

      case 503:
        return {
          title: 'Υπηρεσία μη διαθέσιμη',
          message:
            'Η υπηρεσία δεν είναι προσωρινά διαθέσιμη. Παρακαλώ δοκιμάστε ξανά αργότερα.',
          type: 'error',
        };

      case 0:
        return {
          title: 'Σφάλμα σύνδεσης',
          message:
            'Δεν ήταν δυνατή η σύνδεση με τον διακομιστή. Παρακαλώ ελέγξτε τη σύνδεσή σας.',
          type: 'warning',
        };

      default:
        return {
          title: 'Σφάλμα',
          message:
            'Παρουσιάστηκε ένα απρόσμενο σφάλμα. Παρακαλώ δοκιμάστε ξανά.',
          type: 'error',
        };
    }
  }

  handleAuthError(error: HttpErrorResponse): ErrorDetails {
    switch (error.status) {
      case 401:
        return {
          title: 'Σφάλμα σύνδεσης',
          message: 'Λάθος email ή κωδικός πρόσβασης.',
          type: 'error',
        };

      case 403:
        if (error.error?.isEmailVerified === false) {
          return {
            title: 'Μη επιβεβαιωμένο email',
            message: 'Παρακαλώ επιβεβαιώστε το email σας για να συνεχίσετε.',
            type: 'warning',
          };
        }
        return {
          title: 'Απαγορευμένη πρόσβαση',
          message: 'Ο λογαριασμός σας έχει απενεργοποιηθεί.',
          type: 'error',
        };

      case 404:
        return {
          title: 'Λογαριασμός δεν βρέθηκε',
          message: 'Δεν υπάρχει λογαριασμός με αυτό το email.',
          type: 'error',
        };

      case 429:
        return {
          title: 'Πολλές προσπάθειες',
          message:
            'Έχετε κάνει πολλές προσπάθειες. Παρακαλώ δοκιμάστε ξανά αργότερα.',
          type: 'warning',
        };

      default:
        return this.handleError(error);
    }
  }
}
