using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace XieJiang.CommonModule.Ava;

public class DoubleSubtractiveConverter : IMultiValueConverter
{
    private static DoubleSubtractiveConverter? _default;
    public static  DoubleSubtractiveConverter  Default => _default ??= new DoubleSubtractiveConverter();

    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values?.Count != 2 || !targetType.IsAssignableFrom(typeof(double)))
            throw new NotSupportedException();

        if (values[0] is double d0 && values[1] is double d1)
        {
            return d0 - d1;
        }


        return null;
    }
}