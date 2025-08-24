# Implementation Plan

- [x] 1. Create notifications dropdown component structure

  - Create the component directory and files at `src/app/ui/components/notifications-dropdown/`
  - Generate component with TypeScript, HTML, and CSS files
  - Set up basic component class structure with required imports
  - _Requirements: 1.1, 2.1_

- [x] 2. Implement notification data management and polling

  - Add properties for notifications array, loading states, and polling interval

  - Implement `loadNotifications()` method using `notificationService.queryMineUnread()`
  - Implement 15-second polling mechanism with `startPolling()` and `stopPolling()` methods
  - Use `utilsService.combineDistinct()` to merge new notifications with existing ones
  - Add proper lifecycle management in `ngOnInit()` and `ngOnDestroy()`
  - _Requirements: 3.1, 3.2, 3.3, 3.4_

- [x] 3. Implement notification read functionality

  - Create `markAsRead(notificationId: string)` method using `notificationService.read()`
  - Create `markAllAsRead()` method to mark all notifications as read

  - Remove read notifications from the local notifications array
  - Add loading states for read operations
  - Handle API errors with appropriate user feedback
  - _Requirements: 4.1, 4.2, 4.3, 4.4, 5.1, 5.2, 5.3, 5.4_

- [ ] 4. Create notification dropdown template


  - Design HTML template with dropdown container and notification list
  - Implement notification item template with title, content, and date display
  - Use `[innerHTML]` for HTML content rendering to preserve styling
  - Add date formatting using `dateTimeFormatter` pipe with "dd/MM/yyyy : HH:mm" format
  - Include "mark as read" button for each notification
  - Add "mark all as read" button at the top of the dropdown
  - Implement empty state message when no notifications exist
  - _Requirements: 2.2, 2.3, 2.4, 4.1, 5.1, 5.5_

-

- [x] 5. Style notifications dropdown with Tailwind CSS

  - Apply Tailwind utility classes for dropdown positioning and appearance
  - Style notification items with proper spacing and typography
  - Implement responsive design for mobile and desktop layouts
  - Add hover and focus states for interactive elements
  - Ensure adequate spacing for dynamic HTML content
  - Add loading spinners and transition animations
  - _Requirements: 2.5, 6.2, 6.3_

- [x] 6. Implement dropdown visibility and interaction logic


  - Add `toggleDropdown()` and `closeDropdown()` methods
  - Implement outside click detection with `handleOutsideClick()` method
  - Add keyboard navigation support (escape key to close)
  - Manage dropdown state with `isOpen` property
  - _Requirements: 1.5, 2.6_

- [ ] 7. Integrate notifications button into header component








  - Add notifications button to header template positioned correctly for desktop and mobile
  - Add click handler to toggle notifications dropdown

  - Include notification count badge if there are unread notifications
  - Use Lucide Angular bell icon for the notifications button
  - Ensure button is only visible for logged-in users
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 6.4_

- [ ] 8. Add notifications dropdown to header component template









  - Include notifications dropdown component in header template
  - Position dropdown correctly under the notifications button
  - Integrate with existing outside click handling in header component
  - Ensure dropdown closes when other dropdowns open (language, user menu)
  - _Requirements: 2.6, 6.3_


-

- [-] 9. Implement i18n support for all text content




  - Add translation keys for all user-facing text using APP.\* structure
  - Include keys for: "Mark as read", "Mark all as read", "No notifications", loading states, error messages
  - Apply translation pipe to all text in templates

  - Ensure proper pluralization for notification counts
  - _Requirements: 6.1_

- [ ] 10. Add error handling and user feedback



  - Implement error handling for API failures in notification loading
  - Add error handling for mark as read operations

  - Display user-friendly error messages using ex
isting snackbar service
  - Handle network connectivity issues gracefully
  - Add retry mechanisms for failed operations
  - _Requirements: 4.4, 5.4_

- [ ] 11. Ensure component integration and module imports





  - Add notifications dropdown component to appropriate Angular module
  - Ensure all required services are properly injected
  - Verify DateTimeFormatPipe is available in the component
  - Check that Lucide Angular icons are properly imported
  - Test that component builds without errors
  - _Requirements: 6.5_

- [ ] 12. Implement accessibility features


  - Add ARIA labels and roles for screen reader support
  - Implement keyboard navigation for dropdown and notification items
  - Ensure proper focus management when opening/closing dropdown
  - Add live region announcements for notification updates
  - Verify color contrast and text readability
  - _Requirements: 6.3_
