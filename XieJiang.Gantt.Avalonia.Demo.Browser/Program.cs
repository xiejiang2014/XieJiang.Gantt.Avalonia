using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Browser;
using XieJiang.Gantt.Avalonia.Demo;

internal sealed partial class Program
{
    private static Task Main(string[] args) => BuildAvaloniaApp()
                                               //.WithInterFont()
                                              .WithFont_SourceHanSansCN()
                                              .StartBrowserAppAsync("out");

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>();
}