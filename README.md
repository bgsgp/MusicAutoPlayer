# 🎵 自动音乐播放器 (MusicAutoPlayer)

> 一款运行于 Windows 平台的桌面工具，可根据预设时间表自动切换背景图并播放本地音乐库中的最新曲目。  
> 项目基于 .NET 开发，采用 MIT 开源协议。

[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

---

## ✨ 功能特性

- ⏱️ **定时任务** – 自定义多个时间点，到达时刻自动切换背景图片并触发音乐播放。
- 🖼️ **背景图片自适应** – 根据屏幕尺寸智能缩放，始终保持最佳显示比例。
- 🔔 **倒计时显示** – 清晰展示距离下一任务剩余时间，支持自定义颜色与字体。
- 🎶 **智能音乐选择** – 自动扫描系统“音乐”文件夹，播放最新修改的音频文件（支持 MP3/WAV/FLAC 等 20+ 格式）。
- 💬 **托盘隐藏** – 可通过托盘图标控制窗口显隐，不干扰正常工作。
- 🌈 **高度可配置** – 通过 `config.json` 灵活设置日程、颜色、默认图片等。

---

## 📥 下载与安装

前往 [Releases 页面](https://github.com/bgsgp/MusicAutoPlayer/releases) 下载最新版本安装包：

- `MusicAutoPlayer_Setup.exe` – 一键安装程序（推荐）

安装包会自动：
1. 将程序安装至指定目录。
2. 注册所需字体到系统（需管理员权限）。
3. 创建桌面快捷方式（可选）。

> 若希望手动部署，也可直接下载 `.zip` 压缩包解压运行 `MusicAutoPlayer.exe`（需自行安装 .NET 运行时）。

---

## ⚙️ 配置说明

程序启动后会在同目录下寻找 `config.json` 文件。若不存在则使用默认设置。

### 示例配置文件

```json
{
  "Schedule": "t=09:00,i=bg_morning.jpg,c=#00FF00;t=12:00,i=bg_noon.jpg,c=#FFFF00",
  "TimeColor": "#00FFFF",
  "NoTaskImage": "bg_default.jpg",
  "NoTaskTimeColor": "#FF0000"
}
```

### 配置项详解

| 字段 | 类型 | 说明 |
|------|------|------|
| `Schedule` | string | 任务时间表，格式 `t=HH:mm,i=图片路径,c=颜色代码`，多个任务以分号 `;` 分隔。<br>颜色支持 HTML 十六进制（如 `#00FF00`）。 |
| `TimeColor` | string | 默认倒计时颜色（有任务时）。 |
| `NoTaskImage` | string | 当天任务全部结束后的默认背景图路径。 |
| `NoTaskTimeColor` | string | 无任务剩余时的倒计时颜色。 |

> **注意**：图片路径相对于程序运行目录。

---

## 🎮 使用方式

1. 双击 `MusicAutoPlayer.exe` 启动程序（首次运行可能无窗口显示，请查看系统托盘图标）。
2. 单击托盘图标可显示/隐藏主窗口。
3. 主窗口显示倒计时及当前状态文字，可任意拖动位置。
4. 右键托盘图标可打开菜单，选择“退出”关闭程序。

---

## 🛠️ 开发相关

### 环境要求

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Visual Studio 2026 或 JetBrains Rider 2025.3+

### 克隆与编译

```bash
git clone https://github.com/bgsgp/MusicAutoPlayer.git
cd MusicAutoPlayer
dotnet build
```

---

## 📄 许可证

本项目采用 [MIT License](LICENSE) 开源协议，可自由使用、修改及分发。

---

## 🙏 致谢

- 字体：方正像素12 – 等宽像素中文字体
- 制作单位：丐帮集团第一院·物理版象棋开发与研究院™
