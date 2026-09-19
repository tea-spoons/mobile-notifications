namespace TeaSpoons.Mobile.Notifications
{
	using System;

    /// <summary>
    /// All data required to schedule a local notification.
    /// </summary>
    public class NotificationData
    {
        /// <summary>
        /// Unique identifier for this notification.
        /// Used for deduplication, cancellation, status checks, and cache lookups.
        /// Example: "plane_repair_123"
        /// </summary>
        public string NotificationId;

        /// <summary>
        /// The notification title shown in bold.
        /// </summary>
        public string Title;

        /// <summary>
        /// The notification body text.
        /// </summary>
        public string Body;

        /// <summary>
        /// Android only. Small icon shown in the status bar.
        /// Must be a white-on-transparent PNG registered in Mobile Notification Settings.
        /// Ignored on iOS.
        /// </summary>
        public string SmallIcon;

        /// <summary>
        /// Android only. Large icon shown in the notification shade.
        /// Ignored on iOS.
        /// </summary>
        public string LargeIcon;

        /// <summary>
        /// The absolute time at which this notification should fire.
        /// On reschedule, the delay is recomputed as FireAt - DateTime.Now.
        /// </summary>
        public DateTime FireAt;

        /// <summary>
        /// If set, the notification repeats at this interval after the first delivery.
        /// Null means one-time only. Repeating notifications are not cancelled
        /// on foreground — the OS manages their repetition.
        /// </summary>
        public TimeSpan? RepeatInterval;

        /// <summary>
        /// Game-defined category identifier.
        /// Used for per-category cancellation and player opt-in/out in settings.
        /// Define your own constants in a game-side static class.
        /// </summary>
        public int Category;

        /// <summary>
        /// Controls the interruption level of the notification.
        /// </summary>
        public NotificationImportance Importance;

        /// <summary>
        /// If true, the notification is shown even while the game is in the foreground.
        /// If false, it is cancelled on foreground and rescheduled on background.
        /// </summary>
        public bool ShowDuringRuntime;

        /// <summary>
        /// Deep link data embedded in the notification payload.
        /// When the player taps the notification, the game uses this to navigate
        /// to the correct screen via the game-side deep link registry.
        /// </summary>
        public NotificationDeepLinkData DeepLink;
    }
}
