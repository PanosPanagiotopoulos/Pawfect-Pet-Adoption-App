import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { NgxMaterialTimepickerModule } from 'ngx-material-timepicker';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

export interface OperatingHours {
  day: string;
  openTime: string;
  closeTime: string;
}

@Component({
  selector: 'app-time-picker',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    MatFormFieldModule,
    MatInputModule,
    NgxMaterialTimepickerModule
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="mb-6 border border-gray-700/50 rounded-xl p-4 hover:border-gray-600/70 transition-colors">
      <div class="flex justify-between items-center mb-4">
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
      
      <!-- Time inputs (shown only when not closed) -->
      <div *ngIf="!isClosed" class="time-picker-container" [class.mobile]="isMobile">
        <div class="flex-1">
          <mat-form-field appearance="outline" class="w-full">
            <mat-label>Ώρα ανοίγματος</mat-label>
            <input matInput
                  [formControl]="openTimeControl"
                  [ngxTimepicker]="openPicker"
                  readonly
                  placeholder="Επιλέξτε ώρα"
                  [attr.aria-label]="'Ώρα ανοίγματος για ' + day">
            <ngx-material-timepicker #openPicker
                                    [format]="24"
                                    (timeSet)="onTimeChange()">
            </ngx-material-timepicker>
          </mat-form-field>
        </div>

        <span class="mx-4 text-gray-400 self-center" *ngIf="!isMobile">έως</span>

        <div class="flex-1">
          <mat-form-field appearance="outline" class="w-full">
            <mat-label>Ώρα κλεισίματος</mat-label>
            <input matInput
                  [formControl]="closeTimeControl"
                  [ngxTimepicker]="closePicker"
                  readonly
                  placeholder="Επιλέξτε ώρα"
                  [attr.aria-label]="'Ώρα κλεισίματος για ' + day">
            <ngx-material-timepicker #closePicker
                                    [format]="24"
                                    (timeSet)="onTimeChange()">
            </ngx-material-timepicker>
          </mat-form-field>
        </div>
      </div>

      <div *ngIf="error" class="text-red-400 text-sm mt-1" role="alert">
        {{ error }}
      </div>
    </div>
  `,
  styles: [`
    .time-picker-container {
      display: flex;
      align-items: flex-start;
      gap: 1rem;
    }

    .time-picker-container.mobile {
      flex-direction: column;
    }

    :host ::ng-deep .mat-form-field-appearance-outline .mat-form-field-outline {
      background-color: rgba(255, 255, 255, 0.05);
    }

    :host ::ng-deep .mat-form-field-appearance-outline .mat-form-field-outline-thick {
      color: rgba(255, 255, 255, 0.1);
    }

    :host ::ng-deep .mat-form-field-appearance-outline.mat-focused .mat-form-field-outline-thick {
      color: var(--color-primary);
    }

    :host ::ng-deep .mat-form-field-label {
      color: rgba(255, 255, 255, 0.7);
    }

    :host ::ng-deep .mat-input-element {
      color: white;
    }
  `]
})
export class TimePickerComponent implements OnInit, OnDestroy {
  @Input() day!: string;
  @Input() initialOpenTime?: string;
  @Input() initialCloseTime?: string;

  @Output() timeChange = new EventEmitter<OperatingHours>();

  openTimeControl = new FormControl('');
  closeTimeControl = new FormControl('');
  error: string | null = null;
  isMobile = false;
  isClosed = false;

  private destroy$ = new Subject<void>();

  constructor(private breakpointObserver: BreakpointObserver) {}

  ngOnInit() {
    this.initializeTimeControls();
    this.setupBreakpointObserver();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initializeTimeControls() {
    if (this.initialOpenTime === 'closed') {
      this.isClosed = true;
    } else {
      if (this.initialOpenTime) {
        this.openTimeControl.setValue(this.initialOpenTime);
      } else {
        this.openTimeControl.setValue('09:00');
      }
      
      if (this.initialCloseTime) {
        this.closeTimeControl.setValue(this.initialCloseTime);
      } else {
        this.closeTimeControl.setValue('17:00');
      }
      
      // Emit initial value if both times are set
      if (this.openTimeControl.value && this.closeTimeControl.value) {
        this.onTimeChange();
      }
    }
  }

  private setupBreakpointObserver() {
    this.breakpointObserver
      .observe([Breakpoints.HandsetPortrait])
      .pipe(takeUntil(this.destroy$))
      .subscribe(result => {
        this.isMobile = result.matches;
      });
  }

  onClosedChange() {
    if (this.isClosed) {
      this.timeChange.emit({
        day: this.day,
        openTime: 'closed',
        closeTime: 'closed'
      });
    } else {
      // Restore previous time values or defaults
      const openTime = this.openTimeControl.value || '09:00';
      const closeTime = this.closeTimeControl.value || '17:00';
      
      this.timeChange.emit({
        day: this.day,
        openTime,
        closeTime
      });
    }
  }

  onTimeChange() {
    if (this.isClosed) return;
    
    const openTime = this.openTimeControl.value;
    const closeTime = this.closeTimeControl.value;

    if (openTime && closeTime) {
      if (this.isValidTimeRange(openTime, closeTime)) {
        this.error = null;
        this.timeChange.emit({
          day: this.day,
          openTime,
          closeTime
        });
      } else {
        this.error = 'Η ώρα κλεισίματος πρέπει να είναι μετά την ώρα ανοίγματος';
      }
    }
  }

  private isValidTimeRange(openTime: string, closeTime: string): boolean {
    const [openHour, openMinute] = openTime.split(':').map(Number);
    const [closeHour, closeMinute] = closeTime.split(':').map(Number);

    if (closeHour > openHour) {
      return true;
    }
    if (closeHour === openHour) {
      return closeMinute > openMinute;
    }
    return false;
  }
}