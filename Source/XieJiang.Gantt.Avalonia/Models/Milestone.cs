using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XieJiang.Gantt.Avalonia.Models;

public class Milestone : INotifyPropertyChanged
{
    internal bool IsEditing { get; set; }
    
    #region DateTime

    private DateTime _dateTime;

    public DateTime DateTime
    {
        get => _dateTime;
        set
        {
            if (value.Equals(_dateTime)) return;
            _dateTime = value;
            OnPropertyChanged();
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
            if (Equals(value, _content)) return;
            _content = value;
            OnPropertyChanged();
        }
    }

    #endregion
    
    #region ToolTip

    private object? _toolTip;

    public object? ToolTip
    {
        get => _toolTip;
        set
        {
            if (Equals(value, _toolTip)) return;
            _toolTip = value;
            OnPropertyChanged();
        }
    }

    #endregion
    
    #region Title

    private string _title = string.Empty;

    public string Title
    {
        get => _title;
        set
        {
            if (value == _title) return;
            _title = value;
            OnPropertyChanged();
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