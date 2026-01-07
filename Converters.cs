using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace InstantTimeTracker
{
    // 1. Controls Badge Visibility based on the "View" Dropdown
    public class FilterToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If the selected filter is "Everything" (or null), HIDE the badges
            string filter = value as string;
            if (string.IsNullOrEmpty(filter) || filter == "Everything")
                return Visibility.Collapsed;

            // Otherwise, show them (if you want tags in specific modes)
            return Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // 2. Original Category Logic (Optional, keeps "General" hidden)
    public class CategoryVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string cat = value as string;
            return (string.IsNullOrEmpty(cat) || cat == "General") ? Visibility.Collapsed : Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    // 3. Progress Bar Width
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v) return v;
            return 0;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}