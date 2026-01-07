using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace InstantTimeTracker
{
    public partial class App : Application
    {
        private TrackerEngine _engine;
        private WidgetWindow _widget;
        private MainWindow _dashboard;
        private NotifyIcon _trayIcon;

        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // FIX: Explicitly write "System.Windows.MessageBox" to fix the ambiguity
            System.Windows.MessageBox.Show("Crash prevented: " + e.Exception.Message, "Tracker Error");

            e.Handled = true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _engine = new TrackerEngine();
            _dashboard = new MainWindow(_engine);
            _widget = new WidgetWindow(_engine);

            // FIX: We must subscribe to this event to fix the "Event never used" error
            _widget.MinimizeRequested += () => _widget.Hide();

            _widget.OpenDashboardRequested += () => { _dashboard.Show(); _dashboard.Activate(); };

            _widget.ExitRequested += () =>
            {
                _engine.SaveData();
                if (_trayIcon != null) _trayIcon.Dispose();
                Shutdown();
                System.Environment.Exit(0);
            };

            _trayIcon = new NotifyIcon { Icon = SystemIcons.Application, Visible = true, Text = "Time Tracker" };

            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Open Dashboard", null, (s, args) => _dashboard.Show());
            menu.Items.Add("Quit", null, (s, args) =>
            {
                _engine.SaveData();
                if (_trayIcon != null) _trayIcon.Dispose();
                Shutdown();
                System.Environment.Exit(0);
            });
            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.DoubleClick += (s, args) => _widget.Show();

            _engine.OnUpdateUI += (appLog, session, total) =>
            {
                _widget.Dispatcher.Invoke(() =>
                {
                    _widget.UpdateData(appLog, session, total);
                    if (_trayIcon != null) _trayIcon.Text = $"Total: {total:hh\\:mm}";
                });
            };

            _widget.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _engine?.SaveData();
            if (_trayIcon != null) _trayIcon.Dispose();
            base.OnExit(e);
        }
    }
}