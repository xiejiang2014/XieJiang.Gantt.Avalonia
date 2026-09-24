using Avalonia.Interactivity;
using XieJiang.Gantt.Avalonia.Controls;

namespace XieJiang.Gantt.Avalonia.Tests;

public class DependencyLineTests
{
    [AvaloniaFact]
    public void Dependency_To_Task_Outside_Model_Is_Ignored()
    {
        var model = TestHelper.CreateModel(1);
        model.GanttTasks[0].AddingDependentTask(new Models.GanttTask());
        new Models.GanttTask().AddingDependentTask(model.GanttTasks[0]);

        var (window, gantt, _) = TestHelper.Create(model);

        Assert.Single(TestHelper.TaskBars(gantt));
        Assert.Empty(TestHelper.DependencyLines(gantt));
        window.Close();
    }

    [AvaloniaFact]
    public void RemoveDependencyLine_Keeps_Sibling_Lines_Of_Same_Parent()
    {
        var model = TestHelper.CreateModel(3);
        model.GanttTasks[0].AddingDependentTask(model.GanttTasks[1]);
        model.GanttTasks[0].AddingDependentTask(model.GanttTasks[2]);
        var (window, gantt, _) = TestHelper.Create(model);

        var parentBar = TestHelper.TaskBar(gantt, model.GanttTasks[0]);
        var line      = TestHelper.DependencyLines(gantt).Single(l => ReferenceEquals(l.ChildTaskBar.DataContext, model.GanttTasks[1]));

        gantt.RemoveDependencyLine(line);

        Assert.NotNull(gantt.ReadDependencyLine(parentBar, TestHelper.TaskBar(gantt, model.GanttTasks[2])));
        Assert.Null(gantt.ReadDependencyLine(parentBar, TestHelper.TaskBar(gantt, model.GanttTasks[1])));
        Assert.DoesNotContain(model.GanttTasks[1], (IEnumerable<Models.GanttTask>)model.GanttTasks[0].Children);

        // dragging the parent must not throw
        parentBar.RaiseEvent(new RoutedEventArgs(TaskBar.MainDragDeltaEvent));
        window.Close();
    }
}
