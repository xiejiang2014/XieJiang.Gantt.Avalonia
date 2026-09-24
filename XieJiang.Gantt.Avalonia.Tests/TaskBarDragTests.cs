using Avalonia.Controls;
using Avalonia.Interactivity;
using XieJiang.Gantt.Avalonia.Controls;

namespace XieJiang.Gantt.Avalonia.Tests;

public class TaskBarDragTests
{
    [AvaloniaFact]
    public void MainDrag_Of_Task_Shorter_Than_MinWidth_Keeps_Duration()
    {
        var (window, gantt, model) = TestHelper.Create(1, g => g.DateMode = DateModes.Yearly);
        var task     = model.GanttTasks[0];
        var duration = task.DateLength;
        var bar      = TestHelper.TaskBar(gantt, task);

        Canvas.SetLeft(bar, Canvas.GetLeft(bar) + gantt.DayWidth * 2);
        bar.RaiseEvent(new RoutedEventArgs(TaskBar.MainDragDeltaEvent));
        bar.RaiseEvent(new RoutedEventArgs(TaskBar.MainDragCompletedEvent));

        Assert.Equal(duration, task.DateLength);
        window.Close();
    }
}
