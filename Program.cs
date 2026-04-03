using System;
using System.Windows.Forms;

namespace MusicAutoPlayer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            // 这里会去寻找 Form1.cs 里的 Form1 类
            Application.Run(new Form1());
        }
    }
}