import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgIconsModule } from '@ng-icons/core';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, NgIconsModule, TranslatePipe],
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.css'],
})
export class ContactComponent {
  contactMethods = [
    {
      icon: 'lucidePhone',
      titleKey: 'APP.CONTACT.PHONE_TITLE',
      valueKey: 'APP.HOME-PAGE.FOOTER_PHONE',
      descriptionKey: 'APP.CONTACT.PHONE_DESC',
      gradient: 'from-green-400 to-green-600',
    },
    {
      icon: 'lucideMail',
      titleKey: 'APP.CONTACT.EMAIL_TITLE',
      valueKey: 'APP.HOME-PAGE.FOOTER_EMAIL',
      descriptionKey: 'APP.CONTACT.EMAIL_DESC',
      gradient: 'from-blue-400 to-blue-600',
    },
    {
      icon: 'lucideMapPin',
      titleKey: 'APP.CONTACT.ADDRESS_TITLE',
      valueKey: 'APP.CONTACT.ADDRESS_VALUE',
      descriptionKey: 'APP.CONTACT.ADDRESS_DESC',
      gradient: 'from-purple-400 to-purple-600',
    },
    {
      icon: 'lucideClock',
      titleKey: 'APP.CONTACT.HOURS_TITLE',
      valueKey: 'APP.CONTACT.HOURS_VALUE',
      descriptionKey: 'APP.CONTACT.HOURS_DESC',
      gradient: 'from-orange-400 to-orange-600',
    },
  ];

  socialLinks = [
    {
      icon: 'lucideFacebook',
      nameKey: 'APP.CONTACT.SOCIAL_FACEBOOK',
      url: 'https://facebook.com/pawfectpetadoption',
      gradient: 'from-blue-500 to-blue-700',
    },
    {
      icon: 'lucideInstagram',
      nameKey: 'APP.CONTACT.SOCIAL_INSTAGRAM',
      url: 'https://instagram.com/pawfectpets_official',
      gradient: 'from-pink-400 to-purple-600',
    },
  ];

  departments = [
    {
      titleKey: 'APP.CONTACT.DEPT_ADOPTION_TITLE',
      descriptionKey: 'APP.CONTACT.DEPT_ADOPTION_DESC',
      icon: 'lucideHeart',
    },
    {
      titleKey: 'APP.CONTACT.DEPT_SHELTER_TITLE',
      descriptionKey: 'APP.CONTACT.DEPT_SHELTER_DESC',
      icon: 'lucideHouse',
    },
    {
      titleKey: 'APP.CONTACT.DEPT_SUPPORT_TITLE',
      descriptionKey: 'APP.CONTACT.DEPT_SUPPORT_DESC',
      icon: 'lucideCircleHelp',
    },
  ];
}
