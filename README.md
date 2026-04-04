# 🎵 自动音乐播放器 (MusicAutoPlayer)

**版本：2.6.9** | 一个定时触发音乐播放的桌面小工具，支持倒计时、背景图切换、托盘隐藏等特性。

## ✨ 功能特性

- ⏰ **精准定时**：支持多个时间点设置，到点自动播放音乐（从“我的音乐”文件夹中选取最近修改的音频文件）
- 🖼️ **背景切换**：每个时间点可绑定一张背景图，倒计时窗口自动适应图片比例
- 🎨 **颜色自定义**：每个任务可单独设置倒计时数字颜色，无任务时显示不同颜色
- 🔘 **无边框窗口**：鼠标左键拖动移动，双击隐藏至系统托盘
- 🧩 **托盘图标**：左键单击显示/隐藏主窗口，右键菜单可退出程序
- ⚙️ **JSON 配置**：通过 `config.json` 灵活设置日程表、颜色、背景图

## 📥 下载与运行

从 [Releases](../../releases) 页面下载最新版本，解压后直接运行 `MusicAutoPlayer.exe`。

### 系统要求
- Windows 7 / 8 / 10 / 11
- .NET Framework 4.6.1 或更高版本（一般 Windows 10 以上已自带）

## ⚙️ 配置说明

在程序同目录下创建 `config.json` 文件，示例内容如下：

```json
{
  "Schedule": "t=09:00,i=bg_morning.jpg,c=#00FF00;t=12:00,i=bg_noon.jpg,c=#FFFF00;t=18:00,i=bg_evening.jpg,c=#FFA500",
  "TimeColor": "#00FFFF",
  "NoTaskImage": "bg_default.jpg",
  "NoTaskTimeColor": "#FF0000"
}
