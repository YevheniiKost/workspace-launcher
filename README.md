# WorkspaceLauncher

A lightweight Windows desktop tool for managing and launching development workspaces.  
Define named profiles — each containing a set of executables, folders, and URLs — and launch
your entire workspace with a single click.

---

## Features

- **Profiles** — create any number of named profiles, each with its own accent color and icon.
- **Launch items** — each profile holds an ordered list of items:
  - `Executable` — runs an `.exe` with an optional startup delay.
  - `Folder` — opens a directory in Windows Explorer.
  - `Url` — opens a URL in the default browser.
- **Delay support** — set a per-item delay (ms) so applications start in the right sequence.
- **Session management** — track running processes per profile; end the whole session at once.
  Items marked *Close on session end* are terminated when the session is closed.
- **System tray** — minimize to tray; double-click the icon to restore; launch or quit profiles
  directly from the tray context menu.
- **Auto-launch** — optionally launch a selected profile automatically on application start.
- **Run on startup** — register the app in the Windows registry to start with Windows
  (supports `--minimized` flag so it starts quietly in the tray).
- **Auto-update** — on startup the app silently checks GitHub for a new release; if one is
  found you are prompted to update and the app installs it automatically then restarts.
- **Persistent settings** — all profiles and settings are saved to
  `%AppData%\WorkspaceLauncher\profiles.json`.

---

## Requirements

| Component | Version |
|-----------|---------|
| Windows   | 10 / 11 |
| .NET      | 8.0     |

---

## Getting Started

### Build from source

```bash
git clone https://github.com/your-org/workspace-launcher.git
cd workspace-launcher
dotnet build WorkspaceLauncher.slnx
```

### Run

```bash
dotnet run --project WorkspaceLauncher/WorkspaceLauncher.csproj
```

### Run minimized (tray-only)

```bash
dotnet run --project WorkspaceLauncher/WorkspaceLauncher.csproj -- --minimized
```

---

## Project Structure

```
WorkspaceLauncher.sln
│
├── WorkspaceLauncher/              # WPF application (UI layer)
│   ├── Views/                      # Windows and XAML files
│   ├── ViewModels/                 # MVVM view-models (CommunityToolkit.Mvvm)
│   ├── Converters/                 # IValueConverter / IMultiValueConverter helpers
│   ├── Services/                   # TrayService, StartupService (UI-side)
│   └── Resources/Styles.xaml       # Global dark-theme styles
│
├── WorkspaceLauncher.Core/         # Class library (business logic, no UI dependency)
│   ├── Models/                     # Profile, LaunchItem, AppSettings, UpdateInfo
│   └── Services/                   # ProfileService, LaunchService, UpdateService
│
└── WorkspaceLauncher.Tests/        # xUnit test project
    ├── ModelTests.cs
    ├── ProfileServiceTests.cs
    └── LaunchServiceTests.cs
```

---

## Dependencies

