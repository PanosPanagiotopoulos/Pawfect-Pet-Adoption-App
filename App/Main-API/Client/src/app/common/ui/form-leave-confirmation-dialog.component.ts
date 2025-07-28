import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

export interface ConfirmationDialogData {
  title: string;
  message: string;
  confirmText: string;
  cancelText: string;
  icon?: string;
  confirmButtonClass?: string;
}

@Component({
  selector: 'app-form-leave-confirmation-dialog',
  templateUrl: './form-leave-confirmation-dialog.component.html',
  styleUrls: ['./form-leave-confirmation-dialog.component.scss']
})
export class FormLeaveConfirmationDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<FormLeaveConfirmationDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmationDialogData
  ) {}

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}