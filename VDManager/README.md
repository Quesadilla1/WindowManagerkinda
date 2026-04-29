# 🪟 DeskBulldozer

**DeskBulldozer** is a Windows 11 utility for managing windows across virtual desktops — move, snap, and organize windows with rules, hotkeys, and automatic layout enforcement.

![Dark mode screenshot](docs/screenshot-dark.png)

---

## ✨ Features

### 🖥️ Window Browser
- Lists all visible, manageable windows currently open on the system
- Displays each window as `ProcessName – Title` for easy identification
- One-click **Refresh** to update the list at any time

### 📐 Position & Layout
Move any selected window to a target virtual desktop and snap it to one of **21 layout presets**:

| Group | Layouts |
|-------|---------|
| Quadrants | Top Left, Top Right, Bottom Left, Bottom Right |
| Halves | Left Half, Right Half, Top Half, Bottom Half |
| Thirds | Left Third, Center Third, Right Third |
| Two-Thirds | Left 2/3, Right 2/3 |
| Center | Center Half, Center Third |
| Quarters (25%) | Left Quarter, Center-Left, Center-Right, Right Quarter |
| Three-Quarters (75%) | Left 3/4, Right 3/4 |
| Full | Maximized |

The visual layout picker shows a miniature monitor thumbnail for every preset so you can see exactly where the window will land before you click.

Multi-monitor support — choose which physical monitor to snap to from a dropdown.

---

### 📋 Window Rules
Define rules that automatically move and position windows when they open. Rules are matched against:

| Field | Description |
|-------|-------------|
| **Process Name** | Executable name (e.g. `notepad`, `chrome`) |
| **Window Title Filter** | Exact text or regex pattern against the window title |
| **Instance Number** | Target the 1st, 2nd, 3rd… instance of a process |
| **Priority** | Higher-priority rules win when multiple rules match |
| **Desktop** | Which virtual desktop (1–N) to send the window to |
| **Layout** | Which of the 21 position presets to snap to |
| **Monitor** | Which physical monitor |
| **Enforce Position** | Snap the window back if it is moved away after being placed (see [Position Enforcement](#-position-enforcement-snap-back)) |

**Rule matching is two-phase:**
1. Title-filter rules are evaluated first (exclusive claim — a title rule can only match one window at a time)
2. Instance/catch-all rules run next on unclaimed windows, ordered by arrival-order instance number

**Specificity scoring** ensures that more-specific rules (title + instance + regex) always beat general catch-all rules of the same priority — no more unexpected rule order surprises.

Rules are managed from the **Rules tab**:
- Add / Edit / Delete rules
- Enable or disable individual rules without deleting them
- **Apply All Rules** button — immediately apply all rules to every open window
- **+ Add as Rule** shortcut — select a window in the list, choose a desktop and layout, and instantly create a pre-filled rule for it

---

### ⚡ Auto-Apply (Window Monitor)
Enable **Auto-apply rules** to have DeskBulldozer watch for newly opened windows and automatically apply matching rules as soon as they appear — no manual intervention needed.

The monitor runs in the background and fires whenever a new window handle is detected. It seeds instance numbers from arrival order, so the 1st Notepad always gets instance 1, even after a restart.

---

### 🔒 Position Enforcement (Snap-Back)

Keep rules enforced even after a window has been placed. When **Enforce position** is enabled on a rule, DeskBulldozer continuously monitors that window and snaps it back to its rule-defined desktop and layout if it is ever moved or dragged away.

#### How it works

| Concept | Detail |
|---------|--------|
| **Per-rule opt-in** | Enable the "Enforce position (snap back if moved)" checkbox on any individual rule |
| **Global master switch** | Settings tab → *Position Enforcement* group — must be ON for any enforcement to occur |
| **Grace period** | How long (ms) to wait after initial placement before monitoring begins (default 3000 ms). Lets apps that reposition themselves during startup settle first |
| **Cooldown** | Minimum gap (ms) between consecutive snap-backs for the same window (default 1000 ms). Prevents thrashing while a drag is in progress |
| **20 px tolerance** | Minor subpixel positioning differences are ignored; only genuine moves trigger a snap |

#### Enabling enforcement

1. Go to **Settings → Position Enforcement (Snap-Back)**
2. Check **Enable global position enforcement**
3. Optionally adjust *Grace period* and *Cooldown* to your preference
4. Open the **Rules tab** and edit a rule — enable **"Enforce position (snap back if moved)"**
5. Make sure **Auto-apply rules** is also enabled (the monitor must be running for enforcement to check positions)

#### Performance

Enforcement is designed to be lightweight:
- When the global switch is **off** (the default), a single boolean check short-circuits the entire path — **zero overhead**
- When active, the check adds only 2–3 lightweight Win32 P/Invoke calls per tracked window per monitor tick (every 2 s by default)
- The expensive `EnumWindows` window scan only runs when a window is actually out of position and needs snapping back

---

### ⌨️ Hotkeys
Configure global keyboard shortcuts for common actions. Available hotkey actions:

| Action | Description |
|--------|-------------|
| Switch to Desktop 1–9 | Instantly jump to a specific virtual desktop |
| Move Window to Desktop 1–9 | Send the currently focused window to a desktop |
| Previous / Next Desktop | Cycle through desktops |
| Apply All Rules | Trigger rule enforcement for all open windows |
| Apply Rule to Active Window | Apply a matching rule to the focused window only |
| Show/Focus DeskBulldozer | Bring the app window to the front from anywhere |
| Pin Active Window | Pin the focused window to appear on all desktops |
| Unpin Active Window | Remove the pin from the focused window |
| Create New Desktop | Programmatically add a new virtual desktop |
| Remove Current Desktop | Remove the current virtual desktop |
| Launch Profile | Trigger a named App Launcher profile |

Double-click any hotkey row to assign a key combination. Checkbox toggles enable/disable individual hotkeys without losing their assignment. Conflicts (key already taken by another app) are reported per-hotkey with a **Conflict** status badge.

---

### 📌 Pin Windows to All Desktops
Pin a window so it appears on every virtual desktop simultaneously — useful for reference documents, chat apps, or music players. Unpin at any time.

---

### 🖥️ Virtual Desktop Management
Create and remove virtual desktops directly from DeskBulldozer via hotkeys or the UI, without having to open Task View.

---

### 🚀 App Launcher

The **Launcher tab** lets you define named **launch profiles** — each profile contains an ordered list of apps to open together, with optional startup delays and desktop-switching per entry.

#### Launch Profiles

| Field | Description |
|-------|-------------|
| **Name** | A friendly label for the profile (e.g. "Work Setup", "Gaming") |
| **Description** | Optional notes |
| **Auto-launch on startup** | Automatically run this profile when DeskBulldozer starts |
| **Hotkey** | A global keyboard shortcut to trigger the profile from anywhere |

#### App Entries (per profile)

| Field | Description |
|-------|-------------|
| **Name** | Display name for this entry |
| **Executable** | Path or command to launch (`chrome.exe`, `code`, full path, etc.) |
| **Arguments** | Command-line arguments passed to the process |
| **Working Directory** | Starting directory for the process (optional) |
| **Startup Delay** | Wait N seconds before launching this entry (0 = immediate) |
| **After launch, switch to** | Optionally switch the virtual desktop focus after this app opens |

Entries within a profile are launched in order. You can reorder them with **Move Up / Move Down** in the profile editor.

#### Quick Presets

When adding an entry, choose a **Quick Preset** to auto-fill the executable and default arguments:

| Category | Presets |
|----------|---------|
| Browsers | Edge, Chrome, Firefox |
| Dev Tools | VS Code, Visual Studio 2022, Cursor |
| Terminals | Windows Terminal, PowerShell |
| Editors | Notepad++ |
| System | File Explorer |
| Communication | Slack, Discord, Microsoft Teams |
| Media / Notes | Spotify, Obsidian |

Presets for user-installed apps (Slack, Discord, Spotify, Obsidian, Cursor) expand `%LOCALAPPDATA%` / `%APPDATA%` automatically. All fields remain fully editable after applying a preset.

#### Tray Integration

When at least one profile exists, a **Launch Profile** submenu appears in the system tray right-click menu, listing every profile by name (with its hotkey shortcut shown inline if assigned).

---

### 🔔 System Tray
DeskBulldozer lives in the system tray when not in use:
- **Minimize to Tray** — minimizing the window sends it to the tray instead of the taskbar
- **Close to Tray** — closing the window keeps the app running in the background
- **Balloon tip notifications** for tray actions (optional, can be disabled)
- Right-click tray menu: Show, Apply All Rules, Exit
- Double-click tray icon to restore

---

### 🎨 Themes
Three theme modes:
- **Light** — clean light grey / white palette
- **Dark** — full dark mode with dark gray backgrounds, light text, and dark-aware layout button thumbnails
- **System** — automatically follows the Windows 11 light/dark app preference and updates live when you change it in Windows Settings

Font preference is also configurable — pick any installed system font and it applies globally throughout the UI.

---

### 🚀 Start with Windows
Register DeskBulldozer to launch automatically at login via the Windows registry (HKCU Run key). Toggle on/off from the Settings tab.

---

## 🛠️ Requirements

- **Windows 11** (Windows 10 may work with limited virtual desktop support)
- **.NET 8.0** runtime
- **VirtualDesktopAccessor.dll** — place in the `Native/` folder

  Download from: [https://github.com/Ciantic/VirtualDesktopAccessor/releases](https://github.com/Ciantic/VirtualDesktopAccessor/releases)

---

## 🚀 Getting Started

1. Clone or download the repository
2. Download `VirtualDesktopAccessor.dll` and place it in `VDManager/Native/`
3. Build with Visual Studio or:
   ```
   dotnet build VDManager/VDManager.sln
   ```
4. Run `VDManager.exe`

---

## 📁 Project Structure

```
VDManager/
├── Controls/
│   ├── VisualQuadrantPanel.cs      — grid of clickable layout preset buttons
│   └── QuadrantSelector.cs         — compact inline layout picker
├── Models/
│   ├── AppSettings.cs              — persisted user settings (includes LaunchProfiles)
│   ├── AppLaunchEntry.cs           — single app entry in a launch profile
│   ├── HotkeyConfig.cs             — hotkey definition model
│   ├── LaunchProfile.cs            — named launch profile (list of AppLaunchEntry)
│   ├── WindowRule.cs               — rule definition + Matches() logic
│   └── WorkspaceProfile.cs         — workspace profile model
├── Services/
│   ├── HotkeyManager.cs            — global hotkey registration (Win32)
│   ├── LauncherService.cs          — launches app profiles with delays + hotkey wiring
│   ├── PersistenceService.cs       — JSON save/load for rules, hotkeys, settings
│   ├── RulesManager.cs             — two-phase rule matching + specificity scoring
│   ├── StartupManager.cs           — Windows startup registry management
│   ├── ThemeManager.cs             — light/dark theme + system theme detection
│   ├── WindowInstanceTracker.cs    — arrival-order instance numbering + rule claims
│   └── WindowMonitor.cs            — background watcher for new windows
├── Native/
│   └── VirtualDesktopAccessor.dll  ← place here
├── Form1.cs                        — main application window
├── LaunchEntryEditorDialog.cs      — dialog for editing a single app launch entry
├── LaunchProfileEditorDialog.cs    — dialog for editing a launch profile + its entries
├── WindowManager.cs                — move/position windows via Win32 + VDA API
├── QuadrantLayout.cs               — layout enum + GetQuadrantBounds() math
└── VirtualDesktopAPI.cs            — P/Invoke wrapper for VirtualDesktopAccessor
```

---

## 📄 License

MIT
