using Avalonia.VisualTree;
using XieJiang.Gantt.Avalonia.Controls;

namespace XieJiang.Gantt.Avalonia.Tests;

public class ReloadPerformanceTests
{
    [AvaloniaFact]
    public void Reload_Replaces_Date_Items_Instead_Of_Adding_One_By_One()
    {
        var (window, gantt, _) = TestHelper.Create(1);
        var header     = gantt.GetVisualDescendants().OfType<GanttHeader>().Single();
        var background = gantt.GetVisualDescendants().OfType<GanttBodyBackground>().Single();

        var notifications = 0;
        header.Row1Items.CollectionChanged     += (_, _) => notifications++;
        header.Row2Items.CollectionChanged     += (_, _) => notifications++;
        background.DateItems.CollectionChanged += (_, _) => notifications++;

        gantt.Reload();

        Assert.Equal(0, notifications);
        Assert.Equal(60, header.Row2Items.Count); // 2025-01-01 .. 2025-03-01
        Assert.Equal(header.Row2Items, background.DateItems);
        window.Close();
    }
}
