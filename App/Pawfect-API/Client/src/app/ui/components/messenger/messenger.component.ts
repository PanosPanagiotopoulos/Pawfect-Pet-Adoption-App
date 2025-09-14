import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { FormControl } from '@angular/forms';
import {
  takeUntil,
  debounceTime,
  distinctUntilChanged,
  switchMap,
  filter,
  take,
} from 'rxjs/operators';
import { of, interval } from 'rxjs';
import { BaseComponent } from '../../../common/ui/base-component';
import { ConversationService } from '../../../services/conversation.service';
import { MessageService } from '../../../services/message.service';
import { ShelterService } from '../../../services/shelter.service';
import { SearchCacheService } from '../../../services/search-cache.service';
import { MessengerHubService } from '../../../hubs/messenger-hub.service';
import { AuthService } from '../../../services/auth.service';
import { UserService } from '../../../services/user.service';
import { LogService } from '../../../common/services/log.service';
import {
  Conversation,
  ConversationPersist,
} from '../../../models/conversation/conversation.model';
import {
  Message,
  MessagePersist,
  MessageReadPersist,
} from '../../../models/message/message.model';
import { Shelter } from '../../../models/shelter/shelter.model';
import { User, UserPresence } from '../../../models/user/user.model';
import { File } from '../../../models/file/file.model';
import { ConversationLookup } from '../../../lookup/conversation-lookup';
import { MessageLookup } from '../../../lookup/message-lookup';
import { ShelterLookup } from '../../../lookup/shelter-lookup';
import { UserLookup } from '../../../lookup/user-lookup';
import { ConversationType } from '../../../common/enum/conversation-type.enum';
import { MessageType } from '../../../common/enum/message-type.enum';
import { MessageStatus } from '../../../common/enum/message-status.enum';
import { UserRole } from '../../../common/enum/user-role.enum';
import { UserStatus } from '../../../common/enum/user-status.enum';
import { nameof } from 'ts-simple-nameof';

@Component({
  selector: 'app-messenger',
  templateUrl: './messenger.component.html',
  styleUrls: ['./messenger.component.css'],
})
export class MessengerComponent extends BaseComponent implements OnInit {
  // UI State
  isMessengerOpen = false;
  isConversationOpen = false;
  isSearching = false;
  isLoadingConversations = false;
  isLoadingMessages = false;
  isSendingMessage = false;
  isCreatingConversation = false;
  isUserLoggedIn = false;

  // Current user
  currentUser?: User;

  // Conversations
  conversations: Conversation[] = [];
  selectedConversation?: Conversation;

  // User presence tracking
  userPresences = new Map<string, UserPresence>();

  // Unread messages counter
  unreadMessagesCount = 0;
  private unreadCountInterval?: any;
  private unreadCountRefreshTimeout?: any;

  // Messages
  messages: Message[] = [];
  messagesPageSize = 50;
  messagesOffset = 0;
  hasMoreMessages = true;
  isLoadingMoreMessages = false;
  selectedMessageId?: string;

  // Search
  shelterSearchControl = new FormControl('');
  shelterSearchResults: Shelter[] = [];
  userSearchResults: User[] = [];
  showShelterDropdown = false;
  recentQueries: string[] = [];
  currentSearchQuery = '';

  // Message input
  messageControl = new FormControl('');

  @ViewChild('messagesContainer') messagesContainer?: ElementRef;
  @ViewChild('messageInput') messageInput?: ElementRef;

  constructor(
    private conversationService: ConversationService,
    private messageService: MessageService,
    private shelterService: ShelterService,
    private searchCacheService: SearchCacheService,
    private messengerHubService: MessengerHubService,
    private authService: AuthService,
    private userService: UserService,
    private logService: LogService
  ) {
    super();
  }

  ngOnInit(): void {
    this.initializeCurrentUser();
    this.setupShelterSearch();
    this.setupSignalRListeners();

    // Initialize hub service
    this.messengerHubService.init();
    
    // Ensure button visibility by checking login status immediately
    this.authService.isLoggedIn().pipe(take(1)).subscribe(isLoggedIn => {
      this.isUserLoggedIn = isLoggedIn;
    });
  }

