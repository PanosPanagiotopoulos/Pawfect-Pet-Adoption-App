import { Component, Inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { NgIconsModule } from '@ng-icons/core';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { FileToUrlPipe } from 'src/app/common/tools/file-to-url.pipe';

export interface ProfilePhotoDialogData {
  currentPhotoUrl?: string;
  userName: string;
}

export interface ProfilePhotoDialogResult {
  action: 'delete' | 'upload' | 'cancel';
  file?: File;
}

@Component({
  selector: 'app-profile-photo-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    NgIconsModule,
    TranslatePipe,
    FileToUrlPipe
  ],
  template: `
    <div class="profile-photo-dialog">
      <div class="dialog-header">
        <h2 class="text-xl font-semibold text-gray-900 mb-2">
          {{ 'APP.PROFILE-PAGE.EDIT.PROFILE_PHOTO_TITLE' | translate }}
        </h2>
        <p class="text-sm text-gray-600">
          {{ 'APP.PROFILE-PAGE.EDIT.PROFILE_PHOTO_SUBTITLE' | translate }}
        </p>
      </div>

      <div class="dialog-content py-6">
        <!-- Current Photo Display -->
        <div class="flex justify-center mb-6">
          <div class="relative">
            <img 
              [src]="selectedFile ? (selectedFile | fileToUrl) : (data.currentPhotoUrl || 'assets/placeholder.jpg')"
              [alt]="data.userName"
              class="w-32 h-32 rounded-full object-cover border-4 border-primary-200 shadow-lg"
            >
            <div *ngIf="selectedFile" class="absolute -top-2 -right-2 bg-green-500 text-white rounded-full p-1">
              <ng-icon name="lucideCheck" class="w-4 h-4"></ng-icon>
            </div>
          </div>
        </div>

        <!-- File Upload Area -->
        <div class="mb-6">
          <div 
            class="border-2 border-dashed border-gray-300 rounded-lg p-6 text-center hover:border-primary-400 transition-colors cursor-pointer"
            (click)="fileInput.click()"
            (dragover)="onDragOver($event)"
            (dragleave)="onDragLeave($event)"
            (drop)="onDrop($event)"
            [class.border-primary-400]="isDragOver"
            [class.bg-primary-50]="isDragOver"
          >
            <ng-icon name="lucideUpload" class="w-8 h-8 text-gray-400 mx-auto mb-2"></ng-icon>
            <p class="text-sm text-gray-600 mb-1">
              {{ 'APP.PROFILE-PAGE.EDIT.DRAG_DROP_PHOTO' | translate }}
            </p>
            <p class="text-xs text-gray-500">
              {{ 'APP.PROFILE-PAGE.EDIT.PHOTO_REQUIREMENTS' | translate }}
            </p>
          </div>
          <input 
            #fileInput
            type="file" 
            accept="image/*" 
            class="hidden"
            (change)="onFileSelected($event)"
          >
        </div>

        <!-- Error Message -->
        <div *ngIf="errorMessage" class="mb-4 p-3 bg-red-50 border border-red-200 rounded-lg">
          <p class="text-sm text-red-600">{{ errorMessage | translate }}</p>
        </div>

        <!-- Selected File Info -->
        <div *ngIf="selectedFile" class="mb-4 p-3 bg-green-50 border border-green-200 rounded-lg">
          <div class="flex items-center justify-between">
            <div>
              <p class="text-sm font-medium text-green-800">{{ selectedFile.name }}</p>
              <p class="text-xs text-green-600">{{ getFileSize(selectedFile.size) }}</p>
            </div>
            <button 
              (click)="clearSelectedFile()"
              class="text-green-600 hover:text-green-800 transition-colors"
            >
              <ng-icon name="lucideX" class="w-4 h-4"></ng-icon>
            </button>
          </div>
        </div>
      </div>

      <div class="dialog-actions flex justify-between pt-4 border-t border-gray-200">
        <div>
          <button 
            *ngIf="data.currentPhotoUrl"
            (click)="deletePhoto()"
            class="px-4 py-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors flex items-center gap-2"
          >
            <ng-icon name="lucideTrash2" class="w-4 h-4"></ng-icon>
            {{ 'APP.PROFILE-PAGE.EDIT.DELETE_PHOTO' | translate }}
          </button>
        </div>
        
        <div class="flex gap-3">
          <button 
            (click)="cancel()"
            class="px-4 py-2 text-gray-600 hover:bg-gray-50 rounded-lg transition-colors"
          >
            {{ 'APP.COMMONS.CANCEL' | translate }}
          </button>
          <button 
            (click)="uploadPhoto()"
            [disabled]="!selectedFile"
            class="px-6 py-2 bg-primary-600 text-white rounded-lg hover:bg-primary-700 transition-colors disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
          >
            <ng-icon name="lucideUpload" class="w-4 h-4"></ng-icon>
            {{ 'APP.PROFILE-PAGE.EDIT.UPLOAD_PHOTO' | translate }}
          </button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .profile-photo-dialog {
      @apply w-full max-w-md mx-auto;
    }
    
    .dialog-header {
      @apply pb-4 border-b border-gray-200;
    }
    
    .dialog-content {
      @apply max-h-96 overflow-y-auto;
    }
    
    .dialog-actions {
      @apply sticky bottom-0 bg-white;
    }
  `]
})
export class ProfilePhotoDialogComponent implements OnDestroy {
  selectedFile: File | null = null;
  isDragOver = false;
  errorMessage: string | null = null;
  
  private readonly maxFileSize = 10 * 1024 * 1024; // 10MB
  private readonly allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif'];

  constructor(
    public dialogRef: MatDialogRef<ProfilePhotoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ProfilePhotoDialogData
  ) {}

  ngOnDestroy() {
    // Clean up object URLs
    if (this.selectedFile) {
      FileToUrlPipe.revokeUrls([this.selectedFile]);
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFile(input.files[0]);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
    
    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.handleFile(event.dataTransfer.files[0]);
    }
  }

  private handleFile(file: File): void {
    this.errorMessage = null;

    // Validate file type
    if (!this.allowedTypes.includes(file.type)) {
      this.errorMessage = 'APP.PROFILE-PAGE.EDIT.ERRORS.INVALID_FILE_TYPE';
      return;
    }

    // Validate file size
    if (file.size > this.maxFileSize) {
      this.errorMessage = 'APP.PROFILE-PAGE.EDIT.ERRORS.FILE_TOO_LARGE';
      return;
    }

    // Clean up previous file URL
    if (this.selectedFile) {
      FileToUrlPipe.revokeUrls([this.selectedFile]);
    }

    this.selectedFile = file;
  }

  clearSelectedFile(): void {
    if (this.selectedFile) {
      FileToUrlPipe.revokeUrls([this.selectedFile]);
      this.selectedFile = null;
    }
    this.errorMessage = null;
  }

  getFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  deletePhoto(): void {
    this.dialogRef.close({ action: 'delete' } as ProfilePhotoDialogResult);
  }

  uploadPhoto(): void {
    if (this.selectedFile) {
      this.dialogRef.close({ 
        action: 'upload', 
        file: this.selectedFile 
      } as ProfilePhotoDialogResult);
    }
  }

  cancel(): void {
    this.dialogRef.close({ action: 'cancel' } as ProfilePhotoDialogResult);
  }
}