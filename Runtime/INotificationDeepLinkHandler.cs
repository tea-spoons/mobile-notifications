namespace TeaSpoons.Mobile.Notifications
{
    /// <summary>
    /// Defines a handler for a specific deep link key triggered when
    /// the game is opened via a tapped notification.
    ///
    /// Implement this interface game-side and register it in your
    /// deep link registry. The package does not handle routing — it
    /// only delivers the <see cref="NotificationDeepLink"/> data.
    ///
    /// Example:
    /// <code>
    /// public class PlaneDetailDeepLinkHandler : INotificationDeepLinkHandler
    /// {
    ///     public string Key => "PlaneDetail";
    ///
    ///     public void Handle(string payload)
    ///     {
    ///         // navigate to plane detail screen using payload as plane ID
    ///     }
    /// }
    /// </code>
    /// </summary>
    public interface INotificationDeepLinkHandler
    {
        /// <summary>
        /// The unique key this handler responds to.
        /// Must match <see cref="NotificationDeepLinkData.Key"/> set when scheduling.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Called when the game is launched via a notification with a matching key.
        /// </summary>
        /// <param name="payload">
        /// The opaque string payload from <see cref="NotificationDeepLinkData.Payload"/>.
        /// Deserialize this game-side as needed.
        /// </param>
        void Handle(string payload);
    }
}
