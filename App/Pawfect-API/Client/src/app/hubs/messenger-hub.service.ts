// messenger-hub.service.ts
import { Injectable, NgZone, Injector } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { InstallationConfigurationService } from '../common/services/installation-configuration.service';
import { BehaviorSubject } from 'rxjs';
import { take } from 'rxjs/operators';
import { Message } from '../models/message/message.model';
import { UserPresence } from '../models/user/user.model';
import { MessengerEvents } from '../common/enum/messenger-events.enum';

@Injectable({ providedIn: 'root' })
export class MessengerHubService {
  private connection?: signalR.HubConnection;
  private isRefreshing = false;

  private joinedConversations = new Set<string>();

  private connected$ = new BehaviorSubject<boolean>(false);
  isConnected$ = this.connected$.asObservable();

  messageReceived$ = new BehaviorSubject<Message | null>(null);
  messageStatusChanged$ = new BehaviorSubject<Message | null>(null);
  messageRead$ = new BehaviorSubject<Message | null>(null);
  lastConversationMessageUpdated$ = new BehaviorSubject<Message | null>(null);
  presenceChanged$ = new BehaviorSubject<UserPresence | null>(null);

  constructor(
    private installationService: InstallationConfigurationService,
    private zone: NgZone,
    private injector: Injector
  ) {}

  init(): void {
    if (this.connection) return;

    const url = `${this.installationService.messengerServiceAddress}hubs/chat`;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(url, {
        withCredentials: true,
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .build();

    // Server -> client handlers
    this.connection.on(MessengerEvents.MessageReceivedEvent, (msg: Message) => {
      this.zone.run(() => this.messageReceived$.next(msg));
    });
    this.connection.on(
      MessengerEvents.MessageStatusChangedEvent,
      (msg: Message) => {
        this.zone.run(() => this.messageStatusChanged$.next(msg));
      }
    );
    this.connection.on(MessengerEvents.MessageReadEvent, (msg: Message) => {
      this.zone.run(() => this.messageRead$.next(msg));
    });
    this.connection.on(
      MessengerEvents.LastConversationMessageUpdatedEvent,
      (msg: Message) => {
        this.zone.run(() => this.lastConversationMessageUpdated$.next(msg));
      }
    );
    this.connection.on(
      MessengerEvents.PresenceChangedEvent,
      (presence: UserPresence) => {
        this.zone.run(() => this.presenceChanged$.next(presence));
      }
    );

    this.connection
      .start()
      .then(() => this.connected$.next(true))
      .catch((error) => {
        this.handleConnectionError(error);
      });

    this.connection.onreconnected(() => {
      this.connected$.next(true);
      this.rejoinAllConversations();
    });
    this.connection.onreconnecting(() => this.connected$.next(false));
    this.connection.onclose((error) => {
      this.connected$.next(false);
      if (error) {
        this.handleConnectionError(error);
      }
    });
  }

  async joinConversation(conversationId: string): Promise<void> {
    if (!conversationId) return;
    if (!this.connection) this.init();
    try {
      await this.connection!.invoke('JoinConversation', conversationId);
      this.joinedConversations.add(conversationId);
    } catch (err) {
      this.handleConnectionError(err);
    }
  }

  /** Call when a conversation view closes */
  async leaveConversation(conversationId: string): Promise<void> {
    if (!this.connection || !conversationId) return;
    try {
      await this.connection.invoke('LeaveConversation', conversationId);
    } catch {
      // swallow; leaving a group on a dropped connection will fail and that’s fine
    } finally {
      this.joinedConversations.delete(conversationId);
    }
  }

   private async rejoinAllConversations(): Promise<void> {
    if (!this.connection) return;
    const ids = Array.from(this.joinedConversations);
    for (const id of ids) {
      try {
        await this.connection.invoke('JoinConversation', id);
      } catch {
        // keep going; failed rejoin for one convo shouldn't block others
      }
    }
  }

  private handleConnectionError(error: any): void {
    debugger;
    if (error?.statusCode === 401 || error?.message?.includes('401')) {
      this.handleUnauthorizedError();
    } else {
      this.connected$.next(false);
    }
  }

  private async handleUnauthorizedError(): Promise<void> {
    if (this.isRefreshing) return;

    this.isRefreshing = true;
    this.connected$.next(false);

    try {
      const { AuthService } = await import('../services/auth.service');
      const authService = this.injector.get(AuthService);

      await authService.refresh().pipe(take(1)).toPromise();

      if (this.connection) {
        await this.connection.start();
        this.connected$.next(true);

        await this.rejoinAllConversations();
      }
    } catch (refreshError) {
      this.connected$.next(false);
    } finally {
      this.isRefreshing = false;
    }
  }

  async disconnect(): Promise<void> {
    if (!this.connection) return;

    try {
      // Remove handlers (optional but clean)
      this.connection.off(MessengerEvents.MessageReceivedEvent);
      this.connection.off(MessengerEvents.MessageStatusChangedEvent);
      this.connection.off(MessengerEvents.MessageReadEvent);
      this.connection.off(MessengerEvents.LastConversationMessageUpdatedEvent);
      this.connection.off(MessengerEvents.PresenceChangedEvent);

      await this.connection.stop();
    } finally {
      this.connection = undefined;
      this.connected$.next(false);

      // reset latest pushed values
      this.messageReceived$.next(null);
      this.messageStatusChanged$.next(null);
      this.messageRead$.next(null);
      this.lastConversationMessageUpdated$.next(null);
      this.presenceChanged$.next(null);

      this.joinedConversations.clear();
    }
  }
}
