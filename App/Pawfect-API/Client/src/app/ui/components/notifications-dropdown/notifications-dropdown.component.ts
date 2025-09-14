import {
  Component,
  OnInit,
  OnDestroy,
  Output,
  EventEmitter,
  ViewEncapsulation,
  ElementRef,
  AfterViewInit,
} from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { CommonModule, DatePipe } from '@angular/common';
import { NgIconsModule } from '@ng-icons/core';
import { BaseComponent } from 'src/app/common/ui/base-component';
import { NotificationService } from 'src/app/services/notification.service';
import { SnackbarService } from 'src/app/common/services/snackbar.service';
import { Notification } from 'src/app/models/notification/notification.model';
import { takeUntil } from 'rxjs/operators';
import { TranslatePipe } from 'src/app/common/tools/translate.pipe';
import { DateTimeFormatPipe } from 'src/app/common/tools/date-time-format.pipe';
import { TranslationService } from 'src/app/common/services/translation.service';
import { TimezoneService } from 'src/app/common/services/time-zone.service';
import { nameof } from 'ts-simple-nameof';

@Component({
  selector: 'app-notifications-dropdown',
  standalone: true,
  imports: [CommonModule, NgIconsModule, TranslatePipe, DateTimeFormatPipe],
  providers: [DatePipe, TimezoneService],
  templateUrl: './notifications-dropdown.component.html',
  styleUrls: ['./notifications-dropdown.component.css'],
  encapsulation: ViewEncapsulation.None, // Disable view encapsulation for complete style override
})
export class NotificationsDropdownComponent
  extends BaseComponent
  implements OnInit, OnDestroy, AfterViewInit
{
  notifications: Notification[] = [];
  isLoading = false;
  isMarkingAsRead = false;
  isAnimatingIn = false;
  isAnimatingOut = false;
  hasError = false;

  @Output() notificationCountChange = new EventEmitter<number>();
  @Output() animationComplete = new EventEmitter<'enter' | 'exit'>();

  constructor(
    private readonly notificationService: NotificationService,
    private readonly snackbarService: SnackbarService,
    private readonly translateService: TranslationService,
    private readonly elementRef: ElementRef,
    private readonly sanitizer: DomSanitizer
  ) {
    super();
  }

  ngOnInit(): void {
    this.isAnimatingIn = true;
    this.subscribeToNotifications();
    this.subscribeToLoadingState();
  }

  ngAfterViewInit(): void {
    this.setupClickHandlers();
  }

  private subscribeToNotifications(): void {
    this.notificationService.notifications$
      .pipe(takeUntil(this._destroyed))
      .subscribe((notifications) => {
        this.notifications = notifications;
        this.notificationCountChange.emit(notifications.length);
        setTimeout(() => this.setupClickHandlers(), 50);
      });
  }

  private subscribeToLoadingState(): void {
    this.notificationService.isLoading$
      .pipe(takeUntil(this._destroyed))
      .subscribe((isLoading) => {
        this.isLoading = isLoading;
        if (isLoading) {
          this.hasError = false;
        }
      });
  }

  private setupClickHandlers(): void {
    setTimeout(() => {
      const cards = this.elementRef.nativeElement.querySelectorAll('.notification-card');
      cards.forEach((card: HTMLElement) => {
        card.removeEventListener('click', this.handleCardClick.bind(this));
        card.addEventListener('click', this.handleCardClick.bind(this));
      });
    }, 100);
  }

  private handleCardClick(event: Event): void {
    const target = event.target as HTMLElement;
    const card = event.currentTarget as HTMLElement;
    const notificationId = this.getNotificationId(card);
    
    if (notificationId) {
      this.processClick(notificationId, target, event);
    }
  }

  private getNotificationId(card: HTMLElement): string | null {
    const element = card.closest('[data-notification-id]') || 
                   card.parentElement?.parentElement?.querySelector('[data-notification-id]');
    
    if (element) {
      return element.getAttribute('data-notification-id');
    }
    
    // Fallback: find by index
    const allCards = Array.from(this.elementRef.nativeElement.querySelectorAll('.notification-card'));
    const cardIndex = allCards.indexOf(card);
    
    if (cardIndex >= 0 && cardIndex < this.notifications.length) {
      return this.notifications[cardIndex].id || null;
    }
    
    return null;
  }

  private processClick(notificationId: string, target: HTMLElement, event: Event): void {
    if (target.closest('.mark-read-btn') || target.classList.contains('mark-read-btn')) {
      return;
    }
    
    if (target.tagName === 'A' || target.tagName === 'BUTTON' || target.closest('a') || target.closest('button')) {
      setTimeout(() => this.markAsRead(notificationId), 150);
      return;
    }
    
    event.preventDefault();
    event.stopPropagation();
    this.markAsRead(notificationId);
  }

  retry(): void {
    this.hasError = false;
    this.refresh();
  }

  override ngOnDestroy(): void {
    const cards = this.elementRef.nativeElement.querySelectorAll('.notification-card');
    cards.forEach((card: HTMLElement) => {
      card.removeEventListener('click', this.handleCardClick.bind(this));
    });
    super.ngOnDestroy();
  }

  refresh(): void {
    this.notificationService.refreshNotifications();
  }

  refreshNotifications(): void {
    this.refresh();
  }

  retryLoadNotifications(): void {
    this.retry();
  }

  startExitAnimation(): void {
    this.isAnimatingIn = false;
    this.isAnimatingOut = true;
  }

  onAnimationEnd(event: AnimationEvent): void {
    if (event.animationName === 'dropdown-enter') {
      this.isAnimatingIn = false;
      this.animationComplete.emit('enter');
    } else if (event.animationName === 'dropdown-exit') {
      this.isAnimatingOut = false;
      this.animationComplete.emit('exit');
    }
  }

  markAsRead(notificationId: string): void {
    if (!notificationId || this.isMarkingAsRead) {
      return;
    }

    this.isMarkingAsRead = true;

    this.notificationService
      .read(
        [notificationId],
        [
          nameof<Notification>((x) => x.id),
          nameof<Notification>((x) => x.isRead),
        ]
      )
      .pipe(takeUntil(this._destroyed))
      .subscribe({
        next: () => {
          this.isMarkingAsRead = false;
        },
        error: (error) => {
          this.hasError = true;
          this.snackbarService.showError({
            message: this.translateService.translate(
              'APP.NOTIFICATIONS.DROPDOWN.ERROR_MARK_READ'
            ),
            subMessage: this.translateService.translate(
              'APP.NOTIFICATIONS.DROPDOWN.ERROR_MARK_READ_SUBTITLE'
            ),
          });
          this.isMarkingAsRead = false;
        },
      });
  }

  markAllAsRead(): void {
    if (this.notifications.length === 0 || this.isMarkingAsRead) {
      return;
    }

    const notificationIds = this.notifications
      .filter((n) => n.id)
      .map((n) => n.id!);

    if (notificationIds.length === 0) {
      return;
    }

    this.isMarkingAsRead = true;

    this.notificationService
      .read(notificationIds, [
        nameof<Notification>((x) => x.id),
        nameof<Notification>((x) => x.isRead),
      ])
      .pipe(takeUntil(this._destroyed))
      .subscribe({
        next: () => {
          this.isMarkingAsRead = false;
        },
        error: (error) => {
          this.hasError = true;
          this.snackbarService.showError({
            message: this.translateService.translate(
              'APP.NOTIFICATIONS.DROPDOWN.ERROR_MARK_ALL_READ'
            ),
            subMessage: this.translateService.translate(
              'APP.NOTIFICATIONS.DROPDOWN.ERROR_MARK_ALL_READ_SUBTITLE'
            ),
          });
          this.isMarkingAsRead = false;
        },
      });
  }

  trackByNotificationId(index: number, notification: Notification): string {
    return notification.id || index.toString();
  }

  private extractHtmlContent(htmlString?: string): string {
    if (!htmlString) return '';

    try {
      if (htmlString.includes('<!DOCTYPE html>') || htmlString.includes('<html')) {
        const parser = new DOMParser();
        const doc = parser.parseFromString(htmlString, 'text/html');
        const body = doc.body;
        
        if (body) {
          body.querySelectorAll('script, style').forEach(el => el.remove());
          
          const mainContentDiv = body.querySelector('div[style*="max-width"]');
          const linkElement = body.querySelector('a[href]');
          
          let extractedContent = '';
          if (mainContentDiv) {
            extractedContent = linkElement ? linkElement.outerHTML : mainContentDiv.outerHTML;
          } else {
            extractedContent = body.innerHTML;
          }

          const tempDiv = document.createElement('div');
          tempDiv.innerHTML = extractedContent;
          
          tempDiv.querySelectorAll('*').forEach(el => {
            Array.from(el.attributes).forEach(attr => {
              if (attr.name.startsWith('on') || 
                  (attr.name === 'href' && attr.value.startsWith('javascript:'))) {
                el.removeAttribute(attr.name);
              }
            });
          });

          return tempDiv.innerHTML.trim();
        }
      }

      return htmlString;
    } catch (error) {
      console.warn('Error parsing notification HTML content:', error);
      const tempDiv = document.createElement('div');
      tempDiv.innerHTML = htmlString;
      return tempDiv.textContent || tempDiv.innerText || '';
    }
  }

  getTitleHtml(notification: Notification): SafeHtml {
    const content = this.extractHtmlContent(notification.title);
    return this.sanitizer.bypassSecurityTrustHtml(content);
  }

  getContentHtml(notification: Notification): SafeHtml {
    const content = this.extractHtmlContent(notification.content);
    return this.sanitizer.bypassSecurityTrustHtml(content);
  }

  handleNotificationClick(notificationId: string, event: Event): void {
    const target = event.target as HTMLElement;
    
    if (target?.closest('.mark-read-btn') || target?.classList.contains('mark-read-btn')) {
      return;
    }
    
    if (target && (target.tagName === 'A' || target.tagName === 'BUTTON' || target.closest('a') || target.closest('button'))) {
      setTimeout(() => this.markAsRead(notificationId), 100);
      return;
    }
    
    event.stopPropagation();
    this.markAsRead(notificationId);
  }

  handleMarkReadClick(notificationId: string, event: Event): void {
    event.stopPropagation();
    this.markAsRead(notificationId);
  }

  handleContentClick(notificationId: string, event: Event): void {
    const target = event.target as HTMLElement;
    
    if (target && (target.tagName === 'A' || target.tagName === 'BUTTON' || target.closest('a') || target.closest('button'))) {
      setTimeout(() => this.markAsRead(notificationId), 100);
    } else {
      event.stopPropagation();
      this.markAsRead(notificationId);
    }
  }
}
