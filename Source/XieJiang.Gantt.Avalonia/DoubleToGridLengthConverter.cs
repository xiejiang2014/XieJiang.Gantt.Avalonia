using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace XieJiang.Gantt.Avalonia;

public class DoubleToGridLengthConverter : IValueConverter
{
    private static DoubleToGridLengthConverter? _default;
    public static  DoubleToGridLengthConverter  Default => _default ??= new DoubleToGridLengthConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double b)
        {
            return new GridLength(b);
        }

        return new GridLength(1, GridUnitType.Auto);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}