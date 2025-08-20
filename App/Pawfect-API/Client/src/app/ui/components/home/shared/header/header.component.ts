import { Component } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { NgIconsModule } from '@ng-icons/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
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
import { ShelterService } from 'src/app/services/shelter.service';
import { SearchCacheService } from 'src/app/services/search-cache.service';
import { ShelterLookup } from 'src/app/lookup/shelter-lookup';
import { Shelter } from 'src/app/models/shelter/shelter.model';
import { debounceTime, distinctUntilChanged, of } from 'rxjs';
import { FormControl } from '@angular/forms';
import { Location } from 'src/app/models/user/user.model';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
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

  // Shelter search properties
  shelterSearchControl = new FormControl('');
  shelterSearchResults: Shelter[] = [];
  recentQueries: string[] = [];
  showShelterDropdown = false;
  showMobileSearch = false;
  showDesktopSearch = false;
  isSearching = false;
  isResultsFromCache = false;
  currentSearchQuery = ''; // Track the current search query for marking as clicked

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private router: Router,
    private snackbarService: SnackbarService,
    private translationService: TranslationService,
    private shelterService: ShelterService,
    private searchCacheService: SearchCacheService
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

    // Setup shelter search
    this.setupShelterSearch();
  }

  handleOutsideClick(event: MouseEvent) {
    const langDropdown = document.querySelector('.relative[role="group"][aria-label="Language switcher"]');
    if (this.showLangDropdown && langDropdown && !langDropdown.contains(event.target as Node)) {
      this.showLangDropdown = false;
    }

    const desktopSearchContainer = document.querySelector('.desktop-search-container');
    if (this.showDesktopSearch && desktopSearchContainer && !desktopSearchContainer.contains(event.target as Node)) {
      this.showDesktopSearch = false;
      this.showShelterDropdown = false;
    }

    const mobileSearchOverlay = document.querySelector('.mobile-search-overlay');
    if (this.showMobileSearch && mobileSearchOverlay && !mobileSearchOverlay.contains(event.target as Node)) {
      const searchButton = document.querySelector('.mobile-search-button');
      if (!searchButton?.contains(event.target as Node)) {
        this.showMobileSearch = false;
        this.showShelterDropdown = false;
      }
    }
  }

  private setupShelterSearch(): void {
    this.shelterSearchControl.valueChanges.pipe(
      takeUntil(this._destroyed),
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(query => {
        // Reset search state
        this.isSearching = false;
        this.showShelterDropdown = false;
        
        if (!query || query.trim().length < 2) {
          // Load recent queries for empty or short queries
          this.recentQueries = this.searchCacheService.getRecentQueries(5);
          if (this.recentQueries.length > 0 && (!query || query.trim().length === 0)) {
            this.showShelterDropdown = true;
          }
          this.currentSearchQuery = ''; // Clear current search query
          return of([]);
        }

        // Track the current search query
        this.currentSearchQuery = query.trim();

        // Check cache first
        const cachedResults = this.searchCacheService.getCachedResults(query.trim());
        if (cachedResults) {
          this.isResultsFromCache = true;
          return of(cachedResults);
        }
        
        this.isSearching = true;
        const lookup: ShelterLookup = {
          offset: 0,
          pageSize: 5,
          query: query.trim(),
          fields: [
            nameof<Shelter>(x => x.id),
            nameof<Shelter>(x => x.shelterName),
            nameof<Shelter>(x => x.description),
            [nameof<Shelter>(x => x.user), nameof<User>(x => x.location), nameof<Location>(x => x.city)].join('.'),
            [nameof<Shelter>(x => x.user), nameof<User>(x => x.location), nameof<Location>(x => x.address)].join('.'),
            [nameof<Shelter>(x => x.user), nameof<User>(x => x.profilePhoto), nameof<File>(x => x.sourceUrl)].join('.')
          ],
          sortBy: [nameof<Shelter>(x => x.shelterName)]
        };

        return this.shelterService.query(lookup).pipe(
          switchMap(result => {
            const shelters = result.items || [];
            // Cache the results
            this.searchCacheService.cacheResults(query.trim(), shelters);
            this.isResultsFromCache = false;
            return of(shelters);
          })
        );
      })
    ).subscribe({
      next: (shelters: Shelter[]) => {
        this.shelterSearchResults = shelters;
        this.showShelterDropdown = shelters.length > 0 && (this.showDesktopSearch || this.showMobileSearch);
        this.isSearching = false;
      },
      error: (error) => {
        console.error('Shelter search error:', error);
        this.shelterSearchResults = [];
        this.showShelterDropdown = false;
        this.isSearching = false;
      }
    });
  }

  onShelterSelect(shelter: Shelter): void {
    // Mark the current search query as clicked (successful)
    if (this.currentSearchQuery) {
      this.searchCacheService.markQueryAsClicked(this.currentSearchQuery);
    }

    this.showShelterDropdown = false;
    this.showMobileSearch = false;
    this.showDesktopSearch = false;
    this.shelterSearchControl.setValue('');
    this.currentSearchQuery = ''; // Clear current search query
    
    if (shelter.user?.id) {
      const targetUrl = `/profile/${shelter.user.id}`;
      const currentUrl = this.router.url;
      
      if (currentUrl === targetUrl) {
        // If navigating to the same URL, force a reload
        this.router.navigateByUrl('/', { skipLocationChange: true }).then(() => {
          this.router.navigate(['/profile', shelter?.user?.id]);
        });
      } else {
        this.router.navigate(['/profile', shelter.user.id]);
      }
    }
  }

  onRecentQuerySelect(query: string): void {
    this.shelterSearchControl.setValue(query);
    this.currentSearchQuery = query; // Track the selected query
    this.showShelterDropdown = false;
    // Trigger search for the selected recent query
    setTimeout(() => {
      this.onShelterSearchSubmit();
    }, 100);
  }

  onDeleteRecentQuery(query: string, event: Event): void {
    event.stopPropagation(); // Prevent triggering the search
    this.searchCacheService.deleteRecentQuery(query);
    // Refresh the recent queries list
    this.recentQueries = this.searchCacheService.getRecentQueries(5);
  }

  onShelterSearchFocus(): void {
    // Load recent queries when focusing on empty search
    if (!this.shelterSearchControl.value || this.shelterSearchControl.value.trim().length === 0) {
      this.recentQueries = this.searchCacheService.getRecentQueries(5);
      if (this.recentQueries.length > 0) {
        this.showShelterDropdown = true;
      }
    } else if (this.shelterSearchResults.length > 0) {
      this.showShelterDropdown = true;
    }
  }

  onShelterSearchSubmit(): void {
    const query = this.shelterSearchControl.value;
    if (query && query.trim().length >= 2) {
      // Track the current search query
      this.currentSearchQuery = query.trim();
      
      // Check cache first
      const cachedResults = this.searchCacheService.getCachedResults(query.trim());
      if (cachedResults) {
        this.shelterSearchResults = cachedResults;
        this.isResultsFromCache = true;
        this.showShelterDropdown = true;
        this.isSearching = false;
        return;
      }

      // Trigger search manually
      this.isSearching = true;
      const lookup: ShelterLookup = {
        offset: 0,
        pageSize: 5,
        query: query.trim(),
        fields: [
          nameof<Shelter>(x => x.id),
          nameof<Shelter>(x => x.shelterName),
          nameof<Shelter>(x => x.description),
          [nameof<Shelter>(x => x.user), nameof<User>(x => x.location), nameof<Location>(x => x.city)].join('.'),
          [nameof<Shelter>(x => x.user), nameof<User>(x => x.location), nameof<Location>(x => x.address)].join('.'),
          [nameof<Shelter>(x => x.user), nameof<User>(x => x.profilePhoto), nameof<File>(x => x.sourceUrl)].join('.')
        ],
        sortBy: [nameof<Shelter>(x => x.shelterName)]
      };

      this.shelterService.query(lookup).subscribe({
        next: (result) => {
          const shelters = result.items || [];
          this.shelterSearchResults = shelters;
          // Cache the results
          this.searchCacheService.cacheResults(query.trim(), shelters);
          this.isResultsFromCache = false;
          // Always show dropdown when search is triggered manually (to show results or "no results" message)
          this.showShelterDropdown = true;
          this.isSearching = false;
        },
        error: (error) => {
          console.error('Shelter search error:', error);
          this.shelterSearchResults = [];
          this.showShelterDropdown = true; // Show to display "no results" message
          this.isSearching = false;
        }
      });
    } else if (query && query.trim().length < 2) {
      // Show feedback for queries that are too short
      this.shelterSearchResults = [];
      this.showShelterDropdown = false;
    }
  }

  toggleDesktopSearch(): void {
    if (!this.isLoggedIn) return;
    
    this.showDesktopSearch = !this.showDesktopSearch;
    if (!this.showDesktopSearch) {
      this.showShelterDropdown = false;
      this.shelterSearchControl.setValue('');
      this.currentSearchQuery = ''; // Clear current search query
    } else {
      // Load recent queries when opening search
      this.recentQueries = this.searchCacheService.getRecentQueries(5);
      // Focus the input after animation
      setTimeout(() => {
        const input = document.querySelector('.desktop-search-input') as HTMLInputElement;
        if (input) {
          input.focus();
        }
      }, 300);
    }
  }

  trackByShelter(index: number, shelter: Shelter): string {
    return shelter.id || index.toString();
  }

  onImageError(event: Event): void {
    const target = event.target as HTMLImageElement;
    if (target) {
      target.style.display = 'none';
      // Show the fallback icon
      const parent = target.parentElement;
      if (parent) {
        const icon = parent.querySelector('ng-icon');
        if (icon) {
          (icon as HTMLElement).style.display = 'block';
        }
      }
    }
  }

  toggleMobileSearch(): void {
    if (!this.isLoggedIn) return;
    
    this.showMobileSearch = !this.showMobileSearch;
    if (this.showMobileSearch) {
      // Close mobile menu if open
      this.isMobileMenuOpen = false;
      document.body.style.overflow = '';
      // Load recent queries when opening search
      this.recentQueries = this.searchCacheService.getRecentQueries(5);
      // Focus the input after animation
      setTimeout(() => {
        const input = document.querySelector('.mobile-search-input') as HTMLInputElement;
        if (input) {
          input.focus();
        }
      }, 200);
    } else {
      this.showShelterDropdown = false;
      this.shelterSearchControl.setValue('');
      this.currentSearchQuery = ''; // Clear current search query
    }
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
    document.body.style.overflow = this.isMobileMenuOpen ? 'hidden' : '';
    if (this.isMobileMenuOpen) {
      // Close mobile search if open
      this.showMobileSearch = false;
      this.showShelterDropdown = false;
    }
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
    this.showMobileSearch = false;
    this.showDesktopSearch = false;
    this.showShelterDropdown = false;
    this.currentSearchQuery = ''; // Clear current search query
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
      const returnUrl = this.router.url;
      this.router.navigate(['/auth/login'], {
        queryParams: { returnUrl: returnUrl }
      });
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
