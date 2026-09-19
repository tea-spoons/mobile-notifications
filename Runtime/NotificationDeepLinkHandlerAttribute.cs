namespace TeaSpoons.Mobile.Notifications
{
    using System;

    /// <summary>
    /// Marks a class as a notification deep link handler.
    /// Classes marked with this attribute are automatically discovered
    /// and registered in <see cref="NotificationDeepLinkRegistry"/> on startup.
    ///
    /// The marked class must implement <see cref="INotificationDeepLinkHandler"/>
    /// and have a parameterless constructor.
    ///
    /// Example:
    /// <code>
    /// [NotificationDeepLinkHandler]
    /// public class PlaneDetailDeepLinkHandler : INotificationDeepLinkHandler
    /// {
    ///     public string Key => "PlaneDetail";
    ///
    ///     public void Handle(string payload)
    ///         => Services.Get{IPlaneService}().OpenPlaneDetail(payload);
    /// }
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class NotificationDeepLinkHandlerAttribute : Attribute
    {
    }
}
