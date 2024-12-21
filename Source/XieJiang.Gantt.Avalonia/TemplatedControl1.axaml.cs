using Avalonia;
using Avalonia.Controls.Primitives;

namespace XieJiang.Gantt.Avalonia;

public class TemplatedControl1 : TemplatedControl
{
    #region Row1

    public static readonly StyledProperty<double> Row1Property =
        AvaloniaProperty.Register<TemplatedControl1, double>(nameof(Row1), 40);

    public double Row1
    {
        get => GetValue(Row1Property);
        set => SetValue(Row1Property, value);
    }

    //放在静态构造行数
    //Row1Property.Changed.AddClassHandler<TemplatedControl1>((sender, e) => sender.Row1Changed(e));

    private void Row1Changed(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion


    #region Row2

    public static readonly StyledProperty<double> Row2Property =
        AvaloniaProperty.Register<TemplatedControl1, double>(nameof(Row2), 180);

    public double Row2
    {
        get => GetValue(Row2Property);
        set => SetValue(Row2Property, value);
    }

    //放在静态构造行数
    //Row2Property.Changed.AddClassHandler<TemplatedControl1>((sender, e) => sender.Row2Changed(e));

    private void Row2Changed(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion
}