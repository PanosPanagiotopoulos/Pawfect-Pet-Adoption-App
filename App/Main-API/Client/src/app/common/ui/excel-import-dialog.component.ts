import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { NgIconsModule } from '@ng-icons/core';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { LocalFileDropAreaComponent } from './local-file-drop-area.component';
import { AnimalService } from 'src/app/services/animal.service';
import { AnimalPersist } from 'src/app/models/animal/animal.model';
import { ErrorHandlerService } from 'src/app/common/services/error-handler.service';
import { TranslationService } from 'src/app/common/services/translation.service';

@Component({
  selector: 'app-excel-import-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    NgIconsModule,
    TranslatePipe,
    LocalFileDropAreaComponent,
  ],
  template: `
    <div class="excel-import-dialog max-h-[90vh] flex flex-col" (click)="clearErrorOnInteraction()">
      <div class="dialog-header flex-shrink-0">
        <div class="flex items-center space-x-3">
          <div class="icon-container">
            <ng-icon
              name="lucideSheet"
              [size]="'28'"
              class="text-green-600"
            ></ng-icon>
          </div>
          <div>
            <h2 class="dialog-title">
              {{ 'APP.ANIMALS.EXCEL_IMPORT.TITLE' | translate }}
            </h2>
            <p class="dialog-subtitle">
              {{ 'APP.ANIMALS.EXCEL_IMPORT.SUBTITLE' | translate }}
            </p>
          </div>
        </div>
      </div>

      <div class="dialog-content overflow-y-auto flex-1">
        <!-- Instructions -->
        <div class="instructions-section">
          <h3 class="instructions-title">
            {{ 'APP.ANIMALS.EXCEL_IMPORT.INSTRUCTIONS_TITLE' | translate }}
          </h3>
          <ol class="instructions-list">
            <li>{{ 'APP.ANIMALS.EXCEL_IMPORT.STEP_1' | translate }}</li>
            <li>{{ 'APP.ANIMALS.EXCEL_IMPORT.STEP_2' | translate }}</li>
            <li>{{ 'APP.ANIMALS.EXCEL_IMPORT.STEP_3' | translate }}</li>
          </ol>
        </div>

        <!-- Download Template Button -->
        <div class="template-section">
          <button
            type="button"
            (click)="downloadTemplate()"
            [disabled]="isDownloading"
            class="download-btn"
          >
            <ng-icon
              [name]="isDownloading ? 'lucideLoader' : 'lucideDownload'"
              [size]="'20'"
              [class]="isDownloading ? 'animate-spin' : ''"
            ></ng-icon>
            <span>
              {{
                isDownloading
                  ? ('APP.ANIMALS.EXCEL_IMPORT.DOWNLOADING' | translate)
                  : ('APP.ANIMALS.EXCEL_IMPORT.DOWNLOAD_TEMPLATE' | translate)
              }}
            </span>
          </button>
        </div>

        <!-- File Upload Form -->
        <form [formGroup]="importForm" class="upload-form">
          <app-local-file-drop-area
            [form]="importForm"
            controlName="excelFile"
            [label]="'APP.ANIMALS.EXCEL_IMPORT.UPLOAD_LABEL' | translate"
            [hint]="'APP.ANIMALS.EXCEL_IMPORT.UPLOAD_HINT' | translate"
            accept=".xlsx,.xls"
            [multiple]="false"
            [maxFileSize]="5 * 1024 * 1024"
            (filesChange)="onFileSelected($event)"
          ></app-local-file-drop-area>
        </form>

        <!-- Error Message -->
        <div *ngIf="errorMessage" class="error-message">
          <ng-icon
            name="lucideCircleAlert"
            [size]="'20'"
            class="text-red-500"
          ></ng-icon>
          <span>{{ errorMessage }}</span>
        </div>
      </div>

      <!-- Dialog Actions -->
      <div class="dialog-actions flex-shrink-0">
        <button
          type="button"
          (click)="onCancel()"
          class="btn-secondary"
          [disabled]="isImporting"
        >
          {{ 'APP.COMMONS.CANCEL' | translate }}
        </button>
        <button
          type="button"
          (click)="onImport()"
          [disabled]="!canImport || isImporting"
          class="btn-primary"
        >
          <ng-icon
            [name]="isImporting ? 'lucideLoader' : 'lucideUpload'"
            [size]="'20'"
            [class]="isImporting ? 'animate-spin' : ''"
          ></ng-icon>
          <span>
            {{
              isImporting
                ? ('APP.ANIMALS.EXCEL_IMPORT.IMPORTING' | translate)
                : ('APP.ANIMALS.EXCEL_IMPORT.IMPORT' | translate)
            }}
          </span>
        </button>
      </div>
    </div>
  `,
  styles: [
    `
      .excel-import-dialog {
        width: 100%;
        max-width: 600px;
        background: linear-gradient(
          135deg,
          rgba(31, 41, 55, 0.95) 0%,
          rgba(17, 24, 39, 0.95) 100%
        );
        backdrop-filter: blur(16px);
        -webkit-backdrop-filter: blur(16px);
        border-radius: 1rem;
        border: 1px solid rgba(255, 255, 255, 0.1);
        box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
        overflow: hidden;
        animation: slideInScale 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
        min-height: 0;
      }

      .dialog-header {
        padding: 2rem 2rem 1rem 2rem;
        border-bottom: 1px solid rgba(255, 255, 255, 0.1);
        position: relative;
      }

      .dialog-header::before {
        content: '';
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        height: 4px;
        background: linear-gradient(
          90deg,
          rgb(124, 58, 237),
          rgb(219, 39, 119),
          rgb(79, 70, 229)
        );
      }

      .icon-container {
        flex-shrink: 0;
        width: 3rem;
        height: 3rem;
        border-radius: 50%;
        background: linear-gradient(
          135deg,
          rgba(34, 197, 94, 0.2),
          rgba(34, 197, 94, 0.3)
        );
        display: flex;
        align-items: center;
        justify-content: center;
        box-shadow: 0 4px 12px rgba(34, 197, 94, 0.3);
        border: 1px solid rgba(34, 197, 94, 0.3);
      }

      .dialog-title {
        font-size: 1.25rem;
        font-weight: 600;
        color: #ffffff;
        margin: 0 0 0.25rem 0;
        line-height: 1.4;
      }

      .dialog-subtitle {
        font-size: 0.875rem;
        color: #d1d5db;
        margin: 0;
        line-height: 1.4;
      }

      .dialog-content {
        padding: 1.5rem 2rem;
        space-y: 1.5rem;
        min-height: 0;
      }

      .instructions-section {
        margin-bottom: 1.5rem;
      }

      .instructions-title {
        font-size: 1rem;
        font-weight: 600;
        color: #f3f4f6;
        margin: 0 0 0.75rem 0;
      }

      .instructions-list {
        list-style: decimal;
        padding-left: 1.25rem;
        space-y: 0.5rem;
      }

      .instructions-list li {
        font-size: 0.875rem;
        color: #d1d5db;
        line-height: 1.5;
        margin-bottom: 0.5rem;
      }

      .template-section {
        margin-bottom: 1.5rem;
      }

      .download-btn {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.75rem 1.5rem;
        background: linear-gradient(135deg, #22c55e, #16a34a);
        color: white;
        border: none;
        border-radius: 0.5rem;
        font-size: 0.875rem;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
        box-shadow: 0 2px 8px rgba(34, 197, 94, 0.3);
      }

      .download-btn:hover:not(:disabled) {
        background: linear-gradient(135deg, #16a34a, #15803d);
        transform: translateY(-1px);
        box-shadow: 0 6px 16px rgba(34, 197, 94, 0.4);
      }

      .download-btn:disabled {
        opacity: 0.7;
        cursor: not-allowed;
        transform: none;
      }

      .upload-form {
        margin-bottom: 1.5rem;
      }

      .error-message {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.75rem 1rem;
        background: rgba(239, 68, 68, 0.2);
        border: 1px solid rgba(239, 68, 68, 0.3);
        border-radius: 0.5rem;
        font-size: 0.875rem;
        color: #fca5a5;
        margin-bottom: 1.5rem;
      }

      .dialog-actions {
        display: flex;
        justify-content: flex-end;
        gap: 0.75rem;
        padding: 1.5rem 2rem 2rem 2rem;
        border-top: 1px solid rgba(255, 255, 255, 0.1);
      }

      .btn-secondary,
      .btn-primary {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.75rem 1.5rem;
        border-radius: 0.5rem;
        font-size: 0.875rem;
        font-weight: 500;
        border: none;
        cursor: pointer;
        transition: all 0.2s cubic-bezier(0.4, 0, 0.2, 1);
        min-width: 100px;
        justify-content: center;
      }

      .btn-secondary {
        background: rgba(255, 255, 255, 0.1);
        color: #d1d5db;
        border: 1px solid rgba(255, 255, 255, 0.2);
      }

      .btn-secondary:hover:not(:disabled) {
        background: rgba(255, 255, 255, 0.15);
        border-color: rgba(255, 255, 255, 0.3);
        transform: translateY(-1px);
        box-shadow: 0 4px 12px rgba(0, 0, 0, 0.2);
      }

      .btn-primary {
        background: linear-gradient(
          135deg,
          rgb(124, 58, 237),
          rgb(79, 70, 229)
        );
        color: white;
        box-shadow: 0 2px 8px rgba(124, 58, 237, 0.3);
      }

      .btn-primary:hover:not(:disabled) {
        background: linear-gradient(
          135deg,
          rgb(109, 40, 217),
          rgb(67, 56, 202)
        );
        transform: translateY(-1px);
        box-shadow: 0 6px 16px rgba(124, 58, 237, 0.4);
      }

      .btn-secondary:disabled,
      .btn-primary:disabled {
        opacity: 0.7;
        cursor: not-allowed;
        transform: none;
      }

      @keyframes slideInScale {
        0% {
          opacity: 0;
          transform: scale(0.9) translateY(-10px);
        }
        100% {
          opacity: 1;
          transform: scale(1) translateY(0);
        }
      }

      /* Custom scrollbar styling */
      .dialog-content::-webkit-scrollbar {
        width: 6px;
      }

      .dialog-content::-webkit-scrollbar-track {
        background: rgba(31, 41, 55, 0.3);
        border-radius: 3px;
      }

      .dialog-content::-webkit-scrollbar-thumb {
        background: rgba(34, 197, 94, 0.5);
        border-radius: 3px;
      }

      .dialog-content::-webkit-scrollbar-thumb:hover {
        background: rgba(34, 197, 94, 0.7);
      }

      @media (max-width: 640px) {
        .excel-import-dialog {
          max-width: calc(100vw - 2rem);
          margin: 0 1rem;
        }

        .dialog-header {
          padding: 1.5rem 1.5rem 1rem 1.5rem;
        }

        .dialog-content {
          padding: 1rem 1.5rem;
        }

        .dialog-actions {
          padding: 1rem 1.5rem 1.5rem 1.5rem;
          flex-direction: column;
          gap: 0.5rem;
        }

        .btn-secondary,
        .btn-primary {
          width: 100%;
        }
      }
    `,
  ],
})
export class ExcelImportDialogComponent implements OnInit {
  importForm: FormGroup;
  selectedFile: File | null = null;
  isDownloading = false;
  isImporting = false;
  errorMessage: string | null = null;

