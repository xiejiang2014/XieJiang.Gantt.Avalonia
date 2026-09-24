using XieJiang.Gantt.Avalonia.Controls;

namespace XieJiang.Gantt.Avalonia.Tests;

public class MultiInstanceTests
{
    [AvaloniaFact]
    public void Header_And_Background_Collections_Are_Not_Shared_Between_Instances()
    {
        var h1 = new GanttHeader();
        var h2 = new GanttHeader();
        var b1 = new GanttBodyBackground();
        var b2 = new GanttBodyBackground();

        Assert.NotSame(h1.Row1Items, h2.Row1Items);
        Assert.NotSame(h1.Row2Items, h2.Row2Items);
        Assert.NotSame(b1.DateItems, b2.DateItems);
    }
}
