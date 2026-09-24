using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using XieJiang.CommonModule.Ava;

namespace XieJiang.Gantt.Avalonia.Tests;

public class ConverterTests
{
    private static readonly Dictionary<string, IValueConverter> ValueConverters = new()
    {
        [nameof(DoubleMultiplicationConverter)] = DoubleMultiplicationConverter.Default,
        [nameof(DoubleNegationConverter)]       = DoubleNegationConverter.Default,
        [nameof(GanttColorToBrushConverter)]    = GanttColorToBrushConverter.Default
    };

    private static readonly Dictionary<string, IMultiValueConverter> MultiValueConverters = new()
    {
        [nameof(DoubleMultiplicationMultiConverter)] = DoubleMultiplicationMultiConverter.Default,
        [nameof(DoubleSubtractiveConverter)]         = DoubleSubtractiveConverter.Default
    };

    [AvaloniaTheory]
    [InlineData(nameof(DoubleMultiplicationConverter))]
    [InlineData(nameof(DoubleNegationConverter))]
    [InlineData(nameof(GanttColorToBrushConverter))]
    public void Value_Converters_Return_UnsetValue_For_Unexpected_Input(string name)
    {
        var converter = ValueConverters[name];
        Assert.Same(AvaloniaProperty.UnsetValue, converter.Convert(AvaloniaProperty.UnsetValue, typeof(double), null, CultureInfo.InvariantCulture));
    }

    [AvaloniaTheory]
    [InlineData(nameof(DoubleMultiplicationMultiConverter))]
    [InlineData(nameof(DoubleSubtractiveConverter))]
    public void Multi_Value_Converters_Return_UnsetValue_For_Unexpected_Input(string name)
    {
        var converter = MultiValueConverters[name];
        Assert.Same(AvaloniaProperty.UnsetValue, converter.Convert([AvaloniaProperty.UnsetValue, 1d], typeof(double), null, CultureInfo.InvariantCulture));
        Assert.Same(AvaloniaProperty.UnsetValue, converter.Convert([1d], typeof(double), null, CultureInfo.InvariantCulture));
    }

    [AvaloniaFact]
    public void Color_Converter_Still_Resolves_Brushes()
    {
        Assert.NotNull(GanttColorToBrushConverter.Default.Convert(GanttColors.Danger, typeof(object), null, CultureInfo.InvariantCulture) as global::Avalonia.Media.IBrush);
    }
}
