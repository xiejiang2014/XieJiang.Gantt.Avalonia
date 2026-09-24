using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls.Metadata;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using XieJiang.Gantt.Avalonia.Controls;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia;

[TemplatePart("PART_GanttHeader",         typeof(GanttHeader))]
[TemplatePart("PART_GanttBodyBackground", typeof(GanttBodyBackground))]
[TemplatePart("PART_CanvasBody",          typeof(Canvas))]
[TemplatePart("PART_HScrollBar",          typeof(ScrollBar))]
[TemplatePart("PART_VScrollBar",          typeof(ScrollBar))]
[TemplatePart("PART_SecondaryComponents", typeof(Grid))]
public class GanttControl : TemplatedControl
{
    #region Parts

    private GanttHeader?         _ganttHeader;
    private GanttBodyBackground? _ganttBodyBackground;
    private Grid?                _secondaryComponents;
    private Canvas?              _canvasBody;

    public ScrollBar? HScrollBar { get; private set; }
    public ScrollBar? VScrollBar { get; private set; }

    #endregion

    static GanttControl()
    {
        // -----------------------------   Property.Changed:
        TaskBarHeightProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.TaskBarHeightChanged(e));

        RowHeightProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.RowHeightChanged(e));

        HeaderRow1HeightProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.HeaderRow1HeightChanged(e));
        HeaderRow2HeightProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.HeaderRow2HeightChanged(e));

        DateModeProperty.Changed.AddClassHandler<GanttControl>((sender,                 e) => sender.DateModeChanged());
        DayWidthInWeeklyModeProperty.Changed.AddClassHandler<GanttControl>((sender,     e) => sender.DayWidthInModeChanged(DateModes.Weekly));
        DayWidthInMonthlyModeProperty.Changed.AddClassHandler<GanttControl>((sender,    e) => sender.DayWidthInModeChanged(DateModes.Monthly));
        DayWidthInSeasonallyModeProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.DayWidthInModeChanged(DateModes.Seasonally));
        DayWidthInYearlyModelProperty.Changed.AddClassHandler<GanttControl>((sender,    e) => sender.DayWidthInModeChanged(DateModes.Yearly));
        DateTimeNowProperty.Changed.AddClassHandler<GanttControl>((sender,              e) => sender.DateTimeNowChanged(e));

        TaskContentTemplateProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.TaskContentTemplateChanged(e));

        DragUnitProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.DragUnitChanged(e));

        GanttModelProperty.Changed.AddClassHandler<GanttControl>((sender, _) => sender.UpdateGanttModel());

        StartDateProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.StartDateChanged(e));
        EndDateProperty.Changed.AddClassHandler<GanttControl>((sender,   e) => sender.EndDateChanged(e));

        // -----------------------------   Handle events
        TaskBar.MainDragDeltaEvent.AddClassHandler<GanttControl>((sender,      e) => sender.TaskBar_MainDragDelta(e));
        TaskBar.MainDragCompletedEvent.AddClassHandler<GanttControl>((sender,  e) => sender.TaskBar_MainDragCompleted(e));
        TaskBar.WidthDragDeltaEvent.AddClassHandler<GanttControl>((sender,     e) => sender.TaskBar_WidthDragDelta(e));
        TaskBar.WidthDragCompletedEvent.AddClassHandler<GanttControl>((sender, e) => sender.TaskBar_WidthDragCompleted(e));


        MilestoneControl.HThumbDragDeltaEvent.AddClassHandler<GanttControl>((sender,     e) => sender.MilestoneControl_HThumbDragDelta(e));
        MilestoneControl.HThumbDragCompletedEvent.AddClassHandler<GanttControl>((sender, e) => sender.MilestoneControl_HThumbDragCompleted(e));

        GanttHeader.CreateMilestoneRequestedEvent.AddClassHandler<GanttControl>((sender, e) => sender.CreateNewMilestone(e.Date));
        MilestoneControl.DeleteRequestedEvent.AddClassHandler<GanttControl>((sender, e) =>
                                                                            {
                                                                                if (e.Source is MilestoneControl { DataContext: Milestone milestone })
                                                                                {
                                                                                    sender.DeleteMilestone(milestone);
                                                                                }
                                                                            });
    }

    public GanttControl()
    {
        DataContextChanged += (_, _) => UpdateGanttModel();

        _pinout.DragStarted   += Pinout_DragStarted;
        _pinout.DragDelta     += Pinout_DragDelta;
        _pinout.DragCompleted += Pinout_DragCompleted;

        // Resolve from this control so themes and overrides below Application are honored.
        _temporaryDependencyLine.Bind(ThemeProperty, this.GetResourceObservable("TemporaryDependencyLine"));

        DateModeChanged();
    }


    #region GanttModel

    public static readonly StyledProperty<GanttModel?> GanttModelProperty =
        AvaloniaProperty.Register<GanttControl, GanttModel?>(nameof(GanttModel));

    /// <summary>
    /// The data shown by the control. When not set, a <see cref="Models.GanttModel"/> DataContext is used.
    /// </summary>
    public GanttModel? GanttModel
    {
        get => GetValue(GanttModelProperty);
        set => SetValue(GanttModelProperty, value);
    }

    private GanttModel? _ganttModel;

    private void UpdateGanttModel()
    {
        var ganttModel = GanttModel ?? DataContext as GanttModel;
        if (ReferenceEquals(ganttModel, _ganttModel))
        {
            return;
        }

        if (_ganttModel is not null)
        {
            _ganttModel.GanttTasks.CollectionChanged -= GanttTasks_CollectionChanged;
            _ganttModel.Milestones.CollectionChanged -= Milestones_CollectionChanged;
        }

        _ganttModel = ganttModel;

        if (_ganttModel is not null)
        {
            _ganttModel.GanttTasks.CollectionChanged += GanttTasks_CollectionChanged;
            _ganttModel.Milestones.CollectionChanged += Milestones_CollectionChanged;
        }

        Reload();
    }

    private bool _isReloadTasksPending;

    private void GanttTasks_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // Coalesce: adding many tasks one by one must not rebuild all bars for every item.
        if (_isReloadTasksPending)
        {
            return;
        }

        _isReloadTasksPending = true;
        Dispatcher.UIThread.Post(() =>
                                 {
                                     _isReloadTasksPending = false;
                                     ReloadTasks();
                                 });
    }

    private void Milestones_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ReloadMilestones();
    }

    #endregion


    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _ganttHeader         = e.NameScope.Find<GanttHeader>("PART_GanttHeader");
        _ganttBodyBackground = e.NameScope.Find<GanttBodyBackground>("PART_GanttBodyBackground");
        _secondaryComponents = e.NameScope.Find<Grid>("PART_SecondaryComponents");
        _canvasBody          = e.NameScope.Find<Canvas>("PART_CanvasBody");
        HScrollBar           = e.NameScope.Find<ScrollBar>("PART_HScrollBar");
        VScrollBar           = e.NameScope.Find<ScrollBar>("PART_VScrollBar");

        if (_canvasBody is not null)
        {
            _canvasBody.PointerWheelChanged += Canvas_PointerWheelChanged;
        }

        // the panel hosting the canvas is the visible area of the tasks
        _viewportHost = _canvasBody?.Parent as Control;
        if (_viewportHost is not null)
        {
            _viewportHost.SizeChanged += (_, _) => UpdateRealizedItems();
        }

        if (HScrollBar is not null)
        {
            HScrollBar.ValueChanged += (_, _) => UpdateRealizedItems();
        }

        if (VScrollBar is not null)
        {
            VScrollBar.ValueChanged += (_, _) => UpdateRealizedItems();
        }

        if (_ganttBodyBackground is not null)
        {
            _ganttBodyBackground.PointerPressed      += GanttBodyBackground_PointerPressed;
            _ganttBodyBackground.PointerReleased     += GanttBodyBackground_PointerReleased;
            _ganttBodyBackground.PointerMoved        += GanttBodyBackground_PointerMoved;
            _ganttBodyBackground.PointerWheelChanged += Canvas_PointerWheelChanged;
        }

        // DataContext is usually set before the template is applied, when the parts did not exist yet.
        Reload();
    }

    private void GanttBodyBackground_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        PanStart(e);
    }

    private void GanttBodyBackground_PointerMoved(object? sender, PointerEventArgs e)
    {
        HidePinout();
        Pan(e);
    }

    private void GanttBodyBackground_PointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        PanEnd(e);
    }

    private void Canvas_PointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        PointerWheel(e);
    }

    #region TaskBarHeight

    public static readonly StyledProperty<double> TaskBarHeightProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(TaskBarHeight), 24d);

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
        AvaloniaProperty.Register<GanttControl, double>(nameof(RowHeight), 41d);

    public double RowHeight
    {
        get => GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    private void RowHeightChanged(AvaloniaPropertyChangedEventArgs e)
    {
        ReloadTasks();
    }

    #endregion

    #region HeaderRow1Height

    public static readonly StyledProperty<double> HeaderRow1HeightProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(HeaderRow1Height), 33d);

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
        AvaloniaProperty.Register<GanttControl, double>(nameof(HeaderRow2Height), 25d);

    public double HeaderRow2Height
    {
        get => GetValue(HeaderRow2HeightProperty);
        set => SetValue(HeaderRow2HeightProperty, value);
    }

    private void HeaderRow2HeightChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion


    #region DragUnit

    public static readonly StyledProperty<DragUnits> DragUnitProperty =
        AvaloniaProperty.Register<GanttControl, DragUnits>(nameof(DragUnit));

    public DragUnits DragUnit
    {
        get => GetValue(DragUnitProperty);
        set => SetValue(DragUnitProperty, value);
    }

    private void DragUnitChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion


    #region DateMode

    public static readonly StyledProperty<DateModes> DateModeProperty =
        AvaloniaProperty.Register<GanttControl, DateModes>(nameof(DateMode), DateModes.Weekly);

    public DateModes DateMode
    {
        get => GetValue(DateModeProperty);
        set => SetValue(DateModeProperty, value);
    }

    private void DateModeChanged()
    {
        Reload();
    }

    #endregion

    #region DayWidthInWeeklyMode

    public static readonly StyledProperty<double> DayWidthInWeeklyModeProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(DayWidthInWeeklyMode), 72d);

    public double DayWidthInWeeklyMode
    {
        get => GetValue(DayWidthInWeeklyModeProperty);
        set => SetValue(DayWidthInWeeklyModeProperty, value);
    }

    private void DayWidthInModeChanged(DateModes dateMode)
    {
        if (dateMode == DateMode)
        {
            Reload();
        }
    }

    #endregion


    #region DayWidthInMonthlyMode

    public static readonly StyledProperty<double> DayWidthInMonthlyModeProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(DayWidthInMonthlyMode), 36d);

    public double DayWidthInMonthlyMode
    {
        get => GetValue(DayWidthInMonthlyModeProperty);
        set => SetValue(DayWidthInMonthlyModeProperty, value);
    }

    #endregion


    #region DayWidthInSeasonlyMode

    public static readonly StyledProperty<double> DayWidthInSeasonallyModeProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(DayWidthInSeasonallyMode), 12d);

    public double DayWidthInSeasonallyMode
    {
        get => GetValue(DayWidthInSeasonallyModeProperty);
        set => SetValue(DayWidthInSeasonallyModeProperty, value);
    }

    #endregion


    #region DayWidthInYearlyModel

    public static readonly StyledProperty<double> DayWidthInYearlyModelProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(DayWidthInYearlyModel), 4d);

    public double DayWidthInYearlyModel
    {
        get => GetValue(DayWidthInYearlyModelProperty);
        set => SetValue(DayWidthInYearlyModelProperty, value);
    }

    #endregion


    #region DayWidth

    public double DayWidth => DateMode switch
                              {
                                  DateModes.Weekly     => DayWidthInWeeklyMode,
                                  DateModes.Monthly    => DayWidthInMonthlyMode,
                                  DateModes.Seasonally => DayWidthInSeasonallyMode,
                                  DateModes.Yearly     => DayWidthInYearlyModel,
                                  _                    => throw new ArgumentOutOfRangeException(nameof(DateMode), DateMode, null)
                              };

    //public static readonly StyledProperty<double> DayWidthProperty =
    //    AvaloniaProperty.Register<GanttControl, double>(nameof(DayWidth), 72d, true);

    //public double DayWidth
    //{
    //    get => GetValue(DayWidthProperty);
    //    private set => SetValue(DayWidthProperty, value);
    //}

    //private void DayWidthChanged(AvaloniaPropertyChangedEventArgs e)
    //{
    //    ReloadTasks();
    //}

    #endregion


    #region DateTimeNow

    public static readonly StyledProperty<DateTime?> DateTimeNowProperty =
        AvaloniaProperty.Register<GanttControl, DateTime?>(nameof(DateTimeNow), null);

    public DateTime? DateTimeNow
    {
        get => GetValue(DateTimeNowProperty);
        set => SetValue(DateTimeNowProperty, value);
    }

    private void DateTimeNowChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _ganttBodyBackground?.ReloadMarkLineToday(StartDate, DayWidth, DateTimeNow);
    }

    // Without DateTimeNow the today line follows the clock, so it has to move while the control is shown.
    private readonly DispatcherTimer _nowTimer = new() { Interval = TimeSpan.FromMinutes(1) };

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _nowTimer.Tick += NowTimer_Tick;
        _nowTimer.Start();
        ObserveTasks();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _nowTimer.Stop();
        _nowTimer.Tick -= NowTimer_Tick;
        UnobserveTasks();
    }

    private void NowTimer_Tick(object? sender, EventArgs e)
    {
        if (DateTimeNow is null)
        {
            _ganttBodyBackground?.ReloadMarkLineToday(StartDate, DayWidth, DateTimeNow);
        }
    }

    #endregion


    #region StartDate

    public static readonly StyledProperty<DateOnly> StartDateProperty =
        AvaloniaProperty.Register<GanttControl, DateOnly>(nameof(StartDate), DateOnly.FromDateTime(DateTime.Today));

    public DateOnly StartDate
    {
        get => GetValue(StartDateProperty);
        set => SetValue(StartDateProperty, value);
    }

    private void StartDateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        // EndDate keeps its requested value and is only coerced, so re-evaluate it against the new StartDate.
        CoerceValue(EndDateProperty);
        Reload();
    }

    #endregion

    #region EndDate

    public static readonly StyledProperty<DateOnly> EndDateProperty =
        AvaloniaProperty.Register<GanttControl, DateOnly>(nameof(EndDate), DateOnly.FromDateTime(DateTime.Today),
                                                          coerce: (sender, value) => value < sender.GetValue(StartDateProperty)
                                                              ? sender.GetValue(StartDateProperty)
                                                              : value);

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


    public void Reload()
    {
        if (_ganttModel is null)
        {
            _ganttHeader?.Row1Items.Clear();
            _ganttHeader?.Row2Items.Clear();
            _ganttBodyBackground?.ReloadGrid(null);
            ReloadMilestones();
            ReloadTasks();
            return;
        }

        if (_ganttHeader is not null)
        {
            _ganttHeader.Reload(DateMode, StartDate, EndDate, DayWidth);
            _ganttBodyBackground?.Reload(_ganttHeader.Row2Items, StartDate, DayWidth, DateTimeNow);
        }

        ReloadMilestones();
        ReloadTasks();
    }


    // Only the task bars and dependency lines inside the visible area (plus a margin) exist as controls.
    private const double MinTaskBarWidth      = 20;
    private const double RealizationMargin    = 200;
    private const double DependencyLineMargin = 20;

    private readonly Dictionary<GanttTask, int>     _taskRows         = new();
    private readonly Dictionary<GanttTask, TaskBar> _taskBarsDic      = new();
    private readonly Stack<TaskBar>                 _recycledTaskBars = new();
    private readonly List<GanttTask>                _observedTasks    = new();

    private Control? _viewportHost;

    public void ReloadTasks()
    {
        if (_canvasBody is null)
        {
            return;
        }

        foreach (var taskBar in _taskBarsDic.Values)
        {
            taskBar.DataContext = null;
            _recycledTaskBars.Push(taskBar);
        }

        _taskBarsDic.Clear();
        _dependencyLinesDic.Clear();
        _canvasBody.Children.Clear();
        _pinoutTaskBar = null;
        _taskRows.Clear();
        UnobserveTasks();

        if (_ganttModel is null)
        {
            _canvasBody.Height = 0;
            return;
        }

        for (var i = 0; i < _ganttModel.GanttTasks.Count; i++)
        {
            _taskRows[_ganttModel.GanttTasks[i]] = i;
        }

        _canvasBody.Height = _ganttModel.GanttTasks.Count * RowHeight;

        ObserveTasks();
        UpdateRealizedItems();
    }

    #region Task observation

    // Tasks are observed while the control is in the visual tree, so tasks without a bar still move their lines
    // and the model does not keep a detached control alive.
    private void ObserveTasks()
    {
        if (_ganttModel is null || !this.IsAttachedToVisualTree())
        {
            return;
        }

        foreach (var ganttTask in _taskRows.Keys)
        {
            ganttTask.PropertyChanged += GanttTask_PropertyChanged;
            _observedTasks.Add(ganttTask);
        }
    }

    private void UnobserveTasks()
    {
        foreach (var ganttTask in _observedTasks)
        {
            ganttTask.PropertyChanged -= GanttTask_PropertyChanged;
        }

        _observedTasks.Clear();
    }

    private void GanttTask_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is GanttTask ganttTask && e.PropertyName is nameof(GanttTask.StartDate) or nameof(GanttTask.EndDate))
        {
            if (_taskBarsDic.TryGetValue(ganttTask, out var taskBar))
            {
                RefreshTaskBar(taskBar, ganttTask);
            }
            else
            {
                UpdateDependencyLines(ganttTask);
            }

            UpdateRealizedItems();
        }
    }

    #endregion

    #region Realization

    private Rect? Viewport()
    {
        if (_viewportHost is null || _viewportHost.Bounds.Width <= 0 || _viewportHost.Bounds.Height <= 0)
        {
            return null;
        }

        return new Rect(HScrollBar?.Value ?? 0, VScrollBar?.Value ?? 0, _viewportHost.Bounds.Width, _viewportHost.Bounds.Height)
            .Inflate(RealizationMargin);
    }

    /// <summary>
    /// Creates bars and lines that came into view and recycles the ones that left it.
    /// </summary>
    private void UpdateRealizedItems()
    {
        if (_canvasBody is null || _ganttModel is null || Viewport() is not { } viewport)
        {
            return;
        }

        // bars: only the rows in view need to be checked
        var tasks    = _ganttModel.GanttTasks;
        var firstRow = Math.Max(0, (int)(viewport.Top / RowHeight));
        var lastRow  = Math.Min(tasks.Count - 1, (int)(viewport.Bottom / RowHeight));

        var visibleTasks = new HashSet<GanttTask>();
        for (var row = firstRow; row <= lastRow; row++)
        {
            if (TaskBarRect(tasks[row]).Intersects(viewport))
            {
                visibleTasks.Add(tasks[row]);
            }
        }

        // the hovered bar may be dragged, so it is never recycled
        if (_pinoutTaskBar?.GanttTask is { } hoveredTask)
        {
            visibleTasks.Add(hoveredTask);
        }

        foreach (var (ganttTask, taskBar) in _taskBarsDic.ToList())
        {
            if (!visibleTasks.Contains(ganttTask))
            {
                _taskBarsDic.Remove(ganttTask);
                _canvasBody.Children.Remove(taskBar);
                taskBar.DataContext = null;
                _recycledTaskBars.Push(taskBar);
            }
        }

        foreach (var ganttTask in visibleTasks)
        {
            if (!_taskBarsDic.ContainsKey(ganttTask))
            {
                RealizeTaskBar(ganttTask);
            }
        }

        // lines: a line can cross the view while both of its tasks are outside of it
        foreach (var (parentTask, childTask) in Dependencies())
        {
            UpdateDependencyLine(parentTask, childTask, viewport);
        }
    }

    private void RealizeTaskBar(GanttTask ganttTask)
    {
        if (!_recycledTaskBars.TryPop(out var taskBar))
        {
            taskBar = new TaskBar
                      {
                          MinWidth = MinTaskBarWidth,
                          ZIndex   = 1
                      };

            taskBar.PointerEntered += TaskBar_PointerEntered;
        }

        taskBar.DataContext = ganttTask;
        PlaceTaskBar(taskBar, ganttTask);
        Canvas.SetTop(taskBar, TaskBarTop(ganttTask));

        _taskBarsDic.Add(ganttTask, taskBar);
        _canvasBody!.Children.Add(taskBar);
    }

    private double TaskBarTop(GanttTask ganttTask) => _taskRows[ganttTask] * RowHeight + (RowHeight - TaskBarHeight) / 2d;

    private double TaskBarLeft(GanttTask ganttTask) => (ganttTask.StartDate - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays * DayWidth;

    private static double TaskBarWidth(GanttTask ganttTask, double dayWidth) => Math.Max(ganttTask.DateLength.TotalDays * dayWidth, MinTaskBarWidth);

    /// <summary>
    /// The bounds of the bar of a task: the realized bar if there is one (it may be being dragged), otherwise computed from the dates.
    /// </summary>
    private Rect TaskBarRect(GanttTask ganttTask)
    {
        if (_taskBarsDic.TryGetValue(ganttTask, out var taskBar))
        {
            return new Rect(Canvas.GetLeft(taskBar), Canvas.GetTop(taskBar), taskBar.Width, TaskBarHeight);
        }

        return new Rect(TaskBarLeft(ganttTask), TaskBarTop(ganttTask), TaskBarWidth(ganttTask, DayWidth), TaskBarHeight);
    }

    #endregion


    /// <summary>
    /// Positions and sizes the bar from the task dates. Bars never get narrower than MinWidth.
    /// </summary>
    private void PlaceTaskBar(TaskBar taskBar, GanttTask ganttTask)
    {
        taskBar.Width = TaskBarWidth(ganttTask, DayWidth);
        Canvas.SetLeft(taskBar, TaskBarLeft(ganttTask));
    }

    /// <summary>
    /// Re-places the bar from its task dates and updates everything attached to it.
    /// </summary>
    private void RefreshTaskBar(TaskBar taskBar, GanttTask ganttTask)
    {
        PlaceTaskBar(taskBar, ganttTask);
        UpdateDependencyLines(ganttTask);

        if (taskBar == _pinoutTaskBar)
        {
            Canvas.SetLeft(_pinout, Canvas.GetLeft(taskBar) + taskBar.Width);
        }
    }


    #region DependencyLine

    private readonly Thumb _pinout = new()
                                     {
                                         Classes = { "TaskPinout" }
                                     };

    private TaskBar? _pinoutTaskBar;

    private readonly Line _temporaryDependencyLine = new()
                                                     {
                                                         IsHitTestVisible = false
                                                     };


    #region DependencyLinePointerPressed

    public static readonly RoutedEvent<DependencyLinePointerPressedEventArgs> DependencyLinePointerPressedEvent =
        RoutedEvent.Register<GanttControl, DependencyLinePointerPressedEventArgs>(nameof(DependencyLinePointerPressed), RoutingStrategies.Direct);

    public event EventHandler<DependencyLinePointerPressedEventArgs> DependencyLinePointerPressed
    {
        add => AddHandler(DependencyLinePointerPressedEvent, value);
        remove => RemoveHandler(DependencyLinePointerPressedEvent, value);
    }

    protected virtual void OnDependencyLinePointerPressed(DependencyLine dependencyLine)
    {
        var args = new DependencyLinePointerPressedEventArgs(DependencyLinePointerPressedEvent, dependencyLine);
        RaiseEvent(args);
    }

    #endregion


    // realized lines only
    private readonly Dictionary<(GanttTask Parent, GanttTask Child), DependencyLine> _dependencyLinesDic = new();

    /// <summary>
    /// The dependencies between tasks of the model, ignoring tasks that are not in it.
    /// </summary>
    private IEnumerable<(GanttTask Parent, GanttTask Child)> Dependencies()
    {
        foreach (var parentTask in _taskRows.Keys)
        {
            foreach (var childTask in parentTask.Children)
            {
                if (_taskRows.ContainsKey(childTask))
                {
                    yield return (parentTask, childTask);
                }
            }
        }
    }

    /// <summary>
    /// Returns the line from <paramref name="parentTask"/> to <paramref name="childTask"/> when it is currently shown.
    /// </summary>
    public DependencyLine? ReadDependencyLine(GanttTask parentTask, GanttTask childTask) =>
        _dependencyLinesDic.GetValueOrDefault((parentTask, childTask));

    public void RemoveDependencyLine(DependencyLine dependencyLine)
    {
        dependencyLine.ParentTask.RemoveDependentTask(dependencyLine.ChildTask);
        _dependencyLinesDic.Remove((dependencyLine.ParentTask, dependencyLine.ChildTask));
        _canvasBody?.Children.Remove(dependencyLine);
    }

    private void DependencyLine_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is DependencyLine dependencyLine)
        {
            OnDependencyLinePointerPressed(dependencyLine);
        }
    }

    private void UpdateDependencyLines(GanttTask ganttTask)
    {
        if (Viewport() is not { } viewport || !_taskRows.ContainsKey(ganttTask))
        {
            return;
        }

        foreach (var parentTask in ganttTask.Parents)
        {
            if (_taskRows.ContainsKey(parentTask))
            {
                UpdateDependencyLine(parentTask, ganttTask, viewport);
            }
        }

        foreach (var childTask in ganttTask.Children)
        {
            if (_taskRows.ContainsKey(childTask))
            {
                UpdateDependencyLine(ganttTask, childTask, viewport);
            }
        }
    }

    /// <summary>
    /// Shows, updates or removes the line of a dependency depending on whether it is in view.
    /// </summary>
    private void UpdateDependencyLine(GanttTask parentTask, GanttTask childTask, Rect viewport)
    {
        var parentRect = TaskBarRect(parentTask);
        var childRect  = TaskBarRect(childTask);
        var inView     = parentRect.Union(childRect).Inflate(DependencyLineMargin).Intersects(viewport);

        var key = (parentTask, childTask);
        _dependencyLinesDic.TryGetValue(key, out var dependencyLine);

        if (!inView)
        {
            if (dependencyLine is not null)
            {
                _dependencyLinesDic.Remove(key);
                _canvasBody?.Children.Remove(dependencyLine);
            }

            return;
        }

        if (dependencyLine is null)
        {
            dependencyLine = new DependencyLine(parentTask, childTask)
                             {
                                 Classes = { "DependencyLine" },
                                 ZIndex  = 0
                             };

            dependencyLine.PointerPressed += DependencyLine_PointerPressed;
            _dependencyLinesDic.Add(key, dependencyLine);
            _canvasBody?.Children.Add(dependencyLine);
        }

        UpdateDependencyLineGeometry(parentRect, childRect, _taskRows[childTask], dependencyLine);
    }

    private void UpdateDependencyLineGeometry(Rect parentRect, Rect childRect, int childRow, DependencyLine dependencyLine)
    {
        var x0 = parentRect.Right;
        var y0 = parentRect.Top + TaskBarHeight / 2d;

        var streamGeometry = new StreamGeometry();

        // the context must be disposed (which finishes the geometry) before the geometry is used
        using (var sgc = streamGeometry.Open())
        {
            DrawDependencyLine(sgc, parentRect, childRect, childRow, x0, y0);
        }

        dependencyLine.Data = streamGeometry;
        Canvas.SetLeft(dependencyLine, x0);
        Canvas.SetTop(dependencyLine, y0);
    }

    private void DrawDependencyLine(StreamGeometryContext sgc, Rect parentRect, Rect childRect, int childRow, double x0, double y0)
    {
        var down = childRect.Top > parentRect.Top + TaskBarHeight;

        if (childRect.Left >= parentRect.Right)
        {
            var x1 = childRect.Left - parentRect.Right + 10;
            var y1 = childRect.Top - parentRect.Top + (down
                ? -TaskBarHeight / 2d
                : TaskBarHeight  / 2d);

            sgc.BeginFigure(new Point(0, 0), false);
            sgc.LineTo(new Point(x1,     0), true);
            sgc.LineTo(new Point(x1, y1 - (down
                                     ? 3
                                     : -3)), true);
            sgc.EndFigure(false);

            //arrow
            if (down)
            {
                sgc.BeginFigure(new Point(x1, y1), true);
                sgc.LineTo(new Point(x1 - 4,  y1 - 9), false);
                sgc.LineTo(new Point(x1 + 4,  y1 - 9), false);
            }
            else
            {
                sgc.BeginFigure(new Point(x1, y1), true);
                sgc.LineTo(new Point(x1 - 4,  y1 + 9), false);
                sgc.LineTo(new Point(x1 + 4,  y1 + 9), false);
            }

            sgc.EndFigure(true);
        }
        else
        {
            sgc.BeginFigure(new Point(0, 0), false);
            const int x1 = 10;
            sgc.LineTo(new Point(x1, 0), true);

            var y2 = down
                ? childRow       * RowHeight - y0
                : (childRow + 1) * RowHeight - y0;
            sgc.LineTo(new Point(x1, y2), true);

            var x3 = childRect.Left - x0 - 18;
            sgc.LineTo(new Point(x3, y2), true);

            var y3 = childRect.Top + TaskBarHeight / 2d - y0;
            sgc.LineTo(new Point(x3, y3), true);

            var x4 = childRect.Left - x0;
            sgc.LineTo(new Point(x4 - 3, y3), true);
            sgc.EndFigure(false);


            //arrow
            sgc.BeginFigure(new Point(x4, y3), true);
            sgc.LineTo(new Point(x4 - 9,  y3 + 4), false);
            sgc.LineTo(new Point(x4 - 9,  y3 - 4), false);
            sgc.EndFigure(true);
        }
    }

    private void HidePinout()
    {
        if (_canvasBody is not null)
        {
            _canvasBody.Children.Remove(_pinout);
        }

        _pinoutTaskBar = null;
    }

    private double _pinoutOffsetX;
    private double _pinoutOffsetY;

    private void Pinout_DragStarted(object? sender, VectorEventArgs e)
    {
        if (_canvasBody is not null)
        {
            var x = Canvas.GetLeft(_pinout) + _pinout.Bounds.Width  / 2d;
            var y = Canvas.GetTop(_pinout)  + _pinout.Bounds.Height / 2d;

            _pinoutOffsetX = e.Vector.X;
            _pinoutOffsetY = e.Vector.Y;

            _temporaryDependencyLine.StartPoint = new Point(x, y);
            _temporaryDependencyLine.EndPoint   = new Point(x, y);
            _canvasBody.Children.Add(_temporaryDependencyLine);
        }
    }

    private void Pinout_DragDelta(object? sender, VectorEventArgs e)
    {
        if (_canvasBody is not null)
        {
            _temporaryDependencyLine.EndPoint = new Point(Canvas.GetLeft(_pinout) + _pinoutOffsetX + e.Vector.X,
                                                          Canvas.GetTop(_pinout)  + _pinoutOffsetY + e.Vector.Y
                                                         );

            var inputElement = _canvasBody.InputHitTest(_temporaryDependencyLine.EndPoint,
                                                        true
                                                       );

            var            hoveringTaskBar = FindTaskBar(inputElement);
            IPseudoClasses classes         = _temporaryDependencyLine.Classes;

            if (hoveringTaskBar?.GanttTask is not null && _pinoutTaskBar?.GanttTask is not null)
            {
                var canAdd = _pinoutTaskBar.GanttTask.CanAddDependentTask(hoveringTaskBar.GanttTask);

                if (canAdd)
                {
                    classes.Set(":Reject", false);
                    classes.Set(":Accept", true);
                }
                else
                {
                    classes.Set(":Reject", true);
                    classes.Set(":Accept", false);
                }
            }
            else
            {
                classes.Set(":Reject", false);
                classes.Set(":Accept", false);
            }
        }
    }

    private void Pinout_DragCompleted(object? sender, VectorEventArgs e)
    {
        if (_canvasBody is not null)
        {
            _canvasBody.Children.Remove(_temporaryDependencyLine);

            var inputElement = _canvasBody.InputHitTest(_temporaryDependencyLine.EndPoint,
                                                        true
                                                       );
            var hoveringTaskBar = FindTaskBar(inputElement);

            if (hoveringTaskBar?.GanttTask is not null && _pinoutTaskBar?.GanttTask is not null)
            {
                if (_pinoutTaskBar.GanttTask.AddingDependentTask(hoveringTaskBar.GanttTask))
                {
                    UpdateDependencyLines(_pinoutTaskBar.GanttTask);
                }
            }
        }
    }

    private TaskBar? FindTaskBar(IInputElement? inputElement)
    {
        if (inputElement is Visual visual && _pinoutTaskBar?.GanttTask is not null)
        {
            return visual.FindAncestorOfType<TaskBar>(true);
        }

        return null;
    }


    //show pinout
    private void TaskBar_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (_canvasBody is not null && sender is TaskBar taskBar)
        {
            if (!_canvasBody.Children.Contains(_pinout))
            {
                _canvasBody.Children.Add(_pinout);
            }

            Canvas.SetLeft(_pinout, taskBar.Bounds.Right);
            Canvas.SetTop(_pinout, Canvas.GetTop(taskBar));

            _pinoutTaskBar = taskBar;
        }
    }

    #endregion

    #region drag

    private void TaskBar_MainDragDelta(RoutedEventArgs e)
    {
        if (e.Source is TaskBar { DataContext: GanttTask ganttTask } taskBar)
        {
            var l = taskBar.GetValue(Canvas.LeftProperty);

            // The bar may be wider than the task (MinWidth), so keep the duration instead of deriving it from the width.
            // Changing the dates re-places the bar, its lines and the pinout.
            var dateLength = ganttTask.DateLength;
            ganttTask.StartDate = StartDate.ToDateTime(TimeOnly.MinValue).AddDays(l / DayWidth);
            ganttTask.EndDate   = ganttTask.StartDate + dateLength;
        }
    }

    private void TaskBar_MainDragCompleted(RoutedEventArgs e)
    {
        if (e.Source is TaskBar { DataContext: GanttTask ganttTask } taskBar)
        {
            if (DragUnit == DragUnits.Daily)
            {
                var l = taskBar.GetValue(Canvas.LeftProperty);

                var startDate = RoundToDay(StartDate.ToDateTime(TimeOnly.MinValue).AddDays(l / DayWidth));
                var endDate   = startDate.Add(ganttTask.DateLength);

                ganttTask.StartDate = startDate;
                ganttTask.EndDate   = endDate;

                // snap the bar even when the rounded dates did not change
                RefreshTaskBar(taskBar, ganttTask);
            }
        }
    }

    private static DateTime RoundToDay(DateTime dateTime) =>
        dateTime.TimeOfDay >= TimeSpan.FromHours(12)
            ? dateTime.Date.AddDays(1)
            : dateTime.Date;

    private void TaskBar_WidthDragDelta(WidthDragEventArgs e)
    {
        if (e.Source is TaskBar { DataContext: GanttTask ganttTask } taskBar)
        {
            if (e.Edge == Edges.Right)
            {
                Canvas.SetLeft(_pinout, taskBar.Bounds.Right);
            }

            UpdateDependencyLines(ganttTask);
        }
    }

    private void TaskBar_WidthDragCompleted(WidthDragEventArgs e)
    {
        if (e.Source is TaskBar { DataContext: GanttTask ganttTask } taskBar)
        {
            //重新计算task的起止日期
            var l = taskBar.GetValue(Canvas.LeftProperty);
            var w = taskBar.Width;

            //if (DateMode == DateModes.Weekly)
            //{
            if (e.Edge == Edges.Left)
            {
                if (DragUnit == DragUnits.Free)
                {
                    ganttTask.StartDate = StartDate.ToDateTime(TimeOnly.MinValue).AddDays(l / DayWidth);
                }
                else if (DragUnit == DragUnits.Daily)
                {
                    var temp = RoundToDay(StartDate.ToDateTime(TimeOnly.MinValue).AddDays(l / DayWidth));

                    if (ganttTask.EndDate - temp <= TimeSpan.FromDays(1))
                    {
                        temp = ganttTask.EndDate.AddDays(-1);
                    }

                    ganttTask.StartDate = temp;
                }
            }
            else if (e.Edge == Edges.Right)
            {
                if (DragUnit == DragUnits.Free)
                {
                    ganttTask.EndDate = StartDate.ToDateTime(TimeOnly.MinValue).AddDays((l + w) / DayWidth);
                }
                else if (DragUnit == DragUnits.Daily)
                {
                    var temp = RoundToDay(StartDate.ToDateTime(TimeOnly.MinValue).AddDays((l + w) / DayWidth));

                    if (temp - ganttTask.StartDate <= TimeSpan.FromDays(1))
                    {
                        temp = ganttTask.StartDate.AddDays(1);
                    }

                    ganttTask.EndDate = temp;
                }
            }
            //}

            // snap the bar even when the rounded dates did not change
            RefreshTaskBar(taskBar, ganttTask);
        }
    }

    #endregion

    #region Scroll

    public void ScrollToTask(GanttTask ganttTask)
    {
        if (HScrollBar is not null && _taskRows.ContainsKey(ganttTask))
        {
            var nowOffset = TaskBarLeft(ganttTask) - HScrollBar.Bounds.Width / 2d;
            HScrollBar.Value = nowOffset;
        }
    }

    public void ScrollToNow()
    {
        var dateTimeNow = DateTimeNow ?? DateTime.Now;
        ScrollToDateTime(dateTimeNow);
    }

    public void ScrollToDateTime(DateTime dateTime)
    {
        if (HScrollBar is not null)
        {
            var nowOffset = (dateTime - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays * DayWidth - HScrollBar.Bounds.Width / 2d;
            HScrollBar.Value = nowOffset;
        }
    }

    private const double WheelStep = 30;

    private void PointerWheel(PointerWheelEventArgs e)
    {
        // Only handle the wheel when something scrolled, so outer scroll viewers can take over at the edges.
        e.Handled |= ScrollBy(HScrollBar, e.Delta.X);
        e.Handled |= ScrollBy(VScrollBar, e.Delta.Y);
    }

    private static bool ScrollBy(ScrollBar? scrollBar, double delta)
    {
        if (scrollBar is null || delta == 0)
        {
            return false;
        }

        var value = Math.Clamp(scrollBar.Value - delta * WheelStep, scrollBar.Minimum, scrollBar.Maximum);
        if (value.Equals(scrollBar.Value))
        {
            return false;
        }

        scrollBar.SetCurrentValue(RangeBase.ValueProperty, value);
        return true;
    }

    #endregion


    #region Pan

    private bool    _isPanning;
    private Point?  _panBasePoint;
    private Cursor? _cursorBeforePan;

    private void PanStart(PointerPressedEventArgs e)
    {
        var cp = e.GetCurrentPoint(this);
        if (cp.Properties.IsMiddleButtonPressed)
        {
            _panBasePoint    = e.GetPosition(this);
            _cursorBeforePan = Cursor;
            Cursor           = new Cursor(StandardCursorType.SizeAll);
            _isPanning       = true;
        }
    }

    private void Pan(PointerEventArgs e)
    {
        if (_isPanning && _panBasePoint.HasValue)
        {
            var newPoint = e.GetPosition(this);

            var p = newPoint - _panBasePoint.Value;

            if (VScrollBar is not null)
            {
                var temp = VScrollBar.Transitions;
                VScrollBar.Transitions = null;
                VScrollBar.SetCurrentValue(RangeBase.ValueProperty, VScrollBar.Value - p.Y);
                VScrollBar.Transitions = temp;
            }

            if (HScrollBar is not null)
            {
                var temp = HScrollBar.Transitions;
                HScrollBar.Transitions = null;
                HScrollBar.SetCurrentValue(RangeBase.ValueProperty, HScrollBar.Value - p.X);
                HScrollBar.Transitions = temp;
            }

            _panBasePoint = newPoint;
        }
    }

    private void PanEnd(PointerReleasedEventArgs e)
    {
        var cp = e.GetCurrentPoint(this);
        if (_isPanning && !cp.Properties.IsMiddleButtonPressed)
        {
            _panBasePoint = null;
            Cursor        = _cursorBeforePan;
            _isPanning    = false;
        }
    }

    #endregion

    #region Milestone

    private readonly Dictionary<Milestone, MilestoneControl> _milestoneControls = new(20);

    public void ReloadMilestones()
    {
        if (_secondaryComponents is not null)
        {
            foreach (var milestoneControl in _milestoneControls.Values)
            {
                _secondaryComponents.Children.Remove(milestoneControl);
            }

            _milestoneControls.Clear();

            foreach (var milestone in _ganttModel?.Milestones ?? [])
            {
                AddMilestone(milestone);
            }
        }
    }

    private void CreateNewMilestone(DateOnly date)
    {
        var milestone = new Milestone()
                        {
                            DateTime  = date.ToDateTime(TimeOnly.MinValue),
                            IsEditing = true,
                            Title     = this.FindResource("STRING_NEW_MILESTONE") as string ?? string.Empty
                        };

        _ganttModel?.Milestones.Add(milestone);
        var milestoneControl = AddMilestone(milestone);

        milestoneControl?.ApplyTemplate();
        milestoneControl?.ShowFlyout();
    }

    public MilestoneControl? AddMilestone(Milestone milestone)
    {
        if (_milestoneControls.TryGetValue(milestone, out var existing))
        {
            return existing;
        }

        if (_secondaryComponents is not null)
        {
            var milestoneControl = new MilestoneControl()
                                   {
                                       ClipToBounds        = false,
                                       DataContext         = milestone,
                                       HorizontalAlignment = HorizontalAlignment.Left
                                   };

            var left = (milestone.DateTime - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays * DayWidth;
            milestoneControl.Margin = new Thickness(left, 0, 0, 0);

            _milestoneControls.Add(milestone, milestoneControl);
            _secondaryComponents.Children.Add(milestoneControl);

            return milestoneControl;
        }

        return null;
    }


    private void DeleteMilestone(Milestone milestone)
    {
        _ganttModel?.Milestones.Remove(milestone);

        if (_milestoneControls.Remove(milestone, out var milestoneControl))
        {
            _secondaryComponents?.Children.Remove(milestoneControl);
        }
    }

    private void MilestoneControl_HThumbDragDelta(RoutedEventArgs e)
    {
        if (e.Source is MilestoneControl { DataContext: Milestone milestone } maskMilestoneControl)
        {
            var l = maskMilestoneControl.Margin.Left;
            milestone.DateTime = StartDate.ToDateTime(TimeOnly.MinValue).AddDays(l / DayWidth);
        }
    }

    private void MilestoneControl_HThumbDragCompleted(RoutedEventArgs e)
    {
    }

    #endregion


    #region TaskContentTemplate

    public static readonly StyledProperty<IDataTemplate?> TaskContentTemplateProperty =
        AvaloniaProperty.Register<GanttControl, IDataTemplate?>(nameof(TaskContentTemplate), null);

    public IDataTemplate? TaskContentTemplate
    {
        get => GetValue(TaskContentTemplateProperty);
        set => SetValue(TaskContentTemplateProperty, value);
    }

    private void TaskContentTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion
}