using System.Collections.Generic;
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

    public void Reload(List<DateItem> dateItems)
    {
        DateItems.Clear();
        foreach (var t in dateItems)
        {
            DateItems.Add(t);
        }
    }
}