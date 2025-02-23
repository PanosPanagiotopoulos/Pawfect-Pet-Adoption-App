import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgIconsModule } from '@ng-icons/core';
import { lucideMenu, lucideX, lucideHeart } from '@ng-icons/lucide';
import { CommonModule } from '@angular/common';
import { NavLinkComponent } from '../../shared/nav-link/nav-link.component';
import { AuthButtonComponent } from '../../shared/auth-button/auth-button.component';
import { MobileMenuComponent } from '../../shared/mobile-menu/mobile-menu.component';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NgIconsModule,
    NavLinkComponent,
    AuthButtonComponent,
    MobileMenuComponent
  ]
})
export class HeaderComponent {
  isMobileMenuOpen = false;

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
    document.body.style.overflow = this.isMobileMenuOpen ? 'hidden' : '';
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
    document.body.style.overflow = '';
  }
}