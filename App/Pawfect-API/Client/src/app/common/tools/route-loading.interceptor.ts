import { Injectable } from '@angular/core';
import { Router, NavigationStart, NavigationEnd, NavigationCancel, NavigationError } from '@angular/router';
import { LoadingService } from '../services/loading.service';
import { LogService } from '../services/log.service';
import { filter } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class RouteLoadingInterceptor {
  private currentRoute: string | null = null;
  private safetyTimeout: any = null;
  private readonly SAFETY_TIMEOUT_MS = 12000; // 12 seconds

  constructor(
    private router: Router,
    private loadingService: LoadingService,
    private logService: LogService
  ) {
    this.initializeRouteLoading();
  }

  private initializeRouteLoading(): void {
    this.router.events.pipe(
      filter(event => 
        event instanceof NavigationStart || 
        event instanceof NavigationEnd || 
        event instanceof NavigationCancel || 
        event instanceof NavigationError
      )
    ).subscribe(event => {
      if (event instanceof NavigationStart) {
        this.handleNavigationStart(event);
      } else if (event instanceof NavigationEnd) {
        this.handleNavigationEnd(event);
      } else if (event instanceof NavigationCancel || event instanceof NavigationError) {
        this.handleNavigationComplete();
      }
    });
  }

  private handleNavigationStart(event: NavigationStart): void {
    const route = this.extractRoute(event.url);
    this.currentRoute = route;
    
    
    // Clear any existing safety timeout
    this.clearSafetyTimeout();
    
    if (this.loadingService.shouldShowRouteLoading(route)) {
      this.loadingService.startRouteLoading(route);
      
      // Set safety timeout to force stop loading after 12 seconds
      this.setSafetyTimeout(route);
    } 
  }

  private handleNavigationEnd(event: NavigationEnd): void {
    this.clearSafetyTimeout();
  }

  private handleNavigationComplete(): void {
    // Navigation was cancelled or errored - stop loading immediately
    if (this.currentRoute) {
      this.loadingService.stopRouteLoading(this.currentRoute);
      this.currentRoute = null;
    }
    
    // Clear safety timeout
    this.clearSafetyTimeout();
  }

  private extractRoute(url: string): string {
    // Remove query parameters and fragments
    const cleanUrl = url.split('?')[0].split('#')[0];
    
    // Return the full clean URL path
    return cleanUrl || '/';
  }

  private setSafetyTimeout(route: string): void {
    this.safetyTimeout = setTimeout(() => {
      this.loadingService.forceStopLoading();
      this.currentRoute = null;
      this.safetyTimeout = null;
    }, this.SAFETY_TIMEOUT_MS);
    
  }

  private clearSafetyTimeout(): void {
    if (this.safetyTimeout) {
      clearTimeout(this.safetyTimeout);
      this.safetyTimeout = null;
    }
  }
}