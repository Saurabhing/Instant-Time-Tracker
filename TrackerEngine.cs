using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Newtonsoft.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Interop;

namespace InstantTimeTracker
{
    public class AppLog
    {
        public string AppName { get; set; }
        public string FriendlyName { get; set; }
        public string FilePath { get; set; }
        public string Category { get; set; } = "General";
        public TimeSpan Duration { get; set; }
        [JsonIgnore] public ImageSource AppIcon { get; set; }
        public string DurationString => Duration.ToString(@"hh\:mm\:ss");
        [JsonIgnore] public double UsagePercentage { get; set; }
    }

    public class DailyRecord
    {
        public DateTime Date { get; set; }
        public List<AppLog> Logs { get; set; } = new List<AppLog>();
    }

    public class TrackerEngine
    {
        [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        private DispatcherTimer _timer;
        private DailyRecord _todayRecord;
        private string _jsonPath = Path.Combine(Path.GetDirectoryName(System.Environment.ProcessPath), "usage_history.json");
        private string _catPath = Path.Combine(Path.GetDirectoryName(System.Environment.ProcessPath), "categories.json");

        public bool IsPaused { get; set; } = false;
        private DateTime _appStartTime;

        private Dictionary<string, string> _nameCache = new Dictionary<string, string>();
        private Dictionary<string, ImageSource> _iconCache = new Dictionary<string, ImageSource>();

        public string CurrentCategory { get; private set; } = "Everything";
        public List<string> Categories { get; set; } = new List<string>();

        public event Action<AppLog, TimeSpan, TimeSpan> OnUpdateUI;
        public event Action OnCategoriesChanged;

        public TrackerEngine()
        {
            _appStartTime = DateTime.Now;
            LoadData();
            LoadCategories();

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        public void AddCategory(string newCategory)
        {
            if (string.IsNullOrWhiteSpace(newCategory)) return;
            if (Categories.Contains(newCategory)) return;

            Categories.Add(newCategory);
            SaveCategories();
            OnCategoriesChanged?.Invoke();
        }

        // --- NEW: Remove Category ---
        public void RemoveCategory(string categoryToRemove)
        {
            // prevent deleting core modes
            if (categoryToRemove == "Everything" || categoryToRemove == "Study" || categoryToRemove == "General")
                return;

            if (Categories.Contains(categoryToRemove))
            {
                Categories.Remove(categoryToRemove);
                SaveCategories();

                // If we were currently tracking this mode, switch to Everything safety
                if (CurrentCategory == categoryToRemove)
                    CurrentCategory = "Everything";

                OnCategoriesChanged?.Invoke();
            }
        }
        // ----------------------------

        public void SwitchCategory(string newCategory)
        {
            CurrentCategory = newCategory;
        }

        public void TogglePause()
        {
            IsPaused = !IsPaused;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (IsPaused) return;

                string processName = GetActiveProcessName(out string fullPath);
                if (string.IsNullOrEmpty(processName)) return;

                if (!_nameCache.ContainsKey(processName))
                {
                    _nameCache[processName] = GetFriendlyName(fullPath, processName);
                    _iconCache[processName] = GetIcon(fullPath);
                }

                var appEntry = _todayRecord.Logs.FirstOrDefault(x => x.AppName == processName && x.Category == CurrentCategory);
                if (appEntry == null)
                {
                    appEntry = new AppLog
                    {
                        AppName = processName,
                        FilePath = fullPath,
                        Category = CurrentCategory,
                        Duration = TimeSpan.Zero
                    };
                    _todayRecord.Logs.Add(appEntry);
                }

                if (string.IsNullOrEmpty(appEntry.FilePath) && !string.IsNullOrEmpty(fullPath)) appEntry.FilePath = fullPath;
                appEntry.FriendlyName = _nameCache[processName];
                appEntry.AppIcon = _iconCache[processName];
                appEntry.Duration = appEntry.Duration.Add(TimeSpan.FromSeconds(1));

                TimeSpan grandTotal = TimeSpan.FromSeconds(_todayRecord.Logs.Sum(x => x.Duration.TotalSeconds));

                TimeSpan displaySessionTime;
                if (CurrentCategory == "Everything")
                {
                    displaySessionTime = DateTime.Now - _appStartTime;
                }
                else
                {
                    var catLogs = _todayRecord.Logs.Where(x => x.Category == CurrentCategory);
                    displaySessionTime = TimeSpan.FromSeconds(catLogs.Sum(x => x.Duration.TotalSeconds));
                }

                OnUpdateUI?.Invoke(appEntry, displaySessionTime, grandTotal);

                if (DateTime.Now.Second % 10 == 0) SaveData();
            }
            catch { }
        }

        private void LoadCategories()
        {
            try
            {
                if (File.Exists(_catPath))
                {
                    string json = File.ReadAllText(_catPath);
                    Categories = JsonConvert.DeserializeObject<List<string>>(json) ?? new List<string>();
                }
            }
            catch { }

            if (!Categories.Contains("Study"))
            {
                Categories.Add("Study");
                SaveCategories();
            }
        }

        private void SaveCategories()
        {
            try
            {
                File.WriteAllText(_catPath, JsonConvert.SerializeObject(Categories));
            }
            catch { }
        }

        // --- Helpers ---
        public List<AppLog> GetTodayLogs() { RestoreIcons(_todayRecord.Logs); return new List<AppLog>(_todayRecord.Logs); }
        public List<AppLog> GetHistory(DateTime date) { if (File.Exists(_jsonPath)) { try { var allData = JsonConvert.DeserializeObject<List<DailyRecord>>(File.ReadAllText(_jsonPath)); var record = allData?.FirstOrDefault(x => x.Date.Date == date.Date); var logs = record?.Logs ?? new List<AppLog>(); RestoreIcons(logs); return logs; } catch { } } return new List<AppLog>(); }
        private void RestoreIcons(List<AppLog> logs) { foreach (var log in logs) { if (string.IsNullOrEmpty(log.Category)) log.Category = "General"; if (_iconCache.ContainsKey(log.AppName)) log.AppIcon = _iconCache[log.AppName]; else if (!string.IsNullOrEmpty(log.FilePath)) { var icon = GetIcon(log.FilePath); if (icon != null) { _iconCache[log.AppName] = icon; log.AppIcon = icon; } } } }
        private string GetActiveProcessName(out string fullPath) { fullPath = ""; try { IntPtr h = GetForegroundWindow(); if (h == IntPtr.Zero) return ""; GetWindowThreadProcessId(h, out uint pid); var p = Process.GetProcessById((int)pid); try { fullPath = p.MainModule?.FileName; } catch { } return p.ProcessName; } catch { return ""; } }
        private string GetFriendlyName(string path, string processName) { try { if (!string.IsNullOrEmpty(path) && File.Exists(path)) return FileVersionInfo.GetVersionInfo(path).FileDescription ?? processName; } catch { } return processName; }
        private ImageSource GetIcon(string path) { try { if (!string.IsNullOrEmpty(path) && File.Exists(path)) { using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(path)) { var bs = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions()); bs.Freeze(); return bs; } } } catch { } return null; }
        private void LoadData() { try { List<DailyRecord> h = new List<DailyRecord>(); if (File.Exists(_jsonPath)) { string j = File.ReadAllText(_jsonPath); if (!string.IsNullOrWhiteSpace(j)) h = JsonConvert.DeserializeObject<List<DailyRecord>>(j) ?? new List<DailyRecord>(); } _todayRecord = h.FirstOrDefault(x => x.Date.Date == DateTime.Now.Date); if (_todayRecord == null) { _todayRecord = new DailyRecord { Date = DateTime.Now.Date }; h.Add(_todayRecord); File.WriteAllText(_jsonPath, JsonConvert.SerializeObject(h, Formatting.Indented)); } } catch { _todayRecord = new DailyRecord { Date = DateTime.Now.Date }; } }
        public void SaveData() { try { List<DailyRecord> h = new List<DailyRecord>(); if (File.Exists(_jsonPath)) try { h = JsonConvert.DeserializeObject<List<DailyRecord>>(File.ReadAllText(_jsonPath)) ?? new List<DailyRecord>(); } catch { h = new List<DailyRecord>(); } var e = h.FirstOrDefault(x => x.Date.Date == DateTime.Now.Date); if (e != null) h.Remove(e); h.Add(_todayRecord); File.WriteAllText(_jsonPath, JsonConvert.SerializeObject(h, Formatting.Indented)); } catch { } }
    }
}