  override ngOnDestroy(): void {
    // Leave conversation when component is destroyed
    if (this.selectedConversation?.id) {
      this.messengerHubService.leaveConversation(this.selectedConversation.id);
    }

    // Clear unread messages counter interval and timeout
    if (this.unreadCountInterval) {
      clearInterval(this.unreadCountInterval);
    }
    if (this.unreadCountRefreshTimeout) {
      clearTimeout(this.unreadCountRefreshTimeout);
    }

    // Disconnect from hub when component is destroyed
    this.messengerHubService.disconnect();

    super.ngOnDestroy();
  }

  private initializeCurrentUser(): void {
    this.authService
      .isLoggedIn()
      .pipe(takeUntil(this._destroyed))
      .subscribe({
        next: (isLoggedIn) => {
          this.isUserLoggedIn = isLoggedIn;
          if (isLoggedIn) {
            this.loadCurrentUser();
            this.setupUnreadMessagesCounter();
          } else {
            this.currentUser = undefined;
            this.unreadMessagesCount = 0;
            // Clear unread counter interval and timeout
            if (this.unreadCountInterval) {
              clearInterval(this.unreadCountInterval);
            }
            if (this.unreadCountRefreshTimeout) {
              clearTimeout(this.unreadCountRefreshTimeout);
            }
            // Close messenger if user logs out
            if (this.isMessengerOpen) {
              this.isMessengerOpen = false;
              this.closeConversation();
            }
          }
        },
        error: (error) => {
          this.isUserLoggedIn = false;
        },
      });
  }

  private loadCurrentUser(): void {
    this.userService
      .getMe([
        nameof<User>((x) => x.id),
        nameof<User>((x) => x.fullName),
        nameof<User>((x) => x.roles),
        [
          nameof<User>((x) => x.profilePhoto),
          nameof<File>((x) => x.sourceUrl),
        ].join('.'),
      ])
      .pipe(take(1))
      .subscribe({
        next: (user: User) => {
          this.currentUser = user;
        },
        error: (error) => {
          this.logService.logFormatted({
            error: 'Failed to load current user',
            details: error,
          });
        },
      });
  }

  private setupShelterSearch(): void {
    this.shelterSearchControl.valueChanges
      .pipe(
        takeUntil(this._destroyed),
        debounceTime(300),
        distinctUntilChanged(),
        switchMap((query) => {
          this.isSearching = false;
          this.showShelterDropdown = false;

          if (!query || query.trim().length < 2) {
            this.recentQueries = this.searchCacheService.getRecentQueries(5);
            if (
              this.recentQueries.length > 0 &&
              (!query || query.trim().length === 0)
            ) {
              this.showShelterDropdown = true;
            }
            this.currentSearchQuery = '';
            this.shelterSearchResults = [];
            this.userSearchResults = [];
            return of({ shelters: [], users: [] });
          }

          this.currentSearchQuery = query.trim();

          const cachedResults = this.searchCacheService.getCachedResults(
            query.trim()
          );
          if (cachedResults) {
            return of({ shelters: cachedResults, users: [] });
          }

          this.isSearching = true;

          // Check if current user is a shelter
          const isCurrentUserShelter = this.currentUser?.roles?.includes(
            UserRole.Shelter
          );

          if (isCurrentUserShelter) {
            // Search both shelters and users
            const shelterLookup: ShelterLookup = {
              offset: 0,
              pageSize: 3,
              query: query.trim(),
              fields: [
                nameof<Shelter>((x) => x.id),
                nameof<Shelter>((x) => x.shelterName),
                nameof<Shelter>((x) => x.description),
                [
                  nameof<Shelter>((x) => x.user),
                  nameof<User>((x) => x.id),
                ].join('.'),
                [
                  nameof<Shelter>((x) => x.user),
                  nameof<User>((x) => x.fullName),
                ].join('.'),
                [
                  nameof<Shelter>((x) => x.user),
                  nameof<User>((x) => x.profilePhoto),
                  nameof<File>((x) => x.sourceUrl),
                ].join('.'),
              ],
              sortBy: [nameof<Shelter>((x) => x.shelterName)],
            };

            const userLookup: UserLookup = {
              offset: 0,
              pageSize: 3,
              query: query.trim(),
              roles: [UserRole.User],
              fields: [
                nameof<User>((x) => x.id),
                nameof<User>((x) => x.fullName),
                nameof<User>((x) => x.email),
                [
                  nameof<User>((x) => x.profilePhoto),
                  nameof<File>((x) => x.sourceUrl),
                ].join('.'),
              ],
              sortBy: [nameof<User>((x) => x.fullName)],
            };

            return this.shelterService.query(shelterLookup).pipe(
              switchMap((shelterResult) => {
                return this.userService.query(userLookup).pipe(
                  switchMap((userResult) => {
                    const shelters = shelterResult.items || [];
                    const users = userResult.items || [];
                    this.searchCacheService.cacheResults(
                      query.trim(),
                      shelters
                    );
                    return of({ shelters, users });
                  })
                );
              })
            );
          } else {
            // Regular user - search only shelters
            const shelterLookup: ShelterLookup = {
              offset: 0,
              pageSize: 5,
              query: query.trim(),
              fields: [
                nameof<Shelter>((x) => x.id),
                nameof<Shelter>((x) => x.shelterName),
                nameof<Shelter>((x) => x.description),
                [
                  nameof<Shelter>((x) => x.user),
                  nameof<User>((x) => x.id),
                ].join('.'),
                [
                  nameof<Shelter>((x) => x.user),
                  nameof<User>((x) => x.fullName),
                ].join('.'),
                [
                  nameof<Shelter>((x) => x.user),
                  nameof<User>((x) => x.profilePhoto),
                  nameof<File>((x) => x.sourceUrl),
                ].join('.'),
              ],
              sortBy: [nameof<Shelter>((x) => x.shelterName)],
            };

            return this.shelterService.query(shelterLookup).pipe(
              switchMap((result) => {
                const shelters = result.items || [];
                this.searchCacheService.cacheResults(query.trim(), shelters);
                return of({ shelters, users: [] });
              })
            );
          }
        })
      )
      .subscribe({
        next: (results: { shelters: Shelter[]; users: User[] }) => {
          this.shelterSearchResults = results.shelters;
          this.userSearchResults = results.users;
          this.showShelterDropdown =
            (results.shelters.length > 0 || results.users.length > 0) &&
            this.isMessengerOpen;
          this.isSearching = false;
        },
        error: (error) => {
          this.logService.logFormatted({
            error: 'Search error',
            details: error,
          });
          this.shelterSearchResults = [];
          this.userSearchResults = [];
          this.showShelterDropdown = false;
          this.isSearching = false;
        },
      });
  }

