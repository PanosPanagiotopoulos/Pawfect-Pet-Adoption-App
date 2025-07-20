import { Component, OnInit, OnDestroy } from '@angular/core';
import { AuthService } from './services/auth.service';
import { LoggedAccount } from './models/auth/auth.model';
import { TranslationService } from './common/services/translation.service';
import { Subscription } from 'rxjs';
import { AnimalService } from './services/animal.service';
import { saveAs } from 'file-saver';
import { HttpResponse } from '@angular/common/http';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.css',
})
export class AppComponent implements OnInit, OnDestroy {
  reloadKey = 1;
  private translationSub?: Subscription;

  constructor(
    private authService: AuthService,
    private translationService: TranslationService,
    private animalService: AnimalService
  ) {}
  
  ngOnInit(): void {
    // The me() request will be shared with the auth guard
    this.authService.me().subscribe();
    // Reload routed components on language change
    this.translationSub = this.translationService.languageChanged$.subscribe(() => {
      this.reloadKey++;
    });

    this.downloadTemplate();
  }

  ngOnDestroy(): void {
    this.translationSub?.unsubscribe();
  }

  downloadTemplate(): void {
    this.animalService.getImportExcel().subscribe({
      next: (response: HttpResponse<Blob>) => {
        const contentDisposition = response.headers.get('Content-Disposition');
        let filename = 'template.xlsx';
  
        if (contentDisposition) {
          let match = contentDisposition.match(/filename\*=([^']*)''([^;]+)/);
          if (match && match[2]) {
            filename = decodeURIComponent(match[2]);
          } else {
            match = contentDisposition.match(/filename="?([^"]+)"?/);
            if (match && match[1]) {
              filename = match[1];
            }
          }
        }
  
        saveAs(response.body!, filename);
      },
      error: (err) => {
        console.error('Failed to download template:', err);
      }
    });
  }
}
