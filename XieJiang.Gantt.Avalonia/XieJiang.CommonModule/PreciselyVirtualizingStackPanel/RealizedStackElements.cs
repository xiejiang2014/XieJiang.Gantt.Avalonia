using Avalonia.Controls;
using Avalonia.Layout;

namespace XieJiang.CommonModule.Ava;

/// <summary>
/// 存储虚拟化面板的已实现元素状态，该虚拟化面板将其子元素排列在堆栈布局中，例如 <see cref="VirtualizingStackPanel"/>。
/// 本类仅负责管理那些当前需要实现的元素,至于不再需要的元素从被类移除后如何重复使用的问题,本类不考虑.
/// Stores the realized element state for a virtualizing panel that arranges its children
/// in a stack layout, such as <see cref="VirtualizingStackPanel"/>.
/// </summary>
internal class RealizedStackElements
{
    private int             _firstIndex;
    private List<Control?>? _elements;

    /// <summary>
    /// 所有元素的尺寸, 在水平模式下是宽度, 垂直模式下是高度
    /// </summary>
    private List<double>?   _sizes;

    private double          _startU;

    private bool            _startUUnstable;

    /// <summary>
    /// Gets the number of realized elements.
    /// </summary>
    public int Count => _elements?.Count ?? 0;

    /// <summary>
    /// Gets the index of the first realized element, or -1 if no elements are realized.
    /// </summary>
    public int FirstIndex => _elements?.Count > 0
        ? _firstIndex
        : -1;

    /// <summary>
    /// Gets the index of the last realized element, or -1 if no elements are realized.
    /// </summary>
    public int LastIndex => _elements?.Count > 0
        ? _firstIndex + _elements.Count - 1
        : -1;

    /// <summary>
    /// Gets the elements.
    /// </summary>
    public IReadOnlyList<Control?> Elements => _elements ??= new List<Control?>();

    /// <summary>
    /// Gets the sizes of the elements on the primary axis.
    /// </summary>
    public IReadOnlyList<double> SizeU => _sizes ??= new List<double>();

    /// <summary>
    /// Gets the position of the first element on the primary axis, or NaN if the position is
    /// unstable.
    /// </summary>
    public double StartU => _startUUnstable
        ? double.NaN
        : _startU;

    /// <summary>
    /// 添加一个新的已实现元素到集合中.
    /// Adds a newly realized element to the collection.
    /// </summary>
    /// <param name="index">元素的索引号. The index of the element.</param>
    /// <param name="element">元素. The element.</param>
    /// <param name="u">元素在主轴上的位置. The position of the element on the primary axis.</param>
    /// <param name="sizeU">元素在主轴上的尺寸. The size of the element on the primary axis.</param>
    public void Add(int index, Control element, double u, double sizeU)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        _elements ??= new List<Control?>();
        _sizes    ??= new List<double>();

