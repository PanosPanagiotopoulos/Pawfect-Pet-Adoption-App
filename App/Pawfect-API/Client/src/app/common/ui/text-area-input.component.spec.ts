import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { TextAreaInputComponent } from './text-area-input.component';
import { ValidationMessageComponent } from './validation-message.component';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { TranslationService } from 'src/app/common/services/translation.service';
import { CommonModule } from '@angular/common';
import { BehaviorSubject } from 'rxjs';

describe('TextAreaInputComponent', () => {
  let component: TextAreaInputComponent;
  let fixture: ComponentFixture<TextAreaInputComponent>;
  let mockTranslationService: jasmine.SpyObj<TranslationService>;
  let formBuilder: FormBuilder;

  const mockTranslations = {
    'APP.AUTH.SIGNUP.SHELTER_INFO.DESCRIPTION_LABEL': 'Description',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DESCRIPTION_PLACEHOLDER': 'Describe your shelter',
    'APP.AUTH.SIGNUP.SHELTER_INFO.DESCRIPTION_HINT': 'Please provide a short description',
    'APP.UI_COMPONENTS.VALIDATION_MESSAGE.REQUIRED': '{field} is required',
    'APP.UI_COMPONENTS.VALIDATION_MESSAGE.MIN_LENGTH': '{field} must have at least {length} characters'
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
        TextAreaInputComponent,
        ValidationMessageComponent,
        TranslatePipe
      ],
      providers: [
        FormBuilder,
        { provide: TranslationService, useValue: mockTranslationService }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(TextAreaInputComponent);
    component = fixture.componentInstance;
    formBuilder = TestBed.inject(FormBuilder);

    // Setup form
    component.form = formBuilder.group({
      description: ['', [Validators.required, Validators.minLength(10)]]
    });
    component.controlName = 'description';
    component.label = 'APP.AUTH.SIGNUP.SHELTER_INFO.DESCRIPTION_LABEL';
    component.placeholder = 'APP.AUTH.SIGNUP.SHELTER_INFO.DESCRIPTION_PLACEHOLDER';
    component.hint = 'APP.AUTH.SIGNUP.SHELTER_INFO.DESCRIPTION_HINT';
    component.rows = 4;

    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should translate label text', () => {
    const labelElement = fixture.nativeElement.querySelector('label');
    expect(labelElement.textContent.trim()).toBe('Description');
  });

  it('should translate placeholder text', () => {
    const textareaElement = fixture.nativeElement.querySelector('textarea');
    expect(textareaElement.placeholder).toBe('Describe your shelter');
  });

  it('should translate hint text when shown', () => {
    // Make sure form is valid so hint shows
    const control = component.form.get('description');
    control?.setValue('This is a valid description with enough characters');
    fixture.detectChanges();

    const hintElement = fixture.nativeElement.querySelector('p');
    expect(hintElement?.textContent?.trim()).toBe('Please provide a short description');
  });

  it('should accept translation keys as input properties', () => {
    component.label = 'APP.AUTH.SIGNUP.SHELTER_INFO.DESCRIPTION_LABEL';
    component.placeholder = 'APP.AUTH.SIGNUP.SHELTER_INFO.DESCRIPTION_PLACEHOLDER';
    component.hint = 'APP.AUTH.SIGNUP.SHELTER_INFO.DESCRIPTION_HINT';
    fixture.detectChanges();
    
    const labelElement = fixture.nativeElement.querySelector('label');
    const textareaElement = fixture.nativeElement.querySelector('textarea');
    
    expect(labelElement.textContent.trim()).toBe('Description');
    expect(textareaElement.placeholder).toBe('Describe your shelter');
  });

  it('should show validation errors when form control is invalid and touched', () => {
    const control = component.form.get('description');
    control?.setValue('');
    control?.markAsTouched();
    fixture.detectChanges();

    expect(component.isInvalid).toBe(true);
  });

  it('should emit value changes', () => {
    spyOn(component.valueChange, 'emit');
    
    const textareaElement = fixture.nativeElement.querySelector('textarea');
    textareaElement.value = 'This is a test description';
    textareaElement.dispatchEvent(new Event('input'));
    
    expect(component.valueChange.emit).toHaveBeenCalledWith('This is a test description');
  });

  it('should mark control as touched and dirty on blur', () => {
    const control = component.form.get('description');
    const textareaElement = fixture.nativeElement.querySelector('textarea');
    
    textareaElement.dispatchEvent(new Event('blur'));
    
    expect(control?.touched).toBe(true);
  });

  it('should mark control as dirty on input', () => {
    const control = component.form.get('description');
    const textareaElement = fixture.nativeElement.querySelector('textarea');
    
    textareaElement.value = 'Test input';
    textareaElement.dispatchEvent(new Event('input'));
    
    expect(control?.dirty).toBe(true);
  });

  it('should apply error styling when invalid', () => {
    const control = component.form.get('description');
    control?.setValue('');
    control?.markAsTouched();
    
    const inputClass = component.inputClass;
    
    expect(inputClass).toContain('border-red-500');
    expect(inputClass).toContain('focus:border-red-500');
  });

  it('should apply normal styling when valid', () => {
    const control = component.form.get('description');
    control?.setValue('This is a valid description with enough characters');
    
    const inputClass = component.inputClass;
    
    expect(inputClass).toContain('focus:border-primary-500/50');
    expect(inputClass).toContain('focus:ring-primary-500/20');
  });

  it('should hide hint when form control is invalid', () => {
    const control = component.form.get('description');
    control?.setValue('');
    control?.markAsTouched();
    fixture.detectChanges();

    const hintElement = fixture.nativeElement.querySelector('p');
    expect(hintElement).toBeNull();
  });

  it('should set correct number of rows', () => {
    component.rows = 6;
    fixture.detectChanges();
    
    const textareaElement = fixture.nativeElement.querySelector('textarea');
    expect(textareaElement.rows).toBe(6);
  });
});