  constructor(
    public dialogRef: MatDialogRef<ExcelImportDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private fb: FormBuilder,
    private animalService: AnimalService,
    private errorHandler: ErrorHandlerService,
    private translate: TranslationService
  ) {
    this.importForm = this.fb.group({
      excelFile: [null, Validators.required],
    });
  }

  ngOnInit(): void {
    // Clear errors when form value changes
    this.importForm.valueChanges.subscribe(() => {
      if (this.errorMessage) {
        this.errorMessage = null;
      }
    });
  }

  get canImport(): boolean {
    return this.selectedFile !== null && this.importForm.valid;
  }

  onFileSelected(files: File[]): void {
    this.selectedFile = files.length > 0 ? files[0] : null;
    // Clear error immediately when file is selected
    if (this.selectedFile) {
      this.errorMessage = null;
    }
  }

  downloadTemplate(): void {
    this.isDownloading = true;
    // Clear error immediately when download starts
    this.errorMessage = null;

    this.animalService.getImportExcel().subscribe({
      next: (response) => {
        const blob = response.body;
        if (blob) {
          const url = window.URL.createObjectURL(blob);
          const link = document.createElement('a');
          link.href = url;
          link.download = 'animal-import-template.xlsx';
          link.click();
          window.URL.revokeObjectURL(url);
        }
        this.isDownloading = false;
      },
      error: (error) => {
        this.isDownloading = false;
        this.errorMessage = this.translate.translate(
          'APP.ANIMALS.EXCEL_IMPORT.DOWNLOAD_ERROR'
        );
        this.errorHandler.handleError(error);
      },
    });
  }

  onImport(): void {
    if (!this.selectedFile) return;

    this.isImporting = true;
    // Clear error immediately when import starts
    this.errorMessage = null;

    this.animalService.importFromExcelTemplate(this.selectedFile).subscribe({
      next: (animals: AnimalPersist[]) => {
        this.isImporting = false;
        this.dialogRef.close(animals);
      },
      error: (error) => {
        this.isImporting = false;
        this.errorMessage = this.translate.translate(
          'APP.ANIMALS.EXCEL_IMPORT.IMPORT_ERROR'
        );
        this.errorHandler.handleError(error);
      },
    });
  }

  onCancel(): void {
    this.dialogRef.close(null);
  }

  clearErrorOnInteraction(): void {
    // Clear error when user interacts with the dialog
    if (this.errorMessage && !this.isImporting && !this.isDownloading) {
      this.errorMessage = null;
    }
  }
}