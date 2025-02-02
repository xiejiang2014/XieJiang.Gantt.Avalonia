using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XieJiang.Gantt.Avalonia;

public class GanttTask : INotifyPropertyChanged
{
    #region Dependencies

    public FrozenSet<GanttTask> Parents  => _parents.ToFrozenSet();
    public FrozenSet<GanttTask> Children => _children.ToFrozenSet();

    private readonly List<GanttTask> _parents  = new();
    private readonly List<GanttTask> _children = new();

    public bool AddingDependentTask(GanttTask childTask)
    {
        _children.Add(childTask);
        childTask._parents.Add(this);

        return true;
    }

    public bool RemoveDependentTask(GanttTask childTask)
    {
        _children.Remove(childTask);
        childTask._parents.Remove(this);
        return true;
    }


    public bool CircularDependencyCheck(GanttTask childTask)
    {
        return CircularDependencyCheck(this, childTask);
    }

    public bool CircularDependencyCheck(GanttTask parentTask, GanttTask childTask)
    {
        if (ReferenceEquals(parentTask, childTask))
        {
            return false;
        }

        if (parentTask.Children.Contains(childTask))
        {
            return false;
        }

        foreach (var ancestor in TraversingAncestors(parentTask))
        {
            if (ReferenceEquals(ancestor, childTask))
            {
                return false;
            }
        }

        return true;
    }

    public IEnumerable<GanttTask> TraversingChildren(GanttTask ganttTask)
    {
        foreach (var child in ganttTask.Children)
        {
            yield return child;

            foreach (var childChild in TraversingChildren(child))
            {
                yield return childChild;
            }
        }
    }

    public IEnumerable<GanttTask> TraversingAncestors(GanttTask ganttTask)
    {
        foreach (var parent in ganttTask.Parents)
        {
            yield return parent;

            foreach (var parentParent in TraversingAncestors(parent))
            {
                yield return parentParent;
            }
        }
    }

    #endregion
    
    #region Content

    private object? _content;

    public object? Content
    {
        get => _content;
        set
        {
            if (value == _content) return;
            _content = value;
            OnPropertyChanged();
        }
    }

    #endregion


    #region Id

    private int _id;

    public int Id
    {
        get => _id;
        set
        {
            if (value == _id) return;
            _id = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Color

    private GanttColors _color;

    public GanttColors Color
    {
        get => _color;
        set
        {
            if (value == _color) return;
            _color = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Progress

    private double _progress;

    /// <summary>
    /// 0~1
    /// </summary>
    public double Progress
    {
        get => _progress;
        set
        {
            if (value.Equals(_progress)) return;
            _progress = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region DateLength

    public TimeSpan DateLength => EndDate - StartDate;

    #endregion

    #region StartDate

    private DateTime _startDate;

    public DateTime StartDate
    {
        get => _startDate;
        set
        {
            if (value.Equals(_startDate)) return;
            _startDate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DateLength));
        }
    }

    #endregion

    #region EndDate

    private DateTime _endDate;

    public DateTime EndDate
    {
        get => _endDate;
        set
        {
            if (value.Equals(_endDate)) return;
            _endDate = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DateLength));
        }
    }

    #endregion


    #region PropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}