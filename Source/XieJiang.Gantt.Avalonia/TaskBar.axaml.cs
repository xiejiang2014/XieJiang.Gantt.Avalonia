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
    private Border?    _backgroundBorder;
    private Border?    _foregroundBorder;
    private Thumb?     _mainThumb;
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
        _backgroundBorder  = e.NameScope.Find<Border>("PART_BackgroundBorder");
        _foregroundBorder  = e.NameScope.Find<Border>("PART_ForegroundBorder");
        _mainThumb         = e.NameScope.Find<Thumb>("PART_MainThumb");
        _lThumb            = e.NameScope.Find<Thumb>("PART_LThumb");
        _rThumb            = e.NameScope.Find<Thumb>("PART_RThumb");
        _progressThumb     = e.NameScope.Find<Thumb>("PART_ProgressThumb");
        _progressRectangle = e.NameScope.Find<Rectangle>("PART_ProgressRectangle");

        if (_mainThumb is not null)
        {
            _mainThumb.DragStarted   += Main_ThumbOnDragStarted;
            _mainThumb.DragDelta     += Main_ThumbOnDragDelta;
            _mainThumb.DragCompleted += Main_ThumbOnDragCompleted;
        }


        if (_rThumb is not null)
        {
            _rThumb.DragStarted   += RThumb_DragStarted;
            _rThumb.DragDelta     += RThumb_DragDelta;
            _rThumb.DragCompleted += RThumb_DragCompleted;
        }

        if (_lThumb is not null)
        {
            _lThumb.DragStarted   += LThumb_DragStarted;
            _lThumb.DragDelta     += LThumb_DragDelta;
            _lThumb.DragCompleted += LThumb_DragCompleted;
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

            //Debug.Print("Update");
        }
    }

    #region Drag

    #region MainDragDelta

    public static readonly RoutedEvent<RoutedEventArgs> MainDragDeltaEvent =
        RoutedEvent.Register<TaskBar, RoutedEventArgs>(nameof(MainDragDelta), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs> MainDragDelta
    {
        add => AddHandler(MainDragDeltaEvent, value);
        remove => RemoveHandler(MainDragDeltaEvent, value);
    }

    protected virtual void OnMainDragDelta()
    {
        RoutedEventArgs args = new RoutedEventArgs(MainDragDeltaEvent);
        RaiseEvent(args);
    }

    #endregion

    #region MainDragCompleted

    public static readonly RoutedEvent<RoutedEventArgs> MainDragCompletedEvent =
        RoutedEvent.Register<TaskBar, RoutedEventArgs>(nameof(MainDragCompleted), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs> MainDragCompleted
    {
        add => AddHandler(MainDragCompletedEvent, value);
        remove => RemoveHandler(MainDragCompletedEvent, value);
    }

    protected virtual void OnMainDragCompleted()
    {
        RoutedEventArgs args = new RoutedEventArgs(MainDragCompletedEvent);
        RaiseEvent(args);
    }

    #endregion




    #region WidthDragging

    public static readonly RoutedEvent<WidthDragEventArgs> WidthDragDeltaEvent =
        RoutedEvent.Register<TaskBar, WidthDragEventArgs>(nameof(WidthDragDelta), RoutingStrategies.Bubble);

    public event EventHandler<WidthDragEventArgs> WidthDragDelta
    {
        add => AddHandler(WidthDragDeltaEvent, value);
        remove => RemoveHandler(WidthDragDeltaEvent, value);
    }

    protected virtual void OnWidthDragging(Edges edge)
    {
        var args = new WidthDragEventArgs(WidthDragDeltaEvent)
                   {
                       Edge = edge
                   };
        RaiseEvent(args);
    }

    #endregion


    #region WidthDragCompleted

    public static readonly RoutedEvent<WidthDragEventArgs> WidthDragCompletedEvent =
        RoutedEvent.Register<TaskBar, WidthDragEventArgs>(nameof(WidthDragCompleted), RoutingStrategies.Bubble);

    public event EventHandler<WidthDragEventArgs> WidthDragCompleted
    {
        add => AddHandler(WidthDragCompletedEvent, value);
        remove => RemoveHandler(WidthDragCompletedEvent, value);
    }

    protected virtual void OnWidthDragCompleted(Edges edge)
    {
        var args = new WidthDragEventArgs(WidthDragCompletedEvent)
                   {
                       Edge = edge
                   };
        RaiseEvent(args);
    }

    #endregion


    private double _widthDragStarted;
    private double _leftDragStarted;

    private void Main_ThumbOnDragStarted(object? sender, VectorEventArgs e)
    {
        _leftDragStarted = Canvas.GetLeft(this);

        e.Handled = true;
    }

    private void Main_ThumbOnDragDelta(object? sender, VectorEventArgs e)
    {
        var newLeft = _leftDragStarted + e.Vector.X;
        Canvas.SetLeft(this, newLeft);
        _leftDragStarted = newLeft;

        OnMainDragDelta();

        e.Handled = true;
    }

    private void Main_ThumbOnDragCompleted(object? sender, VectorEventArgs e)
    {
        OnMainDragCompleted();
        e.Handled = true;
    }


    private void RThumb_DragStarted(object? sender, VectorEventArgs e)
    {
        _widthDragStarted = Width;
        e.Handled         = true;
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


        OnWidthDragging(Edges.Right);
        e.Handled = true;
    }

    private void RThumb_DragCompleted(object? sender, VectorEventArgs e)
    {
        OnWidthDragCompleted(Edges.Right);
        e.Handled = true;
    }

    private void LThumb_DragStarted(object? sender, VectorEventArgs e)
    {
        _widthDragStarted = Width;
        _leftDragStarted  = Canvas.GetLeft(this);
        e.Handled         = true;
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

        OnWidthDragging(Edges.Left);
        e.Handled = true;
    }

    private void LThumb_DragCompleted(object? sender, VectorEventArgs e)
    {
        OnWidthDragCompleted(Edges.Left);
        e.Handled = true;
    }

    private void ProgressThumb_DragStarted(object? sender, VectorEventArgs e)
    {
        if (_progressThumb is not null)
        {
            _leftDragStarted = Canvas.GetLeft(_progressThumb);
        }

        e.Handled = true;
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

        e.Handled = true;
    }

    #endregion
}