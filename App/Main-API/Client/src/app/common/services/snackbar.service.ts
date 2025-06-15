import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { ComponentType } from '@angular/cdk/portal';
import { ErrorSnackbarComponent, ErrorSnackbarData } from '../ui/error-snackbar.component';
import { WarningSnackbarComponent, WarningSnackbarData } from '../ui/warning-snackbar.component';
import { SuccessSnackbarComponent, SuccessSnackbarData } from '../ui/success-snackbar.component';

@Injectable({
  providedIn: 'root'
})
export class SnackbarService {
  private readonly defaultConfig = {
    horizontalPosition: 'right' as const,
    verticalPosition: 'top' as const,
    panelClass: ['animate__animated', 'animate__fadeInDown'],
    politeness: 'assertive' as const,
    announcementMessage: '',
    duration: 5000,
    viewContainerRef: undefined // This will use the root view container
  };

  constructor(private snackBar: MatSnackBar) {}

  private openSnackbar<T>(component: ComponentType<T>, data: any, duration: number, panelClass: string): void {
    // Dismiss any existing snackbar
    this.snackBar.dismiss();

    // Open new snackbar
    this.snackBar.openFromComponent(component, {
      ...this.defaultConfig,
      duration,
      data,
      panelClass: [...this.defaultConfig.panelClass, panelClass]
    });
  }

  showError(data: ErrorSnackbarData, duration: number = 5000): void {
    this.openSnackbar(ErrorSnackbarComponent, data, duration, 'error-snackbar');
  }

  showSuccess(data: SuccessSnackbarData, duration: number = 3000): void {
    this.openSnackbar(SuccessSnackbarComponent, data, duration, 'success-snackbar');
  }

  showWarning(data: WarningSnackbarData, duration: number = 4000): void {
    this.openSnackbar(WarningSnackbarComponent, data, duration, 'warning-snackbar');
  }

  showCustomComponent(component: ComponentType<any>, duration: number = 3000): void {
    this.openSnackbar(component, {}, duration, 'custom-snackbar');
  }

  dismiss(): void {
    this.snackBar.dismiss();
  }
}