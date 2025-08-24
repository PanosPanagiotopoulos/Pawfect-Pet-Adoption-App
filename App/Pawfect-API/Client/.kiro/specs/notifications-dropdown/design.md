# Design Document

## Overview

The notifications dropdown component will be a standalone Angular component that integrates with the existing header navigation. It will provide a professional interface for users to view and manage their in-app notifications. The component follows the existing design patterns from the header component and uses the established notification service architecture.

## Architecture

### Component Structure
```
src/app/ui/components/notifications-dropdown/
├── notifications-dropdown.component.ts
├── notifications-dropdown.component.html
└── notifications-dropdown.component.css
```

### Integration Points
- **Header Component**: The notifications button and dropdown will be integrated into the existing header component
- **Notification Service**: Uses existing `NotificationService` for API calls
- **Utils Service**: Uses `UtilsService.combineDistinct()` for managing notification lists
- **Date Pipe**: Uses `DateTimeFormatPipe` for date formatting
- **Auth Service**: Checks authentication status to show/hide the component

## Components and Interfaces

### NotificationsDropdownComponent

**Properties:**
- `isOpen: boolean` - Controls dropdown visibility
- `notifications: Notification[]` - Array of unread notifications
- `isLoading: boolean` - Loading state for API calls
- `isMarkingAsRead: boolean` - Loading state for mark as read operations
- `pollingInterval: any` - Reference to the polling interval

**Methods:**
- `ngOnInit()` - Initialize component and start polling
- `ngOnDestroy()` - Clean up polling interval
- `toggleDropdown()` - Toggle dropdown visibility
- `closeDropdown()` - Close dropdown
- `loadNotifications()` - Fetch unread notifications
- `markAsRead(notificationId: string)` - Mark single notification as read
- `markAllAsRead()` - Mark all notifications as read
- `startPolling()` - Start 15-second polling interval
- `stopPolling()` - Stop polling interval
- `handleOutsideClick(event: MouseEvent)` - Handle clicks outside dropdown

### Header Component Integration

**New Properties:**
- `showNotificationsDropdown: boolean` - Controls notifications dropdown visibility
- `notificationCount: number` - Count of unread notifications for badge

**New Methods:**
- `toggleNotificationsDropdown()` - Toggle notifications dropdown
- `closeNotificationsDropdown()` - Close notifications dropdown

## Data Models

### Notification Interface (Existing)
```typescript
interface Notification {
  id?: string;
  user?: User;
  type?: NotificationType;
  title?: string; // HTML content
  content?: string; // HTML content
  isRead?: boolean;
  createdAt?: Date;
  readAt?: Date;
}
```

### NotificationLookup (Existing)
```typescript
interface NotificationLookup extends Lookup {
  ids?: string[];
  userIds?: string[];
  notificationTypes?: NotificationType[];
  isRead?: boolean;
  createFrom?: Date;
  createdTill?: Date;
}
```

## Error Handling

### API Error Handling
- **Network Errors**: Display user-friendly error messages using existing snackbar service
- **Authentication Errors**: Gracefully handle unauthorized access
- **Service Unavailable**: Show appropriate fallback message

### Polling Error Handling
- **Failed Requests**: Continue polling but log errors
- **Component Destruction**: Ensure polling stops to prevent memory leaks

### User Action Errors
- **Mark as Read Failures**: Show error message and maintain notification in list
- **Empty States**: Display appropriate message when no notifications exist

## Testing Strategy

### Unit Tests
- Component initialization and destruction
- Polling mechanism start/stop
- Notification loading and error handling
- Mark as read functionality (single and bulk)
- Dropdown toggle behavior
- Outside click handling

### Integration Tests
- Header component integration
- Service interaction testing
- Date pipe formatting
- HTML content rendering

### User Experience Tests
- Responsive design on mobile and desktop
- Accessibility compliance
- Loading states and transitions
- Error state handling

## UI/UX Design

### Desktop Layout
```
[Logo] [Navigation] [Search] [Notifications] [Profile] [Menu]
                              ↓
                    [Notifications Dropdown]
                    ┌─────────────────────────┐
                    │ Mark All as Read        │
                    ├─────────────────────────┤
                    │ [Notification 1]    [×] │
                    │ Title                   │
                    │ Content                 │
                    │ Date                    │
                    ├─────────────────────────┤
                    │ [Notification 2]    [×] │
                    │ ...                     │
                    └─────────────────────────┘
```

### Mobile Layout
```
[Menu] [Logo] [Search] [Notifications] [Profile]
                        ↓
              [Notifications Dropdown]
              ┌─────────────────────┐
              │ Mark All as Read    │
              ├─────────────────────┤
              │ [Notification] [×]  │
              │ Title               │
              │ Content             │
              │ Date                │
              └─────────────────────┘
```

### Styling Approach
- **Tailwind CSS**: Use utility classes for consistent styling
- **Responsive Design**: Mobile-first approach with responsive breakpoints
- **Accessibility**: ARIA labels, keyboard navigation, focus management
- **Animation**: Smooth dropdown transitions using Tailwind transitions

### Color Scheme
- Follow existing app color palette
- Use semantic colors for actions (success, error, warning)
- Maintain proper contrast ratios for accessibility

## Technical Implementation Details

### Polling Strategy
- **Interval**: 15 seconds as specified
- **Lifecycle Management**: Start on component init, stop on destroy
- **Error Resilience**: Continue polling even if individual requests fail
- **Performance**: Use `combineDistinct` to avoid duplicate notifications

### HTML Content Rendering
- **Security**: Trust HTML content as it comes from the backend
- **Styling Preservation**: Use `[innerHTML]` to render HTML with existing styles
- **Responsive**: Ensure HTML content adapts to dropdown width

### State Management
- **Local State**: Component manages its own notification state
- **Reactive Updates**: Use RxJS for handling async operations
- **Memory Management**: Proper subscription cleanup to prevent leaks

### Integration with Existing Header
- **Minimal Changes**: Add notification button and dropdown without disrupting existing functionality
- **Consistent Behavior**: Follow same patterns as existing dropdowns (language, user menu)
- **Event Handling**: Integrate with existing outside click handling

## Performance Considerations

### Optimization Strategies
- **Lazy Loading**: Component only loads when user is authenticated
- **Efficient Polling**: Only poll when component is active
- **Memory Management**: Clean up intervals and subscriptions
- **Minimal DOM Updates**: Use trackBy functions for notification lists

### Caching Strategy
- **No Client Caching**: Always fetch fresh data to ensure real-time updates
- **Server-Side Caching**: Rely on backend caching for performance
- **Debouncing**: Prevent rapid successive API calls

## Accessibility

### ARIA Support
- **Role Attributes**: Proper roles for dropdown and notifications
- **Labels**: Descriptive labels for screen readers
- **Live Regions**: Announce notification updates

### Keyboard Navigation
- **Tab Order**: Logical tab sequence through notifications
- **Escape Key**: Close dropdown with escape key
- **Enter/Space**: Activate buttons with keyboard

### Visual Accessibility
- **Contrast**: Ensure sufficient color contrast
- **Focus Indicators**: Clear focus states for all interactive elements
- **Text Size**: Readable text sizes for all content