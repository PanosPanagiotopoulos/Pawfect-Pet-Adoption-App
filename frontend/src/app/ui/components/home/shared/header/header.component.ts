import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NgIconsModule } from '@ng-icons/core';
import { CommonModule } from '@angular/common';
import { NavLinkComponent } from '../nav-link/nav-link.component';
import { AuthButtonComponent } from '../auth-button/auth-button.component';
import { MobileMenuComponent } from '../mobile-menu/mobile-menu.component';
import { UserAvatarComponent } from '../user-avatar/user-avatar.component';
import { DropdownComponent } from '../dropdown/dropdown.component';
import { DropdownItemComponent } from '../dropdown/dropdown-item.component';
import { AuthService } from 'src/app/services/auth.service';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { User } from 'src/app/models/user/user.model';
import { takeUntil } from 'rxjs';
import { UserService } from 'src/app/services/user.service';
import { nameof } from 'ts-simple-nameof';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NgIconsModule,
    NavLinkComponent,
    AuthButtonComponent,
    MobileMenuComponent,
    UserAvatarComponent,
    DropdownComponent,
    DropdownItemComponent,
  ],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
})
export class HeaderComponent extends BaseComponent {
  isMobileMenuOpen = false;
  isUserMenuOpen = false;
  isLoggedIn = false;

  currentUser?: User = undefined;

  constructor(
    private authService: AuthService,
    private userService: UserService
  ) {
    super();

    authService.isLoggedIn().subscribe((isLoggedInFlag: boolean) => {
      if (isLoggedInFlag) {
        this.userService
          .getSingle(authService.getUserId()!, [
            nameof<User>((x) => x.Id),
            nameof<User>((x) => x.ProfilePhoto),
            nameof<User>((x) => x.FullName),
          ])
          .pipe(takeUntil(this._destroyed))
          .subscribe(
            (user: User) => {
              this.currentUser = user;
            },

            (error) => {
              console.error(error);
            }
          );
      }

      this.isLoggedIn = isLoggedInFlag;
      this.isLoggedIn = true;
    });
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
    document.body.style.overflow = this.isMobileMenuOpen ? 'hidden' : '';
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
    document.body.style.overflow = '';
  }

  logout(): void {
    this.authService
      .logout()
      .pipe(takeUntil(this._destroyed))
      .subscribe(
        () => {
          this.isUserMenuOpen = false;
        },
        (error) => {
          console.error(error);
        }
      );
  }
}
