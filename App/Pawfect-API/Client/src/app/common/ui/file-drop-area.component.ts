import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  HostListener,
  ElementRef,
  ViewChild,
  ChangeDetectorRef,
  OnInit,
  OnChanges,
  SimpleChanges,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { NgIconsModule } from '@ng-icons/core';
import { ValidationMessageComponent } from './validation-message.component';
import { FileItem, FilePersist, File } from 'src/app/models/file/file.model';
import { FileService } from 'src/app/services/file.service';
import { TranslationService } from 'src/app/common/services/translation.service';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { LogService } from 'src/app/common/services/log.service';

@Component({
  selector: 'app-file-drop-area',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    NgIconsModule,
    ValidationMessageComponent,
    TranslatePipe,
  ],
  changeDetection: ChangeDetectionStrategy.Default,
  template: `
    <div [formGroup]="form" class="relative group mb-10">
      <label
        [for]="controlName"
        class="block text-sm font-medium text-gray-400 mb-2"
      >
        {{ label }}
      </label>

      <div
        #dropArea
        class="relative border-2 border-dashed rounded-xl p-4 sm:p-6 text-center transition-all duration-300 max-w-full"
        [ngClass]="{
          'border-primary-500': isDragging,
          'border-white/20': !isDragging && !isInvalid,
          'border-red-500': isInvalid,
          'bg-primary-500/5': isDragging,
          'bg-white/5': !isDragging,
          'hover:border-primary-400/50 hover:bg-primary-500/5': !isInvalid
        }"
        (dragover)="onDragOver($event)"
        (dragleave)="onDragLeave($event)"
        (drop)="onDrop($event)"
      >
        <input
          #fileInput
          type="file"
          [id]="controlName"
          class="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
          [accept]="accept"
          [multiple]="multiple"
          (change)="onFileSelected($event)"
          [attr.aria-invalid]="isInvalid"
          [attr.aria-describedby]="controlName + '-error'"
        />

        <div class="space-y-2 sm:space-y-4">
          <ng-icon
            name="lucideUpload"
            [size]="'36'"
            class="text-gray-400 group-hover:text-primary-400 transition-colors duration-300"
          ></ng-icon>

          <div class="space-y-1 sm:space-y-2">
            <p class="text-gray-300 text-sm sm:text-base">{{ dragDropText }}</p>
            <p class="text-xs sm:text-sm text-gray-500">{{ acceptText }}</p>
          </div>
        </div>
      </div>

      <div *ngIf="selectedFiles.length > 0" class="mt-4 space-y-2">
        <div *ngIf="hasFailedUploads()" class="mb-3 p-3 bg-red-500/10 border border-red-500/20 rounded-lg">
          <div class="flex items-center justify-between">
            <div class="flex items-center space-x-2">
              <ng-icon name="lucideCircleAlert" [size]="'16'" class="text-red-400"></ng-icon>
              <span class="text-sm text-red-400">
                {{ getFailedUploadsCount() }}
                {{ 'APP.UI_COMPONENTS.FILE_DROP.UPLOAD_FAILED' | translate }}
              </span>
            </div>
            <button
              type="button"
              class="text-xs text-yellow-400 hover:text-yellow-300 underline hover:no-underline transition-colors"
              (click)="clearFailedUploads()"
            >
              {{ 'APP.UI_COMPONENTS.FILE_DROP.CLEAR_FAILED' | translate }}
            </button>
          </div>
        </div>

        <div *ngIf="getUploadStatus().total > 0" class="mb-3 p-2 bg-gray-500/10 border border-gray-500/20 rounded-lg">
          <div class="flex items-center justify-between text-xs text-gray-400">
            <span>{{ 'APP.UI_COMPONENTS.FILE_DROP.STATUS_SUMMARY' | translate }}</span>
            <div class="flex items-center space-x-3">
              <span *ngIf="getUploadStatus().uploading > 0" class="text-yellow-400">
                {{ getUploadStatus().uploading }} {{ 'APP.UI_COMPONENTS.FILE_DROP.UPLOADING' | translate }}
              </span>
              <span *ngIf="getUploadStatus().successful > 0" class="text-green-400">
                {{ getUploadStatus().successful }} {{ 'APP.UI_COMPONENTS.FILE_DROP.UPLOAD_SUCCESS' | translate }}
              </span>
              <span *ngIf="getUploadStatus().existing > 0" class="text-blue-400">
                {{ getUploadStatus().existing }} {{ 'APP.UI_COMPONENTS.FILE_DROP.EXISTING_FILE' | translate }}
              </span>
            </div>
          </div>
        </div>

        <div class="text-sm font-medium text-gray-400 mb-2">
          {{ selectedFiles.length }}
          <ng-container *ngIf="selectedFiles.length === 1; else multipleFiles">
            {{
              'APP.UI_COMPONENTS.FILE_DROP.FILES_SELECTED_SINGLE' | translate
            }}
          </ng-container>
          <ng-template #multipleFiles>
            {{
              'APP.UI_COMPONENTS.FILE_DROP.FILES_SELECTED_MULTIPLE' | translate
            }}
          </ng-template>
          {{ 'APP.UI_COMPONENTS.FILE_DROP.SELECTED' | translate }}
        </div>

        <div class="max-h-40 overflow-y-auto custom-scrollbar">
          <div
            *ngFor="let file of selectedFiles; let i = index"
            class="flex items-center justify-between p-2 rounded-lg mb-2 group transition-all duration-200"
            [ngClass]="{
              'bg-blue-500/10 border border-blue-500/20': file.isExisting,
              'bg-white/5': !file.isExisting,
              'bg-yellow-500/10 border border-yellow-500/20': file.isPersisting,
              'bg-red-500/10 border border-red-500/20': file.uploadFailed
            }"
          >
            <div class="flex items-center overflow-hidden">
              <div class="relative mr-2 flex-shrink-0">
                <ng-icon
                  [name]="getFileIcon(file)"
                  [size]="'20'"
                  [class]="getFileIconClass(file)"
                ></ng-icon>

                <div *ngIf="file.isPersisting" class="absolute -top-1 -right-1">
                  <div
                    class="w-3 h-3 bg-yellow-500 rounded-full animate-pulse"
                  ></div>
                </div>
                <div *ngIf="file.isExisting" class="absolute -top-1 -right-1">
                  <ng-icon
                    name="lucideCloud"
                    [size]="'12'"
                    class="text-blue-400"
                  ></ng-icon>
                </div>
                <div *ngIf="file.uploadFailed" class="absolute -top-1 -right-1">
                  <ng-icon
                    name="lucideCircleAlert"
                    [size]="'12'"
                    class="text-red-400"
                  ></ng-icon>
                </div>
              </div>

              <div class="flex flex-col overflow-hidden">
                <button
                  *ngIf="canDownloadFile(file); else nonDownloadableFilename"
                  type="button"
                  class="text-sm text-gray-300 hover:text-primary-400 transition-colors duration-200 truncate text-left underline decoration-dotted underline-offset-2 hover:decoration-solid"
                  (click)="downloadFileByName(file)"
                  [attr.aria-label]="
                    ('APP.UI_COMPONENTS.FILE_DROP.DOWNLOAD_FILE' | translate) +
                    ': ' +
                    file.file.name
                  "
                  [title]="
                    'APP.UI_COMPONENTS.FILE_DROP.CLICK_TO_DOWNLOAD' | translate
                  "
                >
                  {{ file.file.name }}
                </button>

                <ng-template #nonDownloadableFilename>
                  <span class="text-sm text-gray-300 truncate">{{
                    file.file.name
                  }}</span>
                </ng-template>

                <div class="flex items-center space-x-2 text-xs text-gray-500">
                  <span>({{ formatFileSize(file.file.size!) }})</span>
                  <span *ngIf="file.isExisting" class="text-blue-400">
                    {{
                      'APP.UI_COMPONENTS.FILE_DROP.EXISTING_FILE' | translate
                    }}
                  </span>
                  <span *ngIf="file.isPersisting" class="text-yellow-400">
                    {{ 'APP.UI_COMPONENTS.FILE_DROP.UPLOADING' | translate }}
                  </span>
                  <span *ngIf="file.uploadFailed" class="text-red-400">
                    {{
                      'APP.UI_COMPONENTS.FILE_DROP.UPLOAD_FAILED' | translate
                    }}
                  </span>
                  <span *ngIf="file.persistedId && !file.isExisting" class="text-green-400">
                    {{ 'APP.UI_COMPONENTS.FILE_DROP.UPLOAD_SUCCESS' | translate }}
                  </span>
                  <span *ngIf="canDownloadFile(file)" class="text-primary-400">
                    {{
                      'APP.UI_COMPONENTS.FILE_DROP.CLICK_TO_DOWNLOAD'
                        | translate
                    }}
                  </span>
                </div>
              </div>
            </div>

            <div class="flex items-center space-x-2 flex-shrink-0">
              <button
                *ngIf="file.isExisting && file.sourceUrl"
                type="button"
                class="p-1 text-gray-500 hover:text-blue-400 transition-colors opacity-0 group-hover:opacity-100"
                (click)="downloadFile(file)"
                [attr.aria-label]="
                  'APP.UI_COMPONENTS.FILE_DROP.DOWNLOAD_FILE' | translate
                "
              >
                <ng-icon name="lucideDownload" [size]="'16'"></ng-icon>
              </button>

              <button
                *ngIf="canRetryFile(file)"
                type="button"
                class="p-1 text-gray-500 hover:text-yellow-400 transition-colors opacity-0 group-hover:opacity-100"
                (click)="retryFile(file)"
                [attr.aria-label]="
                  'APP.UI_COMPONENTS.FILE_DROP.RETRY_FILE' | translate
                "
              >
                <ng-icon name="lucideRefreshCw" [size]="'16'"></ng-icon>
              </button>

              <button
                type="button"
                class="p-1 text-gray-500 hover:text-red-400 transition-colors opacity-0 group-hover:opacity-100"
                (click)="removeFile(i)"
                [attr.aria-label]="
                  'APP.UI_COMPONENTS.FILE_DROP.REMOVE_FILE' | translate
                "
                [disabled]="file.isPersisting"
              >
                <ng-icon name="lucideX" [size]="'16'"></ng-icon>
              </button>
            </div>
          </div>
        </div>
      </div>

      <div *ngIf="uploadError" class="mt-2 p-3 bg-red-500/10 border border-red-500/20 rounded-lg">
        <div class="flex items-center space-x-2 mb-2">
          <ng-icon name="lucideCircleAlert" [size]="'16'" class="text-red-400"></ng-icon>
          <span class="text-sm text-red-400 font-medium">{{ uploadError }}</span>
        </div>
        <div class="text-xs text-red-400/80">
          {{ 'APP.UI_COMPONENTS.FILE_DROP.ERROR_HINT' | translate }}
        </div>
      </div>

      <app-validation-message
        [id]="controlName + '-error'"
        [control]="form.get(controlName)"
        [field]="label"
        [showImmediately]="true"
      >
      </app-validation-message>

      <p *ngIf="hint && !isInvalid" class="mt-2 text-sm text-gray-400">
        {{ hint }}
      </p>
    </div>
  `,
  styles: [
    `
      .custom-scrollbar {
        scrollbar-width: thin;
        scrollbar-color: rgba(255, 255, 255, 0.2) rgba(255, 255, 255, 0.1);
      }

      .custom-scrollbar::-webkit-scrollbar {
        width: 6px;
      }

      .custom-scrollbar::-webkit-scrollbar-track {
        background: rgba(255, 255, 255, 0.1);
        border-radius: 3px;
      }

      .custom-scrollbar::-webkit-scrollbar-thumb {
        background: rgba(255, 255, 255, 0.2);
        border-radius: 3px;
      }

      .custom-scrollbar::-webkit-scrollbar-thumb:hover {
        background: rgba(255, 255, 255, 0.3);
      }
    `,
  ],
})
export class FileDropAreaComponent implements OnInit, OnChanges {
  @Input() form!: FormGroup;
  @Input() controlName!: string;
  @Input() label: string = '';
  @Input() hint?: string;
  @Input() accept: string = '*/*';
  @Input() multiple: boolean = false;
  @Input() maxFileSize: number = 10 * 1024 * 1024;
  @Input() maxFiles: number = 5;
  @Input() existingFiles: File[] = [];
  @Output() filesChange = new EventEmitter<FileItem[]>();
  @Output() uploadStateChange = new EventEmitter<boolean>();

