using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;

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

    static TaskBar()
    {
        ValueProperty.Changed.AddClassHandler<TaskBar>((sender, e) => sender.ValueChanged(e));
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
        }

        Update();
    }

    #region Value

    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<TaskBar, double>(nameof(Value));

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    //放在静态构造行数
    //ValueProperty.Changed.AddClassHandler<TaskBar>((sender, e) => sender.ValueChanged(e));

    private void ValueChanged(AvaloniaPropertyChangedEventArgs e)
    {
        Update();
    }

    #endregion


    private void Update()
    {
        var p = Width * Value;

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

            Value = newLeft / Width;
        }
    }

    #endregion
}