  private setupSignalRListeners(): void {
    // Listen for new messages
    this.messengerHubService.messageReceived$
      .pipe(
        takeUntil(this._destroyed),
        filter((message) => !!message)
      )
      .subscribe((message) => {
        if (message) {
          // Update conversation preview without re-querying
          this.updateConversationPreview(message);

          // Refresh unread count when new message arrives from another user
          if (message.sender?.id !== this.currentUser?.id) {
            if (this.selectedConversation?.id !== message.conversation?.id) {
              // Refresh count from server to ensure accuracy (debounced)
              this.debouncedLoadUnreadCount();
            }
          }

          // Add message to current conversation if it's open
          if (this.selectedConversation?.id === message.conversation?.id) {
            this.messages.push(message);
            this.scrollToBottom();

            // Auto-mark as read if conversation is open and message is from other user
            if (message.sender?.id !== this.currentUser?.id) {
              this.markSingleMessageAsRead(message);
            }
          }
        }
      });

    // Listen for message status changes
    this.messengerHubService.messageStatusChanged$
      .pipe(
        takeUntil(this._destroyed),
        filter((message) => !!message)
      )
      .subscribe((message) => {
        if (
          message &&
          this.selectedConversation?.id === message.conversation?.id
        ) {
          const existingMessage = this.messages.find(
            (m) => m.id === message.id
          );
          if (existingMessage) {
            existingMessage.status = message.status;
          }
        }
      });

    // Listen for message read events
    this.messengerHubService.messageRead$
      .pipe(
        takeUntil(this._destroyed),
        filter((readMessage) => !!readMessage)
      )
      .subscribe((readMessage) => {
        if (readMessage) {
          // Update message in current conversation if it's open
          if (this.selectedConversation?.id === readMessage.conversation?.id) {
            const existingMessage = this.messages.find(
              (m) => m.id === readMessage.id
            );
            if (existingMessage) {
              existingMessage.readBy = readMessage.readBy;
            }
          }

          // Update conversation's lastMessagePreview if this is the last message
          const conversation = this.conversations.find(
            (c) => c.id === readMessage.conversation?.id
          );
          if (
            conversation &&
            conversation.lastMessagePreview?.id === readMessage.id
          ) {
            conversation.lastMessagePreview!.readBy = readMessage.readBy;
            
            // Refresh unread count when message is read
            if (readMessage.sender?.id !== this.currentUser?.id) {
              this.debouncedLoadUnreadCount();
            }
          }
        }
      });

    // Listen for conversation updates
    this.messengerHubService.lastConversationMessageUpdated$
      .pipe(
        takeUntil(this._destroyed),
        filter((message) => !!message)
      )
      .subscribe((message) => {
        if (message) {
          // This event should contain the most up-to-date message information
          // including readBy status, so we update the conversation preview
          this.updateConversationPreview(message);
        }
      });

    // Listen for presence changes
    this.messengerHubService.presenceChanged$
      .pipe(
        takeUntil(this._destroyed),
        filter((presence) => !!presence)
      )
      .subscribe((presence) => {
        if (presence && presence.userId) {
          this.logService.logFormatted({
            message: 'User presence changed',
            details: {
              userId: presence.userId,
              status: presence.status,
              isOnline: presence.status === UserStatus.Online,
            },
          });
          this.userPresences.set(presence.userId, presence);
        }
      });
  }

