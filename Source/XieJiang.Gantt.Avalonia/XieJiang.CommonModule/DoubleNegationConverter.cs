using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace XieJiang.CommonModule.Ava;

public class DoubleNegationConverter : IValueConverter
{
    private static DoubleNegationConverter? _default;
    public static  DoubleNegationConverter  Default => _default ??= new DoubleNegationConverter();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double b)
        {
            return -b;
        }

        throw new ArgumentException("value is not double");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double b)
        {
            return -b;
        }

        throw new ArgumentException("value is not double");
    }
}