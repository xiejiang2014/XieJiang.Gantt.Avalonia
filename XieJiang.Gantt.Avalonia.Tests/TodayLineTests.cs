using System.Reflection;
using Avalonia.Threading;

namespace XieJiang.Gantt.Avalonia.Tests;

public class TodayLineTests
{
    private static DispatcherTimer? NowTimer(GanttControl gantt) =>
        typeof(GanttControl).GetField("_nowTimer", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(gantt) as DispatcherTimer;

    [AvaloniaFact]
    public void Today_Line_Is_Refreshed_Periodically_Only_While_Attached()
    {
        var (window, gantt, _) = TestHelper.Create(1);
        Assert.True(NowTimer(gantt)?.IsEnabled);

        window.Content = null;
        Assert.False(NowTimer(gantt)!.IsEnabled);
        window.Close();
    }
}
