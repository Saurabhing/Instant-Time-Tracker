using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace InstantTimeTracker
{
    public partial class StatisticsPage : Page
    {
        private string _jsonPath = "usage_history.json";

        public StatisticsPage()
        {
            InitializeComponent();
            datePicker.SelectedDate = DateTime.Now;
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (datePicker.SelectedDate.HasValue)
                LoadHistory(datePicker.SelectedDate.Value);
        }

        private void LoadHistory(DateTime date)
        {
            if (!File.Exists(_jsonPath)) return;

            try
            {
                var json = File.ReadAllText(_jsonPath);
                var history = JsonConvert.DeserializeObject<List<DailyRecord>>(json);

                var record = history?.FirstOrDefault(x => x.Date.Date == date.Date);

                if (record != null)
                {
                    gridHistory.ItemsSource = record.Logs.OrderByDescending(x => x.Duration).ToList();
                }
                else
                {
                    gridHistory.ItemsSource = null;
                }
            }
            catch { }
        }
    }
}