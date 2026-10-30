; Inno Setup script for the Moon Phase widget.
; Build with installer\build.ps1, which publishes the app and passes AppVersion and PublishDir.

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif
#ifndef PublishDir
  #define PublishDir "..\artifacts\publish"
#endif

#define AppName "Moon Phase Widget"
#define AppExeName "MoonPhasesWidget.exe"
; Shared with the app's start-with-Windows setting, so both read and write the same Run value.
#define RunValueName "MoonPhasesWidget"

[Setup]
; Never change AppId: upgrades and uninstall find the existing install by it.
AppId={{A6206E00-E7BE-4A2A-8633-281BB5FADEF7}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Talty09
AppPublisherURL=https://github.com/Talty09/MoonPhasesWidget
VersionInfoVersion={#AppVersion}
; Per-user install into %LOCALAPPDATA%\Programs, with no admin prompt.
PrivilegesRequired=lowest
DefaultDirName={autopf}\MoonPhasesWidget
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.14393
; Matches App.SingleInstanceMutexName: setup asks the user to close a running widget first.
AppMutex=MoonPhasesWidget.SingleInstance
CloseApplications=yes
SetupIconFile=..\src\MoonPhasesWidget\Assets\moon.ico
UninstallDisplayIcon={app}\{#AppExeName}
UninstallDisplayName={#AppName}
WizardStyle=modern
OutputDir=..\artifacts
OutputBaseFilename=MoonPhasesWidget-Setup-{#AppVersion}
Compression=lzma2/max
SolidCompression=yes

[Tasks]
Name: "startup"; Description: "Start the widget when I sign in to Windows"
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#RunValueName}"; ValueData: """{app}\{#AppExeName}"""; Flags: uninsdeletevalue; Tasks: startup
; Unchecking the task on an upgrade turns autostart off again.
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "{#RunValueName}"; Flags: deletevalue uninsdeletevalue; Tasks: not startup

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent
