using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Media.Imaging;

namespace XieJiang.Gantt.Avalonia.Demo;

public class TaskContent : INotifyPropertyChanged
{
    private Bitmap _headerImg;

    public Bitmap HeaderImg
    {
        get => _headerImg;
        set => SetField(ref _headerImg, value);
    }

    private string _title;
    public string Title
    {
        get => _title;
        set => SetField(ref _title, value);
    }


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