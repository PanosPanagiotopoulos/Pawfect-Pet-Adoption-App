import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgIconsModule } from '@ng-icons/core';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-about',
  standalone: true,
  imports: [CommonModule, NgIconsModule, TranslatePipe],
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.css']
})
export class AboutComponent {
  features = [
    {
      icon: 'lucideHeart',
      titleKey: 'APP.ABOUT.FEATURE_COMPASSION_TITLE',
      descriptionKey: 'APP.ABOUT.FEATURE_COMPASSION_DESC'
    },
    {
      icon: 'lucideShield',
      titleKey: 'APP.ABOUT.FEATURE_SAFETY_TITLE',
      descriptionKey: 'APP.ABOUT.FEATURE_SAFETY_DESC'
    },
    {
      icon: 'lucideUsers',
      titleKey: 'APP.ABOUT.FEATURE_COMMUNITY_TITLE',
      descriptionKey: 'APP.ABOUT.FEATURE_COMMUNITY_DESC'
    },
    {
      icon: 'lucideStar',
      titleKey: 'APP.ABOUT.FEATURE_EXCELLENCE_TITLE',
      descriptionKey: 'APP.ABOUT.FEATURE_EXCELLENCE_DESC'
    }
  ];

  stats = [
    {
      numberKey: 'APP.ABOUT.STAT_ADOPTIONS_NUMBER',
      labelKey: 'APP.ABOUT.STAT_ADOPTIONS_LABEL'
    },
    {
      numberKey: 'APP.ABOUT.STAT_SHELTERS_NUMBER',
      labelKey: 'APP.ABOUT.STAT_SHELTERS_LABEL'
    },
    {
      numberKey: 'APP.ABOUT.STAT_FAMILIES_NUMBER',
      labelKey: 'APP.ABOUT.STAT_FAMILIES_LABEL'
    },
    {
      numberKey: 'APP.ABOUT.STAT_YEARS_NUMBER',
      labelKey: 'APP.ABOUT.STAT_YEARS_LABEL'
    }
  ];

  getStatIcon(index: number): string {
    const icons = ['lucideHeart', 'lucideHouse', 'lucideUsers', 'lucideClock'];
    return icons[index] || 'lucideStar';
  }

  onImageError(event: Event): void {
    const target = event.target as HTMLImageElement;
    const fallback = document.getElementById('fallback-content');
    if (target && fallback) {
      target.style.display = 'none';
      fallback.style.display = 'flex';
    }
  }
}