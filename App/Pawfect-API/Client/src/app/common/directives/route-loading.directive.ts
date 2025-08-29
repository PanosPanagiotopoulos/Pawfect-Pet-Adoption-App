import { Directive, Input, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import { LoadingService } from '../services/loading.service';
import { LogService } from '../services/log.service';

@Directive({
  selector: '[appRouteLoading]',
  standalone: true
})
export class RouteLoadingDirective {
  @Input('appRouteLoading') targetRoute: string = '';
  @Input() loadingTimeout: number = 1500;
  @Input() enableLoading: boolean = true;

  constructor(
    private router: Router,
    private loadingService: LoadingService,
    private logService: LogService
  ) {}

  @HostListener('click', ['$event'])
  onClick(event: Event): void {
    if (!this.enableLoading || !this.targetRoute) {
      return;
    }

    // Add temporary route config if it doesn't exist
    const existingConfig = this.loadingService.getRouteConfigs()
      .find(config => config.route === this.targetRoute);

    if (!existingConfig) {
      this.loadingService.addRouteConfig({
        route: this.targetRoute,
        timeoutMs: this.loadingTimeout,
        enabled: true
      });
    }

    // Navigate to the route (this will trigger the route loading interceptor)
    this.router.navigate([this.targetRoute]);
  }
}