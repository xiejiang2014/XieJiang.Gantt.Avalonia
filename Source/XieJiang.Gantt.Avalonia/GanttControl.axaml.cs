using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace XieJiang.Gantt.Avalonia;

[TemplatePart("PART_GanttHeader",         typeof(GanttHeader))]
[TemplatePart("PART_GanttBodyBackground", typeof(GanttBodyBackground))]
[TemplatePart("PART_CanvasBody",          typeof(Canvas))]
public class GanttControl : TemplatedControl
{
    private GanttHeader?         _ganttHeader;
    private GanttBodyBackground? _ganttBodyBackground;
    private Canvas?              _canvasBody;


    static GanttControl()
    {
        TaskBarsProperty.Changed.AddClassHandler<GanttControl>((sender,  e) => sender.TaskBarsChanged(e));
        RowHeightProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.RowHeightChanged(e));

        DateModeProperty.Changed.AddClassHandler<GanttControl>((sender,  e) => sender.DateModeChanged(e));
        StartDateProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.StartDateChanged(e));
        EndDateProperty.Changed.AddClassHandler<GanttControl>((sender,   e) => sender.EndDateChanged(e));
    }

    public GanttControl()
    {
        TaskBar.StartDateChangedEvent.AddClassHandler(typeof(GanttControl), TaskBars_StartDateChanged, RoutingStrategies.Bubble);
        TaskBar.EndDateChangedEvent.AddClassHandler(typeof(GanttControl), TaskBars_EndDateChanged, RoutingStrategies.Bubble);
    }


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _ganttHeader         = e.NameScope.Find<GanttHeader>("PART_GanttHeader");
        _ganttBodyBackground = e.NameScope.Find<GanttBodyBackground>("PART_GanttBodyBackground");
        _canvasBody          = e.NameScope.Find<Canvas>("PART_CanvasBody");
        Reorder();
    }

    #region TaskBars

    public static readonly StyledProperty<ObservableCollection<TaskBar>> TaskBarsProperty =
        AvaloniaProperty.Register<GanttControl, ObservableCollection<TaskBar>>(nameof(TaskBars), new());

    public ObservableCollection<TaskBar> TaskBars
    {
        get => GetValue(TaskBarsProperty);
        set => SetValue(TaskBarsProperty, value);
    }

    //放在静态构造行数
    //TaskBarsProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.TaskBarsChanged(e));

    private void TaskBarsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        Reorder();
    }

    #endregion

    #region RowHeight

    public static readonly StyledProperty<double> RowHeightProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(RowHeight), 41);

    public double RowHeight
    {
        get => GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    //放在静态构造行数
    //RowHeightProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.RowHeightChanged(e));

    private void RowHeightChanged(AvaloniaPropertyChangedEventArgs e)
    {
        Reorder();
    }

    #endregion


    #region DateMode

    public static readonly StyledProperty<DateModes> DateModeProperty =
        AvaloniaProperty.Register<GanttControl, DateModes>(nameof(DateMode),DateModes.Weekly,true);

    public DateModes DateMode
    {
        get => GetValue(DateModeProperty);
        set => SetValue(DateModeProperty, value);
    }
    
    private void DateModeChanged(AvaloniaPropertyChangedEventArgs e)
    {

    }

    #endregion


    #region StartDate

    public static readonly StyledProperty<DateTime> StartDateProperty =
        AvaloniaProperty.Register<GanttControl, DateTime>(nameof(StartDate),DateTime.Today,true);

    public DateTime StartDate
    {
        get => GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    private void StartDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion

    #region EndDate

    public static readonly StyledProperty<DateTime> EndDateProperty =
        AvaloniaProperty.Register<GanttControl, DateTime>(nameof(EndDate), DateTime.Today, true);

    public DateTime EndDate
    {
        get => GetValue(EndDateProperty);
        set => SetValue(EndDateProperty, value);
    }


    private void EndDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion


    #region TaskBars_Events

    private void TaskBars_StartDateChanged(object? sender, RoutedEventArgs e)
    {
    }

    private void TaskBars_EndDateChanged(object? sender, RoutedEventArgs e)
    {
    }

    #endregion


    private void Reorder()
    {
        if (_canvasBody is null)
        {
            return;
        }

        for (var i = 0; i < TaskBars.Count; i++)
        {
            var taskBar = TaskBars[i];

            if (!_canvasBody.Children.Contains(taskBar))
            {
                _canvasBody.Children.Add(taskBar);
                Canvas.SetLeft(taskBar, 0);
            }

            Canvas.SetTop(taskBar, i * RowHeight);
        }

        _ganttHeader?.InvalidateVisual();
    }
}