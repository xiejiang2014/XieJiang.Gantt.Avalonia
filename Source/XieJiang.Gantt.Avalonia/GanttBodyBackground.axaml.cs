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
        DateItems.Add(new DayItem()
                      {
                          Date      = new DateOnly(2024, 12, 21),
                          IsRestDay = true
                      });
        DateItems.Add(new DayItem()
                      {
                          Date             = new DateOnly(2024, 12, 22),
                          IsRestDay        = true,
                          IsFirstDayOfWeek = true
                      });
        DateItems.Add(new DayItem()
                      {
                          Date      = new DateOnly(2024, 12, 23),
                          IsRestDay = false
                      });
        DateItems.Add(new DayItem()
                      {
                          Date      = new DateOnly(2024, 12, 24),
                          IsRestDay = false
                      });
        DateItems.Add(new DayItem()
                      {
                          Date      = new DateOnly(2024, 12, 25),
                          IsRestDay = false
                      });
        DateItems.Add(new DayItem()
                      {
                          Date      = new DateOnly(2024, 12, 26),
                          IsRestDay = false
                      });
        DateItems.Add(new DayItem()
                      {
                          Date      = new DateOnly(2024, 12, 27),
                          IsRestDay = false
                      });
        DateItems.Add(new DayItem()
                      {
                          Date      = new DateOnly(2024, 12, 28),
                          IsRestDay = true
                      });
        DateItems.Add(new DayItem()
                      {
                          Date      = new DateOnly(2024, 12, 29),
                          IsRestDay = true,
                          IsFirstDayOfWeek = true
                      });
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