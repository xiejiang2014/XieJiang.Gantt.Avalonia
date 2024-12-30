using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Reactive;

namespace XieJiang.Gantt.Avalonia;

[TemplatePart("PART_BackgroundBorder",  typeof(Border))]
[TemplatePart("PART_ForegroundBorder",  typeof(Border))]
[TemplatePart("PART_LThumb",            typeof(Thumb))]
[TemplatePart("PART_RThumb",            typeof(Thumb))]
[TemplatePart("PART_ProgressThumb",     typeof(Thumb))]
[TemplatePart("PART_ProgressRectangle", typeof(Rectangle))]
public class TaskBar : ContentControl
{
    private Border?    _foregroundBorder;
    private Thumb?     _lThumb;
    private Thumb?     _rThumb;
    private Thumb?     _progressThumb;
    private Rectangle? _progressRectangle;

    private GanttTask? _ganttTask;

    static TaskBar()
    {
        //ProgressProperty.Changed.AddClassHandler<TaskBar>((sender,  e) => sender.OnProgressChanged(e));
        //StartDateProperty.Changed.AddClassHandler<TaskBar>((sender, e) => sender.OnStartDateChanged(e));
        //EndDateProperty.Changed.AddClassHandler<TaskBar>((sender,   e) => sender.OnEndDateChanged(e));
    }


    public TaskBar()
    {
        DataContextChanged += TaskBar_DataContextChanged;
    }

