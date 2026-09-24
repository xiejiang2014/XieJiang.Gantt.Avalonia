using Avalonia.VisualTree;
using XieJiang.Gantt.Avalonia.Controls;

namespace XieJiang.Gantt.Avalonia.Tests;

public class DateRangeTests
{
    [AvaloniaFact]
    public void EndDate_Before_StartDate_Is_Coerced_And_Restored()
    {
        var gantt = new GanttControl { EndDate = new DateOnly(2025, 1, 1), StartDate = new DateOnly(2025, 3, 1) };
        Assert.Equal(new DateOnly(2025, 3, 1), gantt.EndDate);

        gantt.StartDate = new DateOnly(2024, 1, 1);
        Assert.Equal(new DateOnly(2025, 1, 1), gantt.EndDate);
    }

    [AvaloniaTheory]
    [InlineData(DateModes.Weekly)]
    [InlineData(DateModes.Monthly)]
    [InlineData(DateModes.Seasonally)]
    [InlineData(DateModes.Yearly)]
    public void Header_Items_Never_Have_Negative_Width(DateModes dateMode)
    {
        var (window, gantt, _) = TestHelper.Create(1, g =>
                                                       {
                                                           g.DateMode  = dateMode;
                                                           g.StartDate = new DateOnly(2025, 3, 1);
                                                           g.EndDate   = new DateOnly(2025, 1, 1);
                                                       });

        var header = gantt.GetVisualDescendants().OfType<GanttHeader>().Single();
        Assert.All(header.Row1Items.Concat(header.Row2Items), item => Assert.True(item.Width >= 0));
        window.Close();
    }
}
