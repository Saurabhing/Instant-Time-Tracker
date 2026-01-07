using System;
using System.Linq;
using System.Windows.Controls;
// FIX: Explicitly tell it to use WPF UserControl to avoid conflict with WinForms
using UserControl = System.Windows.Controls.UserControl;

namespace InstantTimeTracker
{
    public partial class DashboardView : UserControl
    {
        private TrackerEngine _engine;

        public DashboardView(TrackerEngine engine)
        {
            InitializeComponent();
            _engine = engine;
            txtDate.Text = DateTime.Now.ToString("ddd, dd-MM-yyyy");
            RefreshUI();
        }

        public void UpdateStats(TimeSpan session, TimeSpan total)
        {
            txtDashTotal.Text = total.ToString(@"hh\:mm\:ss");
            txtDashSession.Text = session.ToString(@"hh\:mm\:ss");
            RefreshUI();
        }

        private void RefreshUI()
        {
            var logs = _engine.GetTodayLogs();
            if (logs.Count == 0) return;

            double max = logs.Max(x => x.Duration.TotalSeconds);
            if (max > 0)
            {
                foreach (var log in logs)
                    log.UsagePercentage = (log.Duration.TotalSeconds / max) * 100;
            }
            gridDashboard.ItemsSource = logs.OrderByDescending(x => x.Duration).ToList();
        }
    }
}