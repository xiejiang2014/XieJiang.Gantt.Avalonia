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
        // Collection defaults would be shared by all instances, so each instance creates its own.
        SetCurrentValue(DateItemsProperty, new ObservableCollection<DateItem>());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _rootPanel        = e.NameScope.Find<Panel>("PART_RootPanel");
        _partItemsControl = e.NameScope.Find<ItemsControl>("PART_ItemsControl");
        _markLineToday    = e.NameScope.Find<MarkLineToday>("PART_MarkLineToday");

        ReloadMarkLineToday(_startDate, _dayWidth, _dateTimeNow);
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

    private DateItemOffsets? _dateItemOffsets;

    private (int index, double position) EstimateIndexAndPosition(PreciselyVirtualizingStackPanel sender, double viewportStartU, int itemCount) =>
        _dateItemOffsets?.Find(viewportStartU) ?? (0, 0);

    private Size OnCalculateDesiredSize(PreciselyVirtualizingStackPanel sender, Orientation orientation, int itemCount, PreciselyVirtualizingStackPanel.MeasureViewport viewport)
    {
        return new Size(Width, viewport.MeasuredV);
    }

    #region DateItems

    public static readonly StyledProperty<ObservableCollection<DateItem>> DateItemsProperty =
        AvaloniaProperty.Register<GanttBodyBackground, ObservableCollection<DateItem>>(nameof(DateItems));


    public ObservableCollection<DateItem> DateItems
    {
        get => GetValue(DateItemsProperty);
        set => SetValue(DateItemsProperty, value);
    }


    private void DateItemsChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _dateItemOffsets = DateItems is null ? null : new DateItemOffsets(DateItems);
    }

    #endregion

    public void Reload(IList<DateItem>? dateItems, DateOnly startDate, double dayWidth, DateTime? dateTimeNow)
    {
        ReloadGrid(dateItems);
        ReloadMarkLineToday(startDate, dayWidth, dateTimeNow);
    }


    public void ReloadGrid(IList<DateItem>? dateItems)
    {
        // assigned at once: adding the items one by one raises a notification per item
        SetCurrentValue(DateItemsProperty, new ObservableCollection<DateItem>(dateItems ?? []));
    }

    private DateOnly  _startDate;
    private double    _dayWidth;
    private DateTime? _dateTimeNow;

    /// <param name="dateTimeNow">The time to mark; the current time when null.</param>
    public void ReloadMarkLineToday(DateOnly startDate, double dayWidth, DateTime? dateTimeNow)
    {
        // Remember the arguments so the line can be placed once the template is applied.
        _startDate   = startDate;
        _dayWidth    = dayWidth;
        _dateTimeNow = dateTimeNow;

        if (_markLineToday != null)
        {
            var left = ((dateTimeNow ?? DateTime.Now) - startDate.ToDateTime(TimeOnly.MinValue)).TotalDays * dayWidth;

            _markLineToday.Margin = new Thickness(left, 0, 0, 0);
        }
    }

}