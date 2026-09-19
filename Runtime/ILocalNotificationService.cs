namespace TeaSpoons.Mobile.Notifications
{
	using ServiceLocator;
    
	/// <summary>
    /// Contract for the local notification service.
    /// The platform-specific implementation is self-registered on startup
    /// via <see cref="ServiceBinder.Phase"/>.
    ///
    /// Consume via the service locator:
    /// <code>
    /// Services.Get{ILocalNotificationService}().Schedule(data);
    /// </code>
    /// </summary>
    public interface ILocalNotificationService : IService
    {
        /// <summary>
        /// Whether the player has granted permission to post notifications.
        /// On Android 12 and below, always returns true.
        /// On Android 13+ and iOS, reflects the actual system permission status.
        /// Check this before showing notification-related UI.
        /// </summary>
        bool IsPermissionGranted { get; }

        /// <summary>
        /// Requests the player's permission to show notifications.
        /// Called automatically on app start — no need to call this manually.
        /// On iOS — shows the system dialog once, no-op on subsequent calls.
        /// On Android 13+ — shows the system dialog if not permanently denied.
        /// On Android 12 and below — no-op.
        /// </summary>
        void RequestPermission();

        /// <summary>
        /// Opens the system settings page for your app so the player can
        /// manually enable notifications.
        /// Use this when <see cref="IsPermissionGranted"/> is false and
        /// <see cref="RequestPermission"/> is no longer effective.
        /// </summary>
        void OpenNotificationSettings();

        /// <summary>
        /// Schedules a local notification.
        /// If a notification with the same <see cref="NotificationData.NotificationId"/>
        /// is already scheduled, it will be replaced.
        /// Silently ignored if the notification's category is disabled by the player
        /// or if permission has not been granted.
        /// </summary>
        void Schedule(NotificationData data);

        /// <summary>
        /// Cancels a previously scheduled notification by its ID.
        /// No-op if the notification does not exist.
        /// </summary>
        void CancelNotification(string notificationId);

        /// <summary>
        /// Cancels all scheduled notifications belonging to a specific category.
        /// </summary>
        void CancelByCategory(int category);

        /// <summary>
        /// Cancels all scheduled notifications regardless of category.
        /// </summary>
        void CancelAll();

        /// <summary>
        /// Returns the current status of a notification by its ID.
        /// Use this to check if a notification has already been delivered
        /// before rescheduling or pruning the cache.
        /// </summary>
        LocalNotificationStatus GetStatus(string notificationId);

        /// <summary>
        /// Returns true if a notification with the given ID is currently scheduled.
        /// Use this to avoid scheduling duplicates.
        /// </summary>
        bool IsScheduled(string notificationId);

        /// <summary>
        /// Enables or disables a notification category.
        /// Disabled categories are silently skipped in <see cref="Schedule"/>.
        /// Cancels all scheduled notifications in the category when disabled.
        /// Persisted across sessions via PlayerPrefs.
        /// </summary>
        void SetCategoryEnabled(int category, bool enabled);

        /// <summary>
        /// Returns whether the given category is currently enabled.
        /// Categories are enabled by default.
        /// </summary>
        bool IsCategoryEnabled(int category);

        /// <summary>
        /// Returns the deep link data from the notification that launched the game.
        /// Always call this on app start and check
        /// <see cref="NotificationDeepLink.WasOpenedViaNotification"/> before acting on it.
        /// </summary>
        NotificationDeepLink GetLaunchDeepLink();
    }
}
