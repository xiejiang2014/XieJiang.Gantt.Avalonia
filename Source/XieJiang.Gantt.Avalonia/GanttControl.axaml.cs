using System;
using System.Collections.Generic;
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
using Avalonia.VisualTree;
using XieJiang.Gantt.Avalonia.Controls;
using XieJiang.Gantt.Avalonia.Models;
using Avalonia.Styling;

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
        DayWidthInWeeklyModeProperty.Changed.AddClassHandler<GanttControl>((sender,     e) => sender.DayWidthInWeeklyModeChanged(e));
        DayWidthInMonthlyModeProperty.Changed.AddClassHandler<GanttControl>((sender,    e) => sender.DayWidthInMonthlyModeChanged(e));
        DayWidthInSeasonallyModeProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.DayWidthInSeasonallyModeChanged(e));
        DayWidthInYearlyModelProperty.Changed.AddClassHandler<GanttControl>((sender,    e) => sender.DayWidthInYearlyModelChanged(e));
        DateTimeNowProperty.Changed.AddClassHandler<GanttControl>((sender,              e) => sender.DateTimeNowChanged(e));

        TaskContentTemplateProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.TaskContentTemplateChanged(e));

        DragUnitProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.DragUnitChanged(e));

        StartDateProperty.Changed.AddClassHandler<GanttControl>((sender, e) => sender.StartDateChanged(e));
        EndDateProperty.Changed.AddClassHandler<GanttControl>((sender,   e) => sender.EndDateChanged(e));

        // -----------------------------   Handle events
        TaskBar.MainDragDeltaEvent.AddClassHandler<GanttControl>((sender,      e) => sender.TaskBar_MainDragDelta(e));
        TaskBar.MainDragCompletedEvent.AddClassHandler<GanttControl>((sender,  e) => sender.TaskBar_MainDragCompleted(e));
        TaskBar.WidthDragDeltaEvent.AddClassHandler<GanttControl>((sender,     e) => sender.TaskBar_WidthDragDelta(e));
        TaskBar.WidthDragCompletedEvent.AddClassHandler<GanttControl>((sender, e) => sender.TaskBar_WidthDragCompleted(e));

        MilestoneControl.HThumbDragDeltaEvent.AddClassHandler<GanttControl>((sender,     e) => sender.MilestoneControl_HThumbDragDelta(e));
        MilestoneControl.HThumbDragCompletedEvent.AddClassHandler<GanttControl>((sender, e) => sender.MilestoneControl_HThumbDragCompleted(e));

        Button.ClickEvent.AddClassHandler<GanttControl>((sender, e) => sender.ButtonClicked(e));
    }

    public GanttControl()
    {
        DataContextChanged += GanttControl_DataContextChanged;

        _pinout.DragStarted   += Pinout_DragStarted;
        _pinout.DragDelta     += Pinout_DragDelta;
        _pinout.DragCompleted += Pinout_DragCompleted;

        DateModeChanged();
    }

    private void ButtonClicked(RoutedEventArgs e)
    {
        Debug.Print($"ButtonClicked  e.Source:{e.Source}");

        if (e.Source is Button button)
        {
            if (button.DataContext is DayItem dayItem)
            {
                if (button.Classes.Contains("HeaderMilestoneButton"))
                {
                    CreateNewMilestone(dayItem);
                }
            }

            if (button.Classes.Contains("MilestoneDeleteButton") && button.DataContext is Milestone milestone)
            {
                Debug.Print("MilestoneDeleteButton");

                DeleteNewMilestone(milestone);
            }
        }
    }

    #region DataContext

    private GanttModel? _ganttModel;

    private void GanttControl_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is GanttModel ganttModel)
        {
            _ganttModel = ganttModel;
            Reload();
        }
        else
        {
            _ganttModel = null;
        }
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

        ReloadTasks();
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

    private void DayWidthInWeeklyModeChanged(AvaloniaPropertyChangedEventArgs e)
    {
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

    private void DayWidthInMonthlyModeChanged(AvaloniaPropertyChangedEventArgs e)
    {
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

    private void DayWidthInSeasonallyModeChanged(AvaloniaPropertyChangedEventArgs e)
    {
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

    private void DayWidthInYearlyModelChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion


    #region DayWidth

    public double DayWidth => DateMode switch
                              {
                                  DateModes.Weekly     => DayWidthInWeeklyMode,
                                  DateModes.Monthly    => DayWidthInMonthlyMode,
                                  DateModes.Seasonally => DayWidthInSeasonallyMode,
                                  DateModes.Yearly     => DayWidthInYearlyModel,
                                  _                    => DayWidth
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


    public void Reload()
    {
        if (_ganttModel is null)
        {
            //todo clear
            return;
        }

        if (_ganttHeader is not null)
        {
            _ganttHeader.Reload(_ganttModel);
            _ganttBodyBackground?.Reload(_ganttHeader.Row2Items, _ganttModel);
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
            return;
        }

        //load taskBars
        for (var i = 0; i < _ganttModel.GanttTasks.Count; i++)
        {
            var ganttTask = _ganttModel.GanttTasks[i];

            var width = ganttTask.DateLength.TotalDays * DayWidth;

            if (width < 20)
            {
                width = 20;
            }

            var taskBar = new TaskBar
                          {
                              MinWidth    = 20,
                              DataContext = ganttTask,
                              Width       = width,
                              Row         = i,
                              ZIndex      = 1
                          };


            taskBar.PointerEntered += TaskBar_PointerEntered;
            taskBar.PointerExited  += TaskBar_PointerExited;


            Canvas.SetLeft(taskBar, (ganttTask.StartDate      - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays * DayWidth);
            Canvas.SetTop(taskBar, i * RowHeight + (RowHeight - TaskBarHeight) / 2d);
            _canvasBody.Children.Add(taskBar);
            _taskBarsDic.Add(ganttTask, taskBar);
        }

        _canvasBody.Height = _ganttModel.GanttTasks.Count * RowHeight;

        // set parent and child
        for (var i = 0; i < _ganttModel.GanttTasks.Count; i++)
        {
            var ganttTask = _ganttModel.GanttTasks[i];
            var taskBar   = _taskBarsDic[ganttTask];

            foreach (var ganttTaskParent in ganttTask.Parents)
            {
                var parentTaskBar = _taskBarsDic[ganttTaskParent];
                taskBar.ParentsTasks.Add(parentTaskBar);
            }

            foreach (var ganttTaskChild in ganttTask.Children)
            {
                var childTaskBar = _taskBarsDic[ganttTaskChild];
                taskBar.ChildrenTasks.Add(childTaskBar);
            }
        }


        //load lines
        LoadDependencyLines();
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
                                                         Theme            = Application.Current?.FindResource("TemporaryDependencyLine") as ControlTheme,
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

    protected virtual void OnDependencyLinePointerPressed(TaskBar parentTaskBar, TaskBar childTaskBar)
    {
        var args = new DependencyLinePointerPressedEventArgs(DependencyLinePointerPressedEvent, parentTaskBar, childTaskBar);
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

    private void DependencyLine_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is DependencyLine dependencyLine)
        {
            OnDependencyLinePointerPressed(dependencyLine.ParentTaskBar, dependencyLine.ChildTaskBar);
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
                var canAdd = _pinoutTaskBar.GanttTask.CircularDependencyCheck(hoveringTaskBar.GanttTask);

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
                var canAdd = _pinoutTaskBar.GanttTask.CircularDependencyCheck(hoveringTaskBar.GanttTask);

                if (canAdd)
                {
                    _pinoutTaskBar.GanttTask.AddingDependentTask(hoveringTaskBar.GanttTask);
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
            var w = taskBar.Width;

            ganttTask.StartDate = StartDate.ToDateTime(TimeOnly.MinValue).AddDays(l       / DayWidth);
            ganttTask.EndDate   = StartDate.ToDateTime(TimeOnly.MinValue).AddDays((l + w) / DayWidth);

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

                var startDate = StartDate.ToDateTime(TimeOnly.MinValue).AddDays(l / DayWidth);
                var time      = TimeOnly.FromDateTime(startDate);
                startDate = time.Hour > 12
                    ? new DateTime(startDate.Year, startDate.Month, startDate.Day).AddDays(1)
                    : new DateTime(startDate.Year, startDate.Month, startDate.Day);

                var endDate = startDate.Add(ganttTask.DateLength);

                ganttTask.StartDate = startDate;
                ganttTask.EndDate   = endDate;
                Canvas.SetLeft(taskBar, (ganttTask.StartDate - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays * DayWidth);

                l = taskBar.GetValue(Canvas.LeftProperty);
                Canvas.SetLeft(_pinout, l + taskBar.Width);


                UpdateDependencyLine(taskBar);
            }
        }
    }

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
                    var startDate = StartDate.ToDateTime(TimeOnly.MinValue).AddDays(l / DayWidth);
                    var time      = TimeOnly.FromDateTime(startDate);

                    var temp = time.Hour > 12
                        ? new DateTime(startDate.Year, startDate.Month, startDate.Day).AddDays(1)
                        : new DateTime(startDate.Year, startDate.Month, startDate.Day);

                    if (ganttTask.EndDate - temp <= TimeSpan.FromDays(1))
                    {
                        temp = ganttTask.EndDate.AddDays(-1);
                    }

                    ganttTask.StartDate = temp;

                    taskBar.Width = ganttTask.DateLength.TotalDays * DayWidth;

                    Canvas.SetLeft(taskBar, (ganttTask.StartDate - StartDate.ToDateTime(TimeOnly.MinValue)).TotalDays * DayWidth);
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
                    var endDate = StartDate.ToDateTime(TimeOnly.MinValue).AddDays((l + w) / DayWidth);
                    var time    = TimeOnly.FromDateTime(endDate);

                    var temp = time.Hour > 12
                        ? new DateTime(endDate.Year, endDate.Month, endDate.Day).AddDays(1)
                        : new DateTime(endDate.Year, endDate.Month, endDate.Day);

                    if (temp - ganttTask.StartDate <= TimeSpan.FromDays(1))
                    {
                        temp = ganttTask.StartDate.AddDays(1);
                    }

                    ganttTask.EndDate = temp;


                    taskBar.Width = ganttTask.DateLength.TotalDays * DayWidth;
                }

                Canvas.SetLeft(_pinout, l + taskBar.Width);
            }
            //}

            switch (e.Edge)
            {
                case Edges.Left:
                    UpdateDependencyLine(taskBar, true, false);
                    break;
                case Edges.Right:
                    UpdateDependencyLine(taskBar, false);
                    break;
            }
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
        if (_secondaryComponents is not null && _ganttModel is not null)
        {
            foreach (var milestoneControl in _milestoneControls.Values)
            {
                _secondaryComponents.Children.Remove(milestoneControl);
            }

            _milestoneControls.Clear();

            foreach (var milestone in _ganttModel.Milestones)
            {
                AddMilestone(milestone);
            }
        }
    }

    private void CreateNewMilestone(DayItem dayItem)
    {
        var milestone = new Milestone()
                        {
                            DateTime  = dayItem.Date.ToDateTime(TimeOnly.MinValue),
                            IsEditing = true,
                            Title     = Application.Current.FindResource("STRING_NEW_MILESTONE") as string
                        };

        var milestoneControl = AddMilestone(milestone);

        milestoneControl?.ApplyTemplate();
        milestoneControl?.ShowFlyout();
    }

    public MilestoneControl? AddMilestone(Milestone milestone)
    {
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


    private void DeleteNewMilestone(Milestone milestone)
    {
        if (_secondaryComponents is not null)
        {
            _secondaryComponents.Children.Remove(_milestoneControls[milestone]);
        }

        _milestoneControls.Remove(milestone);
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