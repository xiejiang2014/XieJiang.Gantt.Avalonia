using Avalonia.Controls.Shapes;

namespace XieJiang.Gantt.Avalonia.Controls;

public class DependencyLine : Path
{
    public TaskBar ParentTaskBar { get; set; }
    public TaskBar ChildTaskBar { get; set; }
}