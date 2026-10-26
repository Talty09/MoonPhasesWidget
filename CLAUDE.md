# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

A Windows desktop widget, similar to a Rainmeter skin, that shows the current phase of the moon. It is a standalone C# / WPF app on .NET 8 and does not depend on Rainmeter. The repo will be published to the `Talty09` GitHub account.

> Status: the project is new and no code exists yet. The layout and commands below are the planned design. Update this file as the real structure takes shape.

## Planned layout

- `MoonPhasesWidget.sln`
- `src/MoonPhasesWidget/`: the WPF app (window, rendering, settings, tray icon)
- `tests/MoonPhasesWidget.Tests/`: xUnit tests. This project targets `net8.0-windows` rather than `net8.0`, because it must match the WPF app's framework in order to reference it.

The machine has both the .NET 8 and .NET 10 SDKs. Projects target .NET 8, so pass `-f net8.0` (or `net8.0-windows`) to `dotnet new`; otherwise templates default to .NET 10.

## Commands

```powershell
dotnet build                                   # build the solution
dotnet run --project src/MoonPhasesWidget      # launch the widget
dotnet test                                    # run all tests
dotnet test --filter "FullyQualifiedName~MoonPhaseCalculatorTests"   # one test class
dotnet test --filter "FullyQualifiedName=Namespace.Class.Method"     # one test
dotnet publish src/MoonPhasesWidget -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Architecture intent

- **Keep the astronomy separate from the UI.** Moon phase math (illuminated fraction, phase angle, moon age, phase name, next new and full moon) goes in a pure class with no WPF dependencies. It takes a `DateTime` (UTC), so it can be tested against known dates. Compute everything locally; the core feature should not need network calls.
- **The widget window behaves like a Rainmeter skin:** borderless, transparent (`WindowStyle=None`, `AllowsTransparency=True`), hidden from the taskbar, draggable, and optionally pinned to the desktop or always on top. Window position and user settings persist between runs, for example as JSON under `%APPDATA%\MoonPhasesWidget`.
- **Rendering** draws the moon from the computed illumination and waxing/waning state, rather than switching between a fixed set of images. A timer refreshes the display periodically; the phase changes slowly, so a long interval is fine.
- **Lifecycle:** a tray icon provides exit, settings, and an option to start with Windows.
