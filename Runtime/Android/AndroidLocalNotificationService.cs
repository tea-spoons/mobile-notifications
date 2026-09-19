#if UNITY_ANDROID
using UnityEngine.Scripting;
[assembly: AlwaysLinkAssembly]
namespace TeaSpoons.Mobile.Notifications.Android
{
    using ServiceLocator;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using Unity.Notifications.Android;

    internal class AndroidLocalNotificationService : ILocalNotificationService
    {
        private const string PrefKeyPrefix = "notif_category_enabled_";

        private readonly CancellationTokenSource appLifetime = new();
        private readonly Dictionary<int, HashSet<string>> categoryIndex = new();

        [RuntimeInitializeOnLoadMethod(ServiceBinder.Phase)]
        private static void InitializeService()
        {
            ServiceBinder.For<ILocalNotificationService>().Bind(() => new AndroidLocalNotificationService());
        }

        private AndroidLocalNotificationService()
        {
            RegisterDefaultChannels();
            RequestPermission();
            Application.quitting += () => appLifetime.Cancel();
        }

        public bool IsPermissionGranted
            => AndroidNotificationCenter.UserPermissionToPost == PermissionStatus.Allowed;

        public async void RequestPermission()
        {
            try
            {
                var request = new PermissionRequest();
                while (request.Status == PermissionStatus.RequestPending)
                {
                    await Task.Yield();
                    appLifetime.Token.ThrowIfCancellationRequested();
                }
            }
            catch (OperationCanceledException) { }
        }

        public void OpenNotificationSettings()
            => AndroidNotificationCenter.OpenNotificationSettings();

        private static void RequestExactSchedulingIfNeeded()
        {
            if (AndroidNotificationCenter.UsingExactScheduling)
            {
                return;
            }
            
            AndroidNotificationCenter.RequestExactScheduling();
        }
        
        private static void RequestBatteryOptimizationExemption()
        {
            if (AndroidNotificationCenter.IgnoringBatteryOptimizations)
            {
                return;
            }
            AndroidNotificationCenter.RequestIgnoreBatteryOptimizations();
        }

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
            AndroidNotificationCenter.CancelNotification(notificationId.GetHashCode());

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
                AndroidNotificationCenter.CancelNotification(id.GetHashCode());
            }
            
            categoryIndex.Remove(category);
        }

        public void CancelAll()
        {
            AndroidNotificationCenter.CancelAllNotifications();
            categoryIndex.Clear();
        }

        public LocalNotificationStatus GetStatus(string notificationId)
        {
            var status = AndroidNotificationCenter.CheckScheduledNotificationStatus(notificationId.GetHashCode());
            return status switch
            {
                NotificationStatus.Scheduled => LocalNotificationStatus.Scheduled,
                NotificationStatus.Delivered => LocalNotificationStatus.Delivered,
                _ => LocalNotificationStatus.Unknown,
            };
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
            var intent = AndroidNotificationCenter.GetLastNotificationIntent();

            if (intent == null)
            {
                return new NotificationDeepLink { WasOpenedViaNotification = false };
            }

            var deepLink = DeserializeIntentData(intent.Notification.IntentData);

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

            var notification = new AndroidNotification
            {
                Title = data.Title,
                Text = data.Body,
                SmallIcon = data.SmallIcon,
                LargeIcon = data.LargeIcon,
                FireTime = data.FireAt,
                RepeatInterval = data.RepeatInterval,
                ShowInForeground = data.ShowDuringRuntime,
                IntentData = SerializeIntentData(data),
            };

            AndroidNotificationCenter.SendNotificationWithExplicitID(
                notification,
                data.Importance.ToString(),
                data.NotificationId.GetHashCode()
            );
        }

        private void RegisterDefaultChannels()
        {
            AndroidNotificationCenter.RegisterNotificationChannel(new AndroidNotificationChannel
            {
                Id = NotificationImportance.Low.ToString(),
                Name = "Low Priority",
                Importance = Importance.Low,
                Description = "Low priority notifications",
            });

            AndroidNotificationCenter.RegisterNotificationChannel(new AndroidNotificationChannel
            {
                Id = NotificationImportance.Default.ToString(),
                Name = "Default",
                Importance = Importance.Default,
                Description = "Default notifications",
            });

            AndroidNotificationCenter.RegisterNotificationChannel(new AndroidNotificationChannel
            {
                Id = NotificationImportance.High.ToString(),
                Name = "High Priority",
                Importance = Importance.High,
                Description = "High priority notifications",
            });
        }

        [Serializable]
        private class NotificationIntentData
        {
            public string deepLinkKey;
            public string deepLinkPayload;
        }

        private static string SerializeIntentData(NotificationData data)
            => JsonUtility.ToJson(new NotificationIntentData
            {
                deepLinkKey = data.DeepLink.Key,
                deepLinkPayload = data.DeepLink.Payload,
            });

        private static NotificationDeepLinkData DeserializeIntentData(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return new NotificationDeepLinkData(string.Empty, string.Empty);
            }

            var intent = JsonUtility.FromJson<NotificationIntentData>(raw);
            return new NotificationDeepLinkData(intent.deepLinkKey, intent.deepLinkPayload);
        }
    }
}
#endif
