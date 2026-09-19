namespace TeaSpoons.Mobile.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Auto-discovers and manages all <see cref="INotificationDeepLinkHandler"/> 
    /// implementations marked with <see cref="NotificationDeepLinkHandlerAttribute"/>.
    ///
    /// Instantiated automatically by the local notification service on startup.
    /// No manual registration required — just mark your handler with
    /// <see cref="NotificationDeepLinkHandlerAttribute"/>.
    /// </summary>
    public class NotificationDeepLinkRegistry
    {
        private readonly Dictionary<string, INotificationDeepLinkHandler> handlers = new();

        public NotificationDeepLinkRegistry()
        {
            foreach (var type in AppDomain.CurrentDomain.GetAssemblies()
                         .SelectMany(a => a.GetTypes())
                         .Where(t => t.GetCustomAttribute<NotificationDeepLinkHandlerAttribute>() != null
                                     && typeof(INotificationDeepLinkHandler).IsAssignableFrom(t)
                                     && !t.IsAbstract))
            {
                try
                {
                    var handler = (INotificationDeepLinkHandler)Activator.CreateInstance(type);
                    handlers[handler.Key] = handler;
                }
                catch (Exception e)
                {
                    Debug.LogError($"NotificationDeepLinkRegistry :: Failed to instantiate deep link handler {type.Name}: {e.Message}");
                }
            }
        }

        public void Handle(NotificationDeepLink deepLink)
        {
            if (!deepLink.WasOpenedViaNotification)
                return;

            if (string.IsNullOrEmpty(deepLink.DeepLink.Key))
                return;

            if (!handlers.TryGetValue(deepLink.DeepLink.Key, out var handler))
            {
                Debug.LogWarning($"NotificationDeepLinkRegistry :: No handler registered for deep link key: {deepLink.DeepLink.Key}");
                return;
            }

            handler.Handle(deepLink.DeepLink.Payload);
        }
    }
}
