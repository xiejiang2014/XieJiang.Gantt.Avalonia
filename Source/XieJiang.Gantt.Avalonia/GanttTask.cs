using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XieJiang.Gantt.Avalonia;

public class GanttTask : INotifyPropertyChanged
{
    public List<GanttTask> Parents  { get; set; } = new List<GanttTask>();
    public List<GanttTask> Children { get; set; } = new List<GanttTask>();

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