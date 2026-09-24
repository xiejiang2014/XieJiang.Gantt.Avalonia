using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using XieJiang.Gantt.Avalonia.Controls;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Tests;

public class MilestoneTests
{
    private static void ClickHeaderMilestoneButton(GanttControl gantt)
    {
        var button = gantt.GetVisualDescendants().OfType<Button>().First(b => b.Classes.Contains("HeaderMilestoneButton"));
        button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }

    private static void ClickDeleteButton(MilestoneControl milestoneControl)
    {
        var headerButton = milestoneControl.GetVisualDescendants().OfType<Button>().Single(b => b.Name == "PART_HeaderButton");
        var flyout       = (Flyout)headerButton.Flyout!;
        flyout.ShowAt(headerButton);

        var deleteButton = ((Control)flyout.Content!).GetLogicalDescendants().OfType<Button>().Single(b => b.Classes.Contains("MilestoneDeleteButton"));
        deleteButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
    }

    [AvaloniaFact]
    public void Created_Milestone_Is_Added_To_Model_And_Survives_Reload()
    {
        var (window, gantt, model) = TestHelper.Create(1);

        ClickHeaderMilestoneButton(gantt);
        Assert.Single(model.Milestones);
        Assert.False(string.IsNullOrEmpty(model.Milestones[0].Title));

        gantt.Reload();
        Assert.Single(gantt.GetVisualDescendants().OfType<MilestoneControl>());
        window.Close();
    }

    [AvaloniaFact]
    public void Deleted_Milestone_Is_Removed_From_Model_And_Stays_Deleted_After_Reload()
    {
        var model = TestHelper.CreateModel(1);
        model.Milestones.Add(new Milestone { DateTime = new DateTime(2025, 1, 10) });
        var (window, gantt, _) = TestHelper.Create(model);

        ClickDeleteButton(gantt.GetVisualDescendants().OfType<MilestoneControl>().Single());
        Assert.Empty(model.Milestones);

        gantt.Reload();
        TestHelper.Layout(window);
        Assert.Empty(gantt.GetVisualDescendants().OfType<MilestoneControl>());
        window.Close();
    }

    [AvaloniaFact]
    public void AddMilestone_Twice_Does_Not_Throw()
    {
        var (window, gantt, _) = TestHelper.Create(1);
        var milestone = new Milestone { DateTime = new DateTime(2025, 1, 10) };

        var first  = gantt.AddMilestone(milestone);
        var second = gantt.AddMilestone(milestone);

        Assert.Same(first, second);
        window.Close();
    }
}
