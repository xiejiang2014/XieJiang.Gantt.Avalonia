using Avalonia.Controls;
using Avalonia.VisualTree;
using XieJiang.Gantt.Avalonia.Controls;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Tests;

public class ModelSyncTests
{
    [AvaloniaFact]
    public void Changing_Task_Dates_Moves_The_Bar_And_Its_Lines()
    {
        var model = TestHelper.CreateModel(2);
        model.GanttTasks[0].AddingDependentTask(model.GanttTasks[1]);
        var (window, gantt, _) = TestHelper.Create(model);
        var bar  = TestHelper.TaskBar(gantt, model.GanttTasks[1]);
        var line = TestHelper.DependencyLines(gantt).Single();
        var lineBounds = line.Data!.Bounds;

        model.GanttTasks[1].StartDate = new DateTime(2025, 1, 21);
        model.GanttTasks[1].EndDate   = new DateTime(2025, 1, 31);
        TestHelper.Layout(window);

        Assert.Equal(20 * gantt.DayWidth, Canvas.GetLeft(bar), 3);
        Assert.Equal(10 * gantt.DayWidth, bar.Width, 3);
        Assert.NotEqual(lineBounds, line.Data!.Bounds);
        window.Close();
    }

    [AvaloniaFact]
    public void Adding_And_Removing_Tasks_Updates_The_Control()
    {
        var (window, gantt, model) = TestHelper.Create(2);

        model.GanttTasks.Add(new GanttTask { StartDate = new DateTime(2025, 1, 3), EndDate = new DateTime(2025, 1, 4) });
        TestHelper.Layout(window);
        Assert.Equal(3, TestHelper.TaskBars(gantt).Count);

        model.GanttTasks.RemoveAt(0);
        model.GanttTasks.RemoveAt(0);
        TestHelper.Layout(window);
        Assert.Single(TestHelper.TaskBars(gantt));
        window.Close();
    }

    [AvaloniaFact]
    public void Adding_And_Removing_Milestones_Updates_The_Control()
    {
        var (window, gantt, model) = TestHelper.Create(1);
        var milestone = new Milestone { DateTime = new DateTime(2025, 1, 10) };

        model.Milestones.Add(milestone);
        TestHelper.Layout(window);
        Assert.Single(gantt.GetVisualDescendants().OfType<MilestoneControl>());

        model.Milestones.Remove(milestone);
        TestHelper.Layout(window);
        Assert.Empty(gantt.GetVisualDescendants().OfType<MilestoneControl>());
        window.Close();
    }

    [AvaloniaFact]
    public void Replaced_Model_Is_No_Longer_Observed()
    {
        var (window, gantt, model) = TestHelper.Create(1);
        gantt.GanttModel = TestHelper.CreateModel(2);
        TestHelper.Layout(window);

        model.GanttTasks.Add(new GanttTask());
        TestHelper.Layout(window);

        Assert.Equal(2, TestHelper.TaskBars(gantt).Count);
        window.Close();
    }
}
