; 脚本由 Inno Setup 脚本向导生成。
; 有关创建 Inno Setup 脚本文件的详细信息，请参阅帮助文档！
; 仅供非商业使用

#define MyAppName "自动音乐播放器"
#define MyAppVersion "2.7.2"
#define MyAppPublisher "丐帮集团第一院·物理版象棋开发与研究院™"
#define MyAppURL "https://bggp.dpdns.org/1/map/"
#define MyAppExeName "MusicAutoPlayer.exe"

[Setup]
; 注意：AppId 的值唯一标识此应用程序。不要在其他应用程序的安装程序中使用相同的 AppId 值。
; (若要生成新的 GUID，请在 IDE 中单击 "工具|生成 GUID"。)
AppId={{13117BA8-BFFE-414C-AF05-C85655CE43F5}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} 
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName=D:\Program Files\bggp\1\map
UninstallDisplayIcon={app}\{#MyAppExeName}
; "ArchitecturesAllowed=x64compatible" 指定
; 安装程序只能在 x64 和 Windows 11 on Arm 上运行。
ArchitecturesAllowed=x64compatible
; "ArchitecturesInstallIn64BitMode=x64compatible" 要求
; 在 X64 或 Windows 11 on Arm 上以 "64-位模式" 进行安装，
; 这意味着它应该使用本地 64 位 Program Files 目录
; 和注册表的 64 位视图。
ArchitecturesInstallIn64BitMode=x64compatible
DefaultGroupName=丐帮软件
AllowNoIcons=yes

; 强制要求管理员权限（安装字体必需）
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

OutputDir=C:\Users\鸿合HiteVision\OneDrive\桌面
OutputBaseFilename=MusicAutoPlayer_v272_Setup
SetupIconFile=C:\Users\鸿合HiteVision\OneDrive\文档\dec.ico
SolidCompression=yes
WizardStyle=modern dynamic windows11

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Default.isl"
Name: "chinesetraditional"; MessagesFile: "compiler:Languages\ChineseTraditional.isl"
Name: "english"; MessagesFile: "compiler:Languages\English.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startup"; Description: "开机时启动（静默）"; GroupDescription: "其他选项:"; Flags: unchecked

[Files]
; 主程序文件
Source: "C:\Users\鸿合HiteVision\OneDrive\桌面\a\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "C:\Users\鸿合HiteVision\OneDrive\桌面\a\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; 安装字体到系统（源路径已更新）
Source: "C:\Users\鸿合HiteVision\OneDrive\桌面\方正像素12.ttf"; DestDir: "{commonfonts}"; FontInstall: "方正像素12"; Flags: onlyifdoesntexist uninsneveruninstall
; 注意：FontInstall 后的字体名称必须与双击字体文件预览时顶部显示的“字体名称”完全一致，此处为示例名称。

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:ProgramOnTheWeb,{#MyAppName}}"; Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
; 如果勾选了开机自启动，则在 HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Run 下添加键值
; 因为要求管理员权限安装，写入 HKLM 是安全的，且对所有用户生效。
; 注意：ArchitecturesInstallIn64BitMode=x64compatible 确保使用 64 位注册表视图。
Root: HKLM; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "{#MyAppName}"; ValueData: "{app}\{#MyAppExeName}"; Tasks: startup; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent