using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WorkspaceLauncher.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool inverse = parameter is string s && s == "inverse";
        bool boolValue = value is bool b && b;
        return (boolValue ^ inverse) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is Visibility v && v == Visibility.Visible;
}
