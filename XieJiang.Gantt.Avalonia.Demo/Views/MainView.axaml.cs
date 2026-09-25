using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.VisualTree;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Avalonia;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Demo.Views;

public partial class MainView : UserControl
{
    private ScrollBar _dataGridVScrollBar;
    private ScrollBar _ganttControlHScrollBar;
    private ScrollBar _ganttControlVScrollBar;

    public MainView()
    {
        InitializeComponent();

        var ganttModel = new GanttModel();
        ganttModel.GanttTasks.Add(new MyGanttTask()
        {
            Id = 1,
            Progress = 0.5d,
            StartDate = new DateTime(2025, 02, 01),
            EndDate = new DateTime(2025, 02, 03),
            Content = new TaskContent()
            {
                HeaderImg = new Bitmap(AssetLoader.Open(new Uri("avares://XieJiang.Gantt.Avalonia.Demo/Assets/1.jpg"))),
                Title = "Open the refrigerator",
            }
        });

        ganttModel.GanttTasks.Add(new MyGanttTask()
        {
            Id = 2,
            Progress = 0.2d,
            StartDate = new DateTime(2025, 02, 04),
            EndDate = new DateTime(2025, 02, 08),
            Content = new TaskContent()
            {
                HeaderImg = new Bitmap(AssetLoader.Open(new Uri("avares://XieJiang.Gantt.Avalonia.Demo/Assets/2.jpg"))),
                Title = "Put the elephant in the refrigerator",
            },
            Color = GanttColors.Information
        });

        ganttModel.GanttTasks.Add(new MyGanttTask()
        {
            Id = 3,
            Progress = 0.7d,
            StartDate = new DateTime(2025, 02, 08),
            EndDate = new DateTime(2025, 02, 11),
            Content = new TaskContent()
            {
                HeaderImg = new Bitmap(AssetLoader.Open(new Uri("avares://XieJiang.Gantt.Avalonia.Demo/Assets/3.png"))),
                Title = "Close the refrigerator",
            },
            Color = GanttColors.Warning
        });


        // one bitmap shared by all generated tasks instead of decoding it for each of them
        var otherTaskImg = new Bitmap(AssetLoader.Open(new Uri("avares://XieJiang.Gantt.Avalonia.Demo/Assets/3.png")));

        for (int i = 4; i < 300; i++)
        {
            ganttModel.GanttTasks.Add(new MyGanttTask()
            {
                Id = i,
                Progress = 0.7d,
                StartDate = new DateTime(2025, 02, 11),
                EndDate = new DateTime(2025, 02, 15),
                Content = new TaskContent()
                {
                    HeaderImg = otherTaskImg,
                    Title = "some thing else",
                },
                Color = GanttColors.Danger
            });
        }


        ganttModel.GanttTasks[0].AddingDependentTask(ganttModel.GanttTasks[1]);
        //ganttModel.GanttTasks[1].AddingDependentTask(ganttModel.GanttTasks[2]);


        ganttModel.Milestones.Add(new Milestone()
        {
            DateTime = new DateTime(2025, 2, 15, 12, 30, 0),
            Title = "Deadline hahaha",
            Color = GanttColors.Success
        });

        GanttControl.GanttModel = ganttModel;

        DataGrid1.ItemsSource = ganttModel.GanttTasks;
    }



    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        // DataGrid has no public offset API; it scrolls through its vertical scroll bar part.
        _dataGridVScrollBar = DataGrid1.GetVisualDescendants().OfType<ScrollBar>().First(s => s.Name == "PART_VerticalScrollbar");
        _dataGridVScrollBar.ValueChanged += DataGridVScrollBar_ValueChanged;

        _ganttControlHScrollBar = GanttControl.HScrollBar;
        _ganttControlVScrollBar = GanttControl.VScrollBar;
        _ganttControlVScrollBar.ValueChanged += GanttControlVScrollBar_ValueChanged;
    }

    // DataGrid only follows its scroll bar when the user drags it and has no public offset API, so do what its
    // Scroll handler does. DynamicDependency keeps the method when trimming (AOT).
    private static readonly MethodInfo? DataGridProcessVerticalScroll =
        typeof(DataGrid).GetMethod("ProcessVerticalScroll", BindingFlags.Instance | BindingFlags.NonPublic);

    [DynamicDependency("ProcessVerticalScroll", typeof(DataGrid))]
    private void GanttControlVScrollBar_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (_dataGridVScrollBar.Value.Equals(e.NewValue))
        {
            return;
        }

        if (DataGridProcessVerticalScroll is not null)
        {
            _dataGridVScrollBar.SetCurrentValue(RangeBase.ValueProperty, e.NewValue);
            DataGridProcessVerticalScroll.Invoke(DataGrid1, [ScrollEventType.ThumbTrack]);
        }
        else if (DataGrid1.ItemsSource is System.Collections.IList { Count: > 0 } items)
        {
            // fallback: row precision only
            var row = (int)Math.Clamp(e.NewValue / GanttControl.RowHeight, 0, items.Count - 1);
            DataGrid1.ScrollIntoView(items[row], null);
        }
    }

    private void DataGridVScrollBar_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        var temp = _ganttControlVScrollBar.Transitions;
        _ganttControlVScrollBar.Transitions = null;
        _ganttControlVScrollBar.SetCurrentValue(RangeBase.ValueProperty, e.NewValue);
        _ganttControlVScrollBar.Transitions = temp;
    }

    private void GanttControl_OnDependencyLinePointerPressed(object? sender, DependencyLinePointerPressedEventArgs e)
    {
        GanttControl.RemoveDependencyLine(e.DependencyLine);
    }

    private void ButtonReload_OnClick(object? sender, RoutedEventArgs e)
    {
        GanttControl.Reload();
    }

    private void ButtonScrollToNow_OnClick(object? sender, RoutedEventArgs e)
    {
        GanttControl.ScrollToNow();
    }

    private void ButtonWeekly_OnClick(object? sender, RoutedEventArgs e)
    {
        GanttControl.DateMode = DateModes.Weekly;
    }

    private void ButtonMonthly_OnClick(object? sender, RoutedEventArgs e)
    {
        GanttControl.DateMode = DateModes.Monthly;
    }

    private void ButtonSeasonally_OnClick(object? sender, RoutedEventArgs e)
    {
        GanttControl.DateMode = DateModes.Seasonally;
    }

    private void ButtonYearly_OnClick(object? sender, RoutedEventArgs e)
    {
        GanttControl.DateMode = DateModes.Yearly;
    }

    private void DataGrid1_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var selectedItem = e.AddedItems.OfType<GanttTask>().FirstOrDefault();

        if (selectedItem is GanttTask ganttTask)
        {
            GanttControl.ScrollToTask(ganttTask);
        }
    }

    private void ToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        var app = Application.Current;
        if (app is not null)
        {
            var theme = app.ActualThemeVariant;
            app.RequestedThemeVariant = theme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
        }
    }
}