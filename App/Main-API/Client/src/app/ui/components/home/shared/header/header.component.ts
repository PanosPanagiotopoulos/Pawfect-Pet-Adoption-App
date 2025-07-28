import { Component } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { NgIconsModule } from '@ng-icons/core';
import { CommonModule } from '@angular/common';
import { NavLinkComponent } from '../nav-link/nav-link.component';
import { MobileMenuComponent } from '../mobile-menu/mobile-menu.component';
import { UserAvatarComponent } from '../user-avatar/user-avatar.component';
import { DropdownComponent } from '../dropdown/dropdown.component';
import { DropdownItemComponent } from '../dropdown/dropdown-item.component';
import { AuthService } from 'src/app/services/auth.service';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { User } from 'src/app/models/user/user.model';
import { takeUntil } from 'rxjs';
import { switchMap, filter, take } from 'rxjs/operators';
import { UserService } from 'src/app/services/user.service';
import { nameof } from 'ts-simple-nameof';
import { File } from 'src/app/models/file/file.model';
import { SnackbarService } from 'src/app/common/services/snackbar.service';
import { TranslationService, SupportedLanguage, LanguageOption } from 'src/app/common/services/translation.service';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    NgIconsModule,
    NavLinkComponent,
    MobileMenuComponent,
    UserAvatarComponent,
    DropdownComponent,
    DropdownItemComponent,
    TranslatePipe
  ],
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
})
export class HeaderComponent extends BaseComponent {
  isMobileMenuOpen = false;
  isUserMenuOpen = false;
  isLoggedIn = false;

  currentUser?: User = undefined;
  currentLanguage: SupportedLanguage;
  supportedLanguages: LanguageOption[];
  showLangDropdown = false;
  private hasFetchedUser = false;
  private isLoggingOut = false;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private router: Router,
    private snackbarService: SnackbarService,
    private translationService: TranslationService
  ) {
    super();
    this.currentLanguage = this.translationService.getLanguage();
    this.supportedLanguages = this.translationService.supportedLanguages;
    this.translationService.getLanguage$().pipe(takeUntil(this._destroyed)).subscribe(lang => {
      this.currentLanguage = lang;
    });
    document.addEventListener('click', this.handleOutsideClick.bind(this));

    this.authService.isLoggedIn().pipe(
      takeUntil(this._destroyed),
      filter((isLoggedIn: boolean) => isLoggedIn && !this.hasFetchedUser),
      switchMap(() => this.authService.me().pipe(take(1))),
      switchMap(() =>
        this.userService.getMe([
          nameof<User>((x) => x.id),
          [nameof<User>(x => x.profilePhoto), nameof<File>(x => x.sourceUrl)].join('.'),
          nameof<User>((x) => x.fullName),
        ]).pipe(take(1))
      )
    ).subscribe({
      next: (user: User) => {
        this.currentUser = user;
        this.hasFetchedUser = true;
      },
      error: (err) => {
        // Optionally handle error
      }
    });

    this.authService.isLoggedIn().pipe(
      takeUntil(this._destroyed)
    ).subscribe((isLoggedIn: boolean) => {
      if (!isLoggedIn) {
        this.currentUser = undefined;
        this.hasFetchedUser = false;
      }
      this.isLoggedIn = isLoggedIn;
    });
  }

  handleOutsideClick(event: MouseEvent) {
    const dropdown = document.querySelector('.relative[role="group"][aria-label="Language switcher"]');
    if (this.showLangDropdown && dropdown && !dropdown.contains(event.target as Node)) {
      this.showLangDropdown = false;
    }
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
    if (this.isLoggingOut) {
      return; // Prevent duplicate logout calls
    }
    
    this.isLoggingOut = true;
    this.authService
      .logout()
      .pipe(takeUntil(this._destroyed))
      .subscribe({
        next: () => {
          this.isUserMenuOpen = false;
          const currentUrl = this.router.url;
          this.router.navigateByUrl(currentUrl).then(() => {
            window.location.reload();
          });
          this.snackbarService.showSuccess({
            message: 'Αποσυνδεθήκατε με επιτυχία',
            subMessage: 'Ελπίζουμε να σας δούμε σύντομα!'
          });
        },
        error: (error) => {
          console.error('Logout error:', error);
          this.router.navigate(['/']).then(() => {
            window.location.reload();
          });
          this.snackbarService.showError({
            message: 'Παρουσιάστηκε σφάλμα κατά την αποσύνδεση',
            subMessage: 'Παρακαλώ δοκιμάστε ξανά'
          });
        },
        complete: () => {
          this.isLoggingOut = false;
          this.closeMobileMenu();
        },
      });
  }

  navigateToLogin(): void {
    if (this.router.url !== '/auth/login') {
      this.router.navigate(['/auth/login']);
    }
  }

  isLoginRoute(): boolean {
    return this.router.url === '/auth/login';
  }

  setLanguage(lang: SupportedLanguage): void {
    this.translationService.setLanguage(lang);
  }

  selectLanguageDropdown(lang: SupportedLanguage): void {
    this.setLanguage(lang);
    this.showLangDropdown = false;
  }

  get currentLanguageObj(): LanguageOption | undefined {
    return this.supportedLanguages.find(l => l.code === this.currentLanguage);
  }

  override ngOnDestroy() {
    super.ngOnDestroy();
    document.removeEventListener('click', this.handleOutsideClick.bind(this));
  }
}
