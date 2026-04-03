using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
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
    }

    public class PlayItem
    {
        public DateTime Time { get; set; }
        public string ImagePath { get; set; } = string.Empty;
        public Color Color { get; set; } = Color.Cyan;
    }

    public class Form1 : Form
    {
        private Timer? _timer;
        private Label? _lblTime;
        private Label? _lblNotice;
        private Button? _btnClose;
        private Image? _bgImage;
        private NotifyIcon? _trayIcon;

        private string _lastPlayedKey = string.Empty;
        private List<PlayItem> _playList = new List<PlayItem>();
        private Point _mouseOffset;

        private Color _timeColor = Color.Cyan;
        private Color _noTaskTimeColor = Color.Red;
        private string _noTaskImage = string.Empty;

        public Form1()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.Black;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

            InitializeComponents();
            InitializeTrayIcon();
            LoadConfigAndParse();

            _timer = new Timer { Interval = 1000 };
            _timer.Tick += (s, e) => UpdateLogic();
            _timer.Start();

            this.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) _mouseOffset = new Point(-e.X, -e.Y); };
            this.MouseMove += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    Point mPos = Control.MousePosition;
                    mPos.Offset(_mouseOffset.X, _mouseOffset.Y);
                    this.Location = mPos;
                }
            };

            this.DoubleClick += (s, e) => {
                this.Hide();
                if (_trayIcon != null) _trayIcon.Visible = true;
            };
        }

        private void InitializeTrayIcon()
        {
            _trayIcon = new NotifyIcon();
            _trayIcon.Text = "Music Auto Player";

            if (File.Exists("ico.ico"))
                _trayIcon.Icon = new Icon("ico.ico");
            else
                _trayIcon.Icon = SystemIcons.Application;

            _trayIcon.MouseClick += (s, e) => {
                if (e.Button == MouseButtons.Left)
                {
                    this.Show();
                    this.WindowState = FormWindowState.Normal;
                    this.Activate();
                    _trayIcon.Visible = false;
                }
            };
        }

        private void InitializeComponents()
        {
            _lblTime = new Label
            {
                Font = new Font("Consolas", 32F, FontStyle.Bold),
                ForeColor = Color.Cyan,
                AutoSize = true,
                BackColor = Color.Transparent
            };

            _lblNotice = new Label
            {
                Font = new Font("微软雅黑", 9F),
                ForeColor = Color.White,
                AutoSize = true,
                BackColor = Color.Transparent,
                Text = "数据分析中：连接钢铁大陆数据流不稳定。该小时并非固定。"
            };

            _btnClose = new Button
            {
                Text = "×",
                Size = new Size(25, 25),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.DimGray,
                BackColor = Color.Transparent
            };
            _btnClose.FlatAppearance.BorderSize = 0;
            _btnClose.Click += (s, e) => this.Close();

            this.Controls.Add(_lblTime);
            this.Controls.Add(_lblNotice);
            this.Controls.Add(_btnClose);
        }

        private void LoadConfigAndParse()
        {
            try
            {
                if (!File.Exists("config.json")) return;

                string jsonString = File.ReadAllText("config.json");
                var rawConfig = JsonSerializer.Deserialize<ConfigData>(jsonString);

                // 默认颜色
                if (!string.IsNullOrEmpty(rawConfig?.TimeColor))
                    _timeColor = ColorTranslator.FromHtml(rawConfig.TimeColor);

                if (!string.IsNullOrEmpty(rawConfig?.NoTaskTimeColor))
                    _noTaskTimeColor = ColorTranslator.FromHtml(rawConfig.NoTaskTimeColor);

                _noTaskImage = rawConfig?.NoTaskImage ?? "";

                string scheduleStr = rawConfig?.Schedule ?? "";
                if (string.IsNullOrEmpty(scheduleStr)) return;

                _playList.Clear();
                var segments = scheduleStr.Split(';', StringSplitOptions.RemoveEmptyEntries);

                foreach (var seg in segments)
                {
                    var parts = seg.Split(',');

                    string t = parts.FirstOrDefault(p => p.Trim().StartsWith("t="))?.Replace("t=", "").Trim() ?? "";
                    string i = parts.FirstOrDefault(p => p.Trim().StartsWith("i="))?.Replace("i=", "").Trim() ?? "";
                    string c = parts.FirstOrDefault(p => p.Trim().StartsWith("c="))?.Replace("c=", "").Trim() ?? "";

                    if (!string.IsNullOrEmpty(t))
                    {
                        Color itemColor = _timeColor;

                        if (!string.IsNullOrEmpty(c))
                        {
                            try { itemColor = ColorTranslator.FromHtml(c); } catch { }
                        }

                        _playList.Add(new PlayItem
                        {
                            Time = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-dd ") + t),
                            ImagePath = i,
                            Color = itemColor
                        });
                    }
                }
            }
            catch { }
        }

        private void UpdateLogic()
        {
            if (_playList.Count == 0 || _lblTime == null) return;

            DateTime now = DateTime.Now;

            var todayTasks = _playList
                .Select(item => new { Item = item, Target = item.Time })
                .Where(x => x.Target >= now)
                .OrderBy(x => x.Target)
                .ToList();

            var next = todayTasks.FirstOrDefault();

            // ===== 无任务 =====
            if (next == null)
            {
                var tomorrowFirst = _playList
                    .Select(item => item.Time.AddDays(1))
                    .OrderBy(t => t)
                    .FirstOrDefault();

                if (tomorrowFirst == default) return;

                TimeSpan diff = tomorrowFirst - now;

                _lblTime.Text = $"{(int)diff.TotalHours:D2}:{diff.Minutes:D2}:{diff.Seconds:D2}";
                _lblTime.ForeColor = _noTaskTimeColor;

                if (!string.IsNullOrEmpty(_noTaskImage))
                    ChangeBackground(_noTaskImage);

                LayoutUI();
                this.Invalidate();
                return;
            }

            // ===== 正常任务 =====
            TimeSpan normalDiff = next.Target - now;

            _lblTime.Text = $"{(int)normalDiff.TotalHours:D2}:{normalDiff.Minutes:D2}:{normalDiff.Seconds:D2}";
            _lblTime.ForeColor = next.Item.Color;

            ChangeBackground(next.Item.ImagePath);

            string timeKey = next.Target.ToString("yyyyMMddHHmmss");
            if (Math.Abs(normalDiff.TotalSeconds) < 1.0 && _lastPlayedKey != timeKey)
            {
                PlayLatestMusicFromLibrary();
                _lastPlayedKey = timeKey;
            }

            LayoutUI();
            this.Invalidate();
        }

        private void PlayLatestMusicFromLibrary()
        {
            try
            {
                string musicPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                if (!Directory.Exists(musicPath)) return;

                var directory = new DirectoryInfo(musicPath);
                var lastAudioFile = directory.GetFiles("*.*", SearchOption.AllDirectories)
                    .Where(f => ".mp3.wav.flac.m4a.wma".Contains(f.Extension.ToLower()))
                    .OrderByDescending(f => f.LastWriteTime)
                    .FirstOrDefault();

                if (lastAudioFile != null)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = lastAudioFile.FullName,
                        UseShellExecute = true
                    });
                }
            }
            catch { }
        }

        private void ChangeBackground(string path)
        {
            string currentPath = this.Tag as string ?? "";

            if (File.Exists(path) && currentPath != path)
            {
                try
                {
                    _bgImage?.Dispose();
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        _bgImage = Image.FromStream(fs);
                    }

                    this.ClientSize = _bgImage.Size;
                    this.Tag = path;
                    this.CenterToScreen();
                }
                catch { }
            }
        }

        private void LayoutUI()
        {
            if (_lblTime == null || _lblNotice == null) return;

            int w = this.ClientSize.Width, h = this.ClientSize.Height;

            _lblNotice.Top = h - _lblNotice.Height - 30;
            _lblNotice.Left = (w - _lblNotice.Width) / 2;

            _lblTime.Top = _lblNotice.Top - _lblTime.Height - 5;
            _lblTime.Left = (w - _lblTime.Width) / 2;

            if (_btnClose != null)
                _btnClose.Left = w - _btnClose.Width - 5;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_bgImage != null)
                e.Graphics.DrawImage(_bgImage, 0, 0, ClientSize.Width, ClientSize.Height);

            if (_lblTime != null)
            {
                using (Pen p = new Pen(Color.Black, 1))
                    e.Graphics.DrawLine(p, _lblTime.Left, _lblTime.Bottom, _lblTime.Right, _lblTime.Bottom);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _trayIcon?.Dispose();
            base.OnFormClosing(e);
        }
    }
}