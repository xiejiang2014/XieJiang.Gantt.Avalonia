using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace XieJiang.Gantt.Avalonia;

public class GanttHeader : TemplatedControl
{
    static GanttHeader()
    {
        DateItemsProperty.Changed.AddClassHandler<GanttHeader>((sender, e) => sender.DateItemsChanged(e));
    }


    public GanttHeader()
    {
        //DateItems.Add(new MonthItem(new DateOnly(2024, 12, 1)));
        //DateItems.Add(new MonthItem(new DateOnly(2025, 1, 1)));
        //DateItems.Add(new MonthItem(new DateOnly(2025, 2, 1)));
        //DateItems.Add(new MonthItem(new DateOnly(2025, 3, 1)));
        //DateItems.Add(new MonthItem(new DateOnly(2025, 4, 1)));
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


    public void Reload()
    {
        var dateMode  = GetValue(GanttControl.DateModeProperty);
        var startDate = GetValue(GanttControl.StartDateProperty);
        var endDate   = GetValue(GanttControl.EndDateProperty);

        if (dateMode == DateModes.Weekly)
        {
            DateItems.Clear();
            if (startDate.Year == endDate.Year && startDate.Month == endDate.Month)
            {
                DateItems.Add(new MonthItem(startDate, endDate));
            }
        }
    }
}