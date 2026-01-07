using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InstantTimeTracker
{
    public partial class DayPage : Page
    {
        private TrackerEngine _engine;
        private DateTime _currentDate;

        public DayPage(TrackerEngine engine)
        {
            InitializeComponent();
            _engine = engine;
            _currentDate = DateTime.Today;

            _engine.OnCategoriesChanged += RefreshDropdown;
            _engine.OnUpdateUI += OnEngineUpdate;

            RefreshDropdown();
            UpdateDateDisplay();
            RefreshUI();
        }

        private void OnEngineUpdate(AppLog log, TimeSpan session, TimeSpan total)
        {
            if (_currentDate.Date == DateTime.Today)
            {
                Dispatcher.Invoke(RefreshUI);
            }
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_engine != null)
            {
                _engine.OnCategoriesChanged -= RefreshDropdown;
                _engine.OnUpdateUI -= OnEngineUpdate;
            }
        }

        private void RefreshDropdown()
        {
            var selected = cmbFilter.SelectedItem?.ToString();
            cmbFilter.Items.Clear();
            cmbFilter.Items.Add("Everything");
            foreach (var cat in _engine.Categories)
            {
                cmbFilter.Items.Add(cat);
            }

            if (selected != null && cmbFilter.Items.Contains(selected))
                cmbFilter.SelectedItem = selected;
            else
                cmbFilter.SelectedIndex = 0;
        }

        // --- Add Mode ---
        private void BtnAddMode_Click(object sender, RoutedEventArgs e)
        {
            string newMode = txtNewMode.Text.Trim();
            if (!string.IsNullOrEmpty(newMode))
            {
                _engine.AddCategory(newMode);
                txtNewMode.Text = "";
                RefreshDropdown();
            }
        }

        // --- Delete Mode (FIXED AMBIGUITY) ---
        private void BtnDeleteMode_Click(object sender, RoutedEventArgs e)
        {
            if (cmbFilter.SelectedItem == null) return;
            string selected = cmbFilter.SelectedItem.ToString();

            if (selected == "Everything" || selected == "Study")
            {
                // FIX: Explicit System.Windows usage
                System.Windows.MessageBox.Show("You cannot delete 'Everything' or 'Study'.", "System Mode");
                return;
            }

            // FIX: Explicit System.Windows usage for MessageBox, MessageBoxButton, and MessageBoxResult
            if (System.Windows.MessageBox.Show($"Are you sure you want to delete '{selected}'?", "Delete Mode", System.Windows.MessageBoxButton.YesNo) == System.Windows.MessageBoxResult.Yes)
            {
                _engine.RemoveCategory(selected);
                RefreshDropdown();
            }
        }
        // -------------------------

        private void CmbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (_engine == null) return;

            var logs = (_currentDate.Date == DateTime.Today)
                ? _engine.GetTodayLogs()
                : _engine.GetHistory(_currentDate);

            string filter = cmbFilter.SelectedItem?.ToString() ?? "Everything";

            if (filter != "Everything")
            {
                logs = logs.Where(x => x.Category == filter).ToList();
            }

            TimeSpan total = TimeSpan.FromSeconds(logs.Sum(x => x.Duration.TotalSeconds));
            txtDashTotal.Text = total.ToString(@"hh\:mm\:ss");

            if (_currentDate.Date == DateTime.Today && filter == "Everything")
                txtDashSession.Text = DateTime.Now.Subtract(System.Diagnostics.Process.GetCurrentProcess().StartTime).ToString(@"hh\:mm\:ss");
            else
                txtDashSession.Text = "--:--:--";

            if (logs.Count > 0)
            {
                double max = logs.Max(x => x.Duration.TotalSeconds);
                if (max > 0)
                {
                    foreach (var log in logs)
                        log.UsagePercentage = (log.Duration.TotalSeconds / max) * 100;
                }
                gridDashboard.ItemsSource = logs.OrderByDescending(x => x.Duration).ToList();
            }
            else
            {
                gridDashboard.ItemsSource = null;
            }
        }

        private void UpdateDateDisplay() => txtDateDisplay.Text = _currentDate.ToString("ddd, dd-MM-yyyy");
        private void BtnPrev_Click(object sender, RoutedEventArgs e) { _currentDate = _currentDate.AddDays(-1); UpdateDateDisplay(); RefreshUI(); }
        private void BtnNext_Click(object sender, RoutedEventArgs e) { _currentDate = _currentDate.AddDays(1); UpdateDateDisplay(); RefreshUI(); }
        private void BtnToday_Click(object sender, RoutedEventArgs e) { _currentDate = DateTime.Today; UpdateDateDisplay(); RefreshUI(); }
        private void DatePicker_DateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (datePickerHidden.SelectedDate.HasValue)
            {
                _currentDate = datePickerHidden.SelectedDate.Value;
                UpdateDateDisplay();
                RefreshUI();
            }
        }
    }
}