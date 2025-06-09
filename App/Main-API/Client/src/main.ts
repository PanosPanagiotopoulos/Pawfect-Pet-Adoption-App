import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/app.module';
import { enableProdMode } from '@angular/core';

// Enable production mode if needed
// enableProdMode();

platformBrowserDynamic()
  .bootstrapModule(AppModule, {
    ngZoneEventCoalescing: true,
    ngZoneRunCoalescing: true
  })
  .catch((err) => console.error(err));