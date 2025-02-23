import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgIconsModule } from '@ng-icons/core';

@Component({
  selector: 'app-auth-button',
  template: `
    <a [routerLink]="routerLink"
       class="flex items-center px-4 py-2 bg-white/10 hover:bg-white/20 text-white rounded-lg backdrop-blur-sm border border-white/20 transition-all duration-300 hover:scale-105 group">
      <ng-icon name="lucideUser" class="mr-2 group-hover:rotate-12 transition-transform duration-300" [size]="'20'"></ng-icon>
      <ng-content></ng-content>
    </a>
  `,
  standalone: true,
  imports: [RouterLink, NgIconsModule]
})
export class AuthButtonComponent {
  @Input() routerLink!: string;
}