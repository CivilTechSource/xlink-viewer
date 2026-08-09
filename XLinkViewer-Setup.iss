[Setup]
; Basic application information
AppName=XLinkViewer
AppVersion=1.0.0
AppVerName=XLinkViewer 1.0.0
AppPublisher=CTS Software
AppPublisherURL=
AppSupportURL=
AppUpdatesURL=
DefaultDirName={autopf}\XLinkViewer
DefaultGroupName=XLinkViewer
AllowNoIcons=yes
LicenseFile=
InfoBeforeFile=
InfoAfterFile=
OutputDir=installer
OutputBaseFilename=XLinkViewer-Setup
SetupIconFile=
Compression=lzma
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin

; Minimum Windows version
MinVersion=10.0.17763

; Architecture
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Include self-contained published files
Source: "bin\Release\net10.0-windows\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
; Include the icon file
Source: "Icon.ico"; DestDir: "{app}"; Flags: ignoreversion
; Include the launcher files
Source: "XLinkViewer-Launcher.vbs"; DestDir: "{app}"; Flags: ignoreversion
Source: "XLinkViewer-Launcher.bat"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Registry]
; Context menu for .dwg files (with multi-select support)
Root: HKCR; Subkey: "SystemFileAssociations\.dwg\shell\XLinkViewer"; Flags: uninsdeletekey
Root: HKCR; Subkey: "SystemFileAssociations\.dwg\shell\XLinkViewer"; ValueType: string; ValueName: ""; ValueData: "Scan Xrefs"
Root: HKCR; Subkey: "SystemFileAssociations\.dwg\shell\XLinkViewer"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\Icon.ico"""
Root: HKCR; Subkey: "SystemFileAssociations\.dwg\shell\XLinkViewer"; ValueType: string; ValueName: "MultiSelectModel"; ValueData: "Player"
Root: HKCR; Subkey: "SystemFileAssociations\.dwg\shell\XLinkViewer\command"; ValueType: string; ValueName: ""; ValueData: "wscript.exe ""{app}\XLinkViewer-Launcher.vbs"" ""%1"""

; Context menu for folders
Root: HKCR; Subkey: "Directory\shell\XLinkViewer"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Directory\shell\XLinkViewer"; ValueType: string; ValueName: ""; ValueData: "Scan Folder Xrefs"
Root: HKCR; Subkey: "Directory\shell\XLinkViewer"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\Icon.ico"""
Root: HKCR; Subkey: "Directory\shell\XLinkViewer\command"; ValueType: string; ValueName: ""; ValueData: """{app}\XLinkViewer.exe"" ""%1"""

; Context menu for directory background (right-click in empty folder area)
Root: HKCR; Subkey: "Directory\Background\shell\XLinkViewer"; Flags: uninsdeletekey
Root: HKCR; Subkey: "Directory\Background\shell\XLinkViewer"; ValueType: string; ValueName: ""; ValueData: "Scan Folder Xrefs"
Root: HKCR; Subkey: "Directory\Background\shell\XLinkViewer"; ValueType: string; ValueName: "Icon"; ValueData: """{app}\Icon.ico"""
Root: HKCR; Subkey: "Directory\Background\shell\XLinkViewer\command"; ValueType: string; ValueName: ""; ValueData: """{app}\XLinkViewer.exe"" ""%V"""

[Icons]
Name: "{group}\XLinkViewer"; Filename: "{app}\XLinkViewer.exe"; IconFilename: "{app}\Icon.ico"
Name: "{group}\{cm:UninstallProgram,XLinkViewer}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\XLinkViewer"; Filename: "{app}\XLinkViewer.exe"; IconFilename: "{app}\Icon.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\XLinkViewer.exe"; Description: "{cm:LaunchProgram,XLinkViewer}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;

function IsUpgrade(): Boolean;
begin
  Result := (GetUninstallString() <> '');
end;

function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
  // Return Values:
  // 1 - uninstall string is empty
  // 2 - error executing the UnInstallString
  // 3 - successfully executed the UnInstallString

  // default return value
  Result := 0;

  // get the uninstall string of the old app
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep=ssInstall) then
  begin
    if (IsUpgrade()) then
    begin
      UnInstallOldVersion();
    end;
  end;
end;