  toggleMessenger(): void {
    this.isMessengerOpen = !this.isMessengerOpen;
    if (this.isMessengerOpen) {
      this.loadConversations();
      this.recentQueries = this.searchCacheService.getRecentQueries(5);
      // Reset unread counter when opening messenger
      this.resetUnreadCounter();
    } else {
      // Close conversation when messenger is closed
      this.closeConversation();
      this.showShelterDropdown = false;
      this.shelterSearchControl.setValue('');
    }
  }

  private loadConversations(): void {
    if (!this.currentUser?.id) return;

    this.isLoadingConversations = true;
    const lookup: ConversationLookup = {
      offset: 0,
      pageSize: 50,
      participants: [this.currentUser.id],
      fields: ConversationService.getConversationFields(),
      sortBy: [nameof<Conversation>((x) => x.lastMessageAt)],
      sortDescending: true,
    };

    this.conversationService.queryMine(lookup).subscribe({
      next: (result) => {
        this.conversations = result.items || [];
        this.isLoadingConversations = false;
      },
      error: (error) => {
        this.isLoadingConversations = false;
      },
    });
  }

  onShelterSelect(shelter: Shelter): void {
    if (!shelter.user?.id || !this.currentUser?.id) return;

    if (this.currentSearchQuery) {
      this.searchCacheService.markQueryAsClicked(this.currentSearchQuery);
    }

    this.showShelterDropdown = false;
    this.shelterSearchControl.setValue('');
    this.currentSearchQuery = '';

    // Check if conversation already exists
    const existingConversation = this.conversations.find((c) =>
      c.participants?.some((p) => p.id === shelter.user?.id)
    );

    if (existingConversation) {
      this.openConversation(existingConversation);
    } else {
      this.createConversation(shelter.user);
    }
  }

  onUserSelect(user: User): void {
    if (!user.id || !this.currentUser?.id) return;

    if (this.currentSearchQuery) {
      this.searchCacheService.markQueryAsClicked(this.currentSearchQuery);
    }

    this.showShelterDropdown = false;
    this.shelterSearchControl.setValue('');
    this.currentSearchQuery = '';

    // Check if conversation already exists
    const existingConversation = this.conversations.find((c) =>
      c.participants?.some((p) => p.id === user.id)
    );

    if (existingConversation) {
      this.openConversation(existingConversation);
    } else {
      this.createConversation(user);
    }
  }

  private createConversation(user: User): void {
    if (!this.currentUser?.id || !user.id) return;

    this.isCreatingConversation = true;
    const conversationPersist: ConversationPersist = {
      type: ConversationType.Direct,
      participants: [user.id],
      createdBy: this.currentUser.id,
    };

    this.conversationService
      .create(conversationPersist, ConversationService.getConversationFields())
      .subscribe({
        next: (conversation) => {
          this.conversations.unshift(conversation);
          this.isCreatingConversation = false;
          this.openConversation(conversation);
        },
        error: (error) => {
          this.logService.logFormatted({
            error: 'Failed to create conversation',
            details: error,
          });
          this.isCreatingConversation = false;
        },
      });
  }

  openConversation(conversation: Conversation): void {
    this.logService.logFormatted({
      message: 'Opening conversation',
      details: {
        conversationId: conversation.id,
        conversation: conversation,
      },
    });

    // Leave previous conversation if one is open
    if (
      this.selectedConversation?.id &&
      this.selectedConversation.id !== conversation.id
    ) {
      this.messengerHubService.leaveConversation(this.selectedConversation.id);
    }

    this.selectedConversation = conversation;
    this.isConversationOpen = true;
    this.messages = [];
    this.messagesOffset = 0;
    this.hasMoreMessages = true;
    this.selectedMessageId = undefined;
    this.loadMessages();

    // Join conversation group for presence updates
    if (conversation.id) {
      this.messengerHubService.joinConversation(conversation.id);

      // Initialize presence for other participant as offline until we get updates
      const otherParticipant = this.getOtherParticipant(conversation);
      if (
        otherParticipant?.id &&
        !this.userPresences.has(otherParticipant.id)
      ) {
        this.userPresences.set(otherParticipant.id, {
          userId: otherParticipant.id,
          status: UserStatus.Offline,
        });
      }
    }

    // Focus message input after opening
    setTimeout(() => {
      if (this.messageInput?.nativeElement) {
        this.messageInput.nativeElement.focus();
      }
    }, 100);
  }

  closeConversation(): void {
    // Leave conversation group
    if (this.selectedConversation?.id) {
      this.messengerHubService.leaveConversation(this.selectedConversation.id);
    }

    this.isConversationOpen = false;
    this.selectedConversation = undefined;
    this.messages = [];
    this.selectedMessageId = undefined;
    this.messageControl.setValue('');
  }

  private loadMessages(): void {
    if (!this.selectedConversation?.id) return;

    this.isLoadingMessages = true;
    const lookup: MessageLookup = {
      offset: this.messagesOffset,
      pageSize: this.messagesPageSize,
      fields: MessageService.getMessageFields(),
      sortBy: [nameof<Message>((x) => x.createdAt)],
      sortDescending: true,
    };

    this.messageService
      .queryConversationMessages(lookup, this.selectedConversation.id)
      .subscribe({
        next: (result) => {
          const newMessages = (result.items || []).reverse();

          this.logService.logFormatted({
            message: 'Messages loaded',
            details: {
              conversationId: this.selectedConversation?.id,
              messageCount: newMessages.length,
              messages: newMessages,
            },
          });

          if (this.messagesOffset === 0) {
            // Initial load
            this.messages = newMessages;
            this.scrollToBottom();
            // Mark unread messages as read
            this.markMessagesAsRead();
          } else {
            // Loading more messages - prepend to existing messages
            this.messages = [...newMessages, ...this.messages];
          }

          this.hasMoreMessages = newMessages.length === this.messagesPageSize;
          this.messagesOffset += newMessages.length;
          this.isLoadingMessages = false;
        },
        error: (error) => {
          this.logService.logFormatted({
            error: 'Failed to load messages',
            details: error,
          });
          this.isLoadingMessages = false;
        },
      });
  }

  loadMoreMessages(): void {
    if (
      !this.hasMoreMessages ||
      this.isLoadingMessages ||
      this.isLoadingMoreMessages
    )
      return;

    this.isLoadingMoreMessages = true;
    const lookup: MessageLookup = {
      offset: this.messagesOffset,
      pageSize: this.messagesPageSize,
      fields: MessageService.getMessageFields(),
      sortBy: [nameof<Message>((x) => x.createdAt)],
      sortDescending: true,
    };

    this.messageService
      .queryConversationMessages(lookup, this.selectedConversation!.id!)
      .subscribe({
        next: (result) => {
          const newMessages = (result.items || []).reverse();

          if (newMessages.length > 0) {
            // Store current scroll position
            const container = this.messagesContainer?.nativeElement;
            const previousScrollHeight = container?.scrollHeight || 0;

            // Prepend new messages
            this.messages = [...newMessages, ...this.messages];

            // Restore scroll position
            setTimeout(() => {
              if (container) {
                const newScrollHeight = container.scrollHeight;
                container.scrollTop = newScrollHeight - previousScrollHeight;
              }
            }, 0);
          }

          this.hasMoreMessages = newMessages.length === this.messagesPageSize;
          this.messagesOffset += newMessages.length;
          this.isLoadingMoreMessages = false;
        },
        error: (error) => {
          this.logService.logFormatted({
            error: 'Failed to load more messages',
            details: error,
          });
          this.isLoadingMoreMessages = false;
        },
      });
  }

