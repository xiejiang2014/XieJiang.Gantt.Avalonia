using System;

namespace XieJiang.Gantt.Avalonia;

public class DayItem : DateItem
{
    public bool IsFirstDayOfWeek { get; set; }
    public bool IsRestDay        { get; set; }
}

public class DateItem
{
    public double Width { get; set; } = 40;
    public DateOnly Date  { get; set; }
}