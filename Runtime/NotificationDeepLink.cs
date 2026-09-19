namespace TeaSpoons.Mobile.Notifications
{
    /// <summary>
    /// Represents the deep link data extracted from a notification that launched the game.
    /// Retrieved via <see cref="ILocalNotificationService.GetLaunchDeepLink"/> on app start.
    /// </summary>
    public class NotificationDeepLink
    {
        /// <summary>
        /// Whether the game was opened by tapping a notification.
        /// Always check this before reading Key or Payload.
        /// </summary>
        public bool WasOpenedViaNotification;

        /// <summary>
        /// The deep link data embedded in the notification that launched the game.
        /// Only valid when <see cref="WasOpenedViaNotification"/> is true.
        /// </summary>
        public NotificationDeepLinkData DeepLink;
    }
}
