#if UNITY_IOS
using UnityEngine.Scripting;
[assembly: AlwaysLinkAssembly]
namespace TeaSpoons.Mobile.Notifications.IOS
{
    using ServiceLocator;
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.Notifications.iOS;
    
    internal class IOSLocalNotificationService : ILocalNotificationService
    {
        private const string PrefKeyPrefix = "notif_category_enabled_";
        
        private readonly Dictionary<int, HashSet<string>> categoryIndex = new();

        [RuntimeInitializeOnLoadMethod(ServiceBinder.Phase)]
        private static void InitializeService()
        {
            ServiceBinder.For<ILocalNotificationService>().Bind(() => new IOSLocalNotificationService());
        }

        private IOSLocalNotificationService()
        {
            RequestPermission();
        }

        public bool IsPermissionGranted
        {
            get
            {
                var settings = iOSNotificationCenter.GetNotificationSettings();
                return settings.AuthorizationStatus == AuthorizationStatus.Authorized;
            }
        }

        public void RequestPermission()
        {
            using var request = new AuthorizationRequest(
                AuthorizationOption.Alert | AuthorizationOption.Sound | AuthorizationOption.Badge,
                registerForRemoteNotifications: false
            );
        }

        public void OpenNotificationSettings()
            => Application.OpenURL("app-settings:");

        public void Schedule(NotificationData data)
        {
            if (!IsPermissionGranted) return;

            if (!IsCategoryEnabled(data.Category)) return;

            if (!categoryIndex.ContainsKey(data.Category))
            {
                categoryIndex[data.Category] = new HashSet<string>();
            }
            
            categoryIndex[data.Category].Add(data.NotificationId);

            ScheduleOnOS(data);
        }

        public void CancelNotification(string notificationId)
        {
            iOSNotificationCenter.RemoveScheduledNotification(notificationId);
            
            foreach (var identifiers in categoryIndex.Values)
            {
                identifiers.Remove(notificationId);
            }
        }

        public void CancelByCategory(int category)
        {
            if (!categoryIndex.TryGetValue(category, out var identifiers))
            {
                return;
            }

            foreach (var id in identifiers)
            {
                iOSNotificationCenter.RemoveScheduledNotification(id);
            }
            
            categoryIndex.Remove(category);
        }

        public void CancelAll()
        {
            iOSNotificationCenter.RemoveAllScheduledNotifications();
            categoryIndex.Clear();
        }

        public LocalNotificationStatus GetStatus(string notificationId)
        {
            foreach (var notification in iOSNotificationCenter.GetScheduledNotifications())
            {
                if (notification.Identifier == notificationId)
                {
                    return LocalNotificationStatus.Scheduled;
                }
            }

            foreach (var notification in iOSNotificationCenter.GetDeliveredNotifications())
            {
                if (notification.Identifier == notificationId)
                {
                    return LocalNotificationStatus.Delivered;
                }
            }

            return LocalNotificationStatus.Unknown;
        }

        public bool IsScheduled(string notificationId)
            => GetStatus(notificationId) == LocalNotificationStatus.Scheduled;

        public void SetCategoryEnabled(int category, bool enabled)
        {
            PlayerPrefs.SetInt(PrefKeyPrefix + category, enabled ? 1 : 0);
            PlayerPrefs.Save();

            if (!enabled)
            {
                CancelByCategory(category);
            }
        }

        public bool IsCategoryEnabled(int category)
            => PlayerPrefs.GetInt(PrefKeyPrefix + category, 1) == 1;

        public NotificationDeepLink GetLaunchDeepLink()
        {
            var notification = iOSNotificationCenter.QueryLastRespondedNotification().Notification;

            if (notification == null)
            {
                return new NotificationDeepLink { WasOpenedViaNotification = false };
            }

            var deepLink = DeserializeData(notification.Data);

            return new NotificationDeepLink
            {
                WasOpenedViaNotification = true,
                DeepLink = deepLink,
            };
        }

        private void ScheduleOnOS(NotificationData data)
        {
            var delay = data.FireAt - DateTime.Now;
            if (delay <= TimeSpan.Zero && !data.RepeatInterval.HasValue)
            {
                return;
            }

            var notification = new iOSNotification
            {
                Identifier = data.NotificationId,
                Title = data.Title,
                Body = data.Body,
                ShowInForeground = data.ShowDuringRuntime,
                ForegroundPresentationOption = GetForegroundOptions(data.Importance),
                Data = SerializeData(data),
                Trigger = BuildTrigger(data),
            };

            iOSNotificationCenter.ScheduleNotification(notification);
        }

        private static iOSNotificationTrigger BuildTrigger(NotificationData data)
        {
            if (data.RepeatInterval.HasValue)
            {
                return new iOSNotificationCalendarTrigger
                {
                    Repeats = true,
                    Hour = (int?)data.RepeatInterval.Value.TotalHours,
                    Minute = data.RepeatInterval.Value.Minutes,
                    Second = data.RepeatInterval.Value.Seconds,
                };
            }

            return new iOSNotificationTimeIntervalTrigger
            {
                TimeInterval = data.FireAt - DateTime.Now,
                Repeats = false,
            };
        }

        private static PresentationOption GetForegroundOptions(NotificationImportance importance)
            => importance switch
            {
                NotificationImportance.Low => PresentationOption.None,
                NotificationImportance.High => PresentationOption.Alert | PresentationOption.Sound |
                                               PresentationOption.Badge,
                _ => PresentationOption.Alert | PresentationOption.Sound,
            };

        [Serializable]
        private class NotificationPayloadData
        {
            public string deepLinkKey;
            public string deepLinkPayload;
        }

        private static string SerializeData(NotificationData data)
            => JsonUtility.ToJson(new NotificationPayloadData
            {
                deepLinkKey = data.DeepLink.Key,
                deepLinkPayload = data.DeepLink.Payload,
            });

        private static NotificationDeepLinkData DeserializeData(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return new NotificationDeepLinkData(string.Empty, string.Empty);
            }

            var payload = JsonUtility.FromJson<NotificationPayloadData>(raw);
            return new NotificationDeepLinkData(payload.deepLinkKey, payload.deepLinkPayload);
        }
    }
}
#endif
