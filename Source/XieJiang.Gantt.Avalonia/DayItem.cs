using System;

namespace XieJiang.Gantt.Avalonia;

public class WeekItem : DateItem
{
    public DateOnly EndDate { get; set; }

    public string Header => $"{Date.Day:00} - {EndDate.Day:00}";
}

public class MonthItem : DateItem
{
    public MonthItem()
    {
    }

    public MonthItem(DateOnly startDay)
    {
        Date = startDay;
    }
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