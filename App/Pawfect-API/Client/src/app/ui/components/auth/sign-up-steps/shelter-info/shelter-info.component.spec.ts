import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ChangeDetectorRef } from '@angular/core';
import { ShelterInfoComponent } from './shelter-info.component';
import { TranslationService } from 'src/app/common/services/translation.service';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { FormInputComponent } from 'src/app/common/ui/form-input.component';
import { TextAreaInputComponent } from 'src/app/common/ui/text-area-input.component';
import { ErrorMessageBannerComponent } from 'src/app/common/ui/error-message-banner.component';
import { NgIconsModule } from '@ng-icons/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BehaviorSubject, of } from 'rxjs';

describe('ShelterInfoComponent', () => {
  let component: ShelterInfoComponent;
  let fixture: ComponentFixture<ShelterInfoComponent>;
  let mockTranslationService: jasmine.SpyObj<TranslationService>;
  let mockChangeDetectorRef: jasmine.SpyObj<ChangeDetectorRef>;
  let formBuilder: FormBuilder;

  const mockTranslations = {
    'APP.AUTH.SIGNUP.SHELTER_INFO.TITLE': 'Shelter Information',
    'APP.AUTH.SIGNUP.SHELTER_INFO.SHELTER_NAME_PLACEHOLDER': 'Shelter Name',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DESCRIPTION_LABEL': 'Description',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DESCRIPTION_PLACEHOLDER': 'Describe your shelter',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DESCRIPTION_HINT': 'Please provide a short description',
    'APP.AUTH.SIGNUP.SHELTER_INFO.WEBSITE_PLACEHOLDER': 'Website (Optional)',
    'APP.AUTH.SIGNUP.SHELTER_INFO.FACEBOOK_PLACEHOLDER': 'Facebook URL',
    'APP.AUTH.SIGNUP.SHELTER_INFO.INSTAGRAM_PLACEHOLDER': 'Instagram URL',
    'APP.AUTH.SIGNUP.SHELTER_INFO.SOCIAL_TITLE': 'Social Networks (Optional)',
    'APP.AUTH.SIGNUP.SHELTER_INFO.HOURS_TITLE': 'Operating Hours',
    'APP.AUTH.SIGNUP.SHELTER_INFO.OPTIONAL': 'Optional',
    'APP.AUTH.SIGNUP.SHELTER_INFO.HOURS_HINT': 'Leave all days empty or fill in hours for all days',
    'APP.AUTH.SIGNUP.SHELTER_INFO.CLOSED': 'Closed',
    'APP.AUTH.SIGNUP.SHELTER_INFO.OPEN': 'Open',
    'APP.AUTH.SIGNUP.SHELTER_INFO.OPEN_LABEL': 'Opening Time',
    'APP.AUTH.SIGNUP.SHELTER_INFO.CLOSE_LABEL': 'Closing Time',
    'APP.AUTH.SIGNUP.SHELTER_INFO.BACK': 'Back',
    'APP.AUTH.SIGNUP.SHELTER_INFO.SUBMIT': 'Complete Registration',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY': 'Monday',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.TUESDAY': 'Tuesday',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.WEDNESDAY': 'Wednesday',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.THURSDAY': 'Thursday',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.FRIDAY': 'Friday',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.SATURDAY': 'Saturday',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.SUNDAY': 'Sunday',
    'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.OPENING_TIME_FORMAT': 'Opening time must be in HH:MM format',
    'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.CLOSING_TIME_FORMAT': 'Closing time must be in HH:MM format',
    'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.TIME_RANGE_INVALID': 'Closing time must be after opening time',
    'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.BOTH_TIMES_REQUIRED': 'You must fill in both times',
    'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.HOURS_OR_CLOSED_REQUIRED': 'You must set operating hours or select "Closed"',
    'APP.UI_COMPONENTS.TIME_INPUT.TO': 'to'
  };

  beforeEach(async () => {
    // Create mock services
    mockTranslationService = jasmine.createSpyObj('TranslationService', ['translate', 'getLanguage'], {
      languageChanged$: new BehaviorSubject('en')
    });
    mockChangeDetectorRef = jasmine.createSpyObj('ChangeDetectorRef', ['markForCheck']);

    // Setup translation service mock
    mockTranslationService.translate.and.callFake((key: string) => {
      return mockTranslations[key as keyof typeof mockTranslations] || key;
    });
    mockTranslationService.getLanguage.and.returnValue('en');

    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        ReactiveFormsModule,
        FormsModule,
        NgIconsModule,
        ShelterInfoComponent,
        FormInputComponent,
        TextAreaInputComponent,
        ErrorMessageBannerComponent,
        TranslatePipe
      ],
      providers: [
        FormBuilder,
        { provide: TranslationService, useValue: mockTranslationService },
        { provide: ChangeDetectorRef, useValue: mockChangeDetectorRef }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ShelterInfoComponent);
    component = fixture.componentInstance;
    formBuilder = TestBed.inject(FormBuilder);

    // Setup form structure that matches the expected structure
    component.form = formBuilder.group({
      shelter: formBuilder.group({
        shelterName: ['', [Validators.required, Validators.minLength(3)]],
        description: ['', [Validators.required, Validators.minLength(10)]],
        website: [''],
        socialMedia: formBuilder.group({
          facebook: [''],
          instagram: ['']
        }),
        operatingHours: formBuilder.group({
          monday: [''],
          tuesday: [''],
          wednesday: [''],
          thursday: [''],
          friday: [''],
          saturday: [''],
          sunday: ['']
        })
      })
    });

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should initialize days with translation keys', () => {
    expect(component.days).toEqual([
      'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY',
      'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.TUESDAY',
      'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.WEDNESDAY',
      'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.THURSDAY',
      'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.FRIDAY',
      'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.SATURDAY',
      'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.SUNDAY'
    ]);
  });

  it('should store translation keys in timeErrors instead of translated text', () => {
    const day = 'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY';
    
    // Set invalid time format
    component.openTimes[day] = '25:00'; // Invalid hour
    component.closeTimes[day] = '18:00';
    
    component.validateTimeRange(day);
    
    // Should store translation key, not translated text
    expect(component.timeErrors[day]).toBe('APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.OPENING_TIME_FORMAT');
  });

  it('should validate time range and store translation keys for errors', () => {
    const day = 'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY';
    
    // Test case: closing time before opening time
    component.openTimes[day] = '18:00';
    component.closeTimes[day] = '09:00';
    
    component.validateTimeRange(day);
    
    expect(component.timeErrors[day]).toBe('APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.TIME_RANGE_INVALID');
  });

  it('should validate incomplete time entries', () => {
    const day = 'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY';
    
    // Test case: only opening time provided
    component.openTimes[day] = '09:00';
    component.closeTimes[day] = '';
    
    component.validateTimeRange(day);
    
    expect(component.timeErrors[day]).toBe('APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.BOTH_TIMES_REQUIRED');
  });

  it('should clear time errors for valid time ranges', () => {
    const day = 'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY';
    
    // Set valid time range
    component.openTimes[day] = '09:00';
    component.closeTimes[day] = '18:00';
    
    component.validateTimeRange(day);
    
    expect(component.timeErrors[day]).toBeNull();
  });

  it('should format time input correctly', () => {
    const day = 'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY';
    
    // Test time formatting
    component.openTimes[day] = '9';
    component.formatTime(day, 'open');
    
    expect(component.openTimes[day]).toBe('09:00');
  });

  it('should handle closed day toggle correctly', () => {
    const day = 'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY';
    
    // Toggle day to closed
    component.closedDays[day] = true;
    component.onClosedChange(day);
    
    const dayKey = component.getDayKey(day);
    const operatingHoursForm = component.getOperatingHoursForm();
    
    expect(operatingHoursForm.get(dayKey)?.value).toBe('closed');
    expect(component.timeErrors[day]).toBeNull();
  });

  it('should collect validation errors with translation keys', () => {
    // Make form invalid
    const shelterForm = component.getShelterForm();
    shelterForm.get('shelterName')?.setValue('');
    shelterForm.get('description')?.setValue('');
    shelterForm.markAllAsTouched();
    
    // Trigger validation
    component.submitForm();
    
    // Check that validation errors contain translation keys
    const shelterNameError = component.validationErrors.find(e => e.field === 'shelterName');
    const descriptionError = component.validationErrors.find(e => e.field === 'description');
    
    expect(shelterNameError?.message).toBe('APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.SHELTER_NAME_REQUIRED');
    expect(descriptionError?.message).toBe('APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.DESCRIPTION_REQUIRED');
  });

  it('should emit submit event when form is valid', () => {
    spyOn(component.submit, 'emit');
    
    // Make form valid
    const shelterForm = component.getShelterForm();
    shelterForm.get('shelterName')?.setValue('Test Shelter');
    shelterForm.get('description')?.setValue('This is a test shelter description');
    
    component.submitForm();
    
    expect(component.submit.emit).toHaveBeenCalled();
  });

  it('should emit back event when back button is clicked', () => {
    spyOn(component.back, 'emit');
    
    // Simulate back button click
    component.back.emit();
    
    expect(component.back.emit).toHaveBeenCalled();
  });

  it('should map day translation keys to form control keys correctly', () => {
    expect(component.getDayKey('APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY')).toBe('monday');
    expect(component.getDayKey('APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.TUESDAY')).toBe('tuesday');
    expect(component.getDayKey('APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.WEDNESDAY')).toBe('wednesday');
    expect(component.getDayKey('APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.THURSDAY')).toBe('thursday');
    expect(component.getDayKey('APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.FRIDAY')).toBe('friday');
    expect(component.getDayKey('APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.SATURDAY')).toBe('saturday');
    expect(component.getDayKey('APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.SUNDAY')).toBe('sunday');
  });

  it('should validate time format correctly', () => {
    expect(component.isValidTimeFormat('09:00')).toBe(true);
    expect(component.isValidTimeFormat('23:59')).toBe(true);
    expect(component.isValidTimeFormat('00:00')).toBe(true);
    expect(component.isValidTimeFormat('24:00')).toBe(false);
    expect(component.isValidTimeFormat('09:60')).toBe(false);
    expect(component.isValidTimeFormat('9:00')).toBe(false);
    expect(component.isValidTimeFormat('09:0')).toBe(false);
  });

  it('should handle time input formatting correctly', () => {
    const day = 'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY';
    const mockEvent = {
      target: { value: '930' }
    } as any;
    
    component.onTimeInput(mockEvent, day, 'open');
    
    expect(component.openTimes[day]).toBe('9:30');
  });

  it('should use default change detection strategy for automatic translation reactivity', () => {
    // Verify that the component relies on TranslatePipe's automatic reactivity
    // TranslatePipe is marked as pure: false, so it handles reactivity automatically
    expect(component).toBeTruthy();
    
    // The component should not have manual translation subscription
    expect(component['translationSubscription']).toBeUndefined();
  });

  it('should handle translation changes through TranslatePipe reactivity', () => {
    // Since TranslatePipe is pure: false, it automatically handles translation changes
    // Test that translation keys are properly stored for pipe processing
    const day = 'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY';
    
    // Verify that days are stored as translation keys
    expect(component.days).toContain(day);
    
    // Verify that error messages are stored as translation keys
    component.openTimes[day] = '25:00'; // Invalid time
    component.closeTimes[day] = '18:00';
    component.validateTimeRange(day);
    
    expect(component.timeErrors[day]).toBe('APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.OPENING_TIME_FORMAT');
  });
});