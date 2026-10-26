# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

A Windows desktop widget, similar to a Rainmeter skin, that shows the current phase of the moon. It is a standalone C# / WPF app on .NET 8 and does not depend on Rainmeter. The repo will be published to the `Talty09` GitHub account.

> Status: the moon phase math, the widget window, moon rendering and saved settings are done. The tray icon and start-with-Windows are still to be built.

## Planned layout

- `MoonPhasesWidget.sln`
- `src/MoonPhasesWidget/`: the WPF app (window, rendering, settings, tray icon)
- `tests/MoonPhasesWidget.Tests/`: xUnit tests. This project targets `net8.0-windows` rather than `net8.0`, because it must match the WPF app's framework in order to reference it.

The machine has both the .NET 8 and .NET 10 SDKs. Projects target .NET 8, so pass `-f net8.0` (or `net8.0-windows`) to `dotnet new`; otherwise templates default to .NET 10.

## Commands

```powershell
dotnet build                                   # build the solution
dotnet run --project src/MoonPhasesWidget      # launch the widget
dotnet run --project src/MoonPhasesWidget -- --date 2026-11-12   # preview the moon at a given local date/time
dotnet test                                    # run all tests
dotnet test --filter "FullyQualifiedName~MoonPhaseCalculatorTests"   # one test class
dotnet test --filter "FullyQualifiedName=Namespace.Class.Method"     # one test
dotnet publish src/MoonPhasesWidget -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Architecture intent

- **Keep the astronomy separate from the UI.** Moon phase math (illuminated fraction, phase angle, moon age, phase name, next new and full moon) goes in a pure class with no WPF dependencies. It takes a `DateTime` (UTC), so it can be tested against known dates. Compute everything locally; the core feature should not need network calls.
  This lives in `src/MoonPhasesWidget/Astronomy/` and follows Meeus, *Astronomical Algorithms* (2nd ed.), ch. 47–49. When checking it against references, remember that Meeus's results are in Terrestrial Time; `AstroTime` converts to and from UTC using ΔT. Tests can reach `internal` members through `InternalsVisibleTo`.
- **The widget window behaves like a Rainmeter skin:** borderless, transparent (`WindowStyle=None`, `AllowsTransparency=True`), hidden from the taskbar, draggable, and optionally always on top. `Settings/WidgetSettings` saves the window position and on-top setting to `%APPDATA%\MoonPhasesWidget\settings.json`. If the saved position is no longer on any screen, the widget falls back to the top-right corner of the work area.
- **Rendering:** `Controls/MoonDisc` draws `Assets/moon.jpg`, a NASA SVS full-moon frame cropped exactly to the disk. The image must stay cropped edge to edge, because the control maps it straight onto its circle. A darkened copy of the photo forms the night side. A per-pixel lighting mask, built from the illuminated fraction and waxing state, reveals the full-brightness photo on the day side. The mask treats the disk as a sphere lit from phase angle θ, with a soft terminator measured in screen space. It shows the northern-hemisphere view. Keep the credit in `Assets/CREDITS.md` if the image changes. `MainWindow` refreshes every minute. Because the widget has no taskbar entry, the right-click menu (always on top, exit) is currently the only way to close it.
- **Visual checks:** the widget isn't in the taskbar, so `Process.MainWindowHandle` is 0. Find its window by the title "Moon Phase" instead, for example with `FindWindow`. From PowerShell, pass `[NullString]::Value` as the class name, because `$null` becomes an empty string.
- **Lifecycle:** a tray icon provides exit, settings, and an option to start with Windows.
