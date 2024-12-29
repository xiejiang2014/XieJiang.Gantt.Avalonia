using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using XieJiang.CommonModule.Ava;

namespace XieJiang.Gantt.Avalonia;

public class GanttHeader : TemplatedControl
{
    private ItemsControl? _partItemsControlL1;

    static GanttHeader()
    {
        DateItemsProperty.Changed.AddClassHandler<GanttHeader>((sender, e) => sender.DateItemsChanged(e));
    }


    public GanttHeader()
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _partItemsControlL1 = e.NameScope.Find<ItemsControl>("PART_ItemsControlL1");
    }


    protected override void OnLoaded(RoutedEventArgs e)
    {
        if (_partItemsControlL1 is not null)
        {
            var panel = (PreciselyVirtualizingStackPanel)_partItemsControlL1.ItemsPanelRoot!;
            panel.EstimateIndexAndPosition = EstimateIndexAndPosition;
            panel.OnCalculateDesiredSize   = OnCalculateDesiredSize;
        }

        base.OnLoaded(e);
    }


    private (int index, double position) EstimateIndexAndPosition(PreciselyVirtualizingStackPanel sender, double viewportStartU, int itemCount)
    {
        double position = 0;

        for (var index = 0; index < DateItems.Count; index++)
        {
            if (position > viewportStartU)
            {
                return (index, position);
            }

            position += DateItems[index].Width;
        }

        return (itemCount - 1, position);
    }

    private Size OnCalculateDesiredSize(PreciselyVirtualizingStackPanel sender, Orientation orientation, int itemCount, PreciselyVirtualizingStackPanel.MeasureViewport viewport)
    {
        return new Size(Width, viewport.measuredV);
    }

    #region DateItems

    public static readonly StyledProperty<ObservableCollection<DateItem>> DateItemsProperty =
        AvaloniaProperty.Register<GanttHeader, ObservableCollection<DateItem>>(nameof(DateItems), new());


    public ObservableCollection<DateItem> DateItems
    {
        get => GetValue(DateItemsProperty);
        set => SetValue(DateItemsProperty, value);
    }

    private void DateItemsChanged(AvaloniaPropertyChangedEventArgs e)
    {
    }

    #endregion


    public List<DateItem> Reload()
    {
        var result = new List<DateItem>();

        var dayWidth  = GetValue(GanttControl.DayWidthProperty);
        var dateMode  = GetValue(GanttControl.DateModeProperty);
        var startDate = GetValue(GanttControl.StartDateProperty);
        var endDate   = GetValue(GanttControl.EndDateProperty);

        if (dateMode == DateModes.Weekly)
        {
            DateItems.Clear();
            if (startDate.Year  == endDate.Year &&
                startDate.Month == endDate.Month)
            {
                var monthItem = new MonthItem(startDate, dayWidth,endDate);
                monthItem.Width = monthItem.DayItems.Count * dayWidth;
                DateItems.Add(monthItem);
                result.AddRange(monthItem.DayItems);
            }
            else
            {
                while (true)
                {
                    var lastDayOfMonth = new DateOnly(startDate.Year, startDate.Month, 1).AddMonths(1).AddDays(-1);

                    if (lastDayOfMonth >= endDate)
                    {
                        var monthItem = new MonthItem(startDate, dayWidth, endDate);
                        monthItem.Width = monthItem.DayItems.Count * dayWidth;
                        DateItems.Add(monthItem);
                        result.AddRange(monthItem.DayItems);
                        break;
                    }
                    else
                    {
                        var monthItem = new MonthItem(startDate, dayWidth, lastDayOfMonth);
                        monthItem.Width = monthItem.DayItems.Count * dayWidth;
                        DateItems.Add(monthItem);
                        result.AddRange(monthItem.DayItems);

                        startDate = new DateOnly(startDate.Year, startDate.Month, 1).AddMonths(1);
                    }
                }
            }

            Width = result.Count * dayWidth;
        }

        return result;
    }
}