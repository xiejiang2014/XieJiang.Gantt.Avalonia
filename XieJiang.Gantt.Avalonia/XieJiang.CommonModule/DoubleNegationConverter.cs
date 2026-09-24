using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace XieJiang.CommonModule.Ava;

public class DoubleNegationConverter : IValueConverter
{
    private static DoubleNegationConverter? _default;
    public static  DoubleNegationConverter  Default => _default ??= new DoubleNegationConverter();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is double b ? -b : AvaloniaProperty.UnsetValue;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is double b ? -b : AvaloniaProperty.UnsetValue;
}