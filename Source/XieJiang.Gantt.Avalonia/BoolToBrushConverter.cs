using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Globalization;
using System;

namespace XieJiang.Gantt.Avalonia;

public class BoolToBrushConverter : IValueConverter
{
    public IBrush? TrueBrush  { get; set; }
    public IBrush? FalseBrush { get; set; }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
        {
            return b
                ? TrueBrush
                : FalseBrush;
        }

        return FalseBrush;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}