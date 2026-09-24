using Avalonia;
using Avalonia.Headless;
using Avalonia.Input;

namespace XieJiang.Gantt.Avalonia.Tests;

public class PanTests
{
    [AvaloniaFact]
    public void Vertical_Scroll_Range_And_Thumb_Use_The_Same_Visible_Height()
    {
        var (window, gantt, _) = TestHelper.Create(100);
        var v = gantt.VScrollBar!;

        Assert.Equal(v.Bounds.Height, v.ViewportSize, 3);
        Assert.Equal(100 * gantt.RowHeight - v.Bounds.Height, v.Maximum, 3);
        window.Close();
    }

    [AvaloniaFact]
    public void Wheel_Is_Handled_Only_When_The_Chart_Scrolls()
    {
        var (window, gantt, _) = TestHelper.Create(40);
        gantt.VScrollBar!.Transitions = null;
        var bubbled = 0;
        window.AddHandler(InputElement.PointerWheelChangedEvent, (_, _) => bubbled++);
        var point = new Point(900, 300);

        window.MouseWheel(point, new Vector(0, -1)); // scroll down: the chart scrolls
        Assert.Equal(0, bubbled);

        gantt.VScrollBar.Value = 0;
        window.MouseWheel(point, new Vector(0, 1)); // already at the top: let outer scrollers have it
        Assert.Equal(1, bubbled);
        window.Close();
    }

    [AvaloniaFact]
    public void Middle_Button_Pan_Keeps_Working_Over_Task_Bars_And_Restores_The_Cursor()
    {
        var model = TestHelper.CreateModel(40);
        var (window, gantt, _) = TestHelper.Create(model);
        gantt.HScrollBar!.Transitions = null;
        gantt.VScrollBar!.Transitions = null;
        gantt.HScrollBar.Value        = 300;
        TestHelper.Layout(window);

        var cursor = new Cursor(StandardCursorType.Hand);
        gantt.Cursor = cursor;

        // start on empty background (far right of row 0), end over the bar of task 0 after scrolling back
        var start = new Point(900, 300);
        var bar   = TestHelper.TaskBar(gantt, model.GanttTasks[2]);
        var end   = bar.TranslatePoint(new Point(5, 5), window)!.Value;

        window.MouseMove(start);
        window.MouseDown(start, MouseButton.Middle);
        window.MouseMove(end, RawInputModifiers.MiddleMouseButton);
        window.MouseUp(end, MouseButton.Middle);
        TestHelper.Layout(window);

        Assert.Equal(300 - (end.X - start.X), gantt.HScrollBar.Value, 3);
        Assert.Same(cursor, gantt.Cursor);

        // released: moving no longer pans
        var value = gantt.HScrollBar.Value;
        window.MouseMove(start);
        Assert.Equal(value, gantt.HScrollBar.Value);
        window.Close();
    }
}