        if (Count == 0)//添加首个元素
        {
            _elements.Add(element);
            _sizes.Add(sizeU);
            _startU     = u;
            _firstIndex = index;
        }
        else if (index == LastIndex + 1)//在结尾添加新元素
        {
            _elements.Add(element);
            _sizes.Add(sizeU);
        }
        else if (index == FirstIndex - 1)//在头部前添加新元素
        {
            --_firstIndex;
            _elements.Insert(0, element);
            _sizes.Insert(0, sizeU);
            _startU = u;
        }
        else
        {
            //只能在头尾添加新元素,不能在中间插入.
            throw new NotSupportedException("Can only add items to the beginning or end of realized elements.");
        }
    }

    /// <summary>
    /// Gets the element at the specified index, if realized.
    /// </summary>
    /// <param name="index">The index in the source collection of the element to get.</param>
    /// <returns>The element if realized; otherwise null.</returns>
    public Control? GetElement(int index)
    {
        var i = index - FirstIndex;
        if (i >= 0 && i < _elements?.Count)
            return _elements[i];
        return null;
    }

    /// <summary>
    /// 获取指定索引的元素的位置
    /// Gets the position of the element with the requested index on the primary axis, if realized.
    /// </summary>
    /// <returns>
    /// The position of the element, or NaN if the element is not realized.
    /// </returns>
    public double GetElementU(int index)
    {
        if (index < FirstIndex || _sizes is null)
            return double.NaN;

        var endIndex = index - FirstIndex;

        if (endIndex >= _sizes.Count)
            return double.NaN;

        var u = StartU;

        for (var i = 0; i < endIndex; ++i)
            u += _sizes[i];

        return u;
    }

    /// <summary>
    /// 获取指定元素的索引.
    /// Gets the index of the specified element.
    /// </summary>
    /// <param name="element">The element.</param>
    /// <returns>The index or -1 if the element is not present in the collection.</returns>
    public int GetIndex(Control element)
    {
        return _elements?.IndexOf(element) is int index && index >= 0
            ? index + FirstIndex
            : -1;
    }

    /// <summary>
    /// 已在集合中插入新的项,更新元素。
    /// Updates the elements in response to items being inserted into the source collection.
    /// </summary>
    /// <param name="index">The index in the source collection of the insert.</param>
    /// <param name="count">The number of items inserted.</param>
    /// <param name="updateElementIndexCallback">A method used to update the element indexes.</param>
    public void ItemsInserted(int index, int count, Action<Control, int, int> updateElementIndexCallback)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (_elements is null || _elements.Count == 0)
            return;

        // Get the index within the realized _elements collection.
        var first         = FirstIndex;
        var realizedIndex = index - first;

        if (realizedIndex < Count)
        {
            // The insertion point affects the realized elements. Update the index of the
            // elements after the insertion point.
            var elementCount = _elements.Count;
            var start        = Math.Max(realizedIndex, 0);

            for (var i = start; i < elementCount; ++i)
            {
                if (_elements[i] is not Control element)
                    continue;
                var oldIndex = i                               + first;
                updateElementIndexCallback(element, oldIndex, oldIndex + count);
            }

            if (realizedIndex < 0)
            {
                // The insertion point was before the first element, update the first index.
                _firstIndex     += count;
                _startUUnstable =  true;
            }
            else
            {
                // The insertion point was within the realized elements, insert an empty space
                // in _elements and _sizes.
                _elements!.InsertMany(realizedIndex, null, count);
                _sizes!.InsertMany(realizedIndex, double.NaN, count);
            }
        }
    }

    /// <summary>
    /// 已在集合中插入移除某项,更新元素。
    /// Updates the elements in response to items being removed from the source collection.
    /// </summary>
    /// <param name="index">The index in the source collection of the remove.</param>
    /// <param name="count">The number of items removed.</param>
    /// <param name="updateElementIndex">A method used to update the element indexes.</param>
    /// <param name="recycleElementCallback">A method used to recycle elements.</param>
    public void ItemsRemoved(
        int                       index,
        int                       count,
        Action<Control, int, int> updateElementIndex,
        Action<Control>           recycleElementCallback)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (_elements is null || _elements.Count == 0)
            return;

        // Get the removal start and end index within the realized _elements collection.
        var first      = FirstIndex;
        var last       = LastIndex;
        var startIndex = index         - first;
        var endIndex   = index + count - first;

        if (endIndex < 0)
        {
            // The removed range was before the realized elements. Update the first index and
            // the indexes of the realized elements.
            _firstIndex     -= count;
            _startUUnstable =  true;

            var newIndex = _firstIndex;
            for (var i = 0; i < _elements.Count; ++i)
            {
                if (_elements[i] is Control element)
                    updateElementIndex(element, newIndex + count, newIndex);
                ++newIndex;
            }
        }
        else if (startIndex < _elements.Count)
        {
            // Recycle and remove the affected elements.
            var start = Math.Max(startIndex, 0);
            var end   = Math.Min(endIndex, _elements.Count);

            for (var i = start; i < end; ++i)
            {
                if (_elements[i] is Control element)
                {
                    _elements[i] = null;
                    recycleElementCallback(element);
                }
            }

            _elements.RemoveRange(start, end - start);
            _sizes!.RemoveRange(start, end   - start);

            // If the remove started before and ended within our realized elements, then our new
            // first index will be the index where the remove started. Mark StartU as unstable
            // because we can't rely on it now to estimate element heights.
            if (startIndex <= 0 && end < last)
            {
                _firstIndex     = first = index;
                _startUUnstable = true;
            }

            // Update the indexes of the elements after the removed range.
            end = _elements.Count;
            var newIndex = first + start;
            for (var i = start; i < end; ++i)
            {
                if (_elements[i] is Control element)
                    updateElementIndex(element, newIndex + count, newIndex);
                ++newIndex;
            }
        }
    }

    /// <summary>
    /// 已在集合中替换某项
    /// Updates the elements in response to items being replaced in the source collection.
    /// </summary>
    /// <param name="index">The index in the source collection of the remove.</param>
    /// <param name="count">The number of items removed.</param>
    /// <param name="recycleElementCallback">A method used to recycle elements.</param>
    public void ItemsReplaced(int index, int count, Action<Control> recycleElementCallback)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index));
        if (_elements is null || _elements.Count == 0)
            return;

        // Get the index within the realized _elements collection.
        var startIndex = index - FirstIndex;
        var endIndex   = Math.Min(startIndex + count, Count);

        if (startIndex >= 0 && endIndex > startIndex)
        {
            for (var i = startIndex; i < endIndex; ++i)
            {
                if (_elements[i] is { } element)
                {
                    recycleElementCallback(element);
                    _elements[i] = null;
                    _sizes![i]   = double.NaN;
                }
            }
        }
    }

    /// <summary>
    /// 将指定索引之前的已实现元素全部回收(置null)
    /// Recycles elements before a specific index.
    /// </summary>
    /// <param name="index">The index in the source collection of new first element.</param>
    /// <param name="recycleElementCallback">A method used to recycle elements.</param>
    public void RecycleElementsBefore(int index, Action<Control, int> recycleElementCallback)
    {
        if (index <= FirstIndex || _elements is null || _elements.Count == 0)
            return;

        if (index > LastIndex)
        {
            RecycleAllElements(recycleElementCallback);
        }
        else
        {
            var endIndex = index - FirstIndex;

            for (var i = 0; i < endIndex; ++i)
            {
                if (_elements[i] is Control e)
                {
                    _elements[i] = null;
                    recycleElementCallback(e, i + FirstIndex);
                }
            }

            _elements.RemoveRange(0, endIndex);
            _sizes!.RemoveRange(0, endIndex);
            _firstIndex = index;
        }
    }

    /// <summary>
    /// 将指定索引之后的已实现元素全部回收(置null)
    /// </summary>
    /// <param name="index">The index in the source collection of new last element.</param>
    /// <param name="recycleElementCallback">A method used to recycle elements.</param>
    public void RecycleElementsAfter(int index, Action<Control, int> recycleElementCallback)
    {
        if (index >= LastIndex || _elements is null || _elements.Count == 0)
            return;

        if (index < FirstIndex)
        {
            RecycleAllElements(recycleElementCallback);
        }
        else
        {
            var startIndex = index + 1 - FirstIndex;
            var count      = _elements.Count;

            for (var i = startIndex; i < count; ++i)
            {
                if (_elements[i] is Control e)
                {
                    _elements[i] = null;
                    recycleElementCallback(e, i + FirstIndex);
                }
            }

            _elements.RemoveRange(startIndex, _elements.Count - startIndex);
            _sizes!.RemoveRange(startIndex, _sizes.Count      - startIndex);
        }
    }

    /// <summary>
    /// 回收所有已实现元素. 清空 _elements 集合内容会被设置为 null, 不再持有这些 Control 的引用.
    /// Recycles all realized elements.
    /// </summary>
    /// <param name="recycleElementCallback">A method used to recycle elements.</param>
    public void RecycleAllElements(Action<Control, int> recycleElementCallback)
    {
        if (_elements is null || _elements.Count == 0)
            return;

        for (var i = 0; i < _elements.Count; i++)
        {
            if (_elements[i] is Control e)
            {
                _elements[i] = null;
                recycleElementCallback(e, i + FirstIndex);
            }
        }

        _startU = _firstIndex = 0;
        _elements?.Clear();
        _sizes?.Clear();
    }

    /// <summary>
    /// 回收所有已实现元素. 清空 _elements 集合内容会被设置为 null, 不再持有这些 Control 的引用.
    /// Recycles all elements in response to the source collection being reset.
    /// </summary>
    /// <param name="recycleElementCallback">A method used to recycle elements.</param>
    public void ItemsReset(Action<Control> recycleElementCallback)
    {
        if (_elements is null || _elements.Count == 0)
            return;

        for (var i = 0; i < _elements.Count; i++)
        {
            if (_elements[i] is Control e)
            {
                _elements[i] = null;
                recycleElementCallback(e);
            }
        }

        _startU = _firstIndex = 0;
        _elements?.Clear();
        _sizes?.Clear();
    }

    /// <summary>
    /// 重置以便重复使用
    /// Resets the element list and prepares it for reuse.
    /// </summary>
    public void ResetForReuse()
    {
        _startU         = _firstIndex = 0;
        _startUUnstable = false;
        _elements?.Clear();
        _sizes?.Clear();
    }

    /// <summary>
    /// 检查所有每个元素的尺寸是否发生了改变,结果会放在 _startUUnstable 中,如果改变了则为 true 未改变则为false。
    /// Validates that <see cref="StartU"/> is still valid.
    /// </summary>
    /// <param name="orientation">The panel orientation.</param>
    /// <remarks>
    /// If the U size of any element in the realized elements has changed, then the value of
    /// <see cref="StartU"/> should be considered unstable.
    /// </remarks>
    public void ValidateStartU(Orientation orientation)
    {
        if (_elements is null || _sizes is null || _startUUnstable)
            return;

        for (var i = 0; i < _elements.Count; ++i)
        {
            if (_elements[i] is not { } element)
                continue;

            //逐个取得每个元素的宽或高
            var sizeU = orientation == Orientation.Horizontal
                ? element.DesiredSize.Width
                : element.DesiredSize.Height;

            if (sizeU != _sizes[i])
            {
                _startUUnstable = true;
                break;
            }
        }
    }
}