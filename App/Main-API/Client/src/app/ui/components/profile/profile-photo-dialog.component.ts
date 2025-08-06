import { Component, Inject, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { NgIconsModule } from '@ng-icons/core';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { FileDropAreaComponent } from 'src/app/common/ui/file-drop-area.component';
import { FileItem } from 'src/app/models/file/file.model';

export interface ProfilePhotoDialogData {
  currentPhotoUrl?: string;
  userName: string;
}

export interface ProfilePhotoDialogResult {
  action: 'delete' | 'upload' | 'cancel';
  fileId?: string;
  file?: File;
}

@Component({
  selector: 'app-profile-photo-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    NgIconsModule,
    TranslatePipe,
    FileDropAreaComponent
  ],
  template: `
    <div class="profile-photo-dialog max-w-lg mx-auto bg-gradient-to-br from-gray-800 to-gray-900 text-white rounded-xl shadow-2xl overflow-hidden max-h-[90vh] flex flex-col">
      <!-- Compact Header -->
      <div class="dialog-header relative bg-gradient-to-r from-primary-600 to-accent-600 p-4 text-center flex-shrink-0">
        <div class="absolute inset-0 bg-black/20"></div>
        <div class="relative z-10">
          <div class="w-12 h-12 mx-auto mb-3 bg-white/20 rounded-full flex items-center justify-center backdrop-blur-sm">
            <ng-icon name="lucideCamera" class="w-6 h-6 text-white"></ng-icon>
          </div>
          <h2 class="text-xl font-bold text-white mb-1">
            {{ 'APP.PROFILE-PAGE.EDIT.PROFILE_PHOTO_TITLE' | translate }}
          </h2>
          <p class="text-xs text-white/80">
            {{ 'APP.PROFILE-PAGE.EDIT.PROFILE_PHOTO_SUBTITLE' | translate }}
          </p>
        </div>
      </div>

      <div class="dialog-content p-6 overflow-y-auto flex-1">
        <!-- Compact Photo Preview -->
        <div class="flex justify-center mb-6">
          <div class="relative group">
            <div class="absolute inset-0 bg-gradient-to-r from-primary-500 to-accent-500 rounded-full blur-md opacity-25 group-hover:opacity-40 transition-opacity duration-300"></div>
            <div class="relative">
              <img 
                [src]="previewImageUrl"
                [alt]="data.userName"
                class="w-28 h-28 rounded-full object-cover border-3 border-white/20 shadow-xl backdrop-blur-sm transition-transform duration-300 group-hover:scale-105"
              >
              <div *ngIf="hasNewPhoto()" class="absolute -top-2 -right-2 bg-gradient-to-r from-green-400 to-green-600 text-white rounded-full p-1.5 shadow-lg animate-pulse">
                <ng-icon name="lucideCheck" class="w-4 h-4"></ng-icon>
              </div>
              <div *ngIf="isUploading" class="absolute inset-0 bg-black/50 rounded-full flex items-center justify-center backdrop-blur-sm">
                <ng-icon name="lucideLoader" class="w-6 h-6 text-white animate-spin"></ng-icon>
              </div>
            </div>
          </div>
        </div>

        <!-- Compact File Drop Area -->
        <div class="bg-gray-800/60 rounded-lg p-4 border-2 border-dashed border-gray-500/60 hover:border-primary-500/80 transition-colors duration-300 mb-4 backdrop-blur-sm">
          <app-file-drop-area
            [form]="photoForm"
            controlName="profilePhoto"
            [label]="'APP.PROFILE-PAGE.EDIT.DRAG_DROP_PHOTO' | translate"
            accept="image/*"
            [multiple]="false"
            [maxFileSize]="maxFileSize"
            [maxFiles]="1"
            (filesChange)="onFilesChange($event)"
            (uploadStateChange)="onUploadStateChange($event)"
          ></app-file-drop-area>
        </div>

        <!-- Compact File requirements info -->
        <div class="p-3 bg-gray-800/40 rounded-lg border border-gray-600/40">
          <div class="flex items-start gap-2">
            <ng-icon name="lucideInfo" class="w-4 h-4 text-primary-400 mt-0.5 flex-shrink-0"></ng-icon>
            <div class="text-xs text-gray-300">
              <p class="font-medium mb-1 text-white">{{ 'APP.PROFILE-PAGE.EDIT.PHOTO_REQUIREMENTS' | translate }}</p>
              <div class="text-xs text-gray-400 space-y-0.5">
                <div class="flex items-center gap-1">
                  <span class="w-1 h-1 bg-primary-400 rounded-full"></span>
                  <span>{{ 'APP.PROFILE-PAGE.EDIT.PHOTO_FORMAT' | translate }}</span>
                </div>
                <div class="flex items-center gap-1">
                  <span class="w-1 h-1 bg-primary-400 rounded-full"></span>
                  <span>{{ 'APP.PROFILE-PAGE.EDIT.PHOTO_SIZE' | translate }}</span>
                </div>
                <div class="flex items-center gap-1">
                  <span class="w-1 h-1 bg-primary-400 rounded-full"></span>
                  <span>{{ 'APP.PROFILE-PAGE.EDIT.PHOTO_DIMENSIONS' | translate }} ({{ 'APP.PROFILE-PAGE.EDIT.RECOMMENDED' | translate }})</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Compact action buttons -->
      <div class="dialog-actions bg-gray-800/90 backdrop-blur-sm p-4 border-t border-gray-600/50 flex-shrink-0">
        <div class="flex justify-between items-center">
          <div>
            <button 
              *ngIf="data.currentPhotoUrl"
              (click)="deletePhoto()"
              [disabled]="isUploading"
              class="px-4 py-2 text-red-400 hover:bg-red-900/40 rounded-lg transition-all duration-200 flex items-center gap-2 disabled:opacity-50 hover:shadow-md hover:shadow-red-500/20 border border-red-500/30 hover:border-red-500/50 text-sm"
            >
              <ng-icon name="lucideTrash2" class="w-4 h-4"></ng-icon>
              {{ 'APP.PROFILE-PAGE.EDIT.DELETE_PHOTO' | translate }}
            </button>
          </div>
          
          <div class="flex gap-2">
            <button 
              (click)="cancel()"
              [disabled]="isUploading"
              class="px-4 py-2 text-gray-300 hover:bg-gray-700 rounded-lg transition-all duration-200 disabled:opacity-50 border border-gray-600 hover:border-gray-500 text-sm"
            >
              {{ 'APP.COMMONS.CANCEL' | translate }}
            </button>
            <button 
              (click)="savePhoto()"
              [disabled]="!hasNewPhoto() || isUploading"
              class="px-6 py-2 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg hover:from-primary-700 hover:to-accent-700 transition-all duration-200 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2 shadow-lg hover:shadow-lg hover:shadow-primary-500/30 transform hover:-translate-y-0.5 text-sm font-medium"
            >
              <ng-icon 
                [name]="isUploading ? 'lucideLoader' : 'lucideSave'" 
                class="w-4 h-4"
                [ngClass]="{ 'animate-spin': isUploading }"
              ></ng-icon>
              {{ isUploading ? ('APP.PROFILE-PAGE.EDIT.SAVING' | translate) : ('APP.PROFILE-PAGE.EDIT.SAVE' | translate) }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      width: 100%;
      height: 100%;
    }

    .profile-photo-dialog {
      width: 100%;
      background: linear-gradient(135deg, rgba(31, 41, 55, 0.98) 0%, rgba(17, 24, 39, 0.98) 100%);
      border-radius: 12px;
      overflow: hidden;
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.8);
      border: none;
      animation: slideInUp 0.3s ease-out;
      min-height: 0;
    }
    
    .dialog-header {
      position: relative;
      overflow: hidden;
      background: linear-gradient(135deg, rgba(124, 58, 237, 0.9) 0%, rgba(236, 72, 153, 0.9) 100%);
    }
    
    .dialog-content {
      background: linear-gradient(135deg, rgba(31, 41, 55, 0.98) 0%, rgba(17, 24, 39, 0.98) 100%);
      min-height: 0;
    }
    
    .dialog-actions {
      backdrop-filter: blur(10px);
      background: rgba(31, 41, 55, 0.95);
    }

    /* Enhanced animations */
    @keyframes slideInUp {
      from {
        opacity: 0;
        transform: translateY(20px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    /* Hover effects for buttons */
    button:hover:not(:disabled) {
      transform: translateY(-1px);
    }

    button:active:not(:disabled) {
      transform: translateY(0);
    }

    /* Force remove any white backgrounds from child components */
    :host ::ng-deep * {
      border: none !important;
    }

    :host ::ng-deep app-file-drop-area {
      background: transparent !important;
    }

    :host ::ng-deep app-file-drop-area * {
      background: transparent !important;
      color: white !important;
    }

    :host ::ng-deep .file-drop-area {
      background: rgba(31, 41, 55, 0.8) !important;
      border: 2px dashed rgba(75, 85, 99, 0.6) !important;
      color: white !important;
    }

    :host ::ng-deep .file-drop-area:hover {
      background: rgba(31, 41, 55, 0.9) !important;
      border-color: rgba(124, 58, 237, 0.8) !important;
    }

    :host ::ng-deep .file-drop-area .file-drop-label,
    :host ::ng-deep .file-drop-area .file-drop-hint,
    :host ::ng-deep .file-drop-area p,
    :host ::ng-deep .file-drop-area span,
    :host ::ng-deep .file-drop-area div {
      color: white !important;
      background: transparent !important;
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
      background: rgba(124, 58, 237, 0.5);
      border-radius: 3px;
    }

    .dialog-content::-webkit-scrollbar-thumb:hover {
      background: rgba(124, 58, 237, 0.7);
    }
  `]
})
export class ProfilePhotoDialogComponent implements OnDestroy {
  photoForm: FormGroup;
  selectedFiles: FileItem[] = [];
  isUploading = false;
  previewImageUrl: string;
  
