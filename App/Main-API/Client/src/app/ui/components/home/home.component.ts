import { Component } from '@angular/core';
import { TranslationService } from 'src/app/common/services/translation.service';
interface Feature {
  icon: string;
  title: string;
  description: string;
  bgColor: string;
  iconColor: string;
  gradientClass: string;
}

@Component({
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
  standalone: false,
})
export class HomeComponent {
  currentYear = new Date().getFullYear();

  constructor(public translate: TranslationService) {}

  features: Feature[] = [
    {
      icon: 'lucideSearch',
      // i18n: Feature title and description
      title: this.translate.translate('APP.HOME-PAGE.FEATURE_SEARCH_TITLE'),
      description: this.translate.translate('APP.HOME-PAGE.FEATURE_SEARCH_DESC'),
      bgColor: 'bg-gradient-to-br from-primary-500/20 to-primary-400/20',
      iconColor: 'text-primary-400',
      gradientClass: 'feature-card-primary',
    },
    {
      icon: 'lucideHeart',
      title: this.translate.translate('APP.HOME-PAGE.FEATURE_MATCH_TITLE'),
      description: this.translate.translate('APP.HOME-PAGE.FEATURE_MATCH_DESC'),
      bgColor: 'bg-gradient-to-br from-secondary-500/20 to-secondary-400/20',
      iconColor: 'text-secondary-400',
      gradientClass: 'feature-card-secondary',
    },
    {
      icon: 'lucideMessageCircle',
      title: this.translate.translate('APP.HOME-PAGE.FEATURE_ADOPT_TITLE'),
      description: this.translate.translate('APP.HOME-PAGE.FEATURE_ADOPT_DESC'),
      bgColor: 'bg-gradient-to-br from-accent-500/20 to-accent-400/20',
      iconColor: 'text-accent-400',
      gradientClass: 'feature-card-accent',
    },
  ];
}