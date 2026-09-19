# TeaSpoons Mobile Notifications

Local notification abstraction layer for Android and iOS Unity projects.
Built on top of [Unity Mobile Notifications](https://docs.unity3d.com/Packages/com.unity.mobile.notifications@2.3/manual/index.html).

## How It Works

### Startup

On app start the platform service is auto-registered. It:

1. Registers default notification channels (Android only)
2. Requests notification permission
3. Requests exact scheduling permission (Android only)
4. Requests battery optimization exemption (Android only)

### Responsibility Split

The package is intentionally thin. It does not manage focus lifecycle, caching,
or scheduling strategy — those belong to the game.

| Responsibility | Owner |
|---|---|
| Sending notifications to OS | Package |
| Cancelling notifications from OS | Package |
| Permission handling | Package |
| Category opt-in/out persistence | Package |
| Category index for CancelByCategory | Package (in-memory) |
| Deciding what to schedule | Game |
| Scheduling on background | Game |
| Cancelling on foreground | Game |
| Deep link routing | Game |

### Recommended Game-Side Pattern

Subscribe to `Application.focusChanged` in a dedicated `NotificationSchedulerService`
and rebuild all notifications from current game state on every background:

```csharp
public class NotificationSchedulerService
{
    private readonly ILocalNotificationService notifications;
    private readonly IPlaneService planeService;

    public NotificationSchedulerService(ILocalNotificationService notifications, IPlaneService planeService)
    {
        notifications = notifications;
        planeService = planeService;

        Application.focusChanged += OnFocusChanged;
    }

    private void OnFocusChanged(bool hasFocus)
    {
        if (hasFocus)
        {
            OnForegrounded();
        }
        else
        {
            OnBackgrounded();
        }
    }

    private void OnForegrounded()
    {
        notifications.CancelAll();

        var deepLink = notifications.GetLaunchDeepLink();
        if (deepLink.WasOpenedViaNotification)
        {    
            deepLinkRegistry.Handle(deepLink);
        }
    }

    private void OnBackgrounded()
    {
        foreach (var plane in planeService.GetPlanesUnderRepair())
        {
            notifications.Schedule(new NotificationData
            {
                NotificationId    = $"plane_repair_{plane.Id}",
                Title             = "Plane Ready!",
                Body              = $"{plane.Name} has been repaired",
                Category          = MyGameNotificationCategory.FlightReady,
                Importance        = NotificationImportance.High,
                FireAt            = plane.RepairFinishTime,
                ShowDuringRuntime = false,
                DeepLink          = new NotificationDeepLinkData("PlaneDetail", plane.Id),
            });
        }
    }
}
```

Rebuilding from game state on every background ensures notifications always
reflect the true state of the game — including booster modifications or
cancellations that happened while the game was open.

## Setup

### Android

In `Edit > Project Settings > Mobile Notification Settings`:
- Register your small icon (white-on-transparent PNG)
- Register your large icon

The package automatically includes the required Android permissions via
`Plugins/Android/TeaSpoonsNotificationsManifest.xml`:
- `POST_NOTIFICATIONS` — required on Android 13+
- `SCHEDULE_EXACT_ALARM` — for exact notification timing
- `USE_EXACT_ALARM` — alternative exact alarm permission for Android 13+
- `REQUEST_IGNORE_BATTERY_OPTIMIZATIONS` — for reliable delivery on battery saving devices

### iOS

Permission is requested automatically on first launch. No additional setup required.

## Usage

### Defining Categories

Define your game-specific notification categories game-side:

```csharp
public static class MyGameNotificationCategory
{
    public const int BuildingComplete = 100;
    public const int DailyReward      = 101;
    public const int FlightReady      = 102;
    public const int LimitedTimeEvent = 103;
}
```

### Scheduling a Notification

One-time notification:

```csharp
Services.Get<ILocalNotificationService>().Schedule(new NotificationData
{
    NotificationId    = "plane_repair_123",
    Title             = "Plane Ready!",
    Body              = "Your Jumbo Jet has been repaired and is ready to fly.",
    SmallIcon         = "icon_small",
    LargeIcon         = "icon_large",
    Category          = MyGameNotificationCategory.FlightReady,
    Importance        = NotificationImportance.High,
    FireAt            = plane.RepairFinishTime,
    ShowDuringRuntime = false,
    DeepLink          = new NotificationDeepLinkData("PlaneDetail", planeId),
});
```

Repeating notification:

```csharp
Services.Get<ILocalNotificationService>().Schedule(new NotificationData
{
    NotificationId    = "daily_reward",
    Title             = "Daily Reward Available!",
    Body              = "Come back and claim your daily reward.",
    Category          = MyGameNotificationCategory.DailyReward,
    Importance        = NotificationImportance.Default,
    FireAt            = DateTime.Now.AddDays(1),
    RepeatInterval    = TimeSpan.FromDays(1),
    ShowDuringRuntime = false,
});
```

### Cancelling Notifications

Cancel a specific notification:

```csharp
Services.Get<ILocalNotificationService>().CancelNotification("plane_repair_123");
```

Cancel all notifications in a category:

```csharp
Services.Get<ILocalNotificationService>().CancelByCategory(MyGameNotificationCategory.FlightReady);
```

Cancel all notifications:

```csharp
Services.Get<ILocalNotificationService>().CancelAll();
```

### Checking Notification Status

```csharp
var notifications = Services.Get<ILocalNotificationService>();

// Simple scheduled check
bool isPending = notifications.IsScheduled("plane_repair_123");

// Full status
var status = notifications.GetStatus("plane_repair_123");
switch (status)
{
    case LocalNotificationStatus.Scheduled:
        // waiting to fire
        break;
    case LocalNotificationStatus.Delivered:
        // already shown to the player
        break;
    case LocalNotificationStatus.Unknown:
        // not found or cancelled externally
        break;
}
```

### Permission Handling

Permission is requested automatically on startup. Use `IsPermissionGranted` to
reflect the current state in your settings UI. If the player permanently denied
permission, redirect them to system settings:

```csharp
var notifications = Services.Get<ILocalNotificationService>();

if (!notifications.IsPermissionGranted)
{
  notifications.OpenNotificationSettings();
}
```

### Player Category Settings

Allow players to opt in/out of notification categories in your settings menu.
Disabling a category immediately cancels all its scheduled notifications.
Preferences are persisted across sessions via `PlayerPrefs`:

```csharp
var notifications = Services.Get<ILocalNotificationService>();

// Disable a category
notifications.SetCategoryEnabled(MyGameNotificationCategory.LimitedTimeEvent, false);

// Re-enable
notifications.SetCategoryEnabled(MyGameNotificationCategory.LimitedTimeEvent, true);

// Check current state
bool isEnabled = notifications.IsCategoryEnabled(MyGameNotificationCategory.DailyReward);
```

### Deep Linking

When the game is launched by tapping a notification, call `GetLaunchDeepLink()`
and route the player to the correct screen via your game-side registry:

```csharp
var deepLink = Services.Get<ILocalNotificationService>().GetLaunchDeepLink();
if (deepLink.WasOpenedViaNotification)
{
    deepLinkRegistry.Handle(deepLink);
}
```

Mark handlers with `[NotificationDeepLinkHandler]` — they are auto-discovered
and registered by `NotificationDeepLinkRegistry` at runtime:

```csharp
[NotificationDeepLinkHandler]
public class PlaneDetailDeepLinkHandler : INotificationDeepLinkHandler
{
    public string Key => "PlaneDetail";

    public void Handle(string payload)
        => Services.Get<IPlaneService>().OpenPlaneDetail(payload);
}
```

Instantiate the registry game-side and call `Handle` with the deep link:

```csharp
var registry = new NotificationDeepLinkRegistry();
registry.Handle(deepLink);
```

## Platform Notes

| Feature                       | Android               | iOS                      |
|------------------------------|-----------------------|--------------------------|
| Small icon                   | ✅ Required           | ❌ Ignored               |
| Large icon                   | ✅ Optional           | ❌ Ignored               |
| Permission request           | Auto (API 33+)        | ✅ Auto on first launch   |
| Exact scheduling             | ✅ Requested on start | ❌ Not applicable        |
| Battery optimization exempt  | ✅ Requested on start | ❌ Not applicable        |
| Repeating notifications      | ✅ Any interval       | ⚠️ Calendar-based only    |
| Foreground display           | ✅ Configurable       | ✅ Configurable          |
| Scheduled notification query | ✅ Native OS API      | ✅ Native OS API         |
| Delivered notification query | ✅ Native OS API      | ✅ Native OS API         |
| Open system settings         | ✅ Native API         | ✅ Via app-settings:     |

## Installation

In Unity: **Window > Package Manager > + > Add package from git URL**, then enter:

```
https://github.com/tea-spoons/mobile-notifications.git
```

Pin a release by appending a tag, for example `#v0.4.1`.

### Dependencies

Unity cannot resolve git dependencies automatically, so add these to your project first:

- `com.unity.mobile.notifications` 2.4.3
- `com.tea-spoons.service-locator` 0.5.6

## Change plan

See [CHANGE-PLAN.md](CHANGE-PLAN.md) for what changed before publishing and what is planned next.

## License

Copyright (c) 2026 Bigpoint. Authored by Muhammad Tarek Abdou.

Available for research, education and other noncommercial use under the [PolyForm Noncommercial 1.0.0](LICENSE.md)
license. Commercial use is not permitted.
