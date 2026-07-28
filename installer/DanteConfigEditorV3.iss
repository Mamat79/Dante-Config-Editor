#define MyAppName "Dante Config Editor 2026.1 Beta"
#define MyAppVersion "2026.1.0-beta.1"
#define MyAppPublisher "Mamat"
#define MyAppExeName "DanteConfigEditorV3.exe"
#define MyAppShortcutName "DCE 2026.1 Beta"
#define SourceRoot ".."

[Setup]
AppId={{C893F4F8-5ED3-4C2E-AAD8-024F9DCB4A1D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\Dante Config Editor 2026.1 Beta
DefaultGroupName=Dante Config Editor 2026.1 Beta
DisableProgramGroupPage=no
AllowNoIcons=yes
OutputDir={#SourceRoot}\dist
OutputBaseFilename=DanteConfigEditor2026_1_Beta_Installer
SetupIconFile={#SourceRoot}\DanteEdit.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
; La Beta possède une identité distincte et ne modifie aucune installation V3.6.
UsedUserAreasWarning=no
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion=2026.1.0.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Dante Config Editor 2026.1 Beta installer
VersionInfoProductName={#MyAppName}
SetupLogging=yes
CloseApplications=yes
RestartApplications=no
UsePreviousAppDir=yes
UsePreviousGroup=no

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#SourceRoot}\dist\installer_payload\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceRoot}\DanteEdit.ico"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceRoot}\README.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceRoot}\README_EN.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceRoot}\CHANGELOG.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceRoot}\CHANGELOG_V3.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceRoot}\RELEASE_NOTES.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceRoot}\RELEASE_NOTES_EN.md"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceRoot}\Resources\ChannelLabelTemplates\DMT_LICENSE.txt"; DestDir: "{app}\Licenses"; Flags: ignoreversion
Source: "{#SourceRoot}\docs\QuickStart_DanteConfigEditorV3_FR.pdf"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceRoot}\docs\QuickStart_DanteConfigEditorV3_EN.pdf"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceRoot}\docs\Notice_DanteConfigEditorV3_FR.pdf"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceRoot}\docs\Notice_DanteConfigEditorV3_EN.pdf"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceRoot}\docs\SUPPORT_DCE.md"; DestDir: "{app}\docs"; Flags: ignoreversion
Source: "{#SourceRoot}\Resources\Support\paypal-support-qr.png"; DestDir: "{app}\Resources\Support"; Flags: ignoreversion
Source: "{#SourceRoot}\Resources\MachineBanks\Bundled\DCE Generic Roles 2026.1\*"; DestDir: "{code:GetBundledBankDestination}"; Flags: ignoreversion recursesubdirs createallsubdirs onlyifdoesntexist; Check: ShouldInstallBundledBank
Source: "{#SourceRoot}\Resources\MachineBanks\Bundled\DCE Community Devices 2026.1\*"; DestDir: "{code:GetCommunityBankDestination}"; Flags: ignoreversion recursesubdirs createallsubdirs onlyifdoesntexist; Check: ShouldInstallCommunityBank

[InstallDelete]
Type: files; Name: "{app}\QuickStart_DanteConfigEditorV3.pdf"
Type: files; Name: "{app}\Notice_DanteConfigEditorV3.pdf"
Type: files; Name: "{group}\Quick start PDF.lnk"
Type: files; Name: "{group}\Notice PDF.lnk"

[Icons]
Name: "{group}\{code:GetShortcutAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\DanteEdit.ico"
Name: "{group}\Documentation - Français"; Filename: "{app}\README.md"
Name: "{group}\Documentation - English"; Filename: "{app}\README_EN.md"
Name: "{group}\Démarrage rapide - Français"; Filename: "{app}\QuickStart_DanteConfigEditorV3_FR.pdf"
Name: "{group}\Quick start - English"; Filename: "{app}\QuickStart_DanteConfigEditorV3_EN.pdf"
Name: "{group}\Notice complète - Français"; Filename: "{app}\Notice_DanteConfigEditorV3_FR.pdf"
Name: "{group}\Full user guide - English"; Filename: "{app}\Notice_DanteConfigEditorV3_EN.pdf"
Name: "{group}\Notes de version - Français"; Filename: "{app}\RELEASE_NOTES.md"
Name: "{group}\Release notes - English"; Filename: "{app}\RELEASE_NOTES_EN.md"
Name: "{group}\Désinstaller {code:GetShortcutAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{code:GetShortcutAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\DanteEdit.ico"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,Dante Config Editor 2026.1 Beta}"; Flags: nowait postinstall skipifsilent
Filename: "{app}\RELEASE_NOTES.md"; Description: "Ouvrir les notes de version"; Flags: postinstall shellexec unchecked skipifsilent; Check: IsFrenchLanguage
Filename: "{app}\RELEASE_NOTES_EN.md"; Description: "Open the release notes"; Flags: postinstall shellexec unchecked skipifsilent; Check: IsEnglishLanguage
Filename: "{app}\QuickStart_DanteConfigEditorV3_FR.pdf"; Description: "Ouvrir le démarrage rapide en français"; Flags: postinstall shellexec unchecked skipifsilent; Check: IsFrenchLanguage
Filename: "{app}\Notice_DanteConfigEditorV3_FR.pdf"; Description: "Ouvrir la notice complète en français"; Flags: postinstall shellexec unchecked skipifsilent; Check: IsFrenchLanguage
Filename: "{app}\QuickStart_DanteConfigEditorV3_EN.pdf"; Description: "Open the English quick start"; Flags: postinstall shellexec unchecked skipifsilent; Check: IsEnglishLanguage
Filename: "{app}\Notice_DanteConfigEditorV3_EN.pdf"; Description: "Open the full English user guide"; Flags: postinstall shellexec unchecked skipifsilent; Check: IsEnglishLanguage

[Code]
var
  SignatureLabel: TNewStaticText;
  SignatureAgentsLabel: TNewStaticText;
  GithubLabel: TNewStaticText;
  ExistingInstallDir: String;
  ExistingInstallVersion: String;
  BankDirectoriesPage: TInputDirWizardPage;
  BankOptionsPage: TInputOptionWizardPage;
  BundledBankDestination: String;
  CommunityBankDestination: String;
  InstallBundledBankFiles: Boolean;
  InstallCommunityBankFiles: Boolean;

function GetShortcutAppName(Param: String): String;
begin
  Result := '{#MyAppShortcutName}';
end;

function IsFrenchLanguage(): Boolean;
begin
  Result := ActiveLanguage = 'french';
end;

function IsEnglishLanguage(): Boolean;
begin
  Result := ActiveLanguage = 'english';
end;

function InstallerText(FrenchText: String; EnglishText: String): String;
begin
  if IsEnglishLanguage() then
    Result := EnglishText
  else
    Result := FrenchText;
end;

function MachineBankSettingsPath(): String;
begin
  Result := ExpandConstant(
    '{localappdata}\DanteConfigEditor2026.1\machine-bank-location.txt');
end;

function DefaultMachineBankPath(): String;
begin
  Result := ExpandConstant(
    '{userdocs}\Dante Config Editor\Machine Bank');
end;

function DefaultBundledBanksPath(): String;
begin
  Result := ExpandConstant(
    '{userdocs}\Dante Config Editor\Included Machine Banks');
end;

function ConfiguredMachineBankPath(): String;
var
  RawContent: AnsiString;
  Content: String;
begin
  Result := DefaultMachineBankPath();
  if LoadStringFromFile(MachineBankSettingsPath(), RawContent) then
  begin
    Content := UTF8Decode(RawContent);
    StringChangeEx(Content, #13, '', True);
    StringChangeEx(Content, #10, '', True);
    Content := Trim(Content);
    if Content <> '' then
    begin
      Result := Content;
    end;
  end;
end;

function GetBundledBankDestination(Param: String): String;
begin
  Result := BundledBankDestination;
end;

function ShouldInstallBundledBank(): Boolean;
begin
  Result := InstallBundledBankFiles;
end;

function GetCommunityBankDestination(Param: String): String;
begin
  Result := CommunityBankDestination;
end;

function ShouldInstallCommunityBank(): Boolean;
begin
  Result := InstallCommunityBankFiles;
end;

procedure OpenGithub(Sender: TObject);
var
  ErrorCode: Integer;
begin
  ShellExec('open', 'https://github.com/Mamat79/Dante-Config-Editor', '', '', SW_SHOWNORMAL, ewNoWait, ErrorCode);
end;

function QueryInstallValue(AppId: String; ValueName: String; var Value: String): Boolean;
var
  RegistryKey: String;
begin
  RegistryKey := 'Software\Microsoft\Windows\CurrentVersion\Uninstall\{' + AppId + '}_is1';
  Result := RegQueryStringValue(HKLM, RegistryKey, ValueName, Value);
  if not Result then
  begin
    Result := RegQueryStringValue(HKCU, RegistryKey, ValueName, Value);
  end;
end;

function DetectExistingInstall(): Boolean;
begin
  ExistingInstallDir := '';
  ExistingInstallVersion := '';
  Result := QueryInstallValue('C893F4F8-5ED3-4C2E-AAD8-024F9DCB4A1D', 'InstallLocation', ExistingInstallDir);

  if Result then
  begin
    QueryInstallValue('C893F4F8-5ED3-4C2E-AAD8-024F9DCB4A1D', 'DisplayVersion', ExistingInstallVersion);

    if ExistingInstallVersion = '' then
    begin
      ExistingInstallVersion := 'version inconnue';
    end;
  end;
end;

function ExistingInstallPromptText(): String;
begin
  if ActiveLanguage = 'english' then
  begin
    Result :=
      'An existing Dante Config Editor installation was found.' + #13#10#13#10 +
      'Detected version: ' + ExistingInstallVersion + #13#10 +
      'Folder: ' + ExistingInstallDir + #13#10#13#10 +
      'Yes = replace/update this installation.' + #13#10 +
      'No = close the installer without changing the installed version.';
  end
  else
  begin
    Result :=
      'Une installation de Dante Config Editor est déjà présente.' + #13#10#13#10 +
      'Version détectée : ' + ExistingInstallVersion + #13#10 +
      'Dossier : ' + ExistingInstallDir + #13#10#13#10 +
      'Oui = remplacer / mettre à jour cette installation.' + #13#10 +
      'Non = quitter sans modifier la version installée.';
  end;
end;

procedure InitializeWizard();
begin
  if ActiveLanguage = 'english' then
  begin
    BankDirectoriesPage := CreateInputDirPage(
      wpSelectDir,
      'Device banks',
      'Choose where DCE uses and installs device banks.',
      'The active bank may already exist. Each included bank is installed in a separate folder and never overwrites an existing bank.',
      False,
      '');
    BankDirectoriesPage.Add('Active device-bank folder:');
    BankDirectoriesPage.Add('Folder for included banks:');
    BankOptionsPage := CreateInputOptionPage(
      BankDirectoriesPage.ID,
      'Device-bank options',
      'Choose the settings to apply.',
      'These choices can later be changed from the Device bank window.',
      False,
      False);
    BankOptionsPage.Add('Use the selected active-bank folder in DCE');
    BankOptionsPage.Add('Install DCE Generic Roles 2026.1');
    BankOptionsPage.Add('Install DCE Community Devices 2026.1');
  end
  else
  begin
    BankDirectoriesPage := CreateInputDirPage(
      wpSelectDir,
      'Banques de machines',
      'Choisissez où DCE utilise et installe les banques de machines.',
      'La banque active peut déjà exister. Chaque banque fournie est installée dans un dossier séparé et ne remplace jamais une banque existante.',
      False,
      '');
    BankDirectoriesPage.Add('Dossier de la banque active :');
    BankDirectoriesPage.Add('Dossier des banques fournies :');
    BankOptionsPage := CreateInputOptionPage(
      BankDirectoriesPage.ID,
      'Options des banques de machines',
      'Choisissez les réglages à appliquer.',
      'Ces choix restent modifiables depuis la fenêtre Banque de machines.',
      False,
      False);
    BankOptionsPage.Add('Utiliser le dossier de banque active choisi dans DCE');
    BankOptionsPage.Add('Installer DCE Generic Roles 2026.1');
    BankOptionsPage.Add('Installer DCE Community Devices 2026.1');
  end;

  BankDirectoriesPage.Values[0] := ConfiguredMachineBankPath();
  BankDirectoriesPage.Values[1] := DefaultBundledBanksPath();
  BankOptionsPage.Values[0] := True;
  BankOptionsPage.Values[1] := not DirExists(
    AddBackslash(BankDirectoriesPage.Values[1]) + 'DCE Generic Roles 2026.1');
  BankOptionsPage.Values[2] := not DirExists(
    AddBackslash(BankDirectoriesPage.Values[1]) + 'DCE Community Devices 2026.1');

  GithubLabel := TNewStaticText.Create(WizardForm);
  GithubLabel.Parent := WizardForm;
  GithubLabel.Caption := 'GitHub public';
  GithubLabel.Left := ScaleX(12);
  GithubLabel.Top := WizardForm.ClientHeight - ScaleY(28);
  GithubLabel.Font.Color := clBlue;
  GithubLabel.Font.Style := [fsUnderline];
  GithubLabel.Cursor := crHand;
  GithubLabel.OnClick := @OpenGithub;

  SignatureLabel := TNewStaticText.Create(WizardForm);
  SignatureLabel.Parent := WizardForm;
  SignatureLabel.Caption := 'By Mamat';
  SignatureLabel.Left := WizardForm.ClientWidth - ScaleX(82);
  SignatureLabel.Top := WizardForm.ClientHeight - ScaleY(36);
  SignatureLabel.Font.Color := clGray;

  SignatureAgentsLabel := TNewStaticText.Create(WizardForm);
  SignatureAgentsLabel.Parent := WizardForm;
  SignatureAgentsLabel.Caption := 'et ses agents';
  SignatureAgentsLabel.Left := WizardForm.ClientWidth - ScaleX(82);
  SignatureAgentsLabel.Top := WizardForm.ClientHeight - ScaleY(22);
  SignatureAgentsLabel.Font.Color := clGray;
  SignatureAgentsLabel.Font.Size := 7;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  MessageText: String;
begin
  Result := True;
  if CurPageID <> BankOptionsPage.ID then
  begin
    Exit;
  end;

  if BankOptionsPage.Values[0]
    and (Trim(BankDirectoriesPage.Values[0]) = '') then
  begin
    if ActiveLanguage = 'english' then
      MessageText := 'Choose an active device-bank folder.'
    else
      MessageText := 'Choisissez un dossier de banque active.';
    MsgBox(MessageText, mbError, MB_OK);
    Result := False;
    Exit;
  end;

  if (BankOptionsPage.Values[1] or BankOptionsPage.Values[2])
    and (Trim(BankDirectoriesPage.Values[1]) = '') then
  begin
    if ActiveLanguage = 'english' then
      MessageText := 'Choose a folder for the included banks.'
    else
      MessageText := 'Choisissez un dossier pour les banques fournies.';
    MsgBox(MessageText, mbError, MB_OK);
    Result := False;
  end;
end;

function FindAvailableBankDestination(
  BanksRoot: String;
  BankFolderName: String): String;
var
  BaseDestination: String;
  Candidate: String;
  Suffix: Integer;
begin
  BaseDestination := AddBackslash(BanksRoot) + BankFolderName;
  Candidate := BaseDestination;
  Suffix := 2;
  while DirExists(Candidate) or FileExists(Candidate) do
  begin
    Candidate := BaseDestination + ' (' + IntToStr(Suffix) + ')';
    Suffix := Suffix + 1;
  end;
  Result := Candidate;
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ActiveBankPath: String;
  BundledBanksPath: String;
begin
  Result := '';
  ActiveBankPath := BankDirectoriesPage.Values[0];
  BundledBanksPath := BankDirectoriesPage.Values[1];
  if BankOptionsPage.Values[0] then
  begin
    if FileExists(ActiveBankPath) then
    begin
      Result := InstallerText(
        'Le chemin de banque active désigne un fichier : ',
        'The active-bank path points to a file: ') + ActiveBankPath;
      Exit;
    end;
    if not DirExists(ActiveBankPath)
      and not ForceDirectories(ActiveBankPath) then
    begin
      Result := InstallerText(
        'Impossible de créer le dossier de banque active : ',
        'Unable to create the active-bank folder: ') + ActiveBankPath;
      Exit;
    end;
  end;

  InstallBundledBankFiles := BankOptionsPage.Values[1];
  InstallCommunityBankFiles := BankOptionsPage.Values[2];
  BundledBankDestination := '';
  CommunityBankDestination := '';
  if not InstallBundledBankFiles
    and not InstallCommunityBankFiles then
  begin
    Exit;
  end;

  if FileExists(BundledBanksPath) then
  begin
    Result := InstallerText(
      'Le chemin des banques fournies désigne un fichier : ',
      'The included-banks path points to a file: ') + BundledBanksPath;
    Exit;
  end;
  if not DirExists(BundledBanksPath)
    and not ForceDirectories(BundledBanksPath) then
  begin
    Result := InstallerText(
      'Impossible de créer le dossier des banques fournies : ',
      'Unable to create the included-banks folder: ') + BundledBanksPath;
    Exit;
  end;

  if InstallBundledBankFiles then
  begin
    BundledBankDestination := FindAvailableBankDestination(
      BundledBanksPath,
      'DCE Generic Roles 2026.1');
  end;
  if InstallCommunityBankFiles then
  begin
    CommunityBankDestination := FindAvailableBankDestination(
      BundledBanksPath,
      'DCE Community Devices 2026.1');
  end;
end;

procedure SaveMachineBankLocation();
var
  BankPath: String;
  EncodedBankPath: AnsiString;
  SettingsPath: String;
  SettingsDirectory: String;
begin
  if not BankOptionsPage.Values[0] then
  begin
    Exit;
  end;

  BankPath := BankDirectoriesPage.Values[0];
  SettingsPath := MachineBankSettingsPath();
  SettingsDirectory := ExtractFileDir(SettingsPath);
  ForceDirectories(BankPath);
  ForceDirectories(SettingsDirectory);
  if FileExists(SettingsPath) then
  begin
    CopyFile(SettingsPath, SettingsPath + '.bak', False);
  end;
  EncodedBankPath := UTF8Encode(BankPath + #13#10);
  SaveStringToFile(SettingsPath, EncodedBankPath, False);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    SaveMachineBankLocation();
  end;
end;

function InitializeSetup(): Boolean;
var
  Choice: Integer;
begin
  Result := True;

  DetectExistingInstall();

  if WizardSilent then
  begin
    Exit;
  end;

  if ExistingInstallDir <> '' then
  begin
    Choice := MsgBox(ExistingInstallPromptText(), mbConfirmation, MB_YESNO);
    if Choice <> IDYES then
    begin
      Result := False;
      Exit;
    end;
  end;
end;
