import { Injectable } from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { FormLeaveConfirmationDialogComponent } from '../ui/form-leave-confirmation-dialog.component';

export interface CanComponentDeactivate {
  canDeactivate(): Observable<boolean> | Promise<boolean> | boolean;
  hasUnsavedChanges?(): boolean;
}

@Injectable({
  providedIn: 'root',
})
export class FormGuard implements CanDeactivate<CanComponentDeactivate> {
  constructor(private dialog: MatDialog) {}

  canDeactivate(
    component: CanComponentDeactivate
  ): Observable<boolean> | Promise<boolean> | boolean {
    // If component doesn't have unsaved changes, allow navigation
    if (!component.hasUnsavedChanges || !component.hasUnsavedChanges()) {
      return true;
    }

    // If component has canDeactivate method, use it
    if (component.canDeactivate) {
      const result = component.canDeactivate();
      if (result instanceof Observable || result instanceof Promise) {
        return result;
      }
      if (result === true) {
        return true;
      }
    }

    // Show confirmation dialog
    return this.showConfirmationDialog();
  }

  private showConfirmationDialog(): Observable<boolean> {
    const dialogData = {
      title: 'APP.COMMONS.FORM_GUARD.TITLE',
      message: 'APP.COMMONS.FORM_GUARD.MESSAGE',
      confirmText: 'APP.COMMONS.FORM_GUARD.LEAVE',
      cancelText: 'APP.COMMONS.FORM_GUARD.STAY',
      icon: 'lucideCircleAlert',
      confirmButtonClass: 'btn-danger'
    };

    const dialogRef: MatDialogRef<FormLeaveConfirmationDialogComponent> = this.dialog.open(FormLeaveConfirmationDialogComponent, {
      panelClass: 'form-guard-panel',
      backdropClass: 'form-guard-backdrop',
      width: '28rem', // Matches max-w-md (448px)
      disableClose: false, // Allow closing on backdrop click
      autoFocus: false, // Prevent focus issues
      hasBackdrop: true, // Explicitly ensure backdrop is rendered
      data: dialogData
    });

    // Debug: Log to verify backdrop class
    setTimeout(() => {
      const backdrop = document.querySelector('.cdk-overlay-backdrop.form-guard-backdrop');
    }, 0);

    // Add no-scroll class to body
    document.body.classList.add('no-scroll');

    // Return afterClosed observable, remove no-scroll class as a side effect
    return dialogRef.afterClosed().pipe(
      tap(() => {
        document.body.classList.remove('no-scroll');
      })
    );
  }
}