    private void TaskBar_DataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is GanttTask ganttTask)
        {
            _ganttTask = ganttTask;
        }
        else
        {
            _ganttTask = null;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _foregroundBorder  = e.NameScope.Find<Border>("PART_ForegroundBorder");
        _lThumb            = e.NameScope.Find<Thumb>("PART_LThumb");
        _rThumb            = e.NameScope.Find<Thumb>("PART_RThumb");
        _progressThumb     = e.NameScope.Find<Thumb>("PART_ProgressThumb");
        _progressRectangle = e.NameScope.Find<Rectangle>("PART_ProgressRectangle");

        if (_rThumb is not null)
        {
            _rThumb.DragStarted += RThumb_DragStarted;
            _rThumb.DragDelta   += RThumb_DragDelta;
        }

        if (_lThumb is not null)
        {
            _lThumb.DragStarted += LThumb_DragStarted;
            _lThumb.DragDelta   += LThumb_DragDelta;
        }

        if (_progressThumb is not null)
        {
            _progressThumb.DragStarted += ProgressThumb_DragStarted;
            _progressThumb.DragDelta   += ProgressThumb_DragDelta;

            _progressThumb.GetObservable(IsVisibleProperty)
                          .Subscribe(new AnonymousObserver<bool>(b => { Update(); }));
        }

        Update();
    }

    #region Progress

    //public static readonly StyledProperty<double> ProgressProperty =
    //    AvaloniaProperty.Register<TaskBar, double>(nameof(Progress));

    ///// <summary>
    ///// 0~1
    ///// </summary>
    //public double Progress
    //{
    //    get => GetValue(ProgressProperty);
    //    set => SetValue(ProgressProperty, value);
    //}

    //private void OnProgressChanged(AvaloniaPropertyChangedEventArgs e)
    //{
    //    Update();
    //}

    #endregion


    //public TimeSpan DateLength => EndDate - StartDate;

    #region StartDate

    //public static readonly StyledProperty<DateTime> StartDateProperty =
    //    AvaloniaProperty.Register<TaskBar, DateTime>(nameof(StartDate));

    //public DateTime StartDate
    //{
    //    get => GetValue(StartDateProperty);
    //    set => SetValue(StartDateProperty, value);
    //}

    //private void OnStartDateChanged(AvaloniaPropertyChangedEventArgs e)
    //{
    //    OnStartDateChanged();
    //}

    //public static readonly RoutedEvent<RoutedEventArgs> StartDateChangedEvent =
    //    RoutedEvent.Register<TaskBar, RoutedEventArgs>(nameof(StartDateChanged), RoutingStrategies.Direct | RoutingStrategies.Bubble);

    //public event EventHandler<RoutedEventArgs> StartDateChanged
    //{
    //    add => AddHandler(StartDateChangedEvent, value);
    //    remove => RemoveHandler(StartDateChangedEvent, value);
    //}

    //protected virtual void OnStartDateChanged()
    //{
    //    RoutedEventArgs args = new RoutedEventArgs(StartDateChangedEvent);
    //    RaiseEvent(args);
    //}

    #endregion


    #region EndDate

    //public static readonly StyledProperty<DateTime> EndDateProperty =
    //    AvaloniaProperty.Register<TaskBar, DateTime>(nameof(EndDate));

    //public DateTime EndDate
    //{
    //    get => GetValue(EndDateProperty);
    //    set => SetValue(EndDateProperty, value);
    //}

    //private void OnEndDateChanged(AvaloniaPropertyChangedEventArgs e)
    //{
    //    OnEndDateChanged();
    //}


    //public static readonly RoutedEvent<RoutedEventArgs> EndDateChangedEvent =
    //    RoutedEvent.Register<TaskBar, RoutedEventArgs>(nameof(EndDateChanged), RoutingStrategies.Direct | RoutingStrategies.Bubble);

    //public event EventHandler<RoutedEventArgs> EndDateChanged
    //{
    //    add => AddHandler(EndDateChangedEvent, value);
    //    remove => RemoveHandler(EndDateChangedEvent, value);
    //}

    //protected virtual void OnEndDateChanged()
    //{
    //    RoutedEventArgs args = new RoutedEventArgs(EndDateChangedEvent);
    //    RaiseEvent(args);
    //}

    #endregion

    private void Update()
    {
        if (_ganttTask is not null)
        {
            var p = Width * _ganttTask.Progress;

            if (_foregroundBorder is not null)
            {
                _foregroundBorder.Width = p;
            }

            if (_progressRectangle is not null)
            {
                _progressRectangle.Margin = new Thickness(p, 0, 0, 0);
            }

            if (_progressThumb is not null)
            {
                Canvas.SetLeft(_progressThumb, p);
            }

            Debug.Print("Update");
        }
    }

    #region 拖动

    private double _widthDragStarted;
    private double _leftDragStarted;

    private void RThumb_DragStarted(object? sender, VectorEventArgs e)
    {
        _widthDragStarted = Width;
    }

    private void RThumb_DragDelta(object? sender, VectorEventArgs e)
    {
        var newWidth = _widthDragStarted + e.Vector.X;

        if (newWidth <= 50)
        {
            newWidth = 50;
        }


        Width             = newWidth;
        _widthDragStarted = Width;
        Update();
    }

    private void LThumb_DragStarted(object? sender, VectorEventArgs e)
    {
        _widthDragStarted = Width;
        _leftDragStarted  = Canvas.GetLeft(this);
    }

    private void LThumb_DragDelta(object? sender, VectorEventArgs e)
    {
        var newWidth = _widthDragStarted - e.Vector.X;
        if (newWidth < MinWidth)
        {
            newWidth = MinWidth;

            var newLeft = Canvas.GetLeft(this) + Width - newWidth;
            Canvas.SetLeft(this, newLeft);
            _leftDragStarted = newLeft;

            Width             = newWidth;
            _widthDragStarted = newWidth;
        }
        else
        {
            Width             = newWidth;
            _widthDragStarted = newWidth;

            var newLeft = _leftDragStarted + e.Vector.X;
            Canvas.SetLeft(this, newLeft);
            _leftDragStarted = newLeft;
        }


        Update();
    }

    private void ProgressThumb_DragStarted(object? sender, VectorEventArgs e)
    {
        if (_progressThumb is not null)
        {
            _leftDragStarted = Canvas.GetLeft(_progressThumb);
        }
    }

    private void ProgressThumb_DragDelta(object? sender, VectorEventArgs e)
    {
        if (_progressThumb is not null)
        {
            var newLeft = _leftDragStarted + e.Vector.X;
            if (newLeft < 0)
            {
                newLeft = 0;
            }
            else if (newLeft > Width)
            {
                newLeft = Width;
            }

            Canvas.SetLeft(_progressThumb, newLeft);
            _leftDragStarted = newLeft;


            if (_ganttTask is not null)
            {
                _ganttTask.Progress = newLeft / Width;
            }

            Update();
        }
    }

    #endregion
}