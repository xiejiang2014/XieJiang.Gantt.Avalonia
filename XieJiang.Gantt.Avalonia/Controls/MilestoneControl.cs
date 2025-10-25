using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Interactivity;
using System;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Controls;

[TemplatePart("PART_HeaderButton",                typeof(Button))]
[TemplatePart("PART_MilestoneDeleteButton",       typeof(Button))]
[TemplatePart("PART_Line",                        typeof(Line))]
[TemplatePart("PART_PanThumb",                    typeof(Thumb))]
[TemplatePart("PART_ColorRadioButtonsStackPanel", typeof(Panel))]
public class MilestoneControl : TemplatedControl
{
    private Line?   _line;
    private Button? _headerButton;
    private Button? _milestoneDeleteButton;
    private Thumb?  _panThumb;
    private Panel?  _colorRadioButtonsStackPanel;

    private Milestone? _milestone;

    static MilestoneControl()
    {
    }

    public MilestoneControl()
    {
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        _milestone = DataContext as Milestone;

        base.OnDataContextChanged(e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _headerButton                = e.NameScope.Find<Button>("PART_HeaderButton");
        _milestoneDeleteButton       = e.NameScope.Find<Button>("PART_MilestoneDeleteButton");
        _line                        = e.NameScope.Find<Line>("PART_Line");
        _panThumb                    = e.NameScope.Find<Thumb>("PART_PanThumb");
        _colorRadioButtonsStackPanel = e.NameScope.Find<Panel>("PART_ColorRadioButtonsStackPanel");


        if (_headerButton?.Flyout is not null)
        {
            _headerButton.Flyout.Opened += Flyout_Opened;
        }

        if (_colorRadioButtonsStackPanel is not null)
        {
            _colorRadioButtonsStackPanel.AddHandler(ToggleButton.IsCheckedChangedEvent, ToggleButtonIsCheckedChanged, RoutingStrategies.Bubble);
        }


        if (_milestoneDeleteButton is not null)
        {
            _milestoneDeleteButton.Click += MilestoneDeleteButton_Click;
        }

        if (_panThumb is not null)
        {
            _panThumb.DragStarted   += PanThumb_DragStarted;
            _panThumb.DragDelta     += PanThumb_DragDelta;
            _panThumb.DragCompleted += PanThumb_DragCompleted;
        }
    }


    private void MilestoneDeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        var       args  = new RoutedEventArgs(Button.ClickEvent);
        using var route = BuildEventRoute(Button.ClickEvent);
        route.RaiseEvent(_milestoneDeleteButton!, args);
    }

    private void Flyout_Opened(object? sender, EventArgs e)
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

    #region HPan

    #region HThumbDragDelta

    public static readonly RoutedEvent<RoutedEventArgs> HThumbDragDeltaEvent =
        RoutedEvent.Register<MilestoneControl, RoutedEventArgs>(nameof(HThumbDragDelta), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs> HThumbDragDelta
    {
        add => AddHandler(HThumbDragDeltaEvent, value);
        remove => RemoveHandler(HThumbDragDeltaEvent, value);
    }

    protected virtual void OnHThumbDragDelta()
    {
        RoutedEventArgs args = new RoutedEventArgs(HThumbDragDeltaEvent);
        RaiseEvent(args);
    }

    #endregion

    #region HThumbDragCompleted

    public static readonly RoutedEvent<RoutedEventArgs> HThumbDragCompletedEvent =
        RoutedEvent.Register<MilestoneControl, RoutedEventArgs>(nameof(HThumbDragCompleted), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs> HThumbDragCompleted
    {
        add => AddHandler(HThumbDragCompletedEvent, value);
        remove => RemoveHandler(HThumbDragCompletedEvent, value);
    }

    protected virtual void OnHThumbDragCompleted()
    {
        RoutedEventArgs args = new RoutedEventArgs(HThumbDragCompletedEvent);
        RaiseEvent(args);
    }

    #endregion

    private double _leftDragStarted;

    private void PanThumb_DragStarted(object? sender, global::Avalonia.Input.VectorEventArgs e)
    {
        _leftDragStarted = Margin.Left;

        e.Handled = true;
    }

    private void PanThumb_DragDelta(object? sender, global::Avalonia.Input.VectorEventArgs e)
    {
        var newLeft = _leftDragStarted + e.Vector.X;
        Margin = new Thickness(newLeft, 0, 0, 0);

        _leftDragStarted = newLeft;

        OnHThumbDragDelta();

        e.Handled = true;
    }

    private void PanThumb_DragCompleted(object? sender, global::Avalonia.Input.VectorEventArgs e)
    {
        OnHThumbDragCompleted();
        e.Handled = true;
    }

    #endregion

    private static void ToggleButtonIsCheckedChanged(Panel sender, RoutedEventArgs e)
    {
        if (e.Source is RadioButton rb && rb.IsChecked.GetValueOrDefault(false) && rb.DataContext is Milestone milestone)
        {
            milestone.Color = rb.Tag switch
                              {
                                  "Primary" => GanttColors.Primary,
                                  "Secondary" => GanttColors.Secondary,
                                  "Tertiary" => GanttColors.Tertiary,
                                  "Information" => GanttColors.Information,
                                  "Success" => GanttColors.Success,
                                  "Warning" => GanttColors.Warning,
                                  "Danger" => GanttColors.Danger,
                                  _ => GanttColors.Primary
                              };
        }
    }
}