using System;

namespace XieJiang.Gantt.Avalonia.Demo;

public class MyGanttTask : GanttTask
{
    public MyGanttTask()
    {
        PropertyChanged += MyGanttTask_PropertyChanged;
    }

    private void MyGanttTask_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Progress))
        {
            OnPropertyChanged(nameof(ProgressString));
        }
    }

    public string ProgressString
    {
        get => Progress.ToString("P0");
        set
        {
            if (value.EndsWith('%') && float.TryParse(value[..^1], out var newValue))
            {
                newValue /= 100;

                if (newValue > 1)
                {
                    newValue = 1;
                }

                if (newValue < 0)
                {
                    newValue = 0;
                }

                Progress = newValue;
            }
            else if (float.TryParse(value, out var newValue2))
            {
                newValue2 /= 100;

                if (newValue2 > 1)
                {
                    newValue2 = 1;
                }

                if (newValue2 < 0)
                {
                    newValue2 = 0;
                }

                Progress = newValue2;
            }
            else
            {
                throw new ArgumentException("Invalid progress value");
            }

            OnPropertyChanged(nameof(Progress));
        }
    }
}