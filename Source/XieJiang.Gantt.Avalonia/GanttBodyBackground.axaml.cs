using System;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace XieJiang.Gantt.Avalonia;

public class GanttBodyBackground : TemplatedControl
{
    static GanttBodyBackground()
    {
        DateItemsProperty.Changed.AddClassHandler<GanttBodyBackground>((sender, e) => sender.DateItemsChanged(e));
    }


    public GanttBodyBackground()
    {
        var start = new DateOnly(2024, 12, 1);
        for (var i = 0; i < 90; i++)
        {
            var day = start.AddDays(i);


            DateItems.Add(new DayItem()
                          {
                              Date             = day,
                              IsFirstDayOfWeek = day.DayOfWeek == 0,
                              IsRestDay        = day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                          });
        }
    }

    #region DateItems

    public static readonly StyledProperty<ObservableCollection<DateItem>> DateItemsProperty =
        AvaloniaProperty.Register<GanttBodyBackground, ObservableCollection<DateItem>>(nameof(DateItems), new());


    public ObservableCollection<DateItem> DateItems
    {
        get => GetValue(DateItemsProperty);
        set => SetValue(DateItemsProperty, value);
    }


    private void DateItemsChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion
}