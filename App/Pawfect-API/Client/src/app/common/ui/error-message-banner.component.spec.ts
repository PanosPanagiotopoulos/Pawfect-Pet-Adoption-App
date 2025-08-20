import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ErrorMessageBannerComponent, ErrorDetails } from './error-message-banner.component';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { TranslationService } from 'src/app/common/services/translation.service';
import { NgIconsModule } from '@ng-icons/core';
import { CommonModule } from '@angular/common';
import { BehaviorSubject } from 'rxjs';

describe('ErrorMessageBannerComponent', () => {
  let component: ErrorMessageBannerComponent;
  let fixture: ComponentFixture<ErrorMessageBannerComponent>;
  let mockTranslationService: jasmine.SpyObj<TranslationService>;

  const mockTranslations = {
    'APP.SERVICES.ERROR_HANDLER.GENERAL_ERROR': 'General Error',
    'APP.SERVICES.ERROR_HANDLER.GENERAL_ERROR_MESSAGE': 'An unexpected error occurred. Please try again.',
    'APP.SERVICES.ERROR_HANDLER.VALIDATION_ERROR': 'Validation Error',
    'APP.SERVICES.ERROR_HANDLER.VALIDATION_ERROR_MESSAGE': 'Please check your input and try again.',
    'APP.UI_COMPONENTS.PET_DETAILS.DISMISS': 'Dismiss'
  };

  beforeEach(async () => {
    mockTranslationService = jasmine.createSpyObj('TranslationService', ['translate'], {
      languageChanged$: new BehaviorSubject('en')
    });

    mockTranslationService.translate.and.callFake((key: string) => {
      return mockTranslations[key as keyof typeof mockTranslations] || key;
    });

    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        NgIconsModule,
        ErrorMessageBannerComponent,
        TranslatePipe
      ],
      providers: [
        { provide: TranslationService, useValue: mockTranslationService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ErrorMessageBannerComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not display when no error is provided', () => {
    component.error = undefined;
    fixture.detectChanges();

    const errorElement = fixture.nativeElement.querySelector('[role="alert"]');
    expect(errorElement).toBeNull();
  });

  it('should display error message with translation', () => {
    const error: ErrorDetails = {
      title: 'APP.SERVICES.ERROR_HANDLER.GENERAL_ERROR',
      message: 'APP.SERVICES.ERROR_HANDLER.GENERAL_ERROR_MESSAGE',
      type: 'error'
    };

    component.error = error;
    fixture.detectChanges();

    const titleElement = fixture.nativeElement.querySelector('h3');
    const messageElement = fixture.nativeElement.querySelector('div[class*="text-red-600"]');

    expect(titleElement.textContent.trim()).toBe('General Error');
    expect(messageElement.textContent.trim()).toBe('An unexpected error occurred. Please try again.');
  });

  it('should display error message without title', () => {
    const error: ErrorDetails = {
      message: 'APP.SERVICES.ERROR_HANDLER.VALIDATION_ERROR_MESSAGE',
      type: 'error'
    };

    component.error = error;
    fixture.detectChanges();

    const titleElement = fixture.nativeElement.querySelector('h3');
    const messageElement = fixture.nativeElement.querySelector('div[class*="text-red-600"]');

    expect(titleElement).toBeNull();
    expect(messageElement.textContent.trim()).toBe('Please check your input and try again.');
  });

  it('should apply correct styling for error type', () => {
    const error: ErrorDetails = {
      message: 'APP.SERVICES.ERROR_HANDLER.GENERAL_ERROR_MESSAGE',
      type: 'error'
    };

    component.error = error;
    fixture.detectChanges();

    expect(component.getBackgroundClass()).toBe('bg-red-500/5 border border-red-500/20 backdrop-blur-sm');
    expect(component.getIconClass()).toBe('text-red-500');
    expect(component.getIcon()).toBe('lucideAlertCircle');
  });

  it('should apply correct styling for warning type', () => {
    const error: ErrorDetails = {
      message: 'APP.SERVICES.ERROR_HANDLER.GENERAL_ERROR_MESSAGE',
      type: 'warning'
    };

    component.error = error;
    fixture.detectChanges();

    expect(component.getBackgroundClass()).toBe('bg-yellow-500/5 border border-yellow-500/20 backdrop-blur-sm');
    expect(component.getIconClass()).toBe('text-yellow-500');
    expect(component.getIcon()).toBe('lucideAlertCircle');
  });

  it('should apply correct styling for info type', () => {
    const error: ErrorDetails = {
      message: 'APP.SERVICES.ERROR_HANDLER.GENERAL_ERROR_MESSAGE',
      type: 'info'
    };

    component.error = error;
    fixture.detectChanges();

    expect(component.getBackgroundClass()).toBe('bg-blue-500/5 border border-blue-500/20 backdrop-blur-sm');
    expect(component.getIconClass()).toBe('text-blue-500');
    expect(component.getIcon()).toBe('lucideInfo');
  });

  it('should clear error when close button is clicked', () => {
    const error: ErrorDetails = {
      message: 'APP.SERVICES.ERROR_HANDLER.GENERAL_ERROR_MESSAGE',
      type: 'error'
    };

    component.error = error;
    fixture.detectChanges();

    const closeButton = fixture.nativeElement.querySelector('button');
    closeButton.click();

    expect(component.error).toBeUndefined();
  });

  it('should translate dismiss button text', () => {
    const error: ErrorDetails = {
      message: 'APP.SERVICES.ERROR_HANDLER.GENERAL_ERROR_MESSAGE',
      type: 'error'
    };

    component.error = error;
    fixture.detectChanges();

    const dismissText = fixture.nativeElement.querySelector('.sr-only');
    expect(dismissText.textContent.trim()).toBe('Dismiss');
  });

  it('should handle translation keys that do not exist', () => {
    const error: ErrorDetails = {
      title: 'NON_EXISTENT_TITLE_KEY',
      message: 'NON_EXISTENT_MESSAGE_KEY',
      type: 'error'
    };

    component.error = error;
    fixture.detectChanges();

    const titleElement = fixture.nativeElement.querySelector('h3');
    const messageElement = fixture.nativeElement.querySelector('div[class*="text-red-600"]');

    // Should display the key itself when translation is not found
    expect(titleElement.textContent.trim()).toBe('NON_EXISTENT_TITLE_KEY');
    expect(messageElement.textContent.trim()).toBe('NON_EXISTENT_MESSAGE_KEY');
  });

  it('should handle plain text messages (non-translation keys)', () => {
    const error: ErrorDetails = {
      title: 'Plain Title',
      message: 'Plain error message',
      type: 'error'
    };

    component.error = error;
    fixture.detectChanges();

    const titleElement = fixture.nativeElement.querySelector('h3');
    const messageElement = fixture.nativeElement.querySelector('div[class*="text-red-600"]');

    // Should display plain text as-is
    expect(titleElement.textContent.trim()).toBe('Plain Title');
    expect(messageElement.textContent.trim()).toBe('Plain error message');
  });
});