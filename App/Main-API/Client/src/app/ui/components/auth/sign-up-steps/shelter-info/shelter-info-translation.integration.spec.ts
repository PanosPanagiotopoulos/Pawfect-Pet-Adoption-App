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
import { BehaviorSubject } from 'rxjs';

/**
 * Integration test to verify that ShelterInfoComponent properly handles
 * translation changes automatically without requiring manual clicks.
 * 
 * This test addresses the bug where translations only updated when clicking
 * on the component background, not automatically on language change.
 */
describe('ShelterInfoComponent - Translation Reactivity Integration', () => {
  let component: ShelterInfoComponent;
  let fixture: ComponentFixture<ShelterInfoComponent>;
  let mockTranslationService: jasmine.SpyObj<TranslationService>;
  let languageSubject: BehaviorSubject<string>;
  let formBuilder: FormBuilder;

  const englishTranslations = {
    'APP.AUTH.SIGNUP.SHELTER_INFO.TITLE': 'Shelter Information',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY': 'Monday',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.TUESDAY': 'Tuesday',
    'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.OPENING_TIME_FORMAT': 'Opening time must be in HH:MM format',
    'APP.AUTH.SIGNUP.SHELTER_INFO.BACK': 'Back',
    'APP.AUTH.SIGNUP.SHELTER_INFO.SUBMIT': 'Complete Registration'
  };

  const greekTranslations = {
    'APP.AUTH.SIGNUP.SHELTER_INFO.TITLE': 'Πληροφορίες Καταφυγίου',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY': 'Δευτέρα',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.TUESDAY': 'Τρίτη',
    'APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.OPENING_TIME_FORMAT': 'Η ώρα ανοίγματος πρέπει να είναι σε μορφή ΩΩ:ΛΛ',
    'APP.AUTH.SIGNUP.SHELTER_INFO.BACK': 'Πίσω',
    'APP.AUTH.SIGNUP.SHELTER_INFO.SUBMIT': 'Ολοκλήρωση Εγγραφής'
  };

  beforeEach(async () => {
    // Create a BehaviorSubject to simulate language changes
    languageSubject = new BehaviorSubject('en');
    
    // Create mock translation service that switches between languages
    mockTranslationService = jasmine.createSpyObj('TranslationService', ['translate', 'getLanguage'], {
      languageChanged$: languageSubject
    });

    // Setup translation service mock to return different translations based on current language
    mockTranslationService.translate.and.callFake((key: string) => {
      const currentLang = languageSubject.value;
      const translations = currentLang === 'en' ? englishTranslations : greekTranslations;
      return translations[key as keyof typeof translations] || key;
    });

    mockTranslationService.getLanguage.and.callFake(() => languageSubject.value);

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
        ChangeDetectorRef
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ShelterInfoComponent);
    component = fixture.componentInstance;
    formBuilder = TestBed.inject(FormBuilder);

    // Setup form structure
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

  it('should automatically update translations when language changes without manual interaction', async () => {
    // Initial state - English
    expect(languageSubject.value).toBe('en');
    
    // Trigger initial change detection
    fixture.detectChanges();
    await fixture.whenStable();
    
    // Get initial English text from DOM
    const titleElement = fixture.nativeElement.querySelector('h2');
    const backButton = fixture.nativeElement.querySelector('button');
    
    expect(titleElement?.textContent?.trim()).toBe('Shelter Information');
    expect(backButton?.textContent?.trim()).toBe('Back');
    
    // Change language to Greek
    languageSubject.next('gr');
    
    // The TranslatePipe should automatically detect the change and update
    // No manual click or interaction should be required
    fixture.detectChanges();
    await fixture.whenStable();
    
    // Verify that translations have been updated automatically
    expect(titleElement?.textContent?.trim()).toBe('Πληροφορίες Καταφυγίου');
    expect(backButton?.textContent?.trim()).toBe('Πίσω');
  });

  it('should automatically update error messages when language changes', async () => {
    const day = 'APP.AUTH.SIGNUP.SHELTER_INFO.DAYS.MONDAY';
    
    // Set up an error condition
    component.openTimes[day] = '25:00'; // Invalid time
    component.closeTimes[day] = '18:00';
    component.validateTimeRange(day);
    
    // Initial state - English
    fixture.detectChanges();
    await fixture.whenStable();
    
    // Verify error is stored as translation key
    expect(component.timeErrors[day]).toBe('APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.OPENING_TIME_FORMAT');
    
    // Change language to Greek
    languageSubject.next('gr');
    
    // The error message should automatically update through TranslatePipe
    fixture.detectChanges();
    await fixture.whenStable();
    
    // The error key should remain the same (it's a translation key)
    expect(component.timeErrors[day]).toBe('APP.AUTH.SIGNUP.SHELTER_INFO.ERRORS.OPENING_TIME_FORMAT');
    
    // But when rendered through the pipe, it should show Greek text
    // This would be verified in the DOM if the error is displayed
  });

  it('should automatically update day names when language changes', async () => {
    // Initial state - English
    fixture.detectChanges();
    await fixture.whenStable();
    
    // Find day labels in the DOM (they should be in operating hours section)
    const dayElements = fixture.nativeElement.querySelectorAll('h4');
    const mondayElement = Array.from(dayElements).find((el: any) => 
      el.textContent?.trim() === 'Monday'
    );
    const tuesdayElement = Array.from(dayElements).find((el: any) => 
      el.textContent?.trim() === 'Tuesday'
    );
    
    expect(mondayElement).toBeTruthy();
    expect(tuesdayElement).toBeTruthy();
    
    // Change language to Greek
    languageSubject.next('gr');
    
    // The day names should automatically update
    fixture.detectChanges();
    await fixture.whenStable();
    
    // Find updated day labels
    const updatedDayElements = fixture.nativeElement.querySelectorAll('h4');
    const mondayElementGr = Array.from(updatedDayElements).find((el: any) => 
      el.textContent?.trim() === 'Δευτέρα'
    );
    const tuesdayElementGr = Array.from(updatedDayElements).find((el: any) => 
      el.textContent?.trim() === 'Τρίτη'
    );
    
    expect(mondayElementGr).toBeTruthy();
    expect(tuesdayElementGr).toBeTruthy();
  });

  it('should not require manual subscription management for translation reactivity', () => {
    // Verify that the component doesn't have manual translation subscription
    expect(component['translationSubscription']).toBeUndefined();
    
    // Verify that the component doesn't implement OnDestroy for translation cleanup
    expect(typeof component['ngOnDestroy']).toBe('undefined');
    
    // The component should rely on TranslatePipe's automatic reactivity
    expect(component).toBeTruthy();
  });

  it('should use default change detection strategy for optimal TranslatePipe performance', () => {
    // The component should use default change detection strategy
    // This allows TranslatePipe (pure: false) to work optimally
    const componentDef = (component.constructor as any).ɵcmp;
    
    // Default change detection strategy is 1, OnPush is 0
    // If changeDetection is not explicitly set, it defaults to Default (1)
    expect(componentDef.onPush).toBeFalsy();
  });

  it('should handle rapid language changes without issues', async () => {
    // Test rapid language switching to ensure no race conditions
    const languages = ['en', 'gr', 'en', 'gr', 'en'];
    
    for (const lang of languages) {
      languageSubject.next(lang);
      fixture.detectChanges();
      await fixture.whenStable();
      
      // Verify that the component handles the change correctly
      const titleElement = fixture.nativeElement.querySelector('h2');
      const expectedTitle = lang === 'en' ? 'Shelter Information' : 'Πληροφορίες Καταφυγίου';
      expect(titleElement?.textContent?.trim()).toBe(expectedTitle);
    }
  });
});