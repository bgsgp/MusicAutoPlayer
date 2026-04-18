using System;
using System.Windows.Forms;
using MusicAutoPlayer;   // 关键：引入包含 Form1 的命名空间

namespace MusicAutoPlayer
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}