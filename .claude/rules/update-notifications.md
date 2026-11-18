---
paths:
  - 'WandEnhancer/Core/UpdateNotifier.cs'
  - 'WandEnhancer/WandEnhancer.csproj'
---

# Update notifications

- Opt-in at compile time through the `EnableUpdateNotifications` MSBuild property and the `ENABLE_UPDATE_NOTIFICATIONS` symbol. Default and official builds exclude `UpdateNotifier` and its network code entirely.
- This fork publishes offline and update variants through `.github/workflows/build.yml`. Release API URLs use `Constants.Owner` and `Constants.RepoName` (`titonax/kbytestuff`), so notifications open this fork's releases.
- The opt-in variant checks only GitHub's latest public release. The check is on by default (`AppSettings.CheckUpdates`); `CheckPrereleases` also reads the releases list. It shows a native Windows notification and a header badge. Clicking either opens the `UpdatePopup` notes modal on the UI thread (the release page when only the launcher runs). It never downloads or installs anything.
