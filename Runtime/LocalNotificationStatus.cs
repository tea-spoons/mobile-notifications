namespace TeaSpoons.Mobile.Notifications
{
    /// <summary>
    /// Represents the current status of a scheduled local notification.
    /// Platform-agnostic wrapper over Unity's platform-specific notification status enums.
    /// </summary>
    public enum LocalNotificationStatus
    {
        /// <summary>
        /// The notification was never scheduled, was cancelled externally,
        /// or the ID was not found.
        /// </summary>
        Unknown,

        /// <summary>
        /// The notification is scheduled and waiting to fire.
        /// </summary>
        Scheduled,

        /// <summary>
        /// The notification has already been delivered to the player.
        /// For non-repeating notifications this means it can be pruned from the cache.
        /// </summary>
        Delivered,
    }
}
