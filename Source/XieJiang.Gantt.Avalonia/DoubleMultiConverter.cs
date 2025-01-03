using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace XieJiang.Gantt.Avalonia;

public class DoubleMultiConverter : IMultiValueConverter
{
    private static DoubleMultiConverter? _default;
    public static  DoubleMultiConverter  Default => _default ??= new DoubleMultiConverter();

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