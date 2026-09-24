using System.Reflection;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Styling;
using Avalonia.Interactivity;
using XieJiang.Gantt.Avalonia.Controls;

namespace XieJiang.Gantt.Avalonia.Tests;

public class DependencyLineTests
{
    [AvaloniaFact]
    public void Temporary_Line_Theme_Is_Resolved_From_The_Control()
    {
        var theme = new ControlTheme(typeof(Line));
        var (window, gantt, _) = TestHelper.Create(1);
        window.Resources["TemporaryDependencyLine"] = theme;
        TestHelper.Layout(window);

        var line = (Line)typeof(GanttControl)
                                                  .GetField("_temporaryDependencyLine", BindingFlags.Instance | BindingFlags.NonPublic)!
                                                  .GetValue(gantt)!;
        Assert.Same(theme, line.Theme);
        window.Close();
    }

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

    [AvaloniaTheory]
    [InlineData(1, 5)] // child after parent: straight line with arrow
    [InlineData(0, 3)] // child overlapping parent: routed line with arrow
    public void Dependency_Line_Geometry_Is_Complete(int childStartDay, int childEndDay)
    {
        var model = TestHelper.CreateModel(2);
        model.GanttTasks[1].StartDate = model.GanttTasks[0].EndDate.AddDays(childStartDay - 3);
        model.GanttTasks[1].EndDate   = model.GanttTasks[0].EndDate.AddDays(childEndDay - 3);
        model.GanttTasks[0].AddingDependentTask(model.GanttTasks[1]);
        var (window, gantt, _) = TestHelper.Create(model);

        var geometry = (StreamGeometry)TestHelper.DependencyLines(gantt).Single().Data!;
        Assert.True(geometry.Bounds.Width  > 0);
        Assert.True(geometry.Bounds.Height > 0);
        window.Close();
    }

    [AvaloniaFact]
    public void Line_And_Pinout_Use_Theme_Brushes()
    {
        var lineBrush   = new SolidColorBrush(Colors.Red);
        var pinoutBrush = new SolidColorBrush(Colors.Blue);
        var model       = TestHelper.CreateModel(2);
        model.GanttTasks[0].AddingDependentTask(model.GanttTasks[1]);
        var (window, gantt, _) = TestHelper.Create(model);
        window.Resources["DependencyLine"] = lineBrush;
        window.Resources["PinoutBrush"]    = pinoutBrush;
        TestHelper.Layout(window);

        var line = TestHelper.DependencyLines(gantt).Single();
        Assert.Same(lineBrush, line.Stroke);
        Assert.Same(lineBrush, line.Fill);

        var bar = TestHelper.TaskBar(gantt, model.GanttTasks[0]);
        window.MouseMove(bar.TranslatePoint(new Point(5, 5), window)!.Value);
        TestHelper.Layout(window);
        var pinoutEllipse = gantt.GetVisualDescendants().OfType<Ellipse>().Single();
        Assert.Same(pinoutBrush, pinoutEllipse.Stroke);
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
        var line      = TestHelper.DependencyLines(gantt).Single(l => l.ChildTask == model.GanttTasks[1]);

        gantt.RemoveDependencyLine(line);

        Assert.NotNull(gantt.ReadDependencyLine(model.GanttTasks[0], model.GanttTasks[2]));
        Assert.Null(gantt.ReadDependencyLine(model.GanttTasks[0], model.GanttTasks[1]));
        Assert.DoesNotContain(model.GanttTasks[1], (IEnumerable<Models.GanttTask>)model.GanttTasks[0].Children);

        // dragging the parent must not throw
        parentBar.RaiseEvent(new RoutedEventArgs(TaskBar.MainDragDeltaEvent));
        window.Close();
    }
}
