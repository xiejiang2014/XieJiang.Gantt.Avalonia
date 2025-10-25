using Avalonia.Interactivity;

namespace XieJiang.Gantt.Avalonia;

public class WidthDragEventArgs : RoutedEventArgs
{
    public WidthDragEventArgs()
    {
    }

    public WidthDragEventArgs(RoutedEvent? routedEvent) : base(routedEvent)
    {
    }

    public WidthDragEventArgs(RoutedEvent? routedEvent, object? source) : base(routedEvent, source)
    {
    }

    public Edges Edge { get; set; }
}