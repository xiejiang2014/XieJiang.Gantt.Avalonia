using Android.App;
using Android.Content.PM;
using Avalonia.Android;

namespace XieJiang.Gantt.Avalonia.Demo.Android;

[Activity(
    Label = "XieJiang.Gantt.Avalonia.Demo.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity
{
}
