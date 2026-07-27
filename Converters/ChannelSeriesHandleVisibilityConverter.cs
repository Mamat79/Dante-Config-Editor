using System.Globalization;
using System.Windows;
using System.Windows.Data;
using DanteConfigEditor.Services;

namespace DanteConfigEditor.Converters;

public sealed class ChannelSeriesHandleVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ChannelNameSeriesService.CanExtend(value as string)
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
