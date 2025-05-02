using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace QuizApp.ViewModels
{
    public class ScoreToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double score && score > 0)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
