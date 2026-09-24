using Avalonia.Controls;

namespace XieJiang.Gantt.Avalonia.Tests;

public class DayWidthTests
{
    [AvaloniaFact]
    public void Changing_DayWidth_Of_Current_Mode_Relayouts_Tasks()
    {
        var (window, gantt, model) = TestHelper.Create(1);

        gantt.DayWidthInWeeklyMode = 100;
        TestHelper.Layout(window);

        var bar = TestHelper.TaskBar(gantt, model.GanttTasks[0]);
        Assert.Equal(100, Canvas.GetLeft(bar), 3);
        Assert.Equal(300, bar.Width, 3);
        window.Close();
    }

    [AvaloniaFact]
    public void DayWidth_With_Undefined_DateMode_Throws_Instead_Of_Recursing()
    {
        var gantt = new GanttControl { DateMode = (DateModes)99 };

        Assert.Throws<ArgumentOutOfRangeException>(() => gantt.DayWidth);
    }
}
