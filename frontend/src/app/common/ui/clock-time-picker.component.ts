import { Component, Input, Output, EventEmitter, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgIconsModule } from '@ng-icons/core';
import { lucideClock, lucideX } from '@ng-icons/lucide';

@Component({
  selector: 'app-clock-time-picker',
  standalone: true,
  imports: [CommonModule, FormsModule, NgIconsModule],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mb-4 border border-gray-700/50 rounded-xl p-4 hover:border-gray-600/70 transition-colors">
      <div class="flex justify-between items-center mb-3">
        <h4 class="text-white font-medium">{{ day }}</h4>
        
        <!-- Closed toggle -->
        <div class="flex items-center">
          <label class="relative inline-flex items-center cursor-pointer">
            <input
              type="checkbox"
              [(ngModel)]="isClosed"
              (change)="onClosedChange()"
              class="sr-only peer"
            />
            <div
              class="w-11 h-6 bg-gray-700 peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:rounded-full after:h-5 after:w-5 after:transition-all peer-checked:bg-red-600"
            ></div>
          </label>
          <span class="ml-3 text-sm text-gray-300">{{ isClosed ? 'Κλειστό' : 'Ανοιχτό' }}</span>
        </div>
      </div>
      
      <!-- Time selection (shown only when not closed) -->
      <div *ngIf="!isClosed" class="flex items-center justify-between">
        <div class="flex items-center space-x-2">
          <span class="text-gray-400 text-sm">{{ openTime }}</span>
          <button 
            (click)="openTimePicker('open')" 
            class="p-2 bg-gray-700/50 hover:bg-gray-600/50 rounded-lg transition-colors"
            aria-label="Select opening time"
            type="button"
          >
            <ng-icon name="lucideClock" [size]="'18'" class="text-gray-300"></ng-icon>
          </button>
        </div>
        
        <span class="text-gray-400 mx-2">έως</span>
        
        <div class="flex items-center space-x-2">
          <span class="text-gray-400 text-sm">{{ closeTime }}</span>
          <button 
            (click)="openTimePicker('close')" 
            class="p-2 bg-gray-700/50 hover:bg-gray-600/50 rounded-lg transition-colors"
            aria-label="Select closing time"
            type="button"
          >
            <ng-icon name="lucideClock" [size]="'18'" class="text-gray-300"></ng-icon>
          </button>
        </div>
      </div>
      
      <!-- Time picker modal -->
      <div *ngIf="showTimePicker" class="fixed inset-0 z-50 flex items-center justify-center">
        <!-- Backdrop -->
        <div class="absolute inset-0 bg-black/50 backdrop-blur-sm" (click)="closeTimePicker()"></div>
        
        <!-- Modal -->
        <div class="relative bg-gray-800 rounded-xl p-6 shadow-xl max-w-md w-full mx-4 transform transition-all">
          <button 
            (click)="closeTimePicker()" 
            class="absolute top-3 right-3 p-1 text-gray-400 hover:text-white"
            aria-label="Close time picker"
            type="button"
          >
            <ng-icon name="lucideX" [size]="'20'"></ng-icon>
          </button>
          
          <h3 class="text-xl font-semibold text-white mb-4">
            {{ currentPickerType === 'open' ? 'Επιλογή ώρας ανοίγματος' : 'Επιλογή ώρας κλεισίματος' }}
          </h3>
          
          <div class="mb-6">
            <label class="block text-sm font-medium text-gray-400 mb-2">Επιλέξτε ώρα</label>
            <input 
              type="time" 
              [(ngModel)]="tempTime" 
              class="w-full px-4 py-3 bg-white/5 border border-white/10 rounded-xl text-white 
                     focus:border-primary-500/50 focus:ring-2 focus:ring-primary-500/20 focus:outline-none"
            />
          </div>
          
          <div class="flex justify-end space-x-3">
            <button 
              (click)="closeTimePicker()" 
              class="px-4 py-2 border border-gray-600 text-gray-300 rounded-lg hover:bg-white/5 transition-all"
              type="button"
            >
              Ακύρωση
            </button>
            <button 
              (click)="confirmTimeSelection()" 
              class="px-4 py-2 bg-gradient-to-r from-primary-600 to-accent-600 text-white rounded-lg 
                     hover:shadow-lg hover:shadow-primary-500/20 transition-all"
              type="button"
            >
              Επιβεβαίωση
            </button>
          </div>
        </div>
      </div>
      
      <!-- Error message -->
      <div *ngIf="error" class="text-red-400 text-sm mt-2">{{ error }}</div>
    </div>
  `,
  styles: [`
    .time-picker-container {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
  `]
})
export class ClockTimePickerComponent implements OnInit {
  @Input() day!: string;
  @Input() initialValue: string = '09:00,17:00';
  @Output() timeChange = new EventEmitter<{day: string, openTime: string, closeTime: string}>();

  openTime: string = '09:00';
  closeTime: string = '17:00';
  isClosed: boolean = false;
  error: string | null = null;
  
  // Time picker modal
  showTimePicker: boolean = false;
  currentPickerType: 'open' | 'close' = 'open';
  tempTime: string = '';

  constructor(private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.parseInitialValue();
  }

  parseInitialValue() {
    if (this.initialValue === 'closed') {
      this.isClosed = true;
    } else {
      const parts = this.initialValue.split(',');
      if (parts.length === 2) {
        this.openTime = parts[0];
        this.closeTime = parts[1];
      }
    }
  }

  onClosedChange() {
    if (this.isClosed) {
      this.emitTimeChange('closed', 'closed');
    } else {
      this.emitTimeChange(this.openTime, this.closeTime);
    }
  }

  openTimePicker(type: 'open' | 'close') {
    this.currentPickerType = type;
    this.tempTime = type === 'open' ? this.openTime : this.closeTime;
    this.showTimePicker = true;
    this.cdr.markForCheck();
  }

  closeTimePicker() {
    this.showTimePicker = false;
    this.cdr.markForCheck();
  }

  confirmTimeSelection() {
    if (this.tempTime) {
      if (this.currentPickerType === 'open') {
        this.openTime = this.tempTime;
      } else {
        this.closeTime = this.tempTime;
      }
      
      this.validateTimeRange();
      this.closeTimePicker();
      
      if (!this.error) {
        this.emitTimeChange(this.openTime, this.closeTime);
      }
    }
  }

  validateTimeRange() {
    if (this.openTime && this.closeTime) {
      if (this.openTime >= this.closeTime) {
        this.error = 'Η ώρα κλεισίματος πρέπει να είναι μετά την ώρα ανοίγματος';
        return false;
      } else {
        this.error = null;
        return true;
      }
    }
    return true;
  }

  emitTimeChange(openTime: string, closeTime: string) {
    this.timeChange.emit({
      day: this.day,
      openTime,
      closeTime
    });
  }
}