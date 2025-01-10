using Avalonia.Interactivity;
using XieJiang.Gantt.Avalonia.Controls;

namespace XieJiang.Gantt.Avalonia;

public class DependencyLinePointerPressedEventArgs : RoutedEventArgs
{
    public DependencyLinePointerPressedEventArgs(TaskBar parentTaskBar, TaskBar childTaskBar)
    {
        ParentTaskBar = parentTaskBar;
        ChildTaskBar  = childTaskBar;
    }

    public DependencyLinePointerPressedEventArgs(RoutedEvent? routedEvent, TaskBar parentTaskBar, TaskBar childTaskBar) : base(routedEvent)
    {
        ParentTaskBar = parentTaskBar;
        ChildTaskBar  = childTaskBar;
    }

    public DependencyLinePointerPressedEventArgs(RoutedEvent? routedEvent, object? source, TaskBar parentTaskBar, TaskBar childTaskBar) : base(routedEvent, source)
    {
        ParentTaskBar = parentTaskBar;
        ChildTaskBar  = childTaskBar;
    }

    public TaskBar ParentTaskBar { get; set; }
    public TaskBar ChildTaskBar  { get; set; }

}