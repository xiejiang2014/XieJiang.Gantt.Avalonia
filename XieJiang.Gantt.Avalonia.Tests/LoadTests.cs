using Avalonia.VisualTree;
using XieJiang.Gantt.Avalonia.Controls;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Tests;

public class LoadTests
{
    [AvaloniaFact]
    public void DataContext_Set_Before_Template_Loads_Everything_Without_Manual_Reload()
    {
        var model = TestHelper.CreateModel(2);
        model.Milestones.Add(new Milestone { DateTime = new DateTime(2025, 1, 10) });

        var (window, gantt, _) = TestHelper.Create(model, g => g.DateTimeNow = new DateTime(2025, 1, 11));

        var header = gantt.GetVisualDescendants().OfType<GanttHeader>().Single();
        Assert.NotEmpty(header.Row1Items);
        Assert.NotEmpty(header.Row2Items);
        Assert.Single(gantt.GetVisualDescendants().OfType<MilestoneControl>());

        var markLine = gantt.GetVisualDescendants().OfType<MarkLineToday>().Single();
        Assert.Equal(10 * gantt.DayWidth, markLine.Margin.Left, 3);
        window.Close();
    }
}
