# AGENTS.md

## 账号与远端

- GitHub 用户名：**bgsgp**
- 仓库：`bgsgp/MusicAutoPlayer`
- 远端必须使用 SSH：`git@github.com:bgsgp/MusicAutoPlayer.git`。若 `git remote -v` 出现 `https://...`，先改回 SSH 再推送。

## 项目结构

- .NET 10 WinForms 单窗口程序（`net10.0-windows`），无边框置顶窗口 + 托盘图标。
- 本地源码：`D:\bgsgp\MusicAutoPlayer`
- 安装目录：`D:\Program Files\bggp\1\map`（安装版，由 `mapsetup.iss` 打包，勿直接改安装目录里的文件）

## 关键约束

- `config.json` 的 `Schedule` 里 `t=` **只是每天的时刻**，不是具体日期。任何时间计算都必须按「当前日期 + 时刻」投影，禁止在启动时固化成 `DateTime` 后长期复用。
- 时间相关的逻辑集中放在 `ScheduleMath.cs`（纯函数），`Form1.UpdateLogic` 只负责取结果并更新 UI。
- 版本号改动需同步三处：`Form1.cs` 的 `AppVersion`、`index.html` 的 Version、`mapsetup.iss` 的 `MyAppVersion`。
- 提交信息用中文简述改动；推送前先 `dotnet build -c Release` 通过。

## 构建

```powershell
dotnet build -c Release
```
