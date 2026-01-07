using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace InstantTimeTracker
{
    public partial class WidgetWindow : Window
    {
        private TrackerEngine _engine;
        private bool _isExpanded = true;

        public event Action OpenDashboardRequested;
        public event Action MinimizeRequested;
        public event Action ExitRequested;

        public WidgetWindow(TrackerEngine engine)
        {
            InitializeComponent();
            _engine = engine;

            // Subscribe to updates
            _engine.OnCategoriesChanged += RefreshDropdown;

            RefreshDropdown();
        }

        private void RefreshDropdown()
        {
            var current = cmbCategory.SelectedItem?.ToString();

            cmbCategory.Items.Clear();

            // 1. Add "Everything"
            cmbCategory.Items.Add("Everything");

            // 2. Add Custom Categories (Study, etc.)
            foreach (var cat in _engine.Categories)
            {
                if (cat != "General" && cat != "Everything")
                {
                    cmbCategory.Items.Add(cat);
                }
            }

            // 3. Selection Logic: Default to "Everything"
            if (current != null && cmbCategory.Items.Contains(current))
            {
                cmbCategory.SelectedItem = current;
            }
            else
            {
                // FIX: Default is now Everything
                cmbCategory.SelectedItem = "Everything";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Left = SystemParameters.WorkArea.Width - this.Width - 20;
            this.Top = SystemParameters.WorkArea.Height - this.Height - 20;
        }

        private void CmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbCategory.SelectedItem != null)
            {
                string selected = cmbCategory.SelectedItem.ToString();
                _engine.SwitchCategory(selected);
            }
        }

        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            _engine.TogglePause();
            if (_engine.IsPaused)
            {
                txtPauseIcon.Text = "▶";
                txtPauseIcon.Foreground = new SolidColorBrush(System.Windows.Media.Colors.LightGreen);
                txtActiveApp.Text = "Paused";
            }
            else
            {
                txtPauseIcon.Text = "⏸";
                txtPauseIcon.Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 85, 85));
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed) this.DragMove();
        }

        public void UpdateData(AppLog app, TimeSpan session, TimeSpan total)
        {
            if (_engine.IsPaused) return;

            txtActiveApp.Text = app.FriendlyName;
            txtSessionTime.Text = session.ToString(@"hh\:mm\:ss");
            txtTotalTime.Text = total.ToString(@"hh\:mm\:ss");

            if (app.AppIcon != null)
                imgAppIcon.Source = app.AppIcon;
        }

        private void BtnToggle_Click(object sender, RoutedEventArgs e)
        {
            if (_isExpanded) { this.Height = 70; panelActiveApp.Visibility = Visibility.Collapsed; btnToggle.Content = "▲"; }
            else { this.Height = 110; panelActiveApp.Visibility = Visibility.Visible; btnToggle.Content = "▼"; }
            _isExpanded = !_isExpanded;
        }

        private void MinimizeToTray_Click(object sender, RoutedEventArgs e) => MinimizeRequested?.Invoke();
        private void ToggleTopmost_Click(object sender, RoutedEventArgs e) => this.Topmost = !this.Topmost;
        private void OpenDashboard_Click(object sender, RoutedEventArgs e) => OpenDashboardRequested?.Invoke();
        private void Exit_Click(object sender, RoutedEventArgs e) => ExitRequested?.Invoke();
    }
}