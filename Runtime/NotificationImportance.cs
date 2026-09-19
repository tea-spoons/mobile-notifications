namespace TeaSpoons.Mobile.Notifications
{
    /// <summary>
    /// Controls the visual and auditory interruption level of a notification.
    /// Maps to Android notification channel importance and iOS interruption level.
    /// </summary>
    public enum NotificationImportance
    {
        /// <summary>
        /// Silent. No sound, no heads-up. Shows in shade only.
        /// Android: IMPORTANCE_LOW | iOS: passive
        /// </summary>
        Low,

        /// <summary>
        /// Makes sound but no heads-up popup.
        /// Android: IMPORTANCE_DEFAULT | iOS: active
        /// </summary>
        Default,

        /// <summary>
        /// Makes sound and shows as heads-up popup.
        /// Android: IMPORTANCE_HIGH | iOS: timeSensitive
        /// </summary>
        High,
    }
}
