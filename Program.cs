using System;
using System.Threading;
using System.Windows.Forms;
using MusicAutoPlayer;

namespace MusicAutoPlayer
{
    internal static class Program
    {
        // 定义一个唯一的互斥体名称（建议使用 AppId 或项目名 + 特殊后缀）
        private static readonly string MutexName = "MusicAutoPlayer_SingleInstance_2E5A1F8C";

        [STAThread]
        private static void Main()
        {
            // 尝试创建互斥体，如果已存在同名互斥体则说明程序已在运行
            using Mutex mutex = new(true, MutexName, out bool createdNew);

            if (!createdNew)
            {
                // 已有实例在运行，弹出提示后退出
                MessageBox.Show(
                    "自动音乐播放器已经在运行中。\n请检查系统托盘图标或任务管理器。",
                    "程序已在运行",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return; // 直接退出，不启动新窗体
            }

            // 正常启动应用程序
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());

            // mutex 会在 using 块结束时自动释放
        }
    }
}