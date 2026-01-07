using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InstantTimeTracker
{
    public partial class MainWindow : Window
    {
        private TrackerEngine _engine;

        public MainWindow(TrackerEngine engine)
        {
            InitializeComponent();
            _engine = engine;

            // Load the Day Page by default
            MainFrame.Navigate(new DayPage(_engine));
        }

        private void NavDay_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new DayPage(_engine));
        }

        private void NavStatistics_Click(object sender, RoutedEventArgs e)
        {
            // We will fix StatisticsPage later if needed, for now just navigate
            // If you haven't created StatisticsPage yet, comment this line out:
            MainFrame.Navigate(new StatisticsPage());
        }

        private void ChkStartup_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    string exePath = System.Environment.ProcessPath;
                    key.SetValue("InstantTimeTracker", exePath);
                }
            }
            catch (Exception ex)
            {
                // FIX: Explicitly use System.Windows.MessageBox here too!
                System.Windows.MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void ChkStartup_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    key.DeleteValue("InstantTimeTracker", false);
                }
            }
            catch { }
        }

        private void CheckStartup()
        {
            try
            {
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    object val = key.GetValue("InstantTimeTracker");
                    chkStartup.IsChecked = (val != null);
                }
            }
            catch { }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Minimize to tray instead of closing
            e.Cancel = true;
            this.Hide();
        }
    }
}