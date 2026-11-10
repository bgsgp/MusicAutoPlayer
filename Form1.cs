using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace MusicAutoPlayer
{
    public class ConfigData
    {
        public string? Schedule { get; set; }
        public string? TimeColor { get; set; }
        public string? NoTaskImage { get; set; }
        public string? NoTaskTimeColor { get; set; }
        public bool IncludingVideo { get; set; } = false;   // 新增，默认关闭
    }

    public class PlayItem
    {
        // 只保存“每天的时刻”，具体日期在每次计时时按当天投影，避免跨天失效
        public TimeSpan TimeOfDay { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public Color Color { get; set; } = Color.Cyan;
    }

    public class Form1 : Form
    {
        private readonly List<PlayItem> _playList = [];
        private Point _mouseOffset;
        private readonly Timer _timer;
        private Label? _lblTime;
        private Label? _lblNotice;
        private Image? _bgImage;
        private NotifyIcon? _trayIcon;
        private ContextMenuStrip? _trayMenu;
        private string _lastPlayedKey = string.Empty;
        private DateTime _lastTick = DateTime.Now;
        private Color _timeColor = Color.Cyan;
        private Color _noTaskTimeColor = Color.Red;
        private string _noTaskImage = string.Empty;
        private const string AppVersion = "2.9.0";
        private bool _allowVisible = false;
        private bool _includingVideo = false;   // 新增

        public Form1()
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.Black;
            ShowInTaskbar = false;
            TopMost = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

            InitializeComponents();
            InitializeTrayIcon();
            LoadConfigAndParse();

            _timer = new() { Interval = 1000 };
            _timer.Tick += (_, _) => UpdateLogic();
            _timer.Start();

            MouseDown += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                    _mouseOffset = new(-e.X, -e.Y);
            };
            MouseMove += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    Point mousePosition = Control.MousePosition;
                    mousePosition.Offset(_mouseOffset.X, _mouseOffset.Y);
                    Location = mousePosition;
                }
            };
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!_allowVisible)
            {
                value = false;
                if (!IsHandleCreated)
                    CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        private void InitializeTrayIcon()
        {
            _trayMenu = new()
            {
                ShowImageMargin = false,
                ShowCheckMargin = false
            };
            ToolStripMenuItem titleItem = new($"自动音乐播放器 {AppVersion}") { Enabled = false };
            _trayMenu.Items.Add(titleItem);
            _trayMenu.Items.Add(new ToolStripSeparator());
            _trayMenu.Items.Add("退出", null, (_, _) => Application.Exit());

            _trayIcon = new()
            {
                Text = "自动音乐播放器",
                Visible = true,
                ContextMenuStrip = _trayMenu
            };

            _trayIcon.Icon = File.Exists("ico.ico") ? new("ico.ico") : SystemIcons.Application;

            _trayIcon.MouseClick += (_, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (Visible)
                        Hide();
                    else
                    {
                        _allowVisible = true;
                        Show();
                        Activate();
                    }
                }
            };
        }

        private void InitializeComponents()
        {
            FontFamily pixelFamily;
            try
            {
                pixelFamily = new("方正像素12");
            }
            catch
            {
                pixelFamily = new("Consolas");
            }

            _lblTime = new()
            {
                Font = new(pixelFamily, 24f, FontStyle.Bold),
                ForeColor = Color.Cyan,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            _lblNotice = new()
            {
                Font = new("微软雅黑", 9f),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
                Text = "数据分析中：连接钢铁大陆数据流不稳定。此时间并非恒定，包含不规则要素。"
            };

            Controls.Add(_lblTime);
            Controls.Add(_lblNotice);
        }

        private void ChangeBackground(string path)
        {
            string currentPath = Tag as string ?? "";
            if (!File.Exists(path) || currentPath == path)
                return;

            try
            {
                _bgImage?.Dispose();
                using FileStream fs = new(path, FileMode.Open, FileAccess.Read);
                _bgImage = Image.FromStream(fs);

                Screen screen = Screen.FromControl(this);
                double targetArea = (screen.WorkingArea.Width * screen.WorkingArea.Height) / 4.0;
                double ratio = (double)_bgImage.Width / _bgImage.Height;
                int w = (int)Math.Sqrt(targetArea * ratio);
                int h = (int)(w / ratio);
                ClientSize = new(w, h);
                Tag = path;
                CenterToScreen();
            }
            catch
            {
                // 忽略图片加载错误
            }
        }

        private void UpdateLogic()
        {
            DateTime now = DateTime.Now;
            DateTime prev = _lastTick;
            _lastTick = now;

            if (_playList.Count == 0 || _lblTime == null)
                return;

            List<TimeSpan> times = _playList.Select(x => x.TimeOfDay).ToList();

            // 播放：补发 (prev, now] 内刚到达的任务。计时器被 UI 阻塞或系统短暂休眠时，
            // 原来“差值小于 1 秒才播放”的写法会直接漏掉整次任务。
            DateTime? due = ScheduleMath.LatestDue(times, prev, now, TimeSpan.FromSeconds(5));
            if (due != null)
            {
                string key = due.Value.ToString("yyyyMMddHHmm");
                if (_lastPlayedKey != key)
                {
                    PlayMusic();
                    _lastPlayedKey = key;
                }
            }

            DateTime? next = ScheduleMath.NextOccurrence(times, now);
            if (next == null)
                return;

            _lblTime.Text = ScheduleMath.Format(next.Value - now);

            if (next.Value.Date == now.Date)
            {
                // 今天还有任务：用任务自己的颜色和背景图
                PlayItem item = _playList.First(x => x.TimeOfDay == next.Value.TimeOfDay);
                _lblTime.ForeColor = item.Color;
                ChangeBackground(item.ImagePath);
            }
            else
            {
                // 今天的任务已全部结束：倒计时到明天最早的一次
                _lblTime.ForeColor = _noTaskTimeColor;
                if (!string.IsNullOrEmpty(_noTaskImage))
                    ChangeBackground(_noTaskImage);
            }

            LayoutUI();
            Invalidate();
        }

        private void LayoutUI()
        {
            if (_lblTime == null || _lblNotice == null)
                return;

            int w = ClientSize.Width;
            int h = ClientSize.Height;
            float finalFontSize = Math.Clamp(w / 18f, 16f, 72f);

            if (Math.Abs(_lblTime.Font.Size - finalFontSize) > 0.5)
            {
                string familyName = _lblTime.Font.FontFamily.Name;
                _lblTime.Font = new(familyName, finalFontSize, FontStyle.Bold);
            }

            _lblNotice.Left = (w - _lblNotice.Width) / 2;
            _lblNotice.Top = h - _lblNotice.Height - 10;
            _lblTime.Left = (w - _lblTime.Width) / 2;
            _lblTime.Top = _lblNotice.Top - _lblTime.Height - 2;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_bgImage != null)
                e.Graphics.DrawImage(_bgImage, 0, 0, ClientSize.Width, ClientSize.Height);

            if (_lblTime != null)
            {
                using Pen p = new(Color.FromArgb(80, Color.Black), 2f);
                e.Graphics.DrawLine(p, _lblTime.Left, _lblTime.Bottom, _lblTime.Right, _lblTime.Bottom);
            }
        }

        // 替换原来的静态方法，改为实例方法，支持视频比对
        private void PlayMusic()
        {
            try
            {
                string[] audioExts =
                {
                    ".mp3", ".wav", ".wma", ".m4a", ".aac", ".flac", ".ape", ".alac", ".aiff", ".aif", ".m4s",
                    ".ogg", ".opus", ".oga", ".mka", ".dsf", ".dff", ".mid", ".midi", ".m4b", ".mp4", ".m4r"
                };
                string[] videoExts =
                {
                    ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".mpg", ".mpeg", ".m4s", ".3gp", ".3g2", ".vob", ".ts", ".mts", ".m2ts", ".rmvb", ".asf"
                };

                FileInfo? latestAudio = GetLatestFile(Environment.SpecialFolder.MyMusic, audioExts);
                FileInfo? latestVideo = null;

                if (_includingVideo)
                    latestVideo = GetLatestFile(Environment.SpecialFolder.MyVideos, videoExts);

                FileInfo? fileToPlay = latestAudio;
                if (latestVideo != null && (latestAudio == null || latestVideo.LastWriteTime > latestAudio.LastWriteTime))
                    fileToPlay = latestVideo;

                if (fileToPlay != null)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = fileToPlay.FullName,
                        UseShellExecute = true
                    });
                }
            }
            catch
            {
                // 忽略播放错误
            }
        }

        // 辅助方法：在指定特殊文件夹下递归查找最新文件
        private static FileInfo? GetLatestFile(Environment.SpecialFolder folder, string[] extensions)
        {
            try
            {
                string path = Environment.GetFolderPath(folder);
                if (!Directory.Exists(path))
                    return null;

                DirectoryInfo dir = new(path);
                return dir.GetFiles("*.*", SearchOption.AllDirectories)
                          .Where(f => extensions.Contains(f.Extension.ToLower()))
                          .MaxBy(f => f.LastWriteTime);
            }
            catch
            {
                return null;
            }
        }

        private void LoadConfigAndParse()
        {
            try
            {
                if (!File.Exists("config.json"))
                    return;

                string json = File.ReadAllText("config.json");
                ConfigData? config = JsonSerializer.Deserialize<ConfigData>(json);
                if (config == null)
                    return;

                if (!string.IsNullOrEmpty(config.TimeColor))
                    _timeColor = ColorTranslator.FromHtml(config.TimeColor);
                if (!string.IsNullOrEmpty(config.NoTaskTimeColor))
                    _noTaskTimeColor = ColorTranslator.FromHtml(config.NoTaskTimeColor);

                _noTaskImage = config.NoTaskImage ?? "";
                _includingVideo = config.IncludingVideo;   // 读取视频开关

                if (string.IsNullOrEmpty(config.Schedule))
                    return;

                _playList.Clear();
                string[] segments = config.Schedule.Split(';', StringSplitOptions.RemoveEmptyEntries);
                foreach (string seg in segments)
                {
                    string[] parts = seg.Split(',');
                    string t = parts.FirstOrDefault(p => p.Trim().StartsWith("t="))?.Split('=')[1].Trim() ?? "";
                    string img = parts.FirstOrDefault(p => p.Trim().StartsWith("i="))?.Split('=')[1].Trim() ?? "";
                    string c = parts.FirstOrDefault(p => p.Trim().StartsWith("c="))?.Split('=')[1].Trim() ?? "";

                    if (string.IsNullOrEmpty(t))
                        continue;

                    if (!TimeSpan.TryParse(t, CultureInfo.InvariantCulture, out TimeSpan timeOfDay))
                        continue;
                    if (timeOfDay < TimeSpan.Zero || timeOfDay >= TimeSpan.FromDays(1))
                        continue;

                    Color col = _timeColor;
                    if (!string.IsNullOrEmpty(c))
                    {
                        try { col = ColorTranslator.FromHtml(c); } catch { }
                    }

                    _playList.Add(new()
                    {
                        TimeOfDay = timeOfDay,
                        ImagePath = img,
                        Color = col
                    });
                }
            }
            catch
            {
                // 忽略配置解析错误
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _trayIcon?.Dispose();
            base.OnFormClosing(e);
        }
    }
}