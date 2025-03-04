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
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { NgIconsModule } from '@ng-icons/core';
import { lucideUpload, lucideFile, lucideX } from '@ng-icons/lucide';
import { ValidationMessageComponent } from './validation-message.component';

@Component({
  selector: 'app-file-drop-area',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    NgIconsModule,
    ValidationMessageComponent,
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
          {{ selectedFiles.length === 1 ? 'αρχείο' : 'αρχεία' }} επιλεγμένα
        </div>

        <div class="max-h-40 overflow-y-auto custom-scrollbar">
          <div
            *ngFor="let file of selectedFiles; let i = index"
            class="flex items-center justify-between p-2 bg-white/5 rounded-lg mb-2 group"
          >
            <div class="flex items-center overflow-hidden">
              <ng-icon
                name="lucideFile"
                [size]="'20'"
                class="text-gray-400 mr-2 flex-shrink-0"
              ></ng-icon>
              <span class="text-sm text-gray-300 truncate">{{
                file.name
              }}</span>
              <span class="text-xs text-gray-500 ml-2 flex-shrink-0"
                >({{ formatFileSize(file.size) }})</span
              >
            </div>

            <button
              type="button"
              class="p-1 text-gray-500 hover:text-red-400 transition-colors opacity-0 group-hover:opacity-100 flex-shrink-0"
              (click)="removeFile(i)"
              aria-label="Remove file"
            >
              <ng-icon name="lucideX" [size]="'16'"></ng-icon>
            </button>
          </div>
        </div>
      </div>

      <!-- Error message -->
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
export class FileDropAreaComponent {
  @Input() form!: FormGroup;
  @Input() controlName!: string;
  @Input() label: string = 'Επιλογή αρχείων';
  @Input() hint?: string;
  @Input() accept: string = '*/*';
  @Input() multiple: boolean = false;
  @Input() maxFileSize: number = 5 * 1024 * 1024; // 5MB default
  @Input() maxFiles: number = 5;
  @Output() filesChange = new EventEmitter<File[]>();

  @ViewChild('dropArea') dropArea!: ElementRef;

  isDragging = false;
  selectedFiles: File[] = [];

  constructor(private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    // Initialize from existing value if present
    const currentValue = this.form.get(this.controlName)?.value;
    if (currentValue) {
      if (this.multiple && Array.isArray(currentValue)) {
        this.selectedFiles = currentValue;
      } else if (!this.multiple && currentValue instanceof File) {
        this.selectedFiles = [currentValue];
      }
    }
  }

  get isInvalid(): boolean {
    const control = this.form.get(this.controlName);
    return !!(control?.invalid && (control?.touched || control?.dirty));
  }

  get dragDropText(): string {
    return this.multiple
      ? 'Σύρετε και αφήστε αρχεία εδώ ή κάντε κλικ για επιλογή'
      : 'Σύρετε και αφήστε ένα αρχείο εδώ ή κάντε κλικ για επιλογή';
  }

  get acceptText(): string {
    if (this.accept === '*/*') return 'Αποδεκτοί όλοι οι τύποι αρχείων';

    const types = this.accept
      .split(',')
      .map((type) => {
        return type.trim().replace('.', '').toUpperCase();
      })
      .join(', ');

    return `Αποδεκτοί τύποι: ${types}`;
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
    // Convert FileList to array
    const newFiles = Array.from(fileList);

    // Check if we're exceeding max files
    if (this.multiple) {
      if (this.selectedFiles.length + newFiles.length > this.maxFiles) {
        this.setError(`Μπορείτε να επιλέξετε μέχρι ${this.maxFiles} αρχεία`);
        return;
      }
    } else {
      // If not multiple, replace existing files
      this.selectedFiles = [];
    }

    // Check file sizes
    const oversizedFiles = newFiles.filter(
      (file) => file.size > this.maxFileSize
    );
    if (oversizedFiles.length > 0) {
      this.setError(
        `Το μέγιστο μέγεθος αρχείου είναι ${this.formatFileSize(
          this.maxFileSize
        )}`
      );
      return;
    }

    // Check file types if accept is specified
    if (this.accept !== '*/*') {
      const acceptedTypes = this.accept.split(',').map((type) => type.trim());
      const invalidFiles = newFiles.filter((file) => {
        return !acceptedTypes.some((type) => {
          if (type.startsWith('.')) {
            // Check file extension
            return file.name.toLowerCase().endsWith(type.toLowerCase());
          } else {
            // Check MIME type
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

    // Add valid files
    if (this.multiple) {
      this.selectedFiles = [...this.selectedFiles, ...newFiles];
    } else {
      this.selectedFiles = [newFiles[0]];
    }

    // Update form control
    this.updateFormControl();

    // Emit change event
    this.filesChange.emit(this.selectedFiles);
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
    this.updateFormControl();
    this.filesChange.emit(this.selectedFiles);
    this.cdr.markForCheck();
  }

  updateFormControl(): void {
    const control = this.form.get(this.controlName);
    if (control) {
      if (this.selectedFiles.length > 0) {
        // Set the actual File object
        const valueToSet = this.multiple
          ? this.selectedFiles
          : this.selectedFiles[0];
        control.setValue(valueToSet);


        // For debugging
        console.log(
          `File control '${this.controlName}' updated with:`,
          this.multiple
            ? `${this.selectedFiles.length} files`
            : this.selectedFiles[0].name
        );
      } else {
        control.setValue(null);
      }
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
}