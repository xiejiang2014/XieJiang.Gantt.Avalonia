using System;
using System.Collections.Generic;
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


    public List<DateItem> Reload()
    {
        var result = new List<DateItem>();

        var dateMode  = GetValue(GanttControl.DateModeProperty);
        var startDate = GetValue(GanttControl.StartDateProperty);
        var endDate   = GetValue(GanttControl.EndDateProperty);

        if (dateMode == DateModes.Weekly)
        {
            DateItems.Clear();
            if (startDate.Year  == endDate.Year &&
                startDate.Month == endDate.Month)
            {
                DateItems.Add(new MonthItem(startDate, endDate));
            }
            else
            {
                while (true)
                {
                    var lastDayOfMonth = new DateOnly(startDate.Year, startDate.Month, 1).AddMonths(1).AddDays(-1);

                    if (lastDayOfMonth >= endDate)
                    {
                        var monthItem = new MonthItem(startDate, endDate);
                        DateItems.Add(monthItem);
                        result.AddRange(monthItem.DayItems);
                        break;
                    }
                    else
                    {
                        var monthItem = new MonthItem(startDate, lastDayOfMonth);
                        DateItems.Add(monthItem);
                        result.AddRange(monthItem.DayItems);

                        startDate = new DateOnly(startDate.Year, startDate.Month, 1).AddMonths(1);
                    }
                }
            }
        }

        return result;
    }
}