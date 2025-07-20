import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { FormInputComponent } from './form-input.component';
import { ValidationMessageComponent } from './validation-message.component';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { TranslationService } from 'src/app/common/services/translation.service';
import { CommonModule } from '@angular/common';
import { BehaviorSubject } from 'rxjs';

describe('FormInputComponent', () => {
  let component: FormInputComponent;
  let fixture: ComponentFixture<FormInputComponent>;
  let mockTranslationService: jasmine.SpyObj<TranslationService>;
  let formBuilder: FormBuilder;

  const mockTranslations = {
    'APP.AUTH.SIGNUP.SHELTER_INFO.SHELTER_NAME_PLACEHOLDER': 'Shelter Name',
    'APP.AUTH.SIGNUP.PERSONAL_INFO.EMAIL_PLACEHOLDER': 'Email Address',
    'APP.UI_COMPONENTS.VALIDATION_MESSAGE.REQUIRED': '{field} is required',
    'APP.UI_COMPONENTS.VALIDATION_MESSAGE.EMAIL_INVALID': 'Please enter a valid email'
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
        ReactiveFormsModule,
        FormInputComponent,
        ValidationMessageComponent,
        TranslatePipe
      ],
      providers: [
        FormBuilder,
        { provide: TranslationService, useValue: mockTranslationService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FormInputComponent);
    component = fixture.componentInstance;
    formBuilder = TestBed.inject(FormBuilder);

    // Setup form
    component.form = formBuilder.group({
      testField: ['', [Validators.required, Validators.email]]
    });
    component.controlName = 'testField';
    component.placeholder = 'APP.AUTH.SIGNUP.PERSONAL_INFO.EMAIL_PLACEHOLDER';
    component.type = 'email';

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should translate placeholder text in label', () => {
    const labelElement = fixture.nativeElement.querySelector('label');
    expect(labelElement.textContent.trim()).toBe('Email Address');
  });

  it('should accept translation keys as placeholder input', () => {
    component.placeholder = 'APP.AUTH.SIGNUP.SHELTER_INFO.SHELTER_NAME_PLACEHOLDER';
    fixture.detectChanges();
    
    const labelElement = fixture.nativeElement.querySelector('label');
    expect(labelElement.textContent.trim()).toBe('Shelter Name');
  });

  it('should show validation errors when form control is invalid and touched', () => {
    const control = component.form.get('testField');
    control?.setValue('');
    control?.markAsTouched();
    fixture.detectChanges();

    expect(component.isInvalid).toBe(true);
  });

  it('should emit value changes', () => {
    spyOn(component.valueChange, 'emit');
    
    const inputElement = fixture.nativeElement.querySelector('input');
    inputElement.value = 'test@example.com';
    inputElement.dispatchEvent(new Event('input'));
    
    expect(component.valueChange.emit).toHaveBeenCalledWith('test@example.com');
  });

  it('should mark control as touched on blur', () => {
    const control = component.form.get('testField');
    const inputElement = fixture.nativeElement.querySelector('input');
    
    inputElement.dispatchEvent(new Event('blur'));
    
    expect(control?.touched).toBe(true);
  });

  it('should apply error styling when invalid', () => {
    const control = component.form.get('testField');
    control?.setValue('');
    control?.markAsTouched();
    
    const inputClass = component.inputClass;
    
    expect(inputClass).toContain('border-red-500');
    expect(inputClass).toContain('focus:border-red-500');
  });

  it('should apply normal styling when valid', () => {
    const control = component.form.get('testField');
    control?.setValue('test@example.com');
    
    const inputClass = component.inputClass;
    
    expect(inputClass).toContain('focus:border-primary-500/50');
    expect(inputClass).toContain('focus:ring-primary-500/20');
  });

  it('should handle readonly state correctly', () => {
    component.readonly = true;
    fixture.detectChanges();
    
    const inputElement = fixture.nativeElement.querySelector('input');
    expect(inputElement.readOnly).toBe(true);
    expect(inputElement.classList.contains('cursor-not-allowed')).toBe(true);
    expect(inputElement.classList.contains('opacity-75')).toBe(true);
  });
});