  onMessagesScroll(event: Event): void {
    const element = event.target as HTMLElement;
    if (
      element.scrollTop === 0 &&
      this.hasMoreMessages &&
      !this.isLoadingMessages &&
      !this.isLoadingMoreMessages
    ) {
      this.loadMoreMessages();
    }
  }

  sendMessage(): void {
    const content = this.messageControl.value?.trim();
    if (
      !content ||
      !this.selectedConversation?.id ||
      !this.currentUser?.id ||
      this.isSendingMessage
    ) {
      return;
    }

    this.isSendingMessage = true;
    const messagePersist: MessagePersist = {
      conversationId: this.selectedConversation.id,
      senderId: this.currentUser.id,
      type: MessageType.Text,
      content: content,
    };

    this.messageService
      .persist(messagePersist, MessageService.getMessageFields())
      .subscribe({
        next: () => {
          // Don't add message locally - SignalR will handle it for all users
          this.messageControl.setValue('');
          this.isSendingMessage = false;
          // Reset textarea height
          setTimeout(() => {
            if (this.messageInput?.nativeElement) {
              this.messageInput.nativeElement.style.height = 'auto';
            }
          }, 0);
          // Note: scrollToBottom and loadConversations will be handled by SignalR events
        },
        error: (error) => {
          this.logService.logFormatted({
            error: 'Failed to send message',
            details: error,
          });
          this.isSendingMessage = false;
        },
      });
  }

  private scrollToBottom(): void {
    setTimeout(() => {
      if (this.messagesContainer?.nativeElement) {
        const element = this.messagesContainer.nativeElement;
        element.scrollTop = element.scrollHeight;
      }
    }, 100);
  }

  onShelterSearchFocus(): void {
    if (
      !this.shelterSearchControl.value ||
      this.shelterSearchControl.value.trim().length === 0
    ) {
      this.recentQueries = this.searchCacheService.getRecentQueries(5);
      if (this.recentQueries.length > 0) {
        this.showShelterDropdown = true;
      }
    } else if (this.shelterSearchResults.length > 0) {
      this.showShelterDropdown = true;
    }
  }

  onRecentQuerySelect(query: string): void {
    this.shelterSearchControl.setValue(query);
    this.currentSearchQuery = query;
    this.showShelterDropdown = false;
  }

  onDeleteRecentQuery(query: string, event: Event): void {
    event.stopPropagation();
    this.searchCacheService.deleteRecentQuery(query);
    this.recentQueries = this.searchCacheService.getRecentQueries(5);
  }

  trackByConversation(index: number, conversation: Conversation): string {
    return conversation.id || index.toString();
  }

  trackByMessage(index: number, message: Message): string {
    return message.id || index.toString();
  }

  trackByShelter(index: number, shelter: Shelter): string {
    return shelter.id || index.toString();
  }

  trackByUser(index: number, user: User): string {
    return user.id || index.toString();
  }

  getOtherParticipant(conversation: Conversation): User | undefined {
    return conversation.participants?.find(
      (p) => p.id !== this.currentUser?.id
    );
  }

  isMessageFromCurrentUser(message: Message): boolean {
    return message.sender?.id === this.currentUser?.id;
  }

  isMessageRead(message: Message): boolean {
    if (!this.isMessageFromCurrentUser(message)) return true;

    const otherParticipant = this.getOtherParticipant(
      this.selectedConversation!
    );
    return (
      message.readBy?.some((user) => user.id === otherParticipant?.id) || false
    );
  }

  getMessageStatusText(message: Message): string {
    if (!this.isMessageFromCurrentUser(message)) return '';

    // Show read status only for the last read message from current user
    if (
      this.isMessageRead(message) &&
      this.isLastReadMessageFromCurrentUser(message)
    ) {
      return 'APP.MESSENGER.MESSAGE_READ';
    }

    // Only show delivery status for the last message from current user
    if (!this.isLastMessageFromCurrentUser(message)) return '';

    switch (message.status) {
      case MessageStatus.Sending:
        return 'APP.MESSENGER.MESSAGE_SENDING';
      case MessageStatus.Delivered:
        return 'APP.MESSENGER.MESSAGE_DELIVERED';
      case MessageStatus.Failed:
        return 'APP.MESSENGER.MESSAGE_FAILED';
      default:
        return '';
    }
  }

