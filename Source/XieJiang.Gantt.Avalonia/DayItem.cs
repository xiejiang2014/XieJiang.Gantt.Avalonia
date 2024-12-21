using System;
using System.Collections.ObjectModel;

namespace XieJiang.Gantt.Avalonia;

public class MonthItem : DateItem
{
    public MonthItem()
    {
    }

    public MonthItem(DateOnly firstDay)
    {
        Date = firstDay;

        for (var i = 0; i < 35; i++)
        {
            var day = firstDay.AddDays(i);

            if (day.Month != firstDay.Month)
            {
                break;
            }

            DayItems.Add(new DayItem()
                         {
                             Date             = day,
                             IsFirstDayOfWeek = day.DayOfWeek == 0,
                             IsRestDay        = day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                         }
                        );


        }
    }

    public ObservableCollection<DayItem> DayItems { get; set; } = new();
}

public class DayItem : DateItem
{
    public bool IsFirstDayOfWeek { get; set; }
    public bool IsRestDay        { get; set; }
}

public class DateItem
{
    public double   Width { get; set; } = 36;
    public DateOnly Date  { get; set; }
}