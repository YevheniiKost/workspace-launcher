using System.Globalization;
using System.Windows.Data;

namespace WorkspaceLauncher.Converters;

public class StringEqualityConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length != 2)
        {
            return false;
        }

        string first = values[0] as string ?? string.Empty;
        string second = values[1] as string ?? string.Empty;

        return string.Equals(first, second, StringComparison.Ordinal);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}

