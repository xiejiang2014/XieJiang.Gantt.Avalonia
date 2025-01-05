using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia;

public class GanttModel : INotifyPropertyChanged
{
    public ObservableCollection<GanttTask> GanttTasks { get; } = new();

    public ObservableCollection<Milestone> Milestones { get; } = new();

    public GanttModel()
    {
        GanttTasks.CollectionChanged += GanttTasks_CollectionChanged;
    }

    private void GanttTasks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {


    }

    #region OnPropertyChanged

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