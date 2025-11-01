using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using XieJiang.CommonModule.Ava;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Controls;

[TemplatePart("PART_RootPanel",     typeof(Panel))]
[TemplatePart("PART_ItemsControl",  typeof(ItemsControl))]
[TemplatePart("PART_MarkLineToday", typeof(MarkLineToday))]
public class GanttBodyBackground : TemplatedControl
{
    private Panel?         _rootPanel;
    private ItemsControl?  _partItemsControl;
    private MarkLineToday? _markLineToday;

    static GanttBodyBackground()
    {
        DateItemsProperty.Changed.AddClassHandler<GanttBodyBackground>((sender, e) => sender.DateItemsChanged(e));
    }

    public GanttBodyBackground()
    {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _rootPanel        = e.NameScope.Find<Panel>("PART_RootPanel");
        _partItemsControl = e.NameScope.Find<ItemsControl>("PART_ItemsControl");
        _markLineToday    = e.NameScope.Find<MarkLineToday>("PART_MarkLineToday");
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        if (_partItemsControl is not null)
        {
            var panel = (PreciselyVirtualizingStackPanel)_partItemsControl.ItemsPanelRoot!;
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
        return new Size(Width, viewport.MeasuredV);
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

    public void Reload(IList<DateItem>? dateItems, GanttModel ganttModel)
    {
        var dateMode  = GetValue(GanttControl.DateModeProperty);
        var startDate = GetValue(GanttControl.StartDateProperty);

        var dayWidth = dateMode switch
                       {
                           DateModes.Weekly     => GetValue(GanttControl.DayWidthInWeeklyModeProperty),
                           DateModes.Monthly    => GetValue(GanttControl.DayWidthInMonthlyModeProperty),
                           DateModes.Seasonally => GetValue(GanttControl.DayWidthInSeasonallyModeProperty),
                           DateModes.Yearly     => GetValue(GanttControl.DayWidthInYearlyModelProperty),
                           _                    => throw new ArgumentOutOfRangeException()
                       };
        ReloadGrid(dateItems, dayWidth);

        ReloadMarkLineToday(startDate, dayWidth);
    }


    public void ReloadGrid(IList<DateItem>? dateItems, double dayWidth)
    {
        DateItems.Clear();

        if (dateItems is not null)
        {
            foreach (var t in dateItems)
            {
                DateItems.Add(t);
            }
        }
    }

    public void ReloadMarkLineToday(DateOnly startDate, double dayWidth)
    {
        if (_markLineToday != null)
        {
            var dateTimeNow = GetValue(GanttControl.DateTimeNowProperty);
            if (dateTimeNow is null)
            {
                dateTimeNow = DateTime.Now;
            }

            var left = (dateTimeNow.Value - startDate.ToDateTime(TimeOnly.MinValue)).TotalDays * dayWidth;

            _markLineToday.Margin = new Thickness(left, 0, 0, 0);
        }
    }

}