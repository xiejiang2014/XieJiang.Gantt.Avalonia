namespace XieJiang.Gantt.Avalonia.Tests;

public class DayWidthTests
{
    [AvaloniaFact]
    public void DayWidth_With_Undefined_DateMode_Throws_Instead_Of_Recursing()
    {
        var gantt = new GanttControl { DateMode = (DateModes)99 };

        Assert.Throws<ArgumentOutOfRangeException>(() => gantt.DayWidth);
    }
}
