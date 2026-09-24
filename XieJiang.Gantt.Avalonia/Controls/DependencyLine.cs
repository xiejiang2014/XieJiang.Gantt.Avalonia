using XieJiang.Gantt.Avalonia.Models;
using Path = Avalonia.Controls.Shapes.Path;

namespace XieJiang.Gantt.Avalonia.Controls;

/// <summary>
/// The line drawn from <see cref="ParentTask"/> to <see cref="ChildTask"/>, which depends on it.
/// </summary>
public class DependencyLine(GanttTask parentTask, GanttTask childTask) : Path
{
    public GanttTask ParentTask { get; } = parentTask;
    public GanttTask ChildTask  { get; } = childTask;
}