using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia;
using Avalonia.Controls.Metadata;

namespace XieJiang.Gantt.Avalonia.Controls;

[TemplatePart("PART_HeaderButton", typeof(Button))]
[TemplatePart("PART_Line",         typeof(Line))]
public class MilestoneHeader : TemplatedControl
{
    private Line?   _line;
    private Button? _headerButton;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _headerButton = e.NameScope.Find<Button>("PART_HeaderButton");
        _line         = e.NameScope.Find<Line>("PART_Line");

        if (_headerButton?.Flyout is not null)
        {
            _headerButton.Flyout.Opened += Flyout_Opened;
        }
    }

    private void Flyout_Opened(object? sender, System.EventArgs e)
    {
        //_headerButton.Flyout.
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (_line is not null)
        {
            _line.StartPoint = new Point(0, 0);
            _line.EndPoint   = new Point(0, GetValue(GanttControl.HeaderRow2HeightProperty));
        }
    }

    internal void ShowFlyout()
    {
        if (_headerButton is not null)
        {
            //await Task.Yield();

            //_headerButton.Focus();

            _headerButton.Flyout?.ShowAt(_headerButton);

            //_headerButton.Flyout?.

            //FlyoutBase.ShowAttachedFlyout(_headerButton);
        }
    }
}