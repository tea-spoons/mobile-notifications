#if !UNITY_ANDROID && !UNITY_IOS
using UnityEngine.Scripting;
[assembly: AlwaysLinkAssembly]
namespace TeaSpoons.Mobile.Notifications
{
    using ServiceLocator;
    using UnityEngine;
    
    internal class NullLocalNotificationService : ILocalNotificationService
    {
        [RuntimeInitializeOnLoadMethod(ServiceBinder.Phase)]
        private static void InitializeService()
        {
            ServiceBinder.For<ILocalNotificationService>().Bind(() => new NullLocalNotificationService());
        }

        private const string Prefix = "NullLocalNotificationService :: ";

        public bool IsPermissionGranted => false;

        public void RequestPermission()
            => Debug.Log($"{Prefix}Permission requested — no-op on this platform.");

        public void OpenNotificationSettings()
            => Debug.Log($"{Prefix}Open notification settings — no-op on this platform.");

        public void Schedule(NotificationData data)
            => Debug.Log($"{Prefix}Schedule '{data.NotificationId}' at {data.FireAt} — no-op on this platform.");

        public void CancelNotification(string notificationId)
            => Debug.Log($"{Prefix}Cancel '{notificationId}' — no-op on this platform.");

        public void CancelByCategory(int category)
            => Debug.Log($"{Prefix}Cancel category '{category}' — no-op on this platform.");

        public void CancelAll()
            => Debug.Log($"{Prefix}Cancel all notifications — no-op on this platform.");

        public LocalNotificationStatus GetStatus(string notificationId)
        {
            Debug.Log($"{Prefix}GetStatus '{notificationId}' — returning Unknown.");
            return LocalNotificationStatus.Unknown;
        }

        public bool IsScheduled(string notificationId)
        {
            Debug.Log($"{Prefix}IsScheduled '{notificationId}' — returning false.");
            return false;
        }

        public void SetCategoryEnabled(int category, bool enabled)
            => Debug.Log($"{Prefix}SetCategoryEnabled '{category}' = {enabled} — no-op on this platform.");

        public bool IsCategoryEnabled(int category)
        {
            Debug.Log($"{Prefix}IsCategoryEnabled '{category}' — returning true.");
            return true;
        }

        public NotificationDeepLink GetLaunchDeepLink()
        {
            Debug.Log($"{Prefix}GetLaunchDeepLink — returning no notification.");
            return new NotificationDeepLink { WasOpenedViaNotification = false };
        }
    }
}
#endif
