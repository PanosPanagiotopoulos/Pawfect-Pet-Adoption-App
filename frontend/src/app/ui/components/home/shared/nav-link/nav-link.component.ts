import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-nav-link',
  standalone: true,
  imports: [RouterLink],
  template: `
    <a [routerLink]="routerLink"
       class="relative text-white/80 hover:text-white transition-colors duration-300 py-2 group">
      <span>
        <ng-content></ng-content>
      </span>
      <span class="absolute bottom-0 left-0 w-0 h-0.5 bg-gradient-to-r from-primary-400 via-secondary-400 to-accent-400 group-hover:w-full transition-all duration-300"></span>
    </a>
  `
})
export class NavLinkComponent {
  @Input() routerLink!: string;
}