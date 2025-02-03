using System;

namespace XieJiang.Gantt.Avalonia.Models;

public class DateItem
{
    public double   Width       { get; set; }
    public DateOnly Date        { get; set; }
    public int      CountOfDays { get; set; }
}

public class DayItem : DateItem
{
    public bool IsFirstDayOfWeek { get; set; }
    public bool IsRestDay        { get; set; }
}

public class WeekItem : DateItem
{
    public DateOnly EndDate { get; set; }

    public string Header => $"{Date.Day:00} - {EndDate.Day:00}";
}

public class MonthItem : DateItem
{
    public string Header1 => $"{Date:y}";
    public string Header2 => $"{Date.Month}";
}

public class YearItem : DateItem
{
    public string Header => $"{Date.Year}";
}