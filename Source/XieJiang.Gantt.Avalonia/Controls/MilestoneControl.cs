using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;

namespace XieJiang.Gantt.Avalonia.Controls;

[TemplatePart("PART_HeaderButton",          typeof(Button))]
[TemplatePart("PART_MilestoneDeleteButton", typeof(Button))]
[TemplatePart("PART_Line",                  typeof(Line))]
public class MilestoneControl : TemplatedControl
{
    private Line?   _line;
    private Button? _headerButton;
    private Button? _milestoneDeleteButton;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _headerButton          = e.NameScope.Find<Button>("PART_HeaderButton");
        _milestoneDeleteButton = e.NameScope.Find<Button>("PART_MilestoneDeleteButton");
        _line                  = e.NameScope.Find<Line>("PART_Line");

        if (_headerButton?.Flyout is not null)
        {
            _headerButton.Flyout.Opened += Flyout_Opened;
        }

        if (_milestoneDeleteButton is not null)
        {
            _milestoneDeleteButton.Click += MilestoneDeleteButton_Click;
        }
    }
    

    private void MilestoneDeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        var args = new RoutedEventArgs(Button.ClickEvent);
        using var route = BuildEventRoute(Button.ClickEvent);
        route.RaiseEvent(_milestoneDeleteButton, args);
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
            _line.EndPoint   = new Point(0, Bounds.Height);
        }
    }

    internal void ShowFlyout()
    {
        if (_headerButton is not null)
        {
            _headerButton.Flyout?.ShowAt(_headerButton);
        }
    }
}