  readonly maxFileSize = 10 * 1024 * 1024; // 10MB

  constructor(
    public dialogRef: MatDialogRef<ProfilePhotoDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ProfilePhotoDialogData,
    private fb: FormBuilder
  ) {
    this.photoForm = this.fb.group({
      profilePhoto: [null]
    });
    
    // Initialize preview image URL
    this.previewImageUrl = this.data.currentPhotoUrl || 'assets/placeholder.jpg';
    
    // Disable body scrolling when dialog opens
    document.body.classList.add('no-scroll');
  }

  ngOnDestroy() {
    // Clean up blob URL to prevent memory leaks
    if (this.previewImageUrl && this.previewImageUrl.startsWith('blob:')) {
      URL.revokeObjectURL(this.previewImageUrl);
    }
    
    // Re-enable body scrolling when dialog is destroyed
    document.body.classList.remove('no-scroll');
  }

  onFilesChange(files: FileItem[]): void {
    this.selectedFiles = files;
    // Update preview image URL when files change
    if (files.length > 0 && files[0].file) {
      // Clean up previous blob URL to prevent memory leaks
      if (this.previewImageUrl && this.previewImageUrl.startsWith('blob:')) {
        URL.revokeObjectURL(this.previewImageUrl);
      }
      this.previewImageUrl = URL.createObjectURL(files[0].file);
    } else {
      // Reset to original photo or placeholder
      if (this.previewImageUrl && this.previewImageUrl.startsWith('blob:')) {
        URL.revokeObjectURL(this.previewImageUrl);
      }
      this.previewImageUrl = this.data.currentPhotoUrl || 'assets/placeholder.jpg';
    }
  }

  onUploadStateChange(isUploading: boolean): void {
    this.isUploading = isUploading;
  }



  hasNewPhoto(): boolean {
    return this.selectedFiles.length > 0 && !!this.selectedFiles[0].persistedId;
  }

  deletePhoto(): void {
    // Re-enable body scrolling before closing
    document.body.classList.remove('no-scroll');
    this.dialogRef.close({ action: 'delete' } as ProfilePhotoDialogResult);
  }

  savePhoto(): void {
    if (this.hasNewPhoto()) {
      // Re-enable body scrolling before closing
      document.body.classList.remove('no-scroll');
      this.dialogRef.close({ 
        action: 'upload', 
        fileId: this.selectedFiles[0].persistedId,
        file: this.selectedFiles[0].file
      } as ProfilePhotoDialogResult);
    }
  }

  cancel(): void {
    // Re-enable body scrolling before closing
    document.body.classList.remove('no-scroll');
    this.dialogRef.close({ action: 'cancel' } as ProfilePhotoDialogResult);
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}