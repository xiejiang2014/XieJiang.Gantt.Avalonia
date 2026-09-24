using System.Diagnostics;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Tests;

public class GanttTaskTests
{
    // Builds top -> (a, b) -> joint -> (a, b) -> joint ... which has 2^layers paths to the bottom.
    private static (GanttTask top, GanttTask bottom) Diamonds(int layers)
    {
        var top    = new GanttTask();
        var bottom = top;
        for (var i = 0; i < layers; i++)
        {
            var a     = new GanttTask();
            var b     = new GanttTask();
            var joint = new GanttTask();
            bottom.AddingDependentTask(a);
            bottom.AddingDependentTask(b);
            a.AddingDependentTask(joint);
            b.AddingDependentTask(joint);
            bottom = joint;
        }

        return (top, bottom);
    }

    [Fact]
    public void Circular_Check_Is_Not_Exponential_On_Diamond_Dependencies()
    {
        var (top, bottom) = Diamonds(20);

        var sw = Stopwatch.StartNew();
        Assert.False(bottom.CanAddDependentTask(top));
        Assert.True(bottom.CanAddDependentTask(new GanttTask())); // walks all ancestors
        sw.Stop();

        Assert.True(sw.ElapsedMilliseconds < 500, $"took {sw.ElapsedMilliseconds} ms");
    }

    [Fact]
    public void AddingDependentTask_Rejects_Self_Duplicate_And_Cycle()
    {
        var a = new GanttTask();
        var b = new GanttTask();

        Assert.True(a.AddingDependentTask(b));
        Assert.False(a.AddingDependentTask(b));
        Assert.False(b.AddingDependentTask(a));
        Assert.False(a.AddingDependentTask(a));

        Assert.Empty(a.Parents);
        Assert.Empty(b.Children);
    }

    [Fact]
    public void RemoveDependentTask_Returns_Whether_Something_Was_Removed()
    {
        var a = new GanttTask();
        var b = new GanttTask();
        a.AddingDependentTask(b);

        Assert.True(a.RemoveDependentTask(b));
        Assert.False(a.RemoveDependentTask(b));
    }

    [Fact]
    public void Traversal_Yields_Each_Task_Once()
    {
        var (top, bottom) = Diamonds(3);

        Assert.Equal(9, top.TraversingChildren(top).Count());
        Assert.Equal(9, bottom.TraversingAncestors(bottom).Count());
    }
}
