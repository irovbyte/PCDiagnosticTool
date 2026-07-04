[Setup]
AppName=PCDiagnosticTool
AppVersion=1.0.0
DefaultDirName={autopf}\PCDiagnosticTool
DefaultGroupName=PCDiagnosticTool
OutputBaseFilename=PCDiagnosticTool_Setup
SetupIconFile=Assets\AppIcon.ico
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
PrivilegesRequired=admin
UninstallDisplayIcon={app}\PCDiagnosticTool.exe

[Files]
Source: "publish_output\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\PCDiagnosticTool"; Filename: "{app}\PCDiagnosticTool.exe"
Name: "{autodesktop}\PCDiagnosticTool"; Filename: "{app}\PCDiagnosticTool.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Run]
Filename: "{app}\PCDiagnosticTool.exe"; Description: "{cm:LaunchProgram,PCDiagnosticTool}"; Flags: nowait postinstall skipifsilent
