using System.Globalization;
using Avalonia.Controls;
using XieJiang.Gantt.Avalonia.Themes;

namespace XieJiang.Gantt.Avalonia.Tests;

public class LocaleTests
{
    private static string? NewMilestoneText(GanttTheme theme) =>
        theme.TryGetResource("STRING_NEW_MILESTONE", null, out var value) ? value as string : null;

    [AvaloniaFact]
    public void Default_Locale_Is_Loaded_When_Locale_Is_Never_Set()
    {
        Assert.NotNull(NewMilestoneText(new GanttTheme()));
    }

    [AvaloniaFact]
    public void Ukrainian_Culture_Uses_Ukrainian_Strings()
    {
        var theme = new GanttTheme { Locale = new CultureInfo("uk-UA") };

        Assert.Equal("Нова віха", NewMilestoneText(theme));
    }

    [AvaloniaTheory]
    [InlineData("en-GB", "New milestone")]
    [InlineData("zh-TW", "新里程碑")]
    [InlineData("fr-FR", "New milestone")]
    public void Locale_Matches_By_Language_And_Falls_Back_To_English(string culture, string expected)
    {
        var theme = new GanttTheme { Locale = new CultureInfo(culture) };

        Assert.Equal(expected, NewMilestoneText(theme));
    }

    [AvaloniaFact]
    public void Unset_Locale_Follows_Current_UI_Culture()
    {
        var previous = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = new CultureInfo("ja-JP");
        try
        {
            Assert.Equal("新たなマイルストーン", NewMilestoneText(new GanttTheme()));
        }
        finally
        {
            CultureInfo.CurrentUICulture = previous;
        }
    }

    [AvaloniaFact]
    public void Locale_Can_Be_Changed_More_Than_Once()
    {
        var theme = new GanttTheme { Locale = new CultureInfo("zh-cn") };

        theme.Locale = new CultureInfo("ja-jp");

        Assert.Equal("新たなマイルストーン", NewMilestoneText(theme));
    }
}
