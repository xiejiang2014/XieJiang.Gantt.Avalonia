using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XieJiang.Gantt.Avalonia.Models;

public class GanttTask : INotifyPropertyChanged
{
    #region Dependencies

    public IReadOnlySet<GanttTask> Parents  => _parents;
    public IReadOnlySet<GanttTask> Children => _children;

    private readonly HashSet<GanttTask> _parents  = new();
    private readonly HashSet<GanttTask> _children = new();

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


    /// <summary>
    /// Returns true when <paramref name="childTask"/> can depend on this task, i.e. it is not this task,
    /// not already a child and would not create a circular dependency.
    /// </summary>
    public bool CanAddDependentTask(GanttTask childTask) => CanAddDependency(this, childTask);

    [Obsolete("Use CanAddDependentTask. Returns true when the dependency can be added.")]
    public bool CircularDependencyCheck(GanttTask childTask) => CanAddDependency(this, childTask);

    [Obsolete("Use parentTask.CanAddDependentTask(childTask).")]
    public bool CircularDependencyCheck(GanttTask parentTask, GanttTask childTask) => CanAddDependency(parentTask, childTask);

    private static bool CanAddDependency(GanttTask parentTask, GanttTask childTask)
    {
        if (ReferenceEquals(parentTask, childTask))
        {
            return false;
        }

        if (parentTask.Children.Contains(childTask))
        {
            return false;
        }

        foreach (var ancestor in Traverse(parentTask, t => t._parents))
        {
            if (ReferenceEquals(ancestor, childTask))
            {
                return false;
            }
        }

        return true;
    }

    public IEnumerable<GanttTask> TraversingChildren(GanttTask ganttTask) => Traverse(ganttTask, t => t._children);

    public IEnumerable<GanttTask> TraversingAncestors(GanttTask ganttTask) => Traverse(ganttTask, t => t._parents);

    // Each task is visited once; walking every path would be exponential for diamond-shaped dependencies.
    private static IEnumerable<GanttTask> Traverse(GanttTask start, Func<GanttTask, HashSet<GanttTask>> next)
    {
        var visited = new HashSet<GanttTask>();
        var stack   = new Stack<GanttTask>(next(start));

        while (stack.Count > 0)
        {
            var task = stack.Pop();
            if (!visited.Add(task))
            {
                continue;
            }

            yield return task;

            foreach (var t in next(task))
            {
                stack.Push(t);
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