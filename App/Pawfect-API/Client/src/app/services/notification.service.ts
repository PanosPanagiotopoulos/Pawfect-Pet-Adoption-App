import { Injectable, OnDestroy } from '@angular/core';
import { BaseHttpService } from '../common/services/base-http.service';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { Observable, throwError, BehaviorSubject, Subject, timer } from 'rxjs';
import { catchError, takeUntil, switchMap, tap, map } from 'rxjs/operators';
import { NotificationLookup } from '../lookup/notification-lookup';
import { Notification } from '../models/notification/notification.model';
import { HttpParams } from '@angular/common/http';
import { QueryResult } from '../common/models/query-result';
import { AuthService } from './auth.service';
import { UtilsService } from '../common/services/utils.service';
import { nameof } from 'ts-simple-nameof';

@Injectable({
  providedIn: 'root',
})
export class NotificationService implements OnDestroy {
  private readonly POLLING_INTERVAL = 15000; // 15 seconds
  private destroy$ = new Subject<void>();
  private pollingActive = false;

  // Centralized notification state
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);
  private notificationCountSubject = new BehaviorSubject<number>(0);
  private isLoadingSubject = new BehaviorSubject<boolean>(false);

  // Public observables
  public notifications$ = this.notificationsSubject.asObservable();
  public notificationCount$ = this.notificationCountSubject.asObservable();
  public isLoading$ = this.isLoadingSubject.asObservable();

  constructor(
    private installationConfiguration: InstallationConfigurationService,
    private http: BaseHttpService,
    private authService: AuthService,
    private utilsService: UtilsService
  ) {
    // Start polling when user is authenticated
    this.authService
      .isLoggedIn()
      .pipe(takeUntil(this.destroy$))
      .subscribe((isLoggedIn) => {
        if (isLoggedIn && !this.pollingActive) {
          this.startPolling();
        } else if (!isLoggedIn) {
          this.stopPolling();
          this.clearNotifications();
        }
      });
  }

  private get apiBase(): string {
    return `${this.installationConfiguration.notificationsServiceAddress}api/notifications`;
  }

  /**
   * Start centralized polling for notifications
   */
  private startPolling(): void {
    if (this.pollingActive) {
      return;
    }

    this.pollingActive = true;

    // Initial load
    this.loadNotifications();

    // Set up polling timer
    timer(this.POLLING_INTERVAL, this.POLLING_INTERVAL)
      .pipe(
        takeUntil(this.destroy$),
        switchMap(() => this.loadNotificationsInternal())
      )
      .subscribe({
        next: (notifications) => {
          // Update state with new notifications
          const currentNotifications = this.notificationsSubject.value;
          const combinedNotifications = this.utilsService.combineDistinct(
            currentNotifications,
            notifications
          );
          this.notificationsSubject.next(combinedNotifications);
          this.notificationCountSubject.next(combinedNotifications.length);
        },
        error: (error) => {
          console.error('Error in notification polling:', error);
          // Continue polling even on error
        },
      });
  }

  /**
   * Stop centralized polling
   */
  private stopPolling(): void {
    this.pollingActive = false;
  }

  /**
   * Clear all notifications from state
   */
  private clearNotifications(): void {
    this.notificationsSubject.next([]);
    this.notificationCountSubject.next(0);
  }

  /**
   * Force refresh notifications (called when dropdown is opened)
   */
  public refreshNotifications(): void {
    this.loadNotifications();
  }

  /**
   * Load notifications and update state
   */
  private loadNotifications(): void {
    this.isLoadingSubject.next(true);

    this.loadNotificationsInternal().subscribe({
      next: (notifications) => {
        this.notificationsSubject.next(notifications);
        this.notificationCountSubject.next(notifications.length);
        this.isLoadingSubject.next(false);
      },
      error: (error) => {
        console.error('Error loading notifications:', error);
        this.isLoadingSubject.next(false);
      },
    });
  }

  /**
   * Internal method to fetch notifications from API
   */
  private loadNotificationsInternal(): Observable<Notification[]> {
    const lookup: NotificationLookup = {
      offset: 0,
      pageSize: 50,
      fields: [
        nameof<Notification>(x => x.id), 
        nameof<Notification>(x => x.title),
        nameof<Notification>(x => x.content), 
        nameof<Notification>(x => x.createdAt), 
        nameof<Notification>(x => x.isRead), 
      ],
      sortBy: [nameof<Notification>(x => x.createdAt), ],
      sortDescending: true,
      isRead: false,
    };

    return this.queryMineUnread(lookup).pipe(
      catchError((error) => {
        console.error('Error fetching notifications:', error);
        return throwError(() => error);
      }),
      tap(() => this.isLoadingSubject.next(false))
    );
  }

  /**
   * Original API method for querying unread notifications
   */
  queryMineUnread(q: NotificationLookup): Observable<Notification[]> {
    const url = `${this.apiBase}/query/mine/unread`;
    return this.http.post<QueryResult<Notification>>(url, q).pipe(
      catchError((error: any) => throwError(() => error)),
      map((response) => response.items || [])
    );
  }

  /**
   * Mark notifications as read and update state
   */
  read(
    notificationIds: string[],
    fields: string[]
  ): Observable<Notification[]> {
    const url = `${this.apiBase}/read`;
    let params = new HttpParams();
    fields.forEach((field) => {
      params = params.append('fields', field);
    });
    const options = { params };

    return this.http.post<Notification[]>(url, notificationIds, options).pipe(
      catchError((error: any) => throwError(() => error)),
      tap(() => {
        // Update local state by removing read notifications
        const currentNotifications = this.notificationsSubject.value;
        const updatedNotifications = currentNotifications.filter(
          (n) => !notificationIds.includes(n.id!)
        );
        this.notificationsSubject.next(updatedNotifications);
        this.notificationCountSubject.next(updatedNotifications.length);
      })
    );
  }

  /**
   * Get current notifications synchronously
   */
  public getCurrentNotifications(): Notification[] {
    return this.notificationsSubject.value;
  }

  /**
   * Get current notification count synchronously
   */
  public getCurrentNotificationCount(): number {
    return this.notificationCountSubject.value;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.stopPolling();
  }
}
