using Avalonia;
using Avalonia.Headless;
using Avalonia.Input;

namespace XieJiang.Gantt.Avalonia.Tests;

public class PanTests
{
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