  @ViewChild('dropArea') dropArea!: ElementRef;
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;

  isDragging = false;
  selectedFiles: FileItem[] = [];
  uploadError: string | null = null;
  isUploading = false;
  private currentUploadBatch: FileItem[] = [];
  private recentlyRemovedFiles: Set<string> = new Set();

  constructor(
    private cdr: ChangeDetectorRef,
    private fileService: FileService,
    private translate: TranslationService,
    private log: LogService
  ) {
    if (!this.label) {
      this.label = this.translate.translate(
        'APP.UI_COMPONENTS.FILE_DROP.DEFAULT_LABEL'
      );
    }
  }

  ngOnInit(): void {
    this.loadExistingFiles();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['existingFiles'] && !changes['existingFiles'].firstChange) {
      this.loadExistingFiles();
    }
  }

  private loadExistingFiles(): void {
    if (this.existingFiles && this.existingFiles.length > 0) {
      const existingFileItems = this.existingFiles.map((file) =>
        this.convertFileToFileItem(file)
      );
      this.selectedFiles = [...existingFileItems];
      this.updateFormControl();
      this.cdr.markForCheck();
    }
  }

  private convertFileToFileItem(file: File): FileItem {
    const fileName = file.filename || (file as any).filename || 'Unknown File';
    const fileSize = file.size || 0;
    const mimeType = file.mimeType || 'application/octet-stream';

    const mockFile = new globalThis.File(
      [new Blob()],
      fileName,
      {
        type: mimeType,
        lastModified: file.updatedAt
          ? new Date(file.updatedAt).getTime()
          : Date.now(),
      }
    );

    if (fileSize > 0) {
      Object.defineProperty(mockFile, 'size', {
        value: fileSize,
        writable: false,
      });
    }

    return {
      file: mockFile,
      addedAt: file.createdAt ? new Date(file.createdAt).getTime() : Date.now(),
      persistedId: file.id,
      isPersisting: false,
      uploadFailed: false,
      isExisting: true,
      sourceUrl: file.sourceUrl,
    };
  }

  get isInvalid(): boolean {
    const control = this.form.get(this.controlName);
    return !!(control?.invalid && (control?.touched || control?.dirty));
  }

  get dragDropText(): string {
    return this.multiple
      ? this.translate.translate(
          'APP.UI_COMPONENTS.FILE_DROP.DRAG_DROP_MULTIPLE'
        )
      : this.translate.translate(
          'APP.UI_COMPONENTS.FILE_DROP.DRAG_DROP_SINGLE'
        );
  }

  get acceptText(): string {
    if (this.accept === '*/*') {
      return this.translate.translate(
        'APP.UI_COMPONENTS.FILE_DROP.ACCEPT_ALL_TYPES'
      );
    }
    const types = this.accept
      .split(',')
      .map((type) => type.trim().replace('.', '').toUpperCase())
      .join(', ');
    return this.translate
      .translate('APP.UI_COMPONENTS.FILE_DROP.ACCEPT_TYPES')
      .replace('{types}', types);
  }

  @HostListener('window:dragover', ['$event'])
  onWindowDragOver(event: DragEvent): void {
    event.preventDefault();
  }

  @HostListener('window:drop', ['$event'])
  onWindowDrop(event: DragEvent): void {
    event.preventDefault();
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
    this.cdr.markForCheck();
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
    this.cdr.markForCheck();
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
    if (event.dataTransfer?.files) {
      this.handleFiles(event.dataTransfer.files);
    }
    this.cdr.markForCheck();
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.handleFiles(input.files);
    }
  }

  handleFiles(fileList: FileList): void {
    this.clearUploadError();
    
    if (this.isUploading) {
      this.log.logFormatted({
        message: 'Upload in progress, cannot start new upload',
        data: { 
          isUploading: this.isUploading
        }
      });
      return;
    }
    
    const newFiles = Array.from(fileList);
    const processedFiles: globalThis.File[] = [];
    const failedFilesToReplace: { oldFile: FileItem, newFile: globalThis.File }[] = [];

    newFiles.forEach(newFile => {
      const fileKey = `${newFile.name}_${newFile.size}`;
      
      const existingFile = this.selectedFiles.find(existing => 
        existing.file.name === newFile.name &&
        existing.file.size === newFile.size
      );

      if (existingFile) {
        const isSuccessfullyPersisted = existingFile.persistedId && !existingFile.uploadFailed;
        
        if (isSuccessfullyPersisted) {
          this.log.logFormatted({
            message: 'Skipping file - already successfully uploaded',
            data: { filename: newFile.name }
          });
        } else {
          failedFilesToReplace.push({ oldFile: existingFile, newFile });
          this.log.logFormatted({
            message: 'Found non-persisted file to re-upload',
            data: { 
              filename: newFile.name,
              state: {
                uploadFailed: existingFile.uploadFailed,
                isPersisting: existingFile.isPersisting,
                persistedId: existingFile.persistedId
              }
            }
          });
        }
      } else if (this.recentlyRemovedFiles.has(fileKey)) {
        processedFiles.push(newFile);
        this.recentlyRemovedFiles.delete(fileKey);
        this.log.logFormatted({
          message: 'Re-uploading previously removed file',
          data: { filename: newFile.name }
        });
      } else {
        processedFiles.push(newFile);
      }
    });

    if (this.multiple) {
      const currentFileCount = this.selectedFiles.length - failedFilesToReplace.length;
      if (currentFileCount + processedFiles.length > this.maxFiles) {
        this.setError(
          this.translate
            .translate('APP.UI_COMPONENTS.FILE_DROP.MAX_FILES_ERROR')
            .replace('{max}', this.maxFiles.toString())
        );
        return;
      }
    } else {
      this.selectedFiles = [];
      failedFilesToReplace.length = 0;
    }

    const allFilesToValidate = [...processedFiles, ...failedFilesToReplace.map(r => r.newFile)];
    const oversizedFiles = allFilesToValidate.filter(
      (file) => file.size > this.maxFileSize
    );
    if (oversizedFiles.length > 0) {
      this.setError(
        this.translate
          .translate('APP.UI_COMPONENTS.FILE_DROP.MAX_FILE_SIZE_ERROR')
          .replace('{size}', this.formatFileSize(this.maxFileSize))
      );
      return;
    }

    if (this.accept !== '*/*') {
      const acceptedTypes = this.accept.split(',').map((type) => type.trim());
      const invalidFiles = allFilesToValidate.filter((file) => {
        return !acceptedTypes.some((type) => {
          if (type.startsWith('.')) {
            return file.name.toLowerCase().endsWith(type.toLowerCase());
          } else {
            return file.type.match(new RegExp(type.replace('*', '.*')));
          }
        });
      });
      if (invalidFiles.length > 0) {
        this.setError(
          `Μη αποδεκτός τύπος αρχείου. Αποδεκτοί τύποι: ${this.accept}`
        );
        return;
      }
    }

    const filesToUpload: FileItem[] = [];
    failedFilesToReplace.forEach(replacement => {
      replacement.oldFile.file = replacement.newFile;
      replacement.oldFile.addedAt = Date.now();
      replacement.oldFile.isPersisting = false;
      replacement.oldFile.uploadFailed = false;
      replacement.oldFile.persistedId = undefined;
      replacement.oldFile.sourceUrl = undefined;
      
      filesToUpload.push(replacement.oldFile);
      
      this.log.logFormatted({
        message: 'Replaced non-persisted file for re-upload',
        data: { filename: replacement.newFile.name }
      });
    });

    const newFileItems = processedFiles.map((file) => ({
      file,
      addedAt: Date.now(),
      isPersisting: false,
      uploadFailed: false,
    }));

    if (this.multiple) {
      this.selectedFiles = [...this.selectedFiles, ...newFileItems];
    } else {
      this.selectedFiles = newFileItems;
    }
    filesToUpload.push(...newFileItems);

    if (filesToUpload.length > 0) {
      this.persistFiles(filesToUpload);
    } else {
      this.log.logFormatted({
        message: 'No files to upload after processing',
        data: { 
          newFiles: newFiles.length,
          skippedSuccessful: newFiles.length - processedFiles.length - failedFilesToReplace.length
        }
      });
    }
  }

  private handleNewBatchUpload(filesToPersist: FileItem[]): void {
    this.clearUploadError();
    
    filesToPersist.forEach((item) => {
      item.isPersisting = true;
      item.uploadFailed = false;
      
      if (!item.isExisting) {
        item.persistedId = undefined;
        item.sourceUrl = undefined;
      }
    });
    
    this.currentUploadBatch = [...filesToPersist];
    
    this.log.logFormatted({
      message: 'Starting new batch upload',
      data: { 
        filesCount: filesToPersist.length, 
        totalFiles: this.selectedFiles.length,
        retriedFiles: filesToPersist.filter(f => f.uploadFailed).length
      }
    });
  }

  persistFiles(filesToPersist: FileItem[]): void {
    if (this.isUploading) {
      this.log.logFormatted({
        message: 'Upload already in progress, skipping new upload request',
        data: { filesCount: filesToPersist.length }
      });
      return;
    }

    this.handleNewBatchUpload(filesToPersist);
    
    this.isUploading = true;
    this.uploadStateChange.emit(true);

    const formData = new FormData();
    filesToPersist.forEach((item: FileItem, index) => {
      formData.append(`files[${index}]`, item.file);
    });

    this.log.logFormatted({
      message: 'Sending upload request',
      data: { 
        filesCount: filesToPersist.length, 
        fileNames: filesToPersist.map(f => f.file.name)
      }
    });

    this.fileService.persistBatchTemporary(formData).subscribe({
      next: (filePersists: FilePersist[]) => {
        this.handleUploadSuccess(filesToPersist, filePersists);
      },
      error: (error: Error) => {
        this.handleUploadError(filesToPersist, error);
      },
    });
  }

  private handleUploadSuccess(filesToPersist: FileItem[], filePersists: FilePersist[]): void {
    this.log.logFormatted({
      message: 'File upload successful',
      data: { 
        requestedFiles: filesToPersist.length, 
        returnedFiles: filePersists.length 
      }
    });

    const matchFiles = (fileItem: FileItem, persist: FilePersist): boolean => {
      const persistFileName = persist.fileName || (persist as any).filename;
      const nameMatch = fileItem.file.name === persistFileName;
      const sizeMatch = !persist.size || fileItem.file.size === persist.size;
      return nameMatch && sizeMatch;
    };

    const usedPersists = new Set<number>();

    filesToPersist.forEach((item) => {
      const persistIndex = filePersists.findIndex((fp, index) => 
        !usedPersists.has(index) && matchFiles(item, fp)
      );
      
      if (persistIndex !== -1) {
        const successfulUpload = filePersists[persistIndex];
        usedPersists.add(persistIndex);
        
        item.persistedId = successfulUpload.id;
        item.isPersisting = false;
        item.uploadFailed = false;
        
        if (successfulUpload.sourceUrl) {
          item.sourceUrl = successfulUpload.sourceUrl;
        }
        
        this.log.logFormatted({
          message: 'File upload completed successfully',
          data: { filename: item.file.name, persistedId: successfulUpload.id }
        });
      } else {
        this.log.logFormatted({
          message: 'File upload failed - no response for file, marking as failed',
          data: { filename: item.file.name }
        });
        
        item.isPersisting = false;
        item.uploadFailed = true;
        item.persistedId = undefined;
        item.sourceUrl = undefined;
      }
    });

    this.isUploading = false;
    this.currentUploadBatch = [];
    this.uploadStateChange.emit(false);
    this.uploadError = null;
    
    this.updateFormControl();
    this.filesChange.emit(this.selectedFiles);
    this.cdr.markForCheck();
  }

  private handleUploadError(filesToPersist: FileItem[], error: Error): void {
    filesToPersist.forEach((item) => {
      const fileKey = `${item.file.name}_${item.file.size}`;
      this.recentlyRemovedFiles.add(fileKey);
      
      item.isPersisting = false;
      item.uploadFailed = true;
      if (!item.isExisting) {
        item.persistedId = undefined;
        item.sourceUrl = undefined;
      }
    });

    this.isUploading = false;
    this.currentUploadBatch = [];
    this.uploadStateChange.emit(false);
    
    this.uploadError = this.translate.translate('APP.UI_COMPONENTS.FILE_DROP.UPLOAD_FAILED_HINT');
    
    this.updateFormControl();
    this.filesChange.emit(this.selectedFiles);
    this.cdr.markForCheck();
  }

  retryFile(fileItem: FileItem): void {
    if (!fileItem.uploadFailed) {
      return;
    }

    this.log.logFormatted({
      message: 'Retrying file upload',
      data: { filename: fileItem.file.name }
    });
    
    fileItem.uploadFailed = false;
    fileItem.isPersisting = false;
    if (!fileItem.isExisting) {
      fileItem.persistedId = undefined;
      fileItem.sourceUrl = undefined;
    }
    
    this.clearUploadError();
    
    if (this.isUploading) {
      this.log.logFormatted({
        message: 'Upload in progress, cannot retry now',
        data: { filename: fileItem.file.name }
      });
      
      fileItem.uploadFailed = true;
      this.setError('Upload in progress. Please wait before retrying.');
      return;
    }
    
    this.cdr.markForCheck();
    this.persistFiles([fileItem]);
  }

  clearFailedUploads(): void {
    const failedCount = this.selectedFiles.filter(f => f.uploadFailed).length;
    if (failedCount > 0) {
      this.selectedFiles = this.selectedFiles.filter(f => !f.uploadFailed);
      this.updateFormControl();
      this.filesChange.emit(this.selectedFiles);
      this.uploadError = null;
      
      this.log.logFormatted({
        message: 'Cleared failed uploads',
        data: { clearedCount: failedCount, remainingFiles: this.selectedFiles.length }
      });
      
      this.cdr.markForCheck();
    }
  }

  canRetryFile(fileItem: FileItem): boolean {
    return fileItem.uploadFailed && !fileItem.isPersisting && !this.isUploading;
  }

  hasFailedUploads(): boolean {
    return this.selectedFiles.some(f => f.uploadFailed);
  }

  hasUploadingFiles(): boolean {
    return this.selectedFiles.some(f => f.isPersisting);
  }

  isReadyForUpload(): boolean {
    return !this.isUploading;
  }

  getFailedUploadsCount(): number {
    return this.selectedFiles.filter(f => f.uploadFailed).length;
  }

  getUploadStatus(): { 
    total: number; 
    uploading: number; 
    failed: number; 
    successful: number; 
    existing: number; 
  } {
    return {
      total: this.selectedFiles.length,
      uploading: this.selectedFiles.filter(f => f.isPersisting).length,
      failed: this.selectedFiles.filter(f => f.uploadFailed).length,
      successful: this.selectedFiles.filter(f => f.persistedId && !f.isExisting).length,
      existing: this.selectedFiles.filter(f => f.isExisting).length
    };
  }

  private clearUploadError(): void {
    this.uploadError = null;
    
    const control = this.form.get(this.controlName);
    if (control && control.errors?.['custom']) {
      const errors = { ...control.errors };
      delete errors['custom'];
      if (Object.keys(errors).length === 0) {
        control.setErrors(null);
      } else {
        control.setErrors(errors);
      }
    }
    
    this.cdr.markForCheck();
  }

  removeFile(index: number): void {
    const fileToRemove = this.selectedFiles[index];
    
    if (fileToRemove && fileToRemove.isPersisting) {
      this.log.logFormatted({
        message: 'Cannot remove file currently being uploaded',
        data: { filename: fileToRemove.file.name }
      });
      return;
    }
    
    if (fileToRemove) {
      const fileKey = `${fileToRemove.file.name}_${fileToRemove.file.size}`;
      this.recentlyRemovedFiles.add(fileKey);
    }
    
    this.selectedFiles.splice(index, 1);
    this.updateFormControl();
    this.filesChange.emit(this.selectedFiles);
    this.uploadError = null;
    
    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
    this.cdr.markForCheck();
  }

  updateFormControl(): void {
    const control = this.form.get(this.controlName);
    if (control) {
      const persistedIds = this.selectedFiles
        .filter((item) => item.persistedId && !item.uploadFailed)
        .map((item) => item.persistedId!);
      
      const valueToSet = this.multiple ? persistedIds : persistedIds[0] || null;
      
      control.setValue(valueToSet);
      control.markAsTouched();
      control.markAsDirty();
      control.updateValueAndValidity();
    }
  }

  setError(errorMessage: string): void {
    const control = this.form.get(this.controlName);
    if (control) {
      control.setErrors({ custom: errorMessage });
      control.markAsTouched();
    }
    this.uploadError = errorMessage;
    this.cdr.markForCheck();
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  getFileIcon(fileItem: FileItem): string {
    if (fileItem.uploadFailed) {
      return 'lucideCircleAlert';
    }

    if (fileItem.isPersisting) {
      return 'lucideLoader';
    }

    const mimeType = fileItem.file.type.toLowerCase();
    const fileName = fileItem.file.name.toLowerCase();

    if (mimeType.startsWith('image/')) {
      return 'lucideImage';
    }

    if (mimeType.includes('pdf') || fileName.endsWith('.pdf')) {
      return 'lucideFileText';
    }

    if (
      mimeType.includes('word') ||
      fileName.endsWith('.doc') ||
      fileName.endsWith('.docx')
    ) {
      return 'lucideFileText';
    }

    if (
      mimeType.includes('excel') ||
      mimeType.includes('spreadsheet') ||
      fileName.endsWith('.xls') ||
      fileName.endsWith('.xlsx') ||
      fileName.endsWith('.csv')
    ) {
      return 'lucideSheet';
    }

    if (
      mimeType.includes('zip') ||
      mimeType.includes('rar') ||
      fileName.endsWith('.zip') ||
      fileName.endsWith('.rar') ||
      fileName.endsWith('.7z')
    ) {
      return 'lucideArchive';
    }

    if (mimeType.startsWith('video/')) {
      return 'lucideVideo';
    }

    if (mimeType.startsWith('audio/')) {
      return 'lucideMusic';
    }

    return 'lucideFile';
  }

  getFileIconClass(fileItem: FileItem): string {
    if (fileItem.uploadFailed) {
      return 'text-red-400';
    }

    if (fileItem.isPersisting) {
      return 'text-yellow-400 animate-spin';
    }

    if (fileItem.isExisting) {
      return 'text-blue-400';
    }

    return 'text-gray-400';
  }

  canDownloadFile(fileItem: FileItem): boolean {
    return (
      !fileItem.isPersisting &&
      !fileItem.uploadFailed &&
      (!!fileItem.sourceUrl ||
        !!fileItem.persistedId ||
        this.hasFileBlob(fileItem))
    );
  }

  private hasFileBlob(fileItem: FileItem): boolean {
    return fileItem.file && fileItem.file.size > 0;
  }

  downloadFileByName(fileItem: FileItem): void {
    this.downloadFileContent(fileItem);
  }

  downloadFile(fileItem: FileItem): void {
    this.downloadFileContent(fileItem);
  }

  private downloadFileContent(fileItem: FileItem): void {
    if (fileItem.sourceUrl) {
      this.downloadFromUrl(fileItem.sourceUrl, fileItem.file.name);
    } else if (this.hasFileBlob(fileItem)) {
      this.downloadFromBlob(fileItem.file, fileItem.file.name);
    } else {
      console.warn(
        'No download source available for file:',
        fileItem.file.name
      );
    }
  }

  private downloadFromUrl(url: string, filename: string): void {
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.target = '_blank';
    link.rel = 'noopener noreferrer';

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  private downloadFromBlob(file: globalThis.File, filename: string): void {
    const blobUrl = URL.createObjectURL(file);

    const link = document.createElement('a');
    link.href = blobUrl;
    link.download = filename;

    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    setTimeout(() => {
      URL.revokeObjectURL(blobUrl);
    }, 100);
  }
}