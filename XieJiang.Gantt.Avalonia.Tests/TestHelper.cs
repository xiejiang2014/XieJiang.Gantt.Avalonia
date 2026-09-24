using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using XieJiang.Gantt.Avalonia.Controls;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Tests;

internal static class TestHelper
{
    public static readonly DateOnly Start = new(2025, 1, 1);

    public static GanttModel CreateModel(int taskCount)
    {
        var model = new GanttModel();
        for (var i = 0; i < taskCount; i++)
        {
            model.GanttTasks.Add(new GanttTask
                                 {
                                     Id        = i,
                                     StartDate = new DateTime(2025, 1, 2).AddDays(i),
                                     EndDate   = new DateTime(2025, 1, 5).AddDays(i),
                                 });
        }

        return model;
    }

    public static (Window window, GanttControl gantt, GanttModel model) Create(int taskCount, Action<GanttControl>? configure = null) =>
        Create(CreateModel(taskCount), configure);

    public static (Window window, GanttControl gantt, GanttModel model) Create(GanttModel model, Action<GanttControl>? configure = null)
    {
        var gantt = new GanttControl
                    {
                        StartDate   = Start,
                        EndDate     = new DateOnly(2025, 3, 1),
                        DataContext = model
                    };
        configure?.Invoke(gantt);

        var window = new Window { Width = 1000, Height = 600, Content = gantt };
        window.Show();
        Layout(window);
        return (window, gantt, model);
    }

    public static void Layout(Window window)
    {
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
        Dispatcher.UIThread.RunJobs();
    }

    public static List<TaskBar> TaskBars(GanttControl gantt) =>
        gantt.GetVisualDescendants().OfType<TaskBar>().ToList();

    public static TaskBar TaskBar(GanttControl gantt, GanttTask task) =>
        TaskBars(gantt).Single(t => ReferenceEquals(t.DataContext, task));

    public static List<DependencyLine> DependencyLines(GanttControl gantt) =>
        gantt.GetVisualDescendants().OfType<DependencyLine>().ToList();
}
