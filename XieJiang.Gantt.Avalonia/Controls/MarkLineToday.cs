using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;

namespace XieJiang.Gantt.Avalonia.Controls;

public class MarkLineToday : TemplatedControl
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
            _line.EndPoint = new Point(0, e.NewSize.Height);
        }
    }
}