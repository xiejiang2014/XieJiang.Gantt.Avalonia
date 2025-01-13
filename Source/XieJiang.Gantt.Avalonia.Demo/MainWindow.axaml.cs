using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Media;
using XieJiang.Gantt.Avalonia.Models;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace XieJiang.Gantt.Avalonia.Demo;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var ganttModel = new GanttModel();
        ganttModel.GanttTasks.Add(new GanttTask()
                                  {
                                      Id        = 1,
                                      Progress  = 0.5d,
                                      StartDate = new DateTime(2024, 12, 21),
                                      EndDate   = new DateTime(2024, 12, 23),
                                      Content = new TaskContent()
                                                {
                                                    HeaderImg = new Bitmap(AssetLoader.Open(new Uri("avares://XieJiang.Gantt.Avalonia.Demo/Assets/1.jpg"))),
                                                    Title = "Open the refrigerator",
                                                }
                                  });

        ganttModel.GanttTasks.Add(new GanttTask()
                                  {
                                      Id        = 2,
                                      Progress  = 0.2d,
                                      StartDate = new DateTime(2024, 12, 23),
                                      EndDate   = new DateTime(2024, 12, 26),
                                      Content = new TaskContent()
                                                {
                                                    HeaderImg = new Bitmap(AssetLoader.Open(new Uri("avares://XieJiang.Gantt.Avalonia.Demo/Assets/2.jpg"))),
                                                    Title = "Put the elephant in the refrigerator",
                                                }
                                  });

        ganttModel.GanttTasks.Add(new GanttTask()
                                  {
                                      Id        = 3,
                                      Progress  = 0.7d,
                                      StartDate = new DateTime(2024, 12, 27),
                                      EndDate   = new DateTime(2024, 12, 31),
                                      Content = new TaskContent()
                                                {
                                                    HeaderImg = new Bitmap(AssetLoader.Open(new Uri("avares://XieJiang.Gantt.Avalonia.Demo/Assets/3.png"))),
                                                    Title = "Close the refrigerator",
                                                }
                                  });

        ganttModel.GanttTasks[0].AddingDependentTask(ganttModel.GanttTasks[1]);
        //ganttModel.GanttTasks[1].AddingDependentTask(ganttModel.GanttTasks[2]);


        ganttModel.Milestones.Add(new Milestone()
                                  {
                                      DateTime = new DateTime(2025, 1, 6, 12, 30, 0),
                                      Title    = "Deadline hahaha",
                                      Color    = GanttColors.Success
                                  });

        GanttControl.DataContext = ganttModel;

        TreeDataGrid1.Source = new FlatTreeDataGridSource<GanttTask>(ganttModel.GanttTasks)
                               {
                                   Columns =
                                   {
                                       new TextColumn<GanttTask, int>("ID", x => x.Id, new GridLength(3,                                     GridUnitType.Auto)),
                                       new TextColumn<GanttTask, string>("Title", x => (((TaskContent)x.Content!)).Title, new GridLength(3, GridUnitType.Auto)),
                                       new TextColumn<GanttTask, DateTime>("StartDate", x => x.StartDate, new GridLength(3, GridUnitType.Auto), new()
                                                                               {
                                                                                   TextAlignment = TextAlignment.Right,
                                                                                   MaxWidth      = new GridLength(150)
                                                                               }),
                                       new TextColumn<GanttTask, DateTime>("EndDate", x => x.EndDate, new GridLength(3, GridUnitType.Auto), new()
                                                                               {
                                                                                   TextAlignment = TextAlignment.Right,
                                                                                   MaxWidth      = new GridLength(150)
                                                                               }),
                                   }
                               };
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        GanttControl.Reload();
    }


    private void GanttControl_OnDependencyLinePointerPressed(object? sender, DependencyLinePointerPressedEventArgs e)
    {
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
}