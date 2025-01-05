using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia;

namespace XieJiang.Gantt.Avalonia.Controls;

public class MilestoneHeader : TemplatedControl
{
    private Line? _line;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _line = e.NameScope.Find<Line>("PART_Line");
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (_line is not null)
        {
            _line.StartPoint = new Point(0,0);
            _line.EndPoint   = new Point(0, GetValue(GanttControl.HeaderRow2HeightProperty));
        }
    }
}