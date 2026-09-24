using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
        TaskBar.DatesChangedEvent.AddClassHandler<GanttControl>((sender,       e) => sender.TaskBar_DatesChanged(e));

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
        ReloadTasks();
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
        AvaloniaProperty.Register<GanttControl, DateModes>(nameof(DateMode), DateModes.Weekly, true);

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
        AvaloniaProperty.Register<GanttControl, double>(nameof(DayWidthInWeeklyMode), 72d, true);

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
        AvaloniaProperty.Register<GanttControl, double>(nameof(DayWidthInMonthlyMode), 36d, true);

    public double DayWidthInMonthlyMode
    {
        get => GetValue(DayWidthInMonthlyModeProperty);
        set => SetValue(DayWidthInMonthlyModeProperty, value);
    }

    #endregion


    #region DayWidthInSeasonlyMode

    public static readonly StyledProperty<double> DayWidthInSeasonallyModeProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(DayWidthInSeasonallyMode), 12d, true);

    public double DayWidthInSeasonallyMode
    {
        get => GetValue(DayWidthInSeasonallyModeProperty);
        set => SetValue(DayWidthInSeasonallyModeProperty, value);
    }

    #endregion


    #region DayWidthInYearlyModel

    public static readonly StyledProperty<double> DayWidthInYearlyModelProperty =
        AvaloniaProperty.Register<GanttControl, double>(nameof(DayWidthInYearlyModel), 4d, true);

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
        AvaloniaProperty.Register<GanttControl, DateTime?>(nameof(DateTimeNow), null, true);

    public DateTime? DateTimeNow
    {
        get => GetValue(DateTimeNowProperty);
        set => SetValue(DateTimeNowProperty, value);
    }

    private void DateTimeNowChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _ganttBodyBackground?.ReloadMarkLineToday(StartDate, DayWidth);
    }

    // Without DateTimeNow the today line follows the clock, so it has to move while the control is shown.
    private readonly DispatcherTimer _nowTimer = new() { Interval = TimeSpan.FromMinutes(1) };

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _nowTimer.Tick += NowTimer_Tick;
        _nowTimer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _nowTimer.Stop();
        _nowTimer.Tick -= NowTimer_Tick;
    }

    private void NowTimer_Tick(object? sender, EventArgs e)
    {
        if (DateTimeNow is null)
        {
            _ganttBodyBackground?.ReloadMarkLineToday(StartDate, DayWidth);
        }
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
        // EndDate keeps its requested value and is only coerced, so re-evaluate it against the new StartDate.
        CoerceValue(EndDateProperty);
        Reload();
    }

    #endregion

    #region EndDate

    public static readonly StyledProperty<DateOnly> EndDateProperty =
        AvaloniaProperty.Register<GanttControl, DateOnly>(nameof(EndDate), DateOnly.FromDateTime(DateTime.Today), true,
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
            _ganttBodyBackground?.Reload(_ganttHeader.Row2Items, StartDate, DayWidth);
        }

        ReloadMilestones();
        ReloadTasks();
    }


    private readonly Dictionary<GanttTask, TaskBar> _taskBarsDic = new(1000);

    public void ReloadTasks()
    {
        if (_canvasBody is null)
        {
            return;
        }

        _canvasBody.Children.Clear();

        foreach (var taskBar in _taskBarsDic.Values)
        {
            taskBar.PointerEntered -= TaskBar_PointerEntered;
            taskBar.PointerExited  -= TaskBar_PointerExited;
        }

        _taskBarsDic.Clear();

        if (_ganttModel is null)
        {
            _canvasBody.Height = 0;
            return;
        }

        //load taskBars
        for (var i = 0; i < _ganttModel.GanttTasks.Count; i++)
        {
            var ganttTask = _ganttModel.GanttTasks[i];

            var taskBar = new TaskBar
                          {
                              MinWidth    = 20,
                              DataContext = ganttTask,
                              Row         = i,
                              ZIndex      = 1
                          };


            taskBar.PointerEntered += TaskBar_PointerEntered;
            taskBar.PointerExited  += TaskBar_PointerExited;


            PlaceTaskBar(taskBar, ganttTask);
            Canvas.SetTop(taskBar, i * RowHeight + (RowHeight - TaskBarHeight) / 2d);
            _canvasBody.Children.Add(taskBar);
            _taskBarsDic.Add(ganttTask, taskBar);
        }

        _canvasBody.Height = _ganttModel.GanttTasks.Count * RowHeight;

        // set parent and child, ignoring dependencies on tasks that are not in the model
        for (var i = 0; i < _ganttModel.GanttTasks.Count; i++)
        {
            var ganttTask = _ganttModel.GanttTasks[i];
            var taskBar   = _taskBarsDic[ganttTask];

            foreach (var ganttTaskParent in ganttTask.Parents)
            {
                if (_taskBarsDic.TryGetValue(ganttTaskParent, out var parentTaskBar))
                {
                    taskBar.ParentsTasks.Add(parentTaskBar);
                }
            }

            foreach (var ganttTaskChild in ganttTask.Children)
            {
                if (_taskBarsDic.TryGetValue(ganttTaskChild, out var childTaskBar))
                {
                    taskBar.ChildrenTasks.Add(childTaskBar);
                }
            }
        }


        //load lines
        LoadDependencyLines();
    }


    /// <summary>
    /// Positions and sizes the bar from the task dates. Bars never get narrower than MinWidth.
    /// </summary>
    private void PlaceTaskBar(TaskBar taskBar, GanttTask ganttTask)
    {
        taskBar.Width = Math.Max(ganttTask.DateLength.TotalDays * DayWidth, taskBar.MinWidth);
        Canvas.SetLeft(taskBar, (ganttTask.StartDate - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays * DayWidth);
    }

    private void TaskBar_DatesChanged(RoutedEventArgs e)
    {
        if (e.Source is TaskBar { GanttTask: { } ganttTask } taskBar && _taskBarsDic.ContainsKey(ganttTask))
        {
            RefreshTaskBar(taskBar, ganttTask);
        }
    }

    /// <summary>
    /// Re-places the bar from its task dates and updates everything attached to it.
    /// </summary>
    private void RefreshTaskBar(TaskBar taskBar, GanttTask ganttTask)
    {
        PlaceTaskBar(taskBar, ganttTask);
        UpdateDependencyLine(taskBar);

        if (taskBar == _pinoutTaskBar)
        {
            Canvas.SetLeft(_pinout, Canvas.GetLeft(taskBar) + taskBar.Width);
        }
    }

    private void TaskBar_PointerExited(object? sender, PointerEventArgs e)
    {
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


    private readonly Dictionary<TaskBar, Dictionary<TaskBar, DependencyLine>> _dependencyLinesDic = new(1000);

    private void SaveDependencyLine(TaskBar parentTaskBar, TaskBar childTaskBar, DependencyLine dependencyLine)
    {
        if (_dependencyLinesDic.TryGetValue(parentTaskBar, out var subDic))
        {
            subDic[childTaskBar] = dependencyLine;
        }
        else
        {
            subDic = new Dictionary<TaskBar, DependencyLine> { { childTaskBar, dependencyLine } };
            _dependencyLinesDic.Add(parentTaskBar, subDic);
        }
    }

    public DependencyLine? ReadDependencyLine(TaskBar parentTaskBar, TaskBar childTaskBar)
    {
        if (_dependencyLinesDic.TryGetValue(parentTaskBar, out var dict2))
        {
            if (dict2.TryGetValue(childTaskBar, out var value))
                return value;
        }

        return null;
    }

    private void LoadDependencyLines()
    {
        _dependencyLinesDic.Clear();
        foreach (var parentTaskBar in _taskBarsDic.Values)
        {
            foreach (var childTaskBar in parentTaskBar.ChildrenTasks)
            {
                LoadDependencyLine(parentTaskBar, childTaskBar);
            }
        }
    }

    private void LoadDependencyLine(TaskBar parentTaskBar, TaskBar childTaskBar)
    {
        var dependencyLine = new DependencyLine(parentTaskBar, childTaskBar)
                             {
                                 Classes = { "DependencyLine" },
                                 ZIndex  = 0
                             };

        dependencyLine.PointerPressed += DependencyLine_PointerPressed;

        //Debug.Print($"parentTaskBar:{parentTaskBar.GanttTask.Id} ->childTask:{childTaskBar.GanttTask.Id}");

        SaveDependencyLine(parentTaskBar, childTaskBar, dependencyLine);
        UpdateDependencyLine(parentTaskBar, childTaskBar, dependencyLine);
        _canvasBody?.Children.Add(dependencyLine);
    }

    public void RemoveDependencyLine(DependencyLine dependencyLine)
    {
        if (_dependencyLinesDic.TryGetValue(dependencyLine.ParentTaskBar, out var subDic))
        {
            subDic.Remove(dependencyLine.ChildTaskBar);
            if (subDic.Count == 0)
            {
                _dependencyLinesDic.Remove(dependencyLine.ParentTaskBar);
            }
        }

        dependencyLine.ParentTaskBar.ChildrenTasks.Remove(dependencyLine.ChildTaskBar);
        dependencyLine.ChildTaskBar.ParentsTasks.Remove(dependencyLine.ParentTaskBar);

        if (dependencyLine.ParentTaskBar.GanttTask is { } parentTask && dependencyLine.ChildTaskBar.GanttTask is { } childTask)
        {
            parentTask.RemoveDependentTask(childTask);
        }

        _canvasBody?.Children.Remove(dependencyLine);
    }

    private void DependencyLine_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is DependencyLine dependencyLine)
        {
            OnDependencyLinePointerPressed(dependencyLine);
        }
    }

    private void UpdateDependencyLine(TaskBar taskBar, bool parents = true, bool children = true)
    {
        if (parents)
        {
            foreach (var parentTaskBar in taskBar.ParentsTasks)
            {
                UpdateDependencyLine(parentTaskBar, taskBar, _dependencyLinesDic[parentTaskBar][taskBar]);
            }
        }

        if (children)
        {
            foreach (var childTaskBar in taskBar.ChildrenTasks)
            {
                var dependencyLine = _dependencyLinesDic[taskBar][childTaskBar];
                UpdateDependencyLine(taskBar, childTaskBar, dependencyLine);
            }
        }
    }

    private void UpdateDependencyLine(TaskBar parentTaskBar, TaskBar childTask, DependencyLine dependencyLine)
    {
        var       streamGeometry = new StreamGeometry();
        using var sgc            = streamGeometry.Open();

        var down = Canvas.GetTop(childTask) > Canvas.GetTop(parentTaskBar) + TaskBarHeight;

        var x0 = Canvas.GetLeft(parentTaskBar) + parentTaskBar.Width;
        var y0 = Canvas.GetTop(parentTaskBar)  + TaskBarHeight / 2d;

        if (Canvas.GetLeft(childTask) >= Canvas.GetLeft(parentTaskBar) + parentTaskBar.Width)
        {
            var x1 = Canvas.GetLeft(childTask) - (Canvas.GetLeft(parentTaskBar) + parentTaskBar.Width) + 10;
            var y1 = Canvas.GetTop(childTask) - Canvas.GetTop(parentTaskBar) + (down
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
                ? childTask.Row       * RowHeight - y0
                : (childTask.Row + 1) * RowHeight - y0;
            sgc.LineTo(new Point(x1, y2), true);

            var x3 = Canvas.GetLeft(childTask) - x0 - 18;
            sgc.LineTo(new Point(x3, y2), true);

            var y3 = Canvas.GetTop(childTask) + TaskBarHeight / 2d - y0;
            sgc.LineTo(new Point(x3, y3), true);

            var x4 = Canvas.GetLeft(childTask) - x0;
            sgc.LineTo(new Point(x4 - 3, y3), true);
            sgc.EndFigure(false);


            //arrow
            sgc.BeginFigure(new Point(x4, y3), true);
            sgc.LineTo(new Point(x4 - 9,  y3 + 4), false);
            sgc.LineTo(new Point(x4 - 9,  y3 - 4), false);
            sgc.EndFigure(false);
        }

        dependencyLine.Data = streamGeometry;
        Canvas.SetLeft(dependencyLine, x0);
        Canvas.SetTop(dependencyLine, y0);
    }

    private void HidePinout()
    {
        if (_canvasBody is not null)
        {
            _canvasBody.Children.Remove(_pinout);
        }
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

                Debug.Print($"InputHitTest:{hoveringTaskBar}({hoveringTaskBar.GanttTask.Id})");
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
                    _pinoutTaskBar.ChildrenTasks.Add(hoveringTaskBar);
                    hoveringTaskBar.ParentsTasks.Add(_pinoutTaskBar);
                    LoadDependencyLine(_pinoutTaskBar, hoveringTaskBar);
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

            Debug.Print($"TaskBar_PointerEntered {taskBar.GanttTask?.Id}");
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
            var dateLength = ganttTask.DateLength;
            ganttTask.StartDate = StartDate.ToDateTime(TimeOnly.MinValue).AddDays(l / DayWidth);
            ganttTask.EndDate   = ganttTask.StartDate + dateLength;

            Canvas.SetLeft(_pinout, l + taskBar.Width);

            UpdateDependencyLine(taskBar);
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
        if (e.Source is TaskBar { DataContext: GanttTask } taskBar)
        {
            if (e.Edge == Edges.Right)
            {
                Canvas.SetLeft(_pinout, taskBar.Bounds.Right);
                UpdateDependencyLine(taskBar, false);
            }
            else if (e.Edge == Edges.Left)
            {
                UpdateDependencyLine(taskBar, true, false);
            }
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

                Debug.Print($"Task:{ganttTask.Id} StartDate=>{ganttTask.StartDate} DateLength:{ganttTask.DateLength}");
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
        if (HScrollBar is not null && _taskBarsDic.TryGetValue(ganttTask, out var taskBar))
        {
            var nowOffset = taskBar.GetValue(Canvas.LeftProperty) - HScrollBar.Bounds.Width / 2d;
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

    private void PointerWheel(PointerWheelEventArgs e)
    {
        if (e.Delta.X != 0)
        {
            HScrollBar?.SetCurrentValue(RangeBase.ValueProperty, HScrollBar.Value - e.Delta.X * 30);
        }

        if (e.Delta.Y != 0)
        {
            VScrollBar?.SetCurrentValue(RangeBase.ValueProperty, VScrollBar.Value - e.Delta.Y * 30);
        }
    }

    #endregion


    #region Pan

    private bool   _isPanning;
    private Point? _panBasePoint;

    private void PanStart(PointerPressedEventArgs e)
    {
        var cp = e.GetCurrentPoint(this);
        if (cp.Properties.IsMiddleButtonPressed)
        {
            _panBasePoint = e.GetPosition(this);
            Cursor        = new Cursor(StandardCursorType.SizeAll);
            _isPanning    = true;
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
        if (!cp.Properties.IsMiddleButtonPressed)
        {
            _panBasePoint = null;
            Cursor        = null;
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
        AvaloniaProperty.Register<GanttControl, IDataTemplate?>(nameof(TaskContentTemplate), null, true);

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