  isLastReadMessageFromCurrentUser(message: Message): boolean {
    if (!this.isMessageFromCurrentUser(message) || !this.isMessageRead(message))
      return false;

    // Find all read messages from current user
    const readMessages = this.messages.filter(
      (m) => this.isMessageFromCurrentUser(m) && this.isMessageRead(m)
    );

    if (readMessages.length === 0) return false;

    // Return true if this is the last read message
    const lastReadMessage = readMessages[readMessages.length - 1];
    return lastReadMessage.id === message.id;
  }

  isLastMessageFromCurrentUser(message: Message): boolean {
    if (!this.isMessageFromCurrentUser(message)) return false;

    // Find the last message from current user
    const currentUserMessages = this.messages.filter((m) =>
      this.isMessageFromCurrentUser(m)
    );
    if (currentUserMessages.length === 0) return false;

    const lastMessage = currentUserMessages[currentUserMessages.length - 1];
    return lastMessage.id === message.id;
  }

  private updateConversationPreview(message: Message): void {
    if (!message.conversation?.id) return;

    const conversation = this.conversations.find(
      (c) => c.id === message.conversation?.id
    );
    if (conversation) {
      // Update last message preview and timestamp
      conversation.lastMessagePreview = message;
      conversation.lastMessageAt = message.createdAt;

      // Move conversation to top of list
      const index = this.conversations.indexOf(conversation);
      if (index > 0) {
        this.conversations.splice(index, 1);
        this.conversations.unshift(conversation);
      }
    } else {
      // If conversation doesn't exist in list, reload conversations
      this.loadConversations();
    }
  }

  onMessageInputKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    } else {
      // Auto-resize textarea
      setTimeout(() => this.autoResizeTextarea(), 0);
    }
  }

  autoResizeTextarea(): void {
    if (this.messageInput?.nativeElement) {
      const textarea = this.messageInput.nativeElement as HTMLTextAreaElement;
      textarea.style.height = 'auto';
      const newHeight = Math.min(textarea.scrollHeight, 100);
      textarea.style.height = newHeight + 'px';
    }
  }

  onImageError(event: Event): void {
    const target = event.target as HTMLImageElement;
    if (target && target.src !== 'assets/placeholder.jpg') {
      target.src = 'assets/placeholder.jpg';
    }
  }

  private markMessagesAsRead(): void {
    if (!this.currentUser?.id || !this.selectedConversation?.id) return;

    // Find unread messages from other users
    const unreadMessages = this.messages.filter(
      (message) =>
        message.sender?.id !== this.currentUser?.id &&
        !message.readBy?.some((user) => user.id === this.currentUser?.id)
    );

    if (unreadMessages.length === 0) return;

    const messageReadPersist: MessageReadPersist[] = unreadMessages.map(
      (message) => ({
        messageId: message.id!,
        userIds: [this.currentUser!.id!],
      })
    );

    this.messageService
      .read(messageReadPersist, [
        [
          nameof<Message>((x) => x.readBy),
          nameof<User>((x) => x.fullName),
        ].join('.'),
        [
          nameof<Message>((x) => x.readBy),
          nameof<User>((x) => x.profilePhoto),
          nameof<File>((x) => x.sourceUrl),
        ].join('.'),
        [
          nameof<Message>((x) => x.conversation),
          nameof<Conversation>((x) => x.id),
        ].join('.'),
      ])
      .subscribe({
        next: () => {
          // Update local messages to mark them as read
          unreadMessages.forEach((message) => {
            if (!message.readBy) {
              message.readBy = [];
            }
            message.readBy.push(this.currentUser!);
          });

          // Update conversation's lastMessagePreview if it matches any of the read messages
          const lastReadMessage = unreadMessages[unreadMessages.length - 1];
          const conversation = this.conversations.find(
            (c) => c.id === lastReadMessage.conversation?.id
          );
          if (
            conversation &&
            conversation.lastMessagePreview?.id === lastReadMessage.id
          ) {
            conversation.lastMessagePreview!.readBy = lastReadMessage.readBy;
          }
        },
        error: (error) => {
          this.logService.logFormatted({
            error: 'Failed to mark messages as read',
            details: error,
          });
        },
      });
  }

  private markSingleMessageAsRead(message: Message): void {
    if (!this.currentUser?.id || !message.id) return;

    // Check if message is already read by current user
    if (message.readBy?.some((user) => user.id === this.currentUser?.id)) {
      return;
    }

    const messageReadPersist: MessageReadPersist[] = [
      {
        messageId: message.id,
        userIds: [this.currentUser.id],
      },
    ];

    this.messageService
      .read(messageReadPersist, [
        [
          nameof<Message>((x) => x.readBy),
          nameof<User>((x) => x.fullName),
        ].join('.'),
        [
          nameof<Message>((x) => x.readBy),
          nameof<User>((x) => x.profilePhoto),
          nameof<File>((x) => x.sourceUrl),
        ].join('.'),
        [
          nameof<Message>((x) => x.conversation),
          nameof<Conversation>((x) => x.id),
        ].join('.'),
      ])
      .subscribe({
        next: () => {
          // Update local message to mark it as read
          if (!message.readBy) {
            message.readBy = [];
          }
          message.readBy.push(this.currentUser!);

          // Update conversation's lastMessagePreview if this is the last message
          const conversation = this.conversations.find(
            (c) => c.id === message.conversation?.id
          );
          if (
            conversation &&
            conversation.lastMessagePreview?.id === message.id
          ) {
            conversation.lastMessagePreview!.readBy = message.readBy;
            
            // Decrease unread counter since we just read a message
            if (this.unreadMessagesCount > 0) {
              this.unreadMessagesCount--;
            }
          }
        },
        error: (error) => {
          this.logService.logFormatted({
            error: 'Failed to mark message as read',
            details: error,
          });
        },
      });
  }

  getUserPresenceStatus(userId: string): UserStatus | undefined {
    return this.userPresences.get(userId)?.status;
  }

  isUserOnline(userId: string): boolean {
    if (!userId) return false;

    const presence = this.userPresences.get(userId);
    const isOnline = presence?.status === UserStatus.Online;

    return isOnline;
  }

  onMessageClick(message: Message): void {
    if (this.selectedMessageId === message.id) {
      this.selectedMessageId = undefined;
    } else {
      this.selectedMessageId = message.id;
    }
  }

  isMessageSelected(message: Message): boolean {
    return this.selectedMessageId === message.id;
  }

  hasUnreadMessages(conversation: Conversation): boolean {
    if (!conversation.lastMessagePreview || !this.currentUser?.id) return false;

    // Check if the last message is from another user and not read by current user
    return (
      conversation.lastMessagePreview.sender?.id !== this.currentUser?.id &&
      !conversation.lastMessagePreview.readBy?.some(
        (user) => user.id === this.currentUser?.id
      )
    );
  }

  hasUnseenLastMessage(conversation: Conversation): boolean {
    if (!conversation.lastMessagePreview || !this.currentUser?.id) return false;

    // Check if the last message is from current user and not read by other participant
    if (conversation.lastMessagePreview.sender?.id === this.currentUser.id) {
      return false;
    }

    // For messages from others, check if current user has read it
    return !conversation.lastMessagePreview.readBy?.some(
      (user) => user.id === this.currentUser?.id
    );
  }

  // Always show message preview regardless of read status
  shouldShowMessagePreview(conversation: Conversation): boolean {
    return !!conversation.lastMessagePreview?.content;
  }

  private setupUnreadMessagesCounter(): void {
    // Clear any existing interval first
    if (this.unreadCountInterval) {
      clearInterval(this.unreadCountInterval);
    }

    // Initial count
    this.loadUnreadMessagesCount();

    // Set up interval to check every 10 seconds
    this.unreadCountInterval = setInterval(() => {
      this.loadUnreadMessagesCount();
    }, 25000);
  }

  private loadUnreadMessagesCount(): void {
    if (!this.isUserLoggedIn) return;

    this.messageService.countUnread().subscribe({
      next: (count) => {
        // Only update if the count is different to avoid unnecessary UI updates
        if (this.unreadMessagesCount !== count) {
          this.unreadMessagesCount = count;
        }
      },
      error: (error) => {
        this.logService.logFormatted({
          error: 'Failed to load unread messages count',
          details: error,
        });
      },
    });
  }

  private debouncedLoadUnreadCount(): void {
    // Clear existing timeout
    if (this.unreadCountRefreshTimeout) {
      clearTimeout(this.unreadCountRefreshTimeout);
    }
    
    // Set new timeout to avoid too many server calls
    this.unreadCountRefreshTimeout = setTimeout(() => {
      this.loadUnreadMessagesCount();
    }, 500);
  }

  // Reset unread counter when opening messenger
  private resetUnreadCounter(): void {
    this.unreadMessagesCount = 0;
    // Also refresh from server to ensure accuracy (debounced)
    this.debouncedLoadUnreadCount();
  }
}
