using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using XieJiang.Gantt.Avalonia.Controls;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Tests;

public class DateItemOffsetsTests
{
    private static ObservableCollection<DateItem> Items(params double[] widths) =>
        new(widths.Select(w => new DateItem { Width = w }));

    [Theory]
    [InlineData(0,    0, 0)]
    [InlineData(9.9,  0, 0)]
    [InlineData(10,   1, 10)]
    [InlineData(29.9, 1, 10)]
    [InlineData(59,   2, 30)]
    [InlineData(1000, 2, 30)] // past the end: the last item and its start
    public void Find_Returns_The_Item_Containing_The_Offset_And_Its_Exact_Start(double offset, int index, double position)
    {
        var offsets = new DateItemOffsets(Items(10, 20, 30));

        Assert.Equal((index, position), offsets.Find(offset));
    }

    [Fact]
    public void Find_Follows_Collection_Changes()
    {
        var items   = Items(10, 20);
        var offsets = new DateItemOffsets(items);
        Assert.Equal((1, 10d), offsets.Find(15));

        items.Insert(0, new DateItem { Width = 100 });

        Assert.Equal((0, 0d), offsets.Find(15));
    }

    [AvaloniaFact]
    public void Header_And_Grid_Stay_Aligned_After_A_Far_Scroll()
    {
        var (window, gantt, _) = TestHelper.Create(1, g => g.EndDate = new DateOnly(2026, 12, 31));
        gantt.HScrollBar!.Transitions = null;
        gantt.HScrollBar.SetCurrentValue(RangeBase.ValueProperty, 20000d);
        TestHelper.Layout(window);

        var header     = gantt.GetVisualDescendants().OfType<GanttHeader>().Single();
        var background = gantt.GetVisualDescendants().OfType<GanttBodyBackground>().Single();

        Point? X(Visual root, DateItem item) =>
            root.GetVisualDescendants().OfType<ContentPresenter>()
                .FirstOrDefault(c => ReferenceEquals(c.Content, item))?.TranslatePoint(new Point(), gantt);

        var compared = 0;
        foreach (var item in header.Row2Items)
        {
            if (X(header, item) is { } h && X(background, item) is { } b)
            {
                Assert.Equal(h.X, b.X, 3);
                compared++;
            }
        }

        Assert.True(compared > 5);
        window.Close();
    }

    [Fact]
    public void Find_On_Empty_Collection_Returns_Start()
    {
        Assert.Equal((0, 0d), new DateItemOffsets(Items()).Find(50));
    }
}
