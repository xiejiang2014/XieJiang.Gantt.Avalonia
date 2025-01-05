using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace XieJiang.Gantt.Avalonia;

public class DoubleMultiplicationMultiConverter : IMultiValueConverter
{
    private static DoubleMultiplicationMultiConverter? _default;
    public static  DoubleMultiplicationMultiConverter  Default => _default ??= new DoubleMultiplicationMultiConverter();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count!=2)
        {
            throw new ApplicationException("Requires 2 values to perform multiplication.");
        }

        if (values[0] is double a && values[1] is double b)
        {
            return a * b;
        }

        throw new ApplicationException("Requires 2 double values for multiplication.");
    }
}

public class DoubleMultiplicationConverter : IValueConverter
{
    private static DoubleMultiplicationConverter? _default;
    public static  DoubleMultiplicationConverter  Default => _default ??= new DoubleMultiplicationConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double a && parameter is double b)
        {
            return a * b;
        }

        throw new ApplicationException("Requires 2 double values for multiplication.");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}