| Package | Purpose |
|---------|---------|
| [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) 8.3 | Source-generated `ObservableObject`, `RelayCommand` |
| [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon) 1.1 | System tray icon |
| [Microsoft.Xaml.Behaviors.Wpf](https://github.com/Microsoft/XamlBehaviorsWpf) 1.1 | XAML interaction triggers |
| xUnit 2.5 + Moq 4.20 | Unit testing |

---

## Data Storage

Profiles and settings are stored in a single JSON file:

```
%AppData%\WorkspaceLauncher\profiles.json
```

The file is written atomically (written to a `.tmp` file then moved) to prevent corruption on
unexpected shutdown.

---

## Running Tests

```bash
dotnet test WorkspaceLauncher.Tests/WorkspaceLauncher.Tests.csproj
```

---

## Release Pipeline

Releases are automated via **GitHub Actions** (`.github/workflows/release.yml`).  
The workflow triggers on any tag that matches `v*.*.*`.

### Steps

| # | Step | Detail |
|---|------|--------|
| 1 | **Checkout** | Full clone of the repository |
| 2 | **Setup .NET 8** | Installs the SDK on the runner |
| 3 | **Extract version** | Strips the `v` prefix from the tag (e.g. `v1.2.0` → `1.2.0`) |
| 4 | **Restore** | `dotnet restore` for all projects |
| 5 | **Build** | Release build with `-p:Version=<version>` injected from the tag |
| 6 | **Test** | Runs the full xUnit suite; workflow fails if any test fails |
| 7 | **Publish** | Self-contained, `win-x64`, folder output to `publish/WorkspaceLauncher/` |
| 8 | **Zip** | Compresses the publish folder to `publish/WorkspaceLauncher.zip` |
| 9 | **Create Release** | Creates a GitHub Release with auto-generated notes and attaches the zip |

### How to publish a new version

1. Update `CurrentVersion` in `WorkspaceLauncher.Core/Services/UpdateService.cs`:
   ```csharp
   public static readonly Version CurrentVersion = new Version(1, 2, 0);
   ```
2. Update `<Version>` in `WorkspaceLauncher/WorkspaceLauncher.csproj`:
   ```xml
   <Version>1.2.0</Version>
   ```
3. Commit and push, then create and push a matching tag:
   ```bash
   git tag v1.2.0
   git push origin v1.2.0
   ```
4. GitHub Actions builds, tests, packages and publishes the release automatically.  
   The attached `WorkspaceLauncher.zip` is what the in-app updater downloads.

### Auto-update flow

```
App starts
    └── CheckForUpdateAsync()  (background, silent)
            └── GET api.github.com/repos/{owner}/{repo}/releases/latest
                    ├── no newer version  →  nothing shown
                    └── newer version found
                            └── UpdateWindow shown
                                    ├── "Remind Me Later"  →  close dialog
                                    └── "Update Now"
                                            ├── download WorkspaceLauncher.zip  →  %TEMP%
                                            ├── extract  →  %TEMP%\WorkspaceLauncher_update\
                                            ├── write %TEMP%\wl_update.ps1
                                            ├── start PowerShell (detached)
                                            └── Application.Shutdown()
                                                    └── PS: wait for PID, copy files, restart .exe
```

> **Note:** The self-update mechanism assumes a **folder publish** (not single-file).  
> `GitHubOwner` and `GitHubRepo` constants in `UpdateService.cs` must match your repository.


---

## Features

- **Profiles** — create any number of named profiles, each with its own accent color and icon.
- **Launch items** — each profile holds an ordered list of items:
  - `Executable` — runs an `.exe` with an optional startup delay.
  - `Folder` — opens a directory in Windows Explorer.
  - `Url` — opens a URL in the default browser.
- **Delay support** — set a per-item delay (ms) so applications start in the right sequence.
- **Session management** — track running processes per profile; end the whole session at once.
  Items marked *Close on session end* are terminated when the session is closed.
- **System tray** — minimize to tray; double-click the icon to restore; launch or quit profiles
  directly from the tray context menu.
- **Auto-launch** — optionally launch a selected profile automatically on application start.
- **Run on startup** — register the app in the Windows registry to start with Windows
  (supports `--minimized` flag so it starts quietly in the tray).
- **Persistent settings** — all profiles and settings are saved to
  `%AppData%\WorkspaceLauncher\profiles.json`.

---

## Requirements

| Component | Version |
|-----------|---------|
| Windows   | 10 / 11 |
| .NET      | 8.0     |

---

## Getting Started

### Build from source

```bash
git clone https://github.com/your-org/workspace-launcher.git
cd workspace-launcher
dotnet build WorkspaceLauncher.slnx
```

### Run

```bash
dotnet run --project WorkspaceLauncher/WorkspaceLauncher.csproj
```

### Run minimized (tray-only)

```bash
dotnet run --project WorkspaceLauncher/WorkspaceLauncher.csproj -- --minimized
```

---

## Project Structure

```
WorkspaceLauncher.sln
│
├── WorkspaceLauncher/              # WPF application (UI layer)
│   ├── Views/                      # Windows and XAML files
│   ├── ViewModels/                 # MVVM view-models (CommunityToolkit.Mvvm)
│   ├── Converters/                 # IValueConverter / IMultiValueConverter helpers
│   ├── Services/                   # TrayService, StartupService (UI-side)
│   └── Resources/Styles.xaml       # Global dark-theme styles
│
├── WorkspaceLauncher.Core/         # Class library (business logic, no UI dependency)
│   ├── Models/                     # Profile, LaunchItem, AppSettings
│   └── Services/                   # ProfileService (JSON persistence), LaunchService
│
└── WorkspaceLauncher.Tests/        # xUnit test project
    ├── ModelTests.cs
    ├── ProfileServiceTests.cs
    └── LaunchServiceTests.cs
```

---

## Dependencies

| Package | Purpose |
|---------|---------|
| [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) 8.3 | Source-generated `ObservableObject`, `RelayCommand` |
| [Hardcodet.NotifyIcon.Wpf](https://github.com/hardcodet/wpf-notifyicon) 1.1 | System tray icon |
| [Microsoft.Xaml.Behaviors.Wpf](https://github.com/Microsoft/XamlBehaviorsWpf) 1.1 | XAML interaction triggers |
| xUnit 2.5 + Moq 4.20 | Unit testing |

---

## Data Storage

Profiles and settings are stored in a single JSON file:

```
%AppData%\WorkspaceLauncher\profiles.json
```

The file is written atomically (written to a `.tmp` file then moved) to prevent corruption on
unexpected shutdown.

---

## Running Tests

```bash
dotnet test WorkspaceLauncher.Tests/WorkspaceLauncher.Tests.csproj
```

