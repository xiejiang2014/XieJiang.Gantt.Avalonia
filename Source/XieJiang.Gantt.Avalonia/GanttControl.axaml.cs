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
        TaskBarsProperty.Changed.AddClassHandler<GanttControl>((sender,      e) => sender.TaskBarsChanged(e));
        TaskBarHeightProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.TaskBarHeightChanged(e));

        RowHeightProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.RowHeightChanged(e));

        HeaderRow1HeightProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.HeaderRow1HeightChanged(e));
        HeaderRow2HeightProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.HeaderRow2HeightChanged(e));

        DateModeProperty.Changed.AddClassHandler<GanttControl>((sender,             e) => sender.DateModeChanged(e));
        DayWidthInWeeklyModeProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.DayWidthInWeeklyModeChanged(e));
        DayWidthInMonthlyModeProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.DayWidthInMonthlyModeChanged(e));
        DayWidthProperty.Changed.AddClassHandler<GanttControl>((sender,             e) => sender.DayWidthChanged(e));

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

    private void TaskBarsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        Reorder();
    }

    #endregion


    #region TaskBarHeight

    public static readonly StyledProperty<double> TaskBarHeightProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(TaskBarHeight), 24d, true);

    public double TaskBarHeight
    {
        get => GetValue(TaskBarHeightProperty);
        set => SetValue(TaskBarHeightProperty, value);
    }

    private void TaskBarHeightChanged(AvaloniaPropertyChangedEventArgs e)
    {
        Reload();
    }

    #endregion


    #region RowHeight

    public static readonly StyledProperty<double> RowHeightProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(RowHeight), 41d, true);

    public double RowHeight
    {
        get => GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    private void RowHeightChanged(AvaloniaPropertyChangedEventArgs e)
    {
        Reorder();
    }

    #endregion


    #region HeaderRow1Height

    public static readonly StyledProperty<double> HeaderRow1HeightProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(HeaderRow1Height), 33d, true);

    public double HeaderRow1Height
    {
        get => GetValue(HeaderRow1HeightProperty);
        set => SetValue(HeaderRow1HeightProperty, value);
    }

    private void HeaderRow1HeightChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion

    #region HeaderRow2Height

    public static readonly StyledProperty<double> HeaderRow2HeightProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(HeaderRow2Height), 25d, true);

    public double HeaderRow2Height
    {
        get => GetValue(HeaderRow2HeightProperty);
        set => SetValue(HeaderRow2HeightProperty, value);
    }

    private void HeaderRow2HeightChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion

    #region DateMode

    public static readonly StyledProperty<DateModes> DateModeProperty =
        AvaloniaProperty.Register<GanttControl, DateModes>(nameof(DateMode), DateModes.Weekly, true);

    public DateModes DateMode
    {
        get => GetValue(DateModeProperty);
        set => SetValue(DateModeProperty, value);
    }

    private void DateModeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (DateMode == DateModes.Weekly)
        {
            DayWidth = e.GetNewValue<double>();
        }
    }

    #endregion


    #region DayWidthInWeeklyMode

    public static readonly StyledProperty<double> DayWidthInWeeklyModeProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(DayWidthInWeeklyMode), 36d, true);

    public double DayWidthInWeeklyMode
    {
        get => GetValue(DayWidthInWeeklyModeProperty);
        set => SetValue(DayWidthInWeeklyModeProperty, value);
    }

    private void DayWidthInWeeklyModeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (DateMode == DateModes.Weekly)
        {
            DayWidth = e.GetNewValue<double>();
        }
    }

    #endregion



    #region DayWidthInMonthlyMode

    public static readonly StyledProperty<double> DayWidthInMonthlyModeProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(DayWidthInMonthlyMode), 72d, true);

    public double DayWidthInMonthlyMode
    {
        get => GetValue(DayWidthInMonthlyModeProperty);
        set => SetValue(DayWidthInMonthlyModeProperty, value);
    }

    //放在静态构造行数
    //

    private void DayWidthInMonthlyModeChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (DateMode == DateModes.Monthly)
        {
            DayWidth = e.GetNewValue<double>();
        }

    }

    #endregion




    #region DayWidth

    public static readonly StyledProperty<double> DayWidthProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(DayWidth), 36d, true);

    public double DayWidth
    {
        get => GetValue(DayWidthProperty);
        private set => SetValue(DayWidthProperty, value);
    }

    //放在静态构造行数
    //

    private void DayWidthChanged(AvaloniaPropertyChangedEventArgs e)
    {
        Reorder();
    }

    #endregion


    #region StartDate

    public static readonly StyledProperty<DateOnly> StartDateProperty =
        AvaloniaProperty.Register<GanttControl, DateOnly>(nameof(StartDate), DateOnly.FromDateTime(DateTime.Today), true);

    public DateOnly StartDate
    {
        get => GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    private void StartDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        Reload();
    }

    #endregion

    #region EndDate

    public static readonly StyledProperty<DateOnly> EndDateProperty =
        AvaloniaProperty.Register<GanttControl, DateOnly>(nameof(EndDate), DateOnly.FromDateTime(DateTime.Today), true);

    public DateOnly EndDate
    {
        get => GetValue(EndDateProperty);
        set => SetValue(EndDateProperty, value);
    }


    private void EndDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        Reload();
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


    public void Reload()
    {
        var dateItems = _ganttHeader?.Reload();
        _ganttBodyBackground?.Reload(dateItems);
        Reorder();
    }


    public void Reorder()
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
            }


            taskBar.Width = taskBar.DateLength.TotalDays * DayWidth;
            Canvas.SetLeft(taskBar, (taskBar.StartDate        -StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays * DayWidth);
            Canvas.SetTop(taskBar, i * RowHeight + (RowHeight - TaskBarHeight) / 2d);
        }

        _ganttHeader?.InvalidateVisual();
    }
}