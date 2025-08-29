import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private activeRequests = new Set<string>();

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

  get isLoading$(): Observable<boolean> {
    return this.loadingSubject.asObservable();
  }

  get isLoading(): boolean {
    return this.loadingSubject.value;
  }

  shouldShowLoading(url: string): boolean {
    return this.loadingUrls.some(loadingUrl => url.includes(loadingUrl));
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
}