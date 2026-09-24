using Avalonia.Controls.Primitives;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Tests;

public class VirtualizationTests
{
    [AvaloniaFact]
    public void Only_Visible_Rows_Get_TaskBars()
    {
        var (window, gantt, model) = TestHelper.Create(1000);

        var bars = TestHelper.TaskBars(gantt);
        Assert.InRange(bars.Count, 1, 50);
        Assert.Contains(bars, b => ReferenceEquals(b.DataContext, model.GanttTasks[0]));
        window.Close();
    }

    [AvaloniaFact]
    public void Scrolling_Realizes_The_Newly_Visible_Rows()
    {
        var model = TestHelper.CreateModel(1000);
        foreach (var task in model.GanttTasks)
        {
            task.StartDate = new DateTime(2025, 1, 2);
            task.EndDate   = new DateTime(2025, 1, 5);
        }

        var (window, gantt, _) = TestHelper.Create(model);
        var row = 500;

        gantt.VScrollBar!.Transitions = null;
        gantt.VScrollBar.SetCurrentValue(RangeBase.ValueProperty, row * gantt.RowHeight);
        TestHelper.Layout(window);

        var bars = TestHelper.TaskBars(gantt);
        Assert.Contains(bars, b => ReferenceEquals(b.DataContext, model.GanttTasks[row]));
        Assert.DoesNotContain(bars, b => ReferenceEquals(b.DataContext, model.GanttTasks[0]));
        window.Close();
    }

    [AvaloniaFact]
    public void Line_Between_Offscreen_Tasks_Crossing_The_View_Is_Shown()
    {
        var model = TestHelper.CreateModel(1000);
        model.GanttTasks[0].AddingDependentTask(model.GanttTasks[999]);
        var (window, gantt, _) = TestHelper.Create(model, g => g.DateMode = DateModes.Yearly);

        gantt.VScrollBar!.Transitions = null;
        gantt.VScrollBar.SetCurrentValue(RangeBase.ValueProperty, 500 * gantt.RowHeight);
        TestHelper.Layout(window);

        Assert.Single(TestHelper.DependencyLines(gantt));
        window.Close();
    }

    [AvaloniaFact]
    public void Line_Follows_Date_Change_Of_A_Task_Without_Bar()
    {
        var model = TestHelper.CreateModel(1000);
        model.GanttTasks[0].AddingDependentTask(model.GanttTasks[999]);
        var (window, gantt, _) = TestHelper.Create(model, g => g.DateMode = DateModes.Yearly);
        var line   = TestHelper.DependencyLines(gantt).Single();
        var bounds = line.Data!.Bounds;

        model.GanttTasks[999].StartDate = model.GanttTasks[999].StartDate.AddDays(30);
        model.GanttTasks[999].EndDate   = model.GanttTasks[999].EndDate.AddDays(30);
        TestHelper.Layout(window);

        Assert.NotEqual(bounds, line.Data!.Bounds);
        window.Close();
    }

    [AvaloniaFact]
    public void ScrollToTask_Works_For_A_Task_Without_Bar()
    {
        var (window, gantt, model) = TestHelper.Create(1000);
        gantt.HScrollBar!.Transitions = null;
        var task = new GanttTask { StartDate = new DateTime(2025, 2, 20), EndDate = new DateTime(2025, 2, 21) };
        model.GanttTasks.Add(task);
        TestHelper.Layout(window);

        gantt.ScrollToTask(task);

        Assert.True(gantt.HScrollBar.Value > 0);
        window.Close();
    }
}
