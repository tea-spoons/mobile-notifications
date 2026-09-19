# Change plan

> Draft. This file tracks what changed on the way to this repo and what I plan to change next. Edit freely.

## Origin

Originally developed at Bigpoint. Published here with Bigpoint's permission for research, education and other noncommercial use. Copyright (c) 2026 Bigpoint; see [LICENSE.md](LICENSE.md).

## Changes made before publishing

- Namespaces are now `TeaSpoons.*` and the package id is `com.tea-spoons.mobile-notifications` (assemblies renamed to match).
- Internal build, registry and tracker references were removed; the repo uses GitHub Actions (`CI` and `Release`) built on `unity-ci-kit`.
- Added `LICENSE.md` (PolyForm Noncommercial 1.0.0), an install section in the README, and package metadata (author, license and documentation URLs).
- Renamed the Android manifest template that carried the old brand name.
- Made standalone: no longer declares Unity Mobile Notifications as a dependency. The Android and iOS implementations are compiled only when it is installed.

## Planned changes

- [x] Tag and publish `v0.5.0` with the Release workflow.
- [ ] Make installs resolve dependencies automatically, for example through a registry such as OpenUPM.
<!-- review-items:start -->
- [ ] **P0** Harden the deep link registry: catch `ReflectionTypeLoadException` per assembly (use the types that did load), use `TypeCache` in the editor, and ship a build-time list or `link.xml` so handlers survive stripping. Verify on a stripped IL2CPP device build.
- [ ] **P1** Replace `async void RequestPermission` with an awaitable that returns the final `LocalNotificationStatus`.
- [ ] **P1** Add tests for the registry (routing, missing handler, bad handler) and scheduling through a fake service.
- [ ] **P1** Evaluate re-basing on Unity's unified `NotificationCenter` to remove the Android/iOS split, if the package minimum allows it.
- [ ] **P1** Declares `unity: 6000.0`, but only Unity 6000.3.8f1 was tested. Add a Unity version matrix to CI once package tests run there (see the `unity-ci-kit` plan), or raise the minimum.
- [ ] **P2** Make the service-locator dependency optional (it is the only own-package dependency).
- [ ] **P2** Add a `CHANGELOG.md`. Unity's package layout lists one next to `README.md`, and the `unity-ci-kit` validator warns without it.
<!-- review-items:end -->

<!-- review:start -->
## Review (September 2026)

Reviewed as a senior Unity engineer would: I read the code and compared the package with similar open-source projects (September 2026). Those projects are listed for ideas only. Nothing was copied from them, and their licenses are noted in case code is ever reused. Priorities: **P0** correctness bug or broken metadata, **P1** should be done soon, **P2** nice to have.

### Compared with

| Project | License | Worth noting |
|---|---|---|
| [Unity Mobile Notifications: unified API](https://docs.unity3d.com/Packages/com.unity.mobile.notifications@2.4/manual/Unified.html) | Unity package | `NotificationCenter` covers Android and iOS with one API: `Initialize`, `RequestPermission` (pending, granted or permanently denied), `ScheduleNotification` with a date-time or interval schedule, `QueryLastRespondedNotification`, `OnNotificationReceived`, badges. |

### Findings from reading the code

- **[Startup / IL2CPP]** `NotificationDeepLinkRegistry` scans every loaded assembly with `GetTypes()` in its constructor, outside any `try` (`ReflectionTypeLoadException` from one bad assembly breaks it), and creates handlers with `Activator.CreateInstance`. The package ships no `[Preserve]` attribute or `link.xml`, so managed code stripping can remove a handler class that is only reached by reflection. Deep links would then do nothing in release builds.
- **[Async]** `AndroidLocalNotificationService.RequestPermission` is `async void` and polls with `Task.Yield()`. Callers cannot await it or see a failure.
- **[Design]** It wraps `AndroidNotificationCenter` and `iOSNotificationCenter` in two platform assemblies. Unity now ships a unified `NotificationCenter` that covers most of both.
- **[Tests]** None, although `ILocalNotificationService` and `NullLocalNotificationService` make the logic easy to test.
<!-- review:end -->

## Notes and ideas

_Add your own here._
