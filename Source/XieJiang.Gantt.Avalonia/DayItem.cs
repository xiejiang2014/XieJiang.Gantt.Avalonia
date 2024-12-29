using System;
using System.Collections.ObjectModel;

namespace XieJiang.Gantt.Avalonia;

public class MonthItem : DateItem
{
    public MonthItem()
    {
    }

    public MonthItem(DateOnly startDay, double dayWidth, DateOnly? endDay = null)
    {
        //如果没有指定 endDay,那么 endDay 默认为 startDay 同月的最后一天
        endDay ??= new DateOnly(startDay.Year, startDay.Month, 1).AddMonths(1).AddDays(-1);

        if (startDay.Year != endDay.Value.Year || startDay.Year != endDay.Value.Year)
        {
            throw new ArgumentException("The year or month of startDay and endDay do not match.");
        }

        if (startDay > endDay)
        {
            throw new ArgumentException("startDay cannot be greater than endDay.");
        }

        Date = startDay;

        for (var i = 0; i < 35; i++)
        {
            var day = startDay.AddDays(i);

            DayItems.Add(new DayItem()
                         {
                             Date             = day,
                             IsFirstDayOfWeek = day.DayOfWeek == 0,
                             IsRestDay        = day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
                             Width            = dayWidth
                         }
                        );

            if (day == endDay)
            {
                break;
            }
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
    public double   Width { get; set; }
    public DateOnly Date  { get; set; }
}