import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
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
    MatFormFieldModule,
    MatInputModule,
    NgxMaterialTimepickerModule
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="time-picker-container mb-6" [class.mobile]="isMobile">
      <div class="flex-1">
        <mat-form-field appearance="outline" class="w-full">
          <mat-label>Opening Time</mat-label>
          <input matInput
                 [formControl]="openTimeControl"
                 [ngxTimepicker]="openPicker"
                 readonly
                 placeholder="Select time">
          <ngx-material-timepicker #openPicker
                                  [format]="24"
                                  (timeSet)="onTimeChange()">
          </ngx-material-timepicker>
        </mat-form-field>
      </div>

      <span class="mx-4 text-gray-400 self-center" *ngIf="!isMobile">to</span>

      <div class="flex-1">
        <mat-form-field appearance="outline" class="w-full">
          <mat-label>Closing Time</mat-label>
          <input matInput
                 [formControl]="closeTimeControl"
                 [ngxTimepicker]="closePicker"
                 readonly
                 placeholder="Select time">
          <ngx-material-timepicker #closePicker
                                  [format]="24"
                                  (timeSet)="onTimeChange()">
          </ngx-material-timepicker>
        </mat-form-field>
      </div>
    </div>

    <div *ngIf="error" class="text-red-400 text-sm mt-1">
      {{ error }}
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
    if (this.initialOpenTime) {
      this.openTimeControl.setValue(this.initialOpenTime);
    }
    if (this.initialCloseTime) {
      this.closeTimeControl.setValue(this.initialCloseTime);
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

  onTimeChange() {
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
        this.error = 'Closing time must be after opening time';
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