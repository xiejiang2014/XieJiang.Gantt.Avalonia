using Avalonia.Interactivity;
using XieJiang.Gantt.Avalonia.Controls;

namespace XieJiang.Gantt.Avalonia;

public class DependencyLinePointerPressedEventArgs : RoutedEventArgs
{
    public DependencyLinePointerPressedEventArgs(DependencyLine dependencyLine)
    {
        DependencyLine = dependencyLine;
    }

    public DependencyLinePointerPressedEventArgs(RoutedEvent? routedEvent, DependencyLine dependencyLine) : base(routedEvent)
    {
        DependencyLine = dependencyLine;
    }

    public DependencyLinePointerPressedEventArgs(RoutedEvent? routedEvent, object? source, DependencyLine dependencyLine) : base(routedEvent, source)
    {
        DependencyLine = dependencyLine;
    }

    

    public DependencyLine DependencyLine { get; set; }

}