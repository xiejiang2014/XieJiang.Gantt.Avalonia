using Avalonia.Headless.XUnit;

namespace XieJiang.Gantt.Avalonia.Tests;

public class SmokeTests
{
    [AvaloniaFact]
    public void GanttControl_Renders_Tasks()
    {
        var (window, gantt, _) = TestHelper.Create(3);

        Assert.Equal(3, TestHelper.TaskBars(gantt).Count);
        window.Close();
    }
}
