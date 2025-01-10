using Avalonia.Controls.Shapes;

namespace XieJiang.Gantt.Avalonia.Controls;

public class DependencyLine(TaskBar parentTaskBar, TaskBar childTaskBar) : Path
{
    public TaskBar ParentTaskBar { get; set; } = parentTaskBar;
    public TaskBar ChildTaskBar  { get; set; } = childTaskBar;
}