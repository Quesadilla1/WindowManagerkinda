# DeskBulldozer

DeskBulldozer is a Windows desktop utility for keeping app windows organized across virtual desktops. It combines manual window controls with rules and hotkeys so frequently used apps open where you expect them.
![Dark mode screenshot](docs/screenshot-dark.png)
## What it does

- Move windows between virtual desktops
- Snap windows into common layouts on the selected monitor
- Create rules that auto-place matching windows
- Register global hotkeys for desktop and window actions
- Pin windows across all desktops
- Run quietly from the system tray

## At a glance

This project is mainly a WinForms app in `VDManager/` targeting .NET 8 on Windows.

- **Main app:** `VDManager/`
- **Tests:** `VDManager.Tests/`
- **Optional packaging project:** `DeskBulldozer.Package/`

The packaging project is only for MSIX / Store-style packaging. The core app can be built and worked on without it.

## Requirements

- Windows 11 recommended
- .NET 8 SDK/runtime
- `VirtualDesktopAccessor.dll` in `VDManager/Native/`

Download the native DLL from:
https://github.com/Ciantic/VirtualDesktopAccessor/releases

## Quick start

```bash
dotnet build VDManager/VDManager.sln
```

Then run the built app from the `VDManager` output folder, or open the solution in Visual Studio.

## Notes

- Generated publish/package output should not be committed
- The repo includes ignore rules for build artifacts and packaging output
- If you do not need MSIX packaging, you can ignore `DeskBulldozer.Package/` for day-to-day development

## License

MIT
