using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using XieJiang.CommonModule.Ava;

namespace XieJiang.Gantt.Avalonia;

public class GanttHeader : TemplatedControl
{
    private ItemsControl? _partItemsControlRow1;
    private ItemsControl? _partItemsControlRow2;

    static GanttHeader()
    {
        Row1ItemsProperty.Changed.AddClassHandler<GanttHeader>((sender, e) => sender.Row1ItemsChanged(e));
        Row2ItemsProperty.Changed.AddClassHandler<GanttHeader>((sender, e) => sender.Row2ItemsChanged(e));
    }


    public GanttHeader()
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _partItemsControlRow1 = e.NameScope.Find<ItemsControl>("PART_ItemsControlRow1");
        _partItemsControlRow2 = e.NameScope.Find<ItemsControl>("PART_ItemsControlRow2");
    }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        Row1Virtualizing();
        Row2Virtualizing();

        base.OnLoaded(e);
    }


    #region Virtualizing

    private void Row1Virtualizing()
    {
        if (_partItemsControlRow1 is not null)
        {
            var panel = (PreciselyVirtualizingStackPanel)_partItemsControlRow1.ItemsPanelRoot!;
            panel.EstimateIndexAndPosition = Row1EstimateIndexAndPosition;
            panel.OnCalculateDesiredSize   = OnCalculateDesiredSize;
        }
    }

    private void Row2Virtualizing()
    {
        if (_partItemsControlRow2 is not null)
        {
            var panel = (PreciselyVirtualizingStackPanel)_partItemsControlRow2.ItemsPanelRoot!;
            panel.EstimateIndexAndPosition = Row2EstimateIndexAndPosition;
            panel.OnCalculateDesiredSize   = OnCalculateDesiredSize;
        }
    }

    private (int index, double position) Row1EstimateIndexAndPosition(PreciselyVirtualizingStackPanel sender, double viewportStartU, int itemCount)
    {
        double position = 0;

        for (var index = 0; index < Row1Items.Count; index++)
        {
            if (position > viewportStartU)
            {
                return (index, position);
            }

            position += Row1Items[index].Width;
        }

        return (itemCount - 1, position);
    }

    private (int index, double position) Row2EstimateIndexAndPosition(PreciselyVirtualizingStackPanel sender, double viewportStartU, int itemCount)
    {
        double position = 0;

        for (var index = 0; index < Row2Items.Count; index++)
        {
            if (position > viewportStartU)
            {
                return (index, position);
            }

            position += Row2Items[index].Width;
        }

        return (itemCount - 1, position);
    }

    private Size OnCalculateDesiredSize(PreciselyVirtualizingStackPanel sender, Orientation orientation, int itemCount, PreciselyVirtualizingStackPanel.MeasureViewport viewport)
    {
        return new Size(Width, viewport.measuredV);
    }

    #endregion


    #region Row1Items

    public static readonly StyledProperty<ObservableCollection<DateItem>> Row1ItemsProperty =
        AvaloniaProperty.Register<GanttHeader, ObservableCollection<DateItem>>(nameof(Row1Items), new());


    public ObservableCollection<DateItem> Row1Items
    {
        get => GetValue(Row1ItemsProperty);
        set => SetValue(Row1ItemsProperty, value);
    }

    private void Row1ItemsChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion

    #region Row2Items

    public static readonly StyledProperty<ObservableCollection<DateItem>> Row2ItemsProperty =
        AvaloniaProperty.Register<GanttHeader, ObservableCollection<DateItem>>(nameof(Row2Items), new());

    public ObservableCollection<DateItem> Row2Items
    {
        get => GetValue(Row2ItemsProperty);
        set => SetValue(Row2ItemsProperty, value);
    }

    private void Row2ItemsChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion


    public IList<DateItem> Reload()
    {
        var dateMode  = GetValue(GanttControl.DateModeProperty);
        var startDate = GetValue(GanttControl.StartDateProperty);
        var endDate   = GetValue(GanttControl.EndDateProperty);


        var dayWidth = dateMode switch
                       {
                           DateModes.Weekly     => GetValue(GanttControl.DayWidthInWeeklyModeProperty),
                           DateModes.Monthly    => GetValue(GanttControl.DayWidthInMonthlyModeProperty),
                           DateModes.Seasonally => GetValue(GanttControl.DayWidthInSeasonallyModeProperty),
                           DateModes.Yearly     => GetValue(GanttControl.DayWidthInYearlyModelProperty),
                           _                    => throw new ArgumentOutOfRangeException()
                       };


        Row1Items.Clear();
        Row2Items.Clear();

        if (dateMode is DateModes.Weekly or DateModes.Monthly)
        {
            var s = startDate;
            while (true)
            {
                var e = new DateOnly(s.Year, s.Month, 1).AddMonths(1).AddDays(-1);
                if (e > endDate)
                {
                    e = endDate;
                }

                var countOfDays = (int)(e.ToDateTime(TimeOnly.MinValue) - s.ToDateTime(TimeOnly.MinValue)).TotalDays + 1;

                var monthItem = new MonthItem()
                                {
                                    Width       = countOfDays * dayWidth,
                                    Date        = s,
                                    CountOfDays = countOfDays
                                };
                Row1Items.Add(monthItem);

                Debug.Print($"MonthItem {s} - {e} days:{countOfDays}");

                if (e == endDate)
                {
                    break;
                }

                s = e.AddDays(1);
            }


            s = startDate;
            while (true)
            {
                Row2Items.Add(new DayItem()
                              {
                                  Date             = s,
                                  IsFirstDayOfWeek = s.DayOfWeek == 0,
                                  IsRestDay        = s.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
                                  Width            = dayWidth,
                                  CountOfDays      = 1
                              });

                if (s >= endDate)
                {
                    break;
                }

                s = s.AddDays(1);
            }

            Width = Row2Items.Sum(v => v.Width);
        }
        else if (dateMode is DateModes.Seasonally)
        {
            var s = startDate;
            while (true)
            {
                var e = new DateOnly(s.Year, s.Month, 1).AddMonths(1).AddDays(-1);
                if (e > endDate)
                {
                    e = endDate;
                }

                var countOfDays = (int)(e.ToDateTime(TimeOnly.MinValue) - s.ToDateTime(TimeOnly.MinValue)).TotalDays + 1;

                var monthItem = new MonthItem()
                                {
                                    Width       = countOfDays * dayWidth,
                                    Date        = s,
                                    CountOfDays = countOfDays
                                };
                Row1Items.Add(monthItem);

                Debug.Print($"MonthItem {s} - {endDate} days:{countOfDays}");

                if (e == endDate)
                {
                    break;
                }

                s = e.AddDays(1);
            }

            s = startDate;
            while (true)
            {
                var dayOfWeek = (int)s.DayOfWeek - 1;
                if (dayOfWeek < 0)
                {
                    dayOfWeek = 6;
                }

                var e = s.AddDays(6 - dayOfWeek);

                if (e > endDate)
                {
                    e = endDate;
                }

                var countOfDays = (int)(e.ToDateTime(TimeOnly.MinValue) - s.ToDateTime(TimeOnly.MinValue)).TotalDays + 1;

                var weekItem = new WeekItem()
                               {
                                   Date        = s,
                                   EndDate     = e,
                                   Width       = dayWidth * countOfDays,
                                   CountOfDays = countOfDays
                };

                Debug.Print($"WeekItem {s} - {e} days:{countOfDays}");

                Row2Items.Add(weekItem);

                if (e >= endDate)
                {
                    break;
                }

                s = e.AddDays(1);
            }

            Width = Row2Items.Sum(v => v.Width);
        }
        else if (dateMode is DateModes.Yearly)
        {
            var s = startDate;
            while (true)
            {
                var e = new DateOnly(s.Year, 1, 1).AddYears(1).AddDays(-1);

                if (e > endDate)
                {
                    e = endDate;
                }

                var countOfDays = (int)(e.ToDateTime(TimeOnly.MinValue) - s.ToDateTime(TimeOnly.MinValue)).TotalDays + 1;

                var yearItem = new YearItem()
                               {
                                   Date        = s,
                                   Width       = countOfDays * dayWidth,
                                   CountOfDays = countOfDays
                               };

                Row1Items.Add(yearItem);

                Debug.Print($"YearItem {s} - {e} days:{countOfDays}  Width:{yearItem.Width}");

                if (e == endDate)
                {
                    break;
                }

                s = e.AddDays(1);
            }

            s = startDate;
            while (true)
            {
                var e = new DateOnly(s.Year, s.Month, 1).AddMonths(1).AddDays(-1);

                if (e > endDate)
                {
                    e = endDate;
                }

                var countOfDays = (int)(e.ToDateTime(TimeOnly.MinValue) - s.ToDateTime(TimeOnly.MinValue)).TotalDays + 1;
                var monthItem = new MonthItem()
                                {
                                    Width       = countOfDays * dayWidth,
                                    Date        = s,
                                    CountOfDays = countOfDays
                                };
                Row2Items.Add(monthItem);

                Debug.Print($"MonthItem {s} - {e} days:{countOfDays}  Width:{monthItem.Width}");


                if (e == endDate)
                {
                    break;
                }

                s = e.AddDays(1);
            }

            Width = Row2Items.Sum(v => v.Width);
        }


        return Row2Items;
    }
}