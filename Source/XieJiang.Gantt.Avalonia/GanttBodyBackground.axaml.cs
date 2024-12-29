using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using XieJiang.CommonModule.Ava;

namespace XieJiang.Gantt.Avalonia;

public class GanttBodyBackground : TemplatedControl
{
    private ItemsControl? _partItemsControl;
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
        _partItemsControl = e.NameScope.Find<ItemsControl>("PART_ItemsControl");
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
        return new Size(Width, viewport.measuredV);
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

    public void Reload(List<DateItem>? dateItems)
    {
        DateItems.Clear();

        if (dateItems is not null)
        {
            foreach (var t in dateItems)
            {
                DateItems.Add(t);
            }
        }


        var mode = GetValue(GanttControl.DateModeProperty);

        //if (mode == DateModes.Weekly)
        //{
        //    var dayWidth = GetValue(GanttControl.DayWidthProperty);
        //    Width = dateItems.Count * dayWidth;
        //}
    }
}