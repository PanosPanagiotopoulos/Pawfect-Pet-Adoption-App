import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ComponentType } from '@angular/cdk/portal';

@Injectable({
  providedIn: 'root'
})
export class SnackbarService {
  constructor(private snackBar: MatSnackBar) {}

  showError(message: string, duration: number = 5000): void {
    this.snackBar.open(message, 'Κλείσιμο', {
      duration,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: ['error-snackbar', 'animate__animated', 'animate__fadeInDown']
    });
  }

  showSuccess(message: string, duration: number = 3000): void {
    this.snackBar.open(message, 'Κλείσιμο', {
      duration,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: ['success-snackbar', 'animate__animated', 'animate__fadeInDown']
    });
  }

  showWarning(message: string, duration: number = 4000): void {
    this.snackBar.open(message, 'Κλείσιμο', {
      duration,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: ['warning-snackbar', 'animate__animated', 'animate__fadeInDown']
    });
  }

  showCustom(component: ComponentType<any>, duration: number = 3000): void {
    this.snackBar.openFromComponent(component, {
      duration,
      horizontalPosition: 'center',
      verticalPosition: 'top',
      panelClass: ['custom-snackbar', 'animate__animated', 'animate__fadeInDown']
    });
  }
} 