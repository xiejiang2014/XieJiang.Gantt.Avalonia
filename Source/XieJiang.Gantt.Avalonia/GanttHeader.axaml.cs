using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace XieJiang.Gantt.Avalonia;

public class GanttHeader : TemplatedControl
{
    static GanttHeader()
    {
        DateItemsProperty.Changed.AddClassHandler<GanttHeader>((sender,  e) => sender.DateItemsChanged(e));
        Row1HeightProperty.Changed.AddClassHandler<GanttHeader>((sender, e) => sender.Row1HeightChanged(e));
        Row2HeightProperty.Changed.AddClassHandler<GanttHeader>((sender, e) => sender.Row2HeightChanged(e));
    }


    public GanttHeader()
    {
        DateItems.Add(new MonthItem(new DateOnly(2024, 12, 1)));
        DateItems.Add(new MonthItem(new DateOnly(2025, 1, 1)));
        DateItems.Add(new MonthItem(new DateOnly(2025, 2, 1)));
        DateItems.Add(new MonthItem(new DateOnly(2025, 3, 1)));
        DateItems.Add(new MonthItem(new DateOnly(2025, 4, 1)));
    }

    #region DateItems

    public static readonly StyledProperty<ObservableCollection<DateItem>> DateItemsProperty =
        AvaloniaProperty.Register<GanttHeader, ObservableCollection<DateItem>>(nameof(DateItems), new());


    public ObservableCollection<DateItem> DateItems
    {
        get => GetValue(DateItemsProperty);
        set => SetValue(DateItemsProperty, value);
    }

    private void DateItemsChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion


    #region Row1Height

    public static readonly StyledProperty<double> Row1HeightProperty =
        AvaloniaProperty.Register<GanttHeader, double>(nameof(Row1Height), 33);

    public double Row1Height
    {
        get => GetValue(Row1HeightProperty);
        set => SetValue(Row1HeightProperty, value);
    }

    private void Row1HeightChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion

    #region Row2Height

    public static readonly StyledProperty<double> Row2HeightProperty =
        AvaloniaProperty.Register<GanttHeader, double>(nameof(Row2Height), 25);

    public double Row2Height
    {
        get => GetValue(Row2HeightProperty);
        set => SetValue(Row2HeightProperty, value);
    }

    private void Row2HeightChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion
}