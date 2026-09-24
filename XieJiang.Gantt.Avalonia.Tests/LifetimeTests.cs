using System.ComponentModel;
using System.Reflection;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Tests;

public class LifetimeTests
{
    private static int HandlerCount(GanttTask task)
    {
        var field = typeof(GanttTask).GetField(nameof(GanttTask.PropertyChanged), BindingFlags.Instance | BindingFlags.NonPublic)!;
        return (field.GetValue(task) as PropertyChangedEventHandler)?.GetInvocationList().Length ?? 0;
    }

    [AvaloniaFact]
    public void Reload_Does_Not_Leave_Old_TaskBars_Subscribed_To_Model()
    {
        var (window, gantt, model) = TestHelper.Create(1);
        var task = model.GanttTasks[0];
        var before = HandlerCount(task);

        gantt.Reload();
        gantt.Reload();
        TestHelper.Layout(window);

        Assert.Equal(before, HandlerCount(task));
        window.Close();
    }
}
