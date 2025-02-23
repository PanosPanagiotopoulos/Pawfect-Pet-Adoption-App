import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

interface Feature {
  icon: string;
  title: string;
  description: string;
  bgColor: string;
  iconColor: string;
}

@Component({
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css'],
})
export class HomeComponent {
  currentYear = new Date().getFullYear();
  
  features: Feature[] = [
    {
      icon: 'lucideSearch',
      title: 'Browse Pets',
      description: 'Find your perfect companion among our carefully curated selection of loving pets',
      bgColor: 'bg-gradient-to-br from-primary-500/20 to-primary-400/20',
      iconColor: 'text-primary-400'
    },
    {
      icon: 'lucideHeart',
      title: 'Match & Connect',
      description: 'Our intelligent matching system helps you find the perfect pet for your lifestyle',
      bgColor: 'bg-gradient-to-br from-secondary-500/20 to-secondary-400/20',
      iconColor: 'text-secondary-400'
    },
    {
      icon: 'lucideMessageCircle',
      title: 'Adopt & Love',
      description: 'Begin your journey of love and companionship with your new furry friend',
      bgColor: 'bg-gradient-to-br from-accent-500/20 to-accent-400/20',
      iconColor: 'text-accent-400'
    }
  ];
}