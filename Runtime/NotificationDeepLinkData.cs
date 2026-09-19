namespace TeaSpoons.Mobile.Notifications
{
    using System;
    
    /// <summary>
    /// Shared deep link data used both when scheduling a notification
    /// and when the game is launched via a tapped notification.
    /// </summary>
    public readonly struct NotificationDeepLinkData
    {
        /// <summary>
        /// Identifies which screen or feature to navigate to.
        /// Matches a key registered in the game-side deep link registry.
        /// Example: "PlaneDetail"
        /// </summary>
        public readonly string Key;

        /// <summary>
        /// Opaque string payload passed to the deep link handler.
        /// The package does not interpret this — the game deserializes it.
        /// Example: a plane ID, or a JSON string.
        /// </summary>
        public readonly string Payload;

        public NotificationDeepLinkData(string key, string payload)
        {
            Key = key;
            Payload = payload;
        }
    }
}
