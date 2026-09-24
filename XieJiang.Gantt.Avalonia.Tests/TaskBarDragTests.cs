using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using XieJiang.Gantt.Avalonia.Controls;

namespace XieJiang.Gantt.Avalonia.Tests;

public class TaskBarDragTests
{
    private const int Steps = 5;
    private const int Step  = 10;

    // Drags with real pointer input in several small steps, like a user would.
    private static void Drag(Window window, Visual thumb)
    {
        var start = thumb.TranslatePoint(new Point(thumb.Bounds.Width / 2, thumb.Bounds.Height / 2), window)!.Value;

        window.MouseMove(start);
        window.MouseDown(start, MouseButton.Left);
        for (var i = 1; i <= Steps; i++)
        {
            window.MouseMove(start + new Point(i * Step, 0));
            TestHelper.Layout(window);
        }

        window.MouseUp(start + new Point(Steps * Step, 0), MouseButton.Left);
        TestHelper.Layout(window);
    }

    private static Thumb Part(TaskBar bar, string name) =>
        bar.GetVisualDescendants().OfType<Thumb>().Single(t => t.Name == name);

    private static (Window, TaskBar) CreateHovered(DragUnits dragUnit = DragUnits.Free)
    {
        var (window, gantt, model) = TestHelper.Create(1, g => g.DragUnit = dragUnit);
        var bar = TestHelper.TaskBar(gantt, model.GanttTasks[0]);

        // hover to make the edge and progress thumbs visible
        window.MouseMove(bar.TranslatePoint(new Point(bar.Bounds.Width / 2, 5), window)!.Value);
        TestHelper.Layout(window);
        return (window, bar);
    }

    [AvaloniaFact]
    public void Daily_Width_Drag_That_Rounds_To_Same_Date_Snaps_Bar_Back()
    {
        var (window, gantt, model) = TestHelper.Create(1, g => g.DragUnit = DragUnits.Daily);
        var bar   = TestHelper.TaskBar(gantt, model.GanttTasks[0]);
        var width = bar.Width;

        bar.Width = width + gantt.DayWidth * 0.2;
        bar.RaiseEvent(new WidthDragEventArgs(TaskBar.WidthDragCompletedEvent) { Edge = Edges.Right });

        Assert.Equal(width, bar.Width, 3);
        window.Close();
    }

    [AvaloniaTheory]
    [InlineData(11.9, 0)]
    [InlineData(12.5, 1)]
    [InlineData(12.99, 1)]
    public void Daily_Drag_Rounds_To_Nearest_Day(double hours, int expectedExtraDays)
    {
        var (window, gantt, model) = TestHelper.Create(1, g => g.DragUnit = DragUnits.Daily);
        var task  = model.GanttTasks[0];
        var start = task.StartDate;
        var bar   = TestHelper.TaskBar(gantt, task);

        Canvas.SetLeft(bar, Canvas.GetLeft(bar) + gantt.DayWidth * hours / 24);
        bar.RaiseEvent(new RoutedEventArgs(TaskBar.MainDragDeltaEvent));
        bar.RaiseEvent(new RoutedEventArgs(TaskBar.MainDragCompletedEvent));

        Assert.Equal(start.AddDays(expectedExtraDays), task.StartDate);
        window.Close();
    }

    [AvaloniaFact]
    public void Main_Thumb_Moves_Bar_By_Pointer_Distance()
    {
        var (window, bar) = CreateHovered();
        var left = Canvas.GetLeft(bar);

        Drag(window, Part(bar, "PART_TaskMainThumb"));

        Assert.Equal(left + Steps * Step, Canvas.GetLeft(bar), 3);
        window.Close();
    }

    [AvaloniaFact]
    public void Right_Thumb_Grows_Bar_By_Pointer_Distance()
    {
        var (window, bar) = CreateHovered();
        var width = bar.Width;

        Drag(window, Part(bar, "PART_RThumb"));

        Assert.Equal(width + Steps * Step, bar.Width, 3);
        window.Close();
    }

    [AvaloniaFact]
    public void Left_Thumb_Shrinks_Bar_By_Pointer_Distance()
    {
        var (window, bar) = CreateHovered();
        var width = bar.Width;
        var left  = Canvas.GetLeft(bar);

        Drag(window, Part(bar, "PART_LThumb"));

        Assert.Equal(width - Steps * Step, bar.Width, 3);
        Assert.Equal(left  + Steps * Step, Canvas.GetLeft(bar), 3);
        window.Close();
    }

    [AvaloniaFact]
    public void Progress_Thumb_Moves_By_Pointer_Distance()
    {
        var (window, bar) = CreateHovered();
        var progressThumb = Part(bar, "PART_ProgressThumb");
        var left          = Canvas.GetLeft(progressThumb);

        Drag(window, progressThumb);

        Assert.Equal(left + Steps * Step, Canvas.GetLeft(progressThumb), 3);
        window.Close();
    }

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
