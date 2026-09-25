using Avalonia;
using Avalonia.Headless;
using Avalonia.Themes.Fluent;
using XieJiang.Gantt.Avalonia.Themes;

[assembly: AvaloniaTestApplication(typeof(XieJiang.Gantt.Avalonia.Tests.TestApp))]

namespace XieJiang.Gantt.Avalonia.Tests;

public class TestApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new GanttTheme());
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<TestApp>()
                  .UseSkia()
                  .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}
