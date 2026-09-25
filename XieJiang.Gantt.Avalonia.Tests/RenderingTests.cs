using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using SkiaSharp;

namespace XieJiang.Gantt.Avalonia.Tests;

public class RenderingTests
{
    [AvaloniaFact]
    public void Renders_With_SkiaSharp_4()
    {
        Assert.Equal(4, typeof(SKCanvas).Assembly.GetName().Version!.Major);

        var model = TestHelper.CreateModel(3);
        model.GanttTasks[0].AddingDependentTask(model.GanttTasks[1]);
        var (window, _, _) = TestHelper.Create(model);

        using var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);

        // the task bars are drawn with the primary brush, so some pixels must have its color
        using var stream = new MemoryStream();
        frame!.Save(stream);
        stream.Position = 0;
        using var bitmap = SKBitmap.Decode(stream);
        var primary = ((ISolidColorBrush)global::Avalonia.Application.Current!.FindResource("BrushPrimary")!).Color;
        var found = false;
        for (var y = 0; y < bitmap.Height && !found; y += 2)
        for (var x = 0; x < bitmap.Width && !found; x += 2)
        {
            var p = bitmap.GetPixel(x, y);
            found = p.Red == primary.R && p.Green == primary.G && p.Blue == primary.B;
        }

        Assert.True(found, "no task bar pixels were rendered");
        window.Close();
    }
}
