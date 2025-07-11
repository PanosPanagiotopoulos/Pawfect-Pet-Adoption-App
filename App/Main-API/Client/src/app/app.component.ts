import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from './services/auth.service';
import { LoggedAccount } from './models/auth/auth.model';
import { TranslationService } from './common/services/translation.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit, OnDestroy {
  reloadKey = 1;
  private translationSub?: Subscription;

  constructor(private authService: AuthService, private translationService: TranslationService) {}
  
  ngOnInit(): void {
    // The me() request will be shared with the auth guard
    this.authService.me().subscribe();
    // Reload routed components on language change
    this.translationSub = this.translationService.languageChanged$.subscribe(() => {
      this.reloadKey++;
    });
  }

  ngOnDestroy(): void {
    this.translationSub?.unsubscribe();
  }
}
