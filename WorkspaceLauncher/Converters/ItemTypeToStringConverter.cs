using System.Globalization;
using System.Windows.Data;
using WorkspaceLauncher.Models;

namespace WorkspaceLauncher.Converters;

public class ItemTypeToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            ItemType.Executable => "App",
            ItemType.Url => "URL",
            ItemType.Folder => "Folder",
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            "App" => ItemType.Executable,
            "URL" => ItemType.Url,
            "Folder" => ItemType.Folder,
            _ => ItemType.Executable
        };
    }
}
