using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace XieJiang.Gantt.Avalonia;

internal class GanttColorToBrushConverter : IValueConverter
{
    private static GanttColorToBrushConverter? _default;
    public static  GanttColorToBrushConverter  Default => _default ??= new GanttColorToBrushConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var key = "BrushPrimary";
        if (value is GanttColors c)
        {
            key = c switch
                  {
                      GanttColors.Primary     => "BrushPrimary",
                      GanttColors.Secondary   => "BrushSecondary",
                      GanttColors.Tertiary    => "BrushTertiary",
                      GanttColors.Information => "BrushInformation",
                      GanttColors.Success     => "BrushSuccess",
                      GanttColors.Warning     => "BrushWarning",
                      GanttColors.Danger      => "BrushDanger",
                      _                       => "BrushPrimary"
                  };
        }

        if (Application.Current.TryGetResource(key, null, out var r))
        {
            return r;
        }

        throw new ApplicationException($"Brush '{key}' not found.");
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

internal class GanttColorToBoolConverter : IValueConverter
{
    private static GanttColorToBoolConverter? _default;
    public static  GanttColorToBoolConverter  Default => _default ??= new GanttColorToBoolConverter();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is GanttColors c && parameter is GanttColors c2)
        {
            return c == c2;
        }

        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}