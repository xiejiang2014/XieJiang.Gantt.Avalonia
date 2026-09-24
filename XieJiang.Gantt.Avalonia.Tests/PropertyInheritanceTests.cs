using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.VisualTree;
using XieJiang.Gantt.Avalonia.Controls;

namespace XieJiang.Gantt.Avalonia.Tests;

public class PropertyInheritanceTests
{
    [AvaloniaFact]
    public void GanttControl_Properties_Are_Not_Inherited_By_Descendants()
    {
        var (window, gantt, model) = TestHelper.Create(1, g => g.DateMode = DateModes.Yearly);
        var bar = TestHelper.TaskBar(gantt, model.GanttTasks[0]);

        Assert.Equal(DateModes.Weekly, bar.GetValue(GanttControl.DateModeProperty));
        window.Close();
    }

    [AvaloniaFact]
    public void Templates_Still_Follow_GanttControl_Sizes_And_Content_Template()
    {
        var template = new FuncDataTemplate<object>((_, _) => new TextBlock());
        var model    = TestHelper.CreateModel(1);
        model.Milestones.Add(new Models.Milestone { DateTime = new DateTime(2025, 1, 10) });
        var (window, gantt, _) = TestHelper.Create(model, g =>
                                                         {
                                                             g.TaskBarHeight       = 30;
                                                             g.HeaderRow1Height    = 40;
                                                             g.HeaderRow2Height    = 20;
                                                             g.TaskContentTemplate = template;
                                                         });
        var bar = TestHelper.TaskBar(gantt, model.GanttTasks[0]);

        Assert.Equal(30, bar.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_BackgroundBorder").Height);
        Assert.Same(template, bar.GetVisualDescendants().OfType<ContentPresenter>().Single(p => p.Name == "PART_ContentPresenter").ContentTemplate);

        var header     = gantt.GetVisualDescendants().OfType<GanttHeader>().Single();
        var rectangles = header.GetVisualDescendants().OfType<Rectangle>().Where(r => r.Classes.Contains("BackgroundRectangle")).ToList();
        Assert.Contains(rectangles, r => r.Height == 40);
        Assert.Contains(rectangles, r => r.Height == 20);

        var milestoneControl = gantt.GetVisualDescendants().OfType<MilestoneControl>().Single();
        Assert.Equal(40, milestoneControl.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "PART_HeaderButton").MaxHeight);

        gantt.TaskBarHeight = 50;
        TestHelper.Layout(window);
        bar = TestHelper.TaskBar(gantt, model.GanttTasks[0]);
        Assert.Equal(50, bar.GetVisualDescendants().OfType<Border>().Single(b => b.Name == "PART_BackgroundBorder").Height);
        window.Close();
    }
}
