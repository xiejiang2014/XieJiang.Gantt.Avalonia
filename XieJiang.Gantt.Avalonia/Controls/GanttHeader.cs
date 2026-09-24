using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using XieJiang.CommonModule.Ava;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Controls;

[TemplatePart("PART_ItemsControlRow1", typeof(ItemsControl))]
[TemplatePart("PART_ItemsControlRow2", typeof(ItemsControl))]
[TemplatePart("PART_RootGrid",         typeof(Grid))]
public class GanttHeader : TemplatedControl
{
    private ItemsControl? _partItemsControlRow1;
    private ItemsControl? _partItemsControlRow2;
    private Grid?         _rootGrid;

    static GanttHeader()
    {
        Row1ItemsProperty.Changed.AddClassHandler<GanttHeader>((sender, e) => sender.Row1ItemsChanged(e));
        Row2ItemsProperty.Changed.AddClassHandler<GanttHeader>((sender, e) => sender.Row2ItemsChanged(e));
    }


    public GanttHeader()
    {
        AddHandler(Button.ClickEvent, HeaderButton_Click);

        // Collection defaults would be shared by all instances, so each instance creates its own.
        SetCurrentValue(Row1ItemsProperty, new ObservableCollection<DateItem>());
        SetCurrentValue(Row2ItemsProperty, new ObservableCollection<DateItem>());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _partItemsControlRow1 = e.NameScope.Find<ItemsControl>("PART_ItemsControlRow1");
        _partItemsControlRow2 = e.NameScope.Find<ItemsControl>("PART_ItemsControlRow2");
        _rootGrid             = e.NameScope.Find<Grid>("PART_RootGrid");
    }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        Row1Virtualizing();
        Row2Virtualizing();

        base.OnLoaded(e);
    }


    #region CreateMilestoneRequested

    public static readonly RoutedEvent<CreateMilestoneRequestedEventArgs> CreateMilestoneRequestedEvent =
        RoutedEvent.Register<GanttHeader, CreateMilestoneRequestedEventArgs>(nameof(CreateMilestoneRequested), RoutingStrategies.Bubble);

    /// <summary>
    /// Raised when the milestone button of a day in the header is clicked.
    /// </summary>
    public event EventHandler<CreateMilestoneRequestedEventArgs> CreateMilestoneRequested
    {
        add => AddHandler(CreateMilestoneRequestedEvent, value);
        remove => RemoveHandler(CreateMilestoneRequestedEvent, value);
    }

    private void HeaderButton_Click(object? sender, RoutedEventArgs e)
    {
        if (e.Source is Button { DataContext: DayItem dayItem } button && button.Classes.Contains("HeaderMilestoneButton"))
        {
            e.Handled = true;
            RaiseEvent(new CreateMilestoneRequestedEventArgs(CreateMilestoneRequestedEvent, dayItem.Date));
        }
    }

    #endregion

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

    private DateItemOffsets? _row1Offsets;
    private DateItemOffsets? _row2Offsets;

    private (int index, double position) Row1EstimateIndexAndPosition(PreciselyVirtualizingStackPanel sender, double viewportStartU, int itemCount) =>
        _row1Offsets?.Find(viewportStartU) ?? (0, 0);

    private (int index, double position) Row2EstimateIndexAndPosition(PreciselyVirtualizingStackPanel sender, double viewportStartU, int itemCount) =>
        _row2Offsets?.Find(viewportStartU) ?? (0, 0);

    private Size OnCalculateDesiredSize(PreciselyVirtualizingStackPanel sender, Orientation orientation, int itemCount, PreciselyVirtualizingStackPanel.MeasureViewport viewport)
    {
        return new Size(Width, viewport.MeasuredV);
    }

    #endregion


    #region Row1Items

    public static readonly StyledProperty<ObservableCollection<DateItem>> Row1ItemsProperty =
        AvaloniaProperty.Register<GanttHeader, ObservableCollection<DateItem>>(nameof(Row1Items));


    public ObservableCollection<DateItem> Row1Items
    {
        get => GetValue(Row1ItemsProperty);
        set => SetValue(Row1ItemsProperty, value);
    }

    private void Row1ItemsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _row1Offsets = Row1Items is null ? null : new DateItemOffsets(Row1Items);
    }

    #endregion

    #region Row2Items

    public static readonly StyledProperty<ObservableCollection<DateItem>> Row2ItemsProperty =
        AvaloniaProperty.Register<GanttHeader, ObservableCollection<DateItem>>(nameof(Row2Items));

    public ObservableCollection<DateItem> Row2Items
    {
        get => GetValue(Row2ItemsProperty);
        set => SetValue(Row2ItemsProperty, value);
    }

    private void Row2ItemsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _row2Offsets = Row2Items is null ? null : new DateItemOffsets(Row2Items);
    }

    #endregion


    public void Reload(DateModes dateMode, DateOnly startDate, DateOnly endDate, double dayWidth)
    {
        var firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;

        // Build the items first and assign them once: adding them one by one raises a notification per item.
        var row1 = new List<DateItem>();
        var row2 = new List<DateItem>();

        switch (dateMode)
        {
            case DateModes.Weekly or DateModes.Monthly:
            {
                foreach (var monthItem in LoadRow1MonthItems(startDate, endDate, dayWidth))
                {
                    row1.Add(monthItem);
                }

                var s = startDate;
                while (true)
                {
                    row2.Add(new DayItem()
                             {
                                 Date             = s,
                                 IsFirstDayOfWeek = s.DayOfWeek == firstDayOfWeek,
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

                Width = row2.Sum(v => v.Width);
                break;
            }
            case DateModes.Seasonally:
            {
                foreach (var monthItem in LoadRow1MonthItems(startDate, endDate, dayWidth))
                {
                    row1.Add(monthItem);
                }

                var s = startDate;
                while (true)
                {
                    var dayOfWeek = ((int)s.DayOfWeek - (int)firstDayOfWeek + 7) % 7;

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

                    Debug.Print($"WeekItem {s} - {e} days:{countOfDays}  Width:{weekItem.Width}");

                    row2.Add(weekItem);

                    if (e >= endDate)
                    {
                        break;
                    }

                    s = e.AddDays(1);
                }

                Width = row2.Sum(v => v.Width);
                break;
            }
            case DateModes.Yearly:
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

                    row1.Add(yearItem);

                    Debug.Print($"YearItem {s} - {e} days:{countOfDays}  Width:{yearItem.Width}");

                    if (e == endDate)
                    {
                        break;
                    }

                    s = e.AddDays(1);
                }

                foreach (var monthItem in LoadRow1MonthItems(startDate, endDate, dayWidth))
                {
                    row2.Add(monthItem);
                }

                Width = row2.Sum(v => v.Width);
                break;
            }
        }

        SetCurrentValue(Row1ItemsProperty, new ObservableCollection<DateItem>(row1));
        SetCurrentValue(Row2ItemsProperty, new ObservableCollection<DateItem>(row2));
    }

    private static IEnumerable<MonthItem> LoadRow1MonthItems(DateOnly startDate, DateOnly endDate, double dayWidth)
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

            yield return monthItem;


            Debug.Print($"MonthItem {s} - {e} days:{countOfDays}");

            if (e == endDate)
            {
                break;
            }

            s = e.AddDays(1);
        }
    }
}