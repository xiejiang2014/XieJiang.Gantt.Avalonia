using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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


    public IList<DateItem> Reload(GanttModel ganttModel)
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

        switch (dateMode)
        {
            case DateModes.Weekly or DateModes.Monthly:
            {
                foreach (var monthItem in LoadRow1MonthItems(startDate, endDate, dayWidth))
                {
                    Row1Items.Add(monthItem);
                }

                var s = startDate;
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
                break;
            }
            case DateModes.Seasonally:
            {
                foreach (var monthItem in LoadRow1MonthItems(startDate, endDate, dayWidth))
                {
                    Row1Items.Add(monthItem);
                }

                var s = startDate;
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

                    Debug.Print($"WeekItem {s} - {e} days:{countOfDays}  Width:{weekItem.Width}");

                    Row2Items.Add(weekItem);

                    if (e >= endDate)
                    {
                        break;
                    }

                    s = e.AddDays(1);
                }

                Width = Row2Items.Sum(v => v.Width);
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

                    Row1Items.Add(yearItem);

                    Debug.Print($"YearItem {s} - {e} days:{countOfDays}  Width:{yearItem.Width}");

                    if (e == endDate)
                    {
                        break;
                    }

                    s = e.AddDays(1);
                }

                foreach (var monthItem in LoadRow1MonthItems(startDate, endDate, dayWidth))
                {
                    Row2Items.Add(monthItem);
                }

                Width = Row2Items.Sum(v => v.Width);
                break;
            }
        }


        ReloadMilestones(ganttModel.Milestones, dayWidth);

        return Row2Items;
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

    private readonly Dictionary<Milestone, MilestoneHeader> _milestoneHeaders = new(20);

    public void ReloadMilestones(IEnumerable<Milestone> milestones, double dayWidth)
    {
        _milestoneHeaders.Clear();

        if (_rootGrid is not null)
        {
            foreach (var milestoneHeader in _milestoneHeaders.Values)
            {
                _rootGrid.Children.Remove(milestoneHeader);
            }

            var startDate = GetValue(GanttControl.StartDateProperty);
            foreach (var milestone in milestones)
            {
                AddMilestone(milestone, dayWidth, startDate);
            }
        }
    }


    public void AddMilestone(Milestone milestone, double dayWidth, DateOnly startDate)
    {
        if (_rootGrid is not null)
        {
            var milestoneHeader = new MilestoneHeader()
                                  {
                                      ClipToBounds        = false,
                                      DataContext         = milestone,
                                      HorizontalAlignment = HorizontalAlignment.Left
                                  };
            milestoneHeader.SetValue(Grid.RowSpanProperty, 2);

            var left = (milestone.DateTime - startDate.ToDateTime(TimeOnly.MinValue)).TotalDays * dayWidth;
            milestoneHeader.Margin = new Thickness(left, 0, 0, 0);

            _milestoneHeaders.Add(milestone, milestoneHeader);
            _rootGrid.Children.Add(milestoneHeader);
        }
    }

    public void RemoveMilestone(Milestone milestone)
    {
        _rootGrid?.Children.Remove(_milestoneHeaders[milestone]);

        _milestoneHeaders.Remove(milestone);
    }
}