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
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div [formGroup]="form" class="relative group mb-10">
      <!-- Label -->
      <label
        [for]="controlName"
        class="block text-sm font-medium text-gray-400 mb-2"
      >
        {{ label }}
      </label>

      <!-- File drop area -->
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

      <!-- Selected files preview -->
      <div *ngIf="selectedFiles.length > 0" class="mt-4 space-y-2">
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
              <!-- File icon with status indicator -->
              <div class="relative mr-2 flex-shrink-0">
                <ng-icon
                  [name]="getFileIcon(file)"
                  [size]="'20'"
                  [class]="getFileIconClass(file)"
                ></ng-icon>

                <!-- Status indicators -->
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
                <!-- Clickable filename for download -->
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

                <!-- Non-downloadable filename (for files being uploaded or failed) -->
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
              <!-- Download button for existing files -->
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

              <!-- Remove button -->
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

      <!-- Error message for upload failure -->
      <div *ngIf="uploadError" class="mt-2 text-sm text-red-500">
        {{ uploadError }}
      </div>

      <!-- Error message from validation -->
      <app-validation-message
        [id]="controlName + '-error'"
        [control]="form.get(controlName)"
        [field]="label"
        [showImmediately]="true"
      >
      </app-validation-message>

      <!-- Hint text -->
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

  constructor(
    private cdr: ChangeDetectorRef,
    private fileService: FileService,
    private translate: TranslationService
  ) {
    // Set default label if not provided
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

  /**
   * Load existing files from AWS S3 and convert them to FileItem format
   */
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

  /**
   * Convert File model (from AWS S3) to FileItem format for display
   */
  private convertFileToFileItem(file: File): FileItem {
    // Handle both fileName and filename properties from API
    const fileName = file.filename || (file as any).filename || 'Unknown File';
    const fileSize = file.size || 0;
    const mimeType = file.mimeType || 'application/octet-stream';

    // Create a mock File object from the existing file data
    const mockFile = new globalThis.File(
      [new Blob()], // Empty blob since we don't have the actual file content
      fileName,
      {
        type: mimeType,
        lastModified: file.updatedAt
          ? new Date(file.updatedAt).getTime()
          : Date.now(),
      }
    );

    // Override the size property if available
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
    const newFiles = Array.from(fileList);

    if (this.multiple) {
      if (this.selectedFiles.length + newFiles.length > this.maxFiles) {
        this.setError(
          this.translate
            .translate('APP.UI_COMPONENTS.FILE_DROP.MAX_FILES_ERROR')
            .replace('{max}', this.maxFiles.toString())
        );
        return;
      }
    } else {
      this.selectedFiles = [];
    }

    const oversizedFiles = newFiles.filter(
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
      const invalidFiles = newFiles.filter((file) => {
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

    const newFileItems = newFiles.map((file) => ({
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

    this.persistFiles(newFileItems);
  }

  persistFiles(filesToPersist: FileItem[]): void {
    filesToPersist.forEach((item) => (item.isPersisting = true));
    this.uploadStateChange.emit(true);

    const formData = new FormData();
    filesToPersist.forEach((item: FileItem, index) => {
      formData.append(`files[${index}]`, item.file);
    });

    this.fileService.persistBatchTemporary(formData).subscribe({
      next: (filePersists: FilePersist[]) => {
        filePersists.forEach((fp, index) => {
          const item = filesToPersist[index];
          if (this.selectedFiles.includes(item)) {
            item.persistedId = fp.id;
          }
          item.isPersisting = false;
        });
        this.updateFormControl();
        this.filesChange.emit(this.selectedFiles);
        this.uploadStateChange.emit(
          this.selectedFiles.some((item) => item.isPersisting)
        );
        this.uploadError = null;
        this.cdr.markForCheck();
      },
      error: (error: Error) => {
        console.error('Error persisting files:', error);
        // Remove failed files from the list
        this.selectedFiles = this.selectedFiles.filter(
          (item) => !filesToPersist.includes(item)
        );
        this.updateFormControl();
        this.filesChange.emit(this.selectedFiles);
        this.uploadStateChange.emit(false);
        this.uploadError = 'Η μεταφόρτωση απέτυχε. Παρακαλώ δοκιμάστε ξανά.';
        this.cdr.markForCheck();
      },
    });
  }

  removeFile(index: number): void {
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
        .filter((item) => item.persistedId)
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
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  /**
   * Get appropriate icon for file type
   */
  getFileIcon(fileItem: FileItem): string {
    if (fileItem.uploadFailed) {
      return 'lucideCircleAlert';
    }

    if (fileItem.isPersisting) {
      return 'lucideLoader';
    }

    const mimeType = fileItem.file.type.toLowerCase();
    const fileName = fileItem.file.name.toLowerCase();

    // Image files
    if (mimeType.startsWith('image/')) {
      return 'lucideImage';
    }

    // Document files
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

    // Spreadsheet files
    if (
      mimeType.includes('excel') ||
      mimeType.includes('spreadsheet') ||
      fileName.endsWith('.xls') ||
      fileName.endsWith('.xlsx') ||
      fileName.endsWith('.csv')
    ) {
      return 'lucideSheet';
    }

    // Archive files
    if (
      mimeType.includes('zip') ||
      mimeType.includes('rar') ||
      fileName.endsWith('.zip') ||
      fileName.endsWith('.rar') ||
      fileName.endsWith('.7z')
    ) {
      return 'lucideArchive';
    }

    // Video files
    if (mimeType.startsWith('video/')) {
      return 'lucideVideo';
    }

    // Audio files
    if (mimeType.startsWith('audio/')) {
      return 'lucideMusic';
    }

    // Default file icon
    return 'lucideFile';
  }

  /**
   * Get appropriate CSS class for file icon based on status
   */
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

  /**
   * Check if a file can be downloaded
   */
  canDownloadFile(fileItem: FileItem): boolean {
    // File can be downloaded if:
    // 1. It has a sourceUrl (existing file from server)
    // 2. It's not currently being uploaded
    // 3. Upload didn't fail
    // 4. It has actual file data (blob) for newly uploaded files
    return (
      !fileItem.isPersisting &&
      !fileItem.uploadFailed &&
      (!!fileItem.sourceUrl ||
        !!fileItem.persistedId ||
        this.hasFileBlob(fileItem))
    );
  }

  /**
   * Check if file has blob data available
   */
  private hasFileBlob(fileItem: FileItem): boolean {
    return fileItem.file && fileItem.file.size > 0;
  }

  /**
   * Download file by clicking on filename
   */
  downloadFileByName(fileItem: FileItem): void {
    this.downloadFileContent(fileItem);
  }

  /**
   * Download existing file from AWS S3 or blob data
   */
  downloadFile(fileItem: FileItem): void {
    this.downloadFileContent(fileItem);
  }

  /**
   * Core download functionality that handles both sourceUrl and blob data
   */
  private downloadFileContent(fileItem: FileItem): void {
    if (fileItem.sourceUrl) {
      // Download from server URL (existing files)
      this.downloadFromUrl(fileItem.sourceUrl, fileItem.file.name);
    } else if (this.hasFileBlob(fileItem)) {
      // Download from blob data (newly uploaded files)
      this.downloadFromBlob(fileItem.file, fileItem.file.name);
    } else {
      console.warn(
        'No download source available for file:',
        fileItem.file.name
      );
    }
  }

  /**
   * Download file from URL
   */
  private downloadFromUrl(url: string, filename: string): void {
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.target = '_blank';
    link.rel = 'noopener noreferrer';

    // Append to body, click, and remove
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  /**
   * Download file from blob data
   */
  private downloadFromBlob(file: globalThis.File, filename: string): void {
    // Create blob URL from file data
    const blobUrl = URL.createObjectURL(file);

    // Create download link
    const link = document.createElement('a');
    link.href = blobUrl;
    link.download = filename;

    // Append to body, click, and remove
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);

    // Clean up blob URL to free memory
    setTimeout(() => {
      URL.revokeObjectURL(blobUrl);
    }, 100);
  }
}
