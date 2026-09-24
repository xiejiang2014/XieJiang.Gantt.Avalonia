using System.Globalization;
using Avalonia.VisualTree;
using XieJiang.Gantt.Avalonia.Controls;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Tests;

public class WeekStartTests
{
    [AvaloniaTheory]
    [InlineData("de-DE", DayOfWeek.Monday)]
    [InlineData("en-US", DayOfWeek.Sunday)]
    public void Weeks_Start_On_Culture_First_Day_In_Weekly_And_Seasonal_Modes(string culture, DayOfWeek firstDay)
    {
        var previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo(culture);
        try
        {
            var (window, gantt, _) = TestHelper.Create(1);
            var header = gantt.GetVisualDescendants().OfType<GanttHeader>().Single();

            Assert.All(header.Row2Items.Cast<DayItem>(), d => Assert.Equal(d.Date.DayOfWeek == firstDay, d.IsFirstDayOfWeek));

            gantt.DateMode = DateModes.Seasonally;
            TestHelper.Layout(window);

            // every week but the (possibly partial) first one starts on the first day of the week
            Assert.All(header.Row2Items.Skip(1), w => Assert.Equal(firstDay, w.Date.DayOfWeek));
            window.Close();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
