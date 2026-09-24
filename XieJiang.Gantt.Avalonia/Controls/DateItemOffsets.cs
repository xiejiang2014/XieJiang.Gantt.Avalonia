using System;
using System.Collections.ObjectModel;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Controls;

/// <summary>
/// Exact start positions of date items laid out side by side, used to place a virtualized panel precisely
/// so that the header and the grid stay aligned while scrolling.
/// </summary>
internal sealed class DateItemOffsets
{
    private readonly ObservableCollection<DateItem> _items;
    private          double[]?                      _starts;

    public DateItemOffsets(ObservableCollection<DateItem> items)
    {
        _items                   =  items;
        _items.CollectionChanged += (_, _) => _starts = null;
    }

    /// <summary>
    /// Returns the item containing <paramref name="offset"/> (the last one past the end) and its start position.
    /// </summary>
    public (int index, double position) Find(double offset)
    {
        if (_items.Count == 0)
        {
            return (0, 0);
        }

        var starts = _starts ??= ComputeStarts();

        // last item whose start is <= offset
        var index = Array.BinarySearch(starts, offset);
        if (index < 0)
        {
            index = Math.Max(0, ~index - 1);
        }

        return (index, starts[index]);
    }

    private double[] ComputeStarts()
    {
        var starts   = new double[_items.Count];
        var position = 0d;
        for (var i = 0; i < _items.Count; i++)
        {
            starts[i] =  position;
            position  += _items[i].Width;
        }

        return starts;
    }
}
