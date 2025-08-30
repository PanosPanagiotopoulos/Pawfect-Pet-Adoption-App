import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { LogService } from './log.service';

export interface RouteLoadingConfig {
  route: string;
  timeoutMs: number;
  enabled: boolean;
}
@Injectable({
  providedIn: 'root',
})
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private activeRequests = new Set<string>();
  private routeTimeouts = new Map<string, any>();

  // URLs that should trigger the loading screen
  private loadingUrls: string[] = [
    '/auth/register/unverified',
    '/auth/register/unverified/google',
    '/auth/login',
    '/auth/logout',
    '/api/users/delete',
    '/api/users/update',
    '/api/adoption-applications/persist',
    '/api/adoption-applications/delete',
    '/api/animals/persist',
    '/api/animals/persist/batch',
    '/api/animals/delete',
    '/api/animal-types/persist',
    '/api/animal-types/delete',
    '/api/breeds/persist',
    '/api/breeds/delete',
    '/api/reports/persist',
    '/api/breeds/delete',
  ];

  // Route loading configurations
  private routeLoadingConfigs: RouteLoadingConfig[] = [
    { route: '/search', timeoutMs: 1200, enabled: true },
    { route: '/auth/login', timeoutMs: 1200, enabled: true },
    { route: '/auth/sign-up', timeoutMs: 1200, enabled: true },
    { route: '/home/about', timeoutMs: 1200, enabled: true },
    { route: '/home/contact', timeoutMs: 1200, enabled: true },
  ];

  constructor(private logService: LogService) {}

  get isLoading$(): Observable<boolean> {
    return this.loadingSubject.asObservable();
  }

  get isLoading(): boolean {
    return this.loadingSubject.value;
  }

  shouldShowLoading(url: string): boolean {
    return this.loadingUrls.some((loadingUrl) => url.includes(loadingUrl));
  }

  startLoading(requestId: string): void {
    this.activeRequests.add(requestId);
    this.updateLoadingState();
  }

  stopLoading(requestId: string): void {
    this.activeRequests.delete(requestId);
    this.updateLoadingState();
  }

  private updateLoadingState(): void {
    const isLoading = this.activeRequests.size > 0;
    if (this.loadingSubject.value !== isLoading) {
      this.loadingSubject.next(isLoading);
    }
  }

  // Route loading methods
  startRouteLoading(route: string): void {
    const config = this.getRouteConfig(route);
    if (!config || !config.enabled) {
      return;
    }

    const requestId = `route-${route}-${Date.now()}`;
    this.logService.logFormatted(
      `Starting route loading for: ${route} (${config.timeoutMs}ms)`
    );

    this.startLoading(requestId);

    // Clear any existing timeout for this route
    if (this.routeTimeouts.has(route)) {
      clearTimeout(this.routeTimeouts.get(route));
    }

    // Set timeout to stop loading
    const timeoutId = setTimeout(() => {
      this.logService.logFormatted(
        `Route loading timeout completed for: ${route}`
      );
      this.stopLoading(requestId);
      this.routeTimeouts.delete(route);
    }, config.timeoutMs);

    this.routeTimeouts.set(route, timeoutId);
  }

  stopRouteLoading(route: string): void {
    if (this.routeTimeouts.has(route)) {
      clearTimeout(this.routeTimeouts.get(route));
      this.routeTimeouts.delete(route);
      this.logService.logFormatted(
        `Route loading manually stopped for: ${route}`
      );
    }
  }

  // Configuration management methods
  getRouteConfigs(): RouteLoadingConfig[] {
    return [...this.routeLoadingConfigs];
  }

  updateRouteConfig(route: string, config: Partial<RouteLoadingConfig>): void {
    const index = this.routeLoadingConfigs.findIndex((c) => c.route === route);
    if (index !== -1) {
      this.routeLoadingConfigs[index] = {
        ...this.routeLoadingConfigs[index],
        ...config,
      };
      this.logService.logFormatted(`Updated route config for: ${route}`);
    }
  }

  addRouteConfig(config: RouteLoadingConfig): void {
    const existingIndex = this.routeLoadingConfigs.findIndex(
      (c) => c.route === config.route
    );
    if (existingIndex !== -1) {
      this.routeLoadingConfigs[existingIndex] = config;
    } else {
      this.routeLoadingConfigs.push(config);
    }
    this.logService.logFormatted(`Added route config for: ${config.route}`);
  }

  removeRouteConfig(route: string): void {
    const index = this.routeLoadingConfigs.findIndex((c) => c.route === route);
    if (index !== -1) {
      this.routeLoadingConfigs.splice(index, 1);
      this.stopRouteLoading(route);
      this.logService.logFormatted(`Removed route config for: ${route}`);
    }
  }

  private getRouteConfig(route: string): RouteLoadingConfig | undefined {
    // First try exact match
    let config = this.routeLoadingConfigs.find((c) => c.route === route);

    // If no exact match, try prefix match (for nested routes)
    if (!config) {
      config = this.routeLoadingConfigs.find((c) => route.startsWith(c.route));
    }

    return config;
  }

  shouldShowRouteLoading(route: string): boolean {
    const config = this.getRouteConfig(route);
    this.logService.logFormatted(
      `Route config lookup for '${route}': ${
        config
          ? `found (${config.route}, enabled: ${config.enabled})`
          : 'not found'
      }`
    );
    return config ? config.enabled : false;
  }

  /**
   * Force stop all loading states - useful for emergency situations
   * where loading states might get stuck
   */
  forceStopLoading(): void {
    this.logService.logFormatted('Force stopping all loading states');
    
    // Clear all active requests
    this.activeRequests.clear();
    
    // Clear all route timeouts
    this.routeTimeouts.forEach((timeoutId, route) => {
      clearTimeout(timeoutId);
      this.logService.logFormatted(`Cleared timeout for route: ${route}`);
    });
    this.routeTimeouts.clear();
    
    // Force update loading state to false
    this.loadingSubject.next(false);
    
    this.logService.logFormatted('All loading states forcefully stopped');
  }
}
