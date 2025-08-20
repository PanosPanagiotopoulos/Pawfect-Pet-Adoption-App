import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ChangeDetectorRef } from '@angular/core';
import { of, Subject } from 'rxjs';
import { ShelterInfoComponent } from './shelter-info.component';
import { TranslationService } from 'src/app/common/services/translation.service';

const mockShelter = {
  shelterName: 'Test Shelter',
  description: 'A test shelter',
  user: {
    profilePhoto: { sourceUrl: '' },
    location: {
      address: 'Test St',
      number: '1',
      city: 'Testville',
      zipCode: '12345',
    },
  },
  website: 'https://example.com',
  socialMedia: {},
  operatingHours: {
    monday: '09:00,17:00',
    tuesday: '09:00,17:00',
    wednesday: '09:00,17:00',
    thursday: '09:00,17:00',
    friday: '09:00,17:00',
    saturday: 'closed',
    sunday: 'closed',
  },
};

describe('ShelterInfoComponent', () => {
  let component: ShelterInfoComponent;
  let fixture: ComponentFixture<ShelterInfoComponent>;
  let translationService: jasmine.SpyObj<TranslationService>;
  let languageChanged$: Subject<string>;

  beforeEach(async () => {
    languageChanged$ = new Subject<string>();
    translationService = jasmine.createSpyObj('TranslationService', ['translate'], {
      languageChanged$: languageChanged$.asObservable(),
    });
    translationService.translate.and.callFake((key: string) => {
      if (key === 'APP.ADOPT.DAYS.MONDAY') return 'Monday';
      if (key === 'APP.ADOPT.DAYS.TUESDAY') return 'Tuesday';
      if (key === 'APP.ADOPT.DAYS.WEDNESDAY') return 'Wednesday';
      if (key === 'APP.ADOPT.DAYS.THURSDAY') return 'Thursday';
      if (key === 'APP.ADOPT.DAYS.FRIDAY') return 'Friday';
      if (key === 'APP.ADOPT.DAYS.SATURDAY') return 'Saturday';
      if (key === 'APP.ADOPT.DAYS.SUNDAY') return 'Sunday';
      if (key === 'APP.ADOPT.CLOSED') return 'Closed';
      return key;
    });

    await TestBed.configureTestingModule({
      declarations: [ShelterInfoComponent],
      providers: [
        { provide: TranslationService, useValue: translationService },
        ChangeDetectorRef,
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(ShelterInfoComponent);
    component = fixture.componentInstance;
    component.shelter = mockShelter as any;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should update days when language changes', () => {
    // Initial days
    expect(component.days[0].label).toBe('Monday');
    // Simulate language change
    translationService.translate.and.callFake((key: string) => {
      if (key === 'APP.ADOPT.DAYS.MONDAY') return 'Δευτέρα';
      if (key === 'APP.ADOPT.DAYS.TUESDAY') return 'Τρίτη';
      if (key === 'APP.ADOPT.DAYS.WEDNESDAY') return 'Τετάρτη';
      if (key === 'APP.ADOPT.DAYS.THURSDAY') return 'Πέμπτη';
      if (key === 'APP.ADOPT.DAYS.FRIDAY') return 'Παρασκευή';
      if (key === 'APP.ADOPT.DAYS.SATURDAY') return 'Σάββατο';
      if (key === 'APP.ADOPT.DAYS.SUNDAY') return 'Κυριακή';
      if (key === 'APP.ADOPT.CLOSED') return 'Κλειστά';
      return key;
    });
    languageChanged$.next('el');
    fixture.detectChanges();
    expect(component.days[0].label).toBe('Δευτέρα');
    expect(component.days[6].label).toBe('Κυριακή');
  });

  it('should display translated day labels in template', () => {
    fixture.detectChanges();
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.textContent).toContain('Monday');
    // Change language
    translationService.translate.and.callFake((key: string) => {
      if (key === 'APP.ADOPT.DAYS.MONDAY') return 'Δευτέρα';
      if (key === 'APP.ADOPT.DAYS.TUESDAY') return 'Τρίτη';
      if (key === 'APP.ADOPT.DAYS.WEDNESDAY') return 'Τετάρτη';
      if (key === 'APP.ADOPT.DAYS.THURSDAY') return 'Πέμπτη';
      if (key === 'APP.ADOPT.DAYS.FRIDAY') return 'Παρασκευή';
      if (key === 'APP.ADOPT.DAYS.SATURDAY') return 'Σάββατο';
      if (key === 'APP.ADOPT.DAYS.SUNDAY') return 'Κυριακή';
      if (key === 'APP.ADOPT.CLOSED') return 'Κλειστά';
      return key;
    });
    languageChanged$.next('el');
    fixture.detectChanges();
    expect(compiled.textContent).toContain('Δευτέρα');
    expect(compiled.textContent).toContain('Κυριακή');
  });
}); 