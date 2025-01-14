using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Globalization;
using System.Linq;
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
        ganttModel.GanttTasks.Add(new MyGanttTask()
                                  {
                                      Id        = 1,
                                      Progress  = 0.5d,
                                      StartDate = new DateTime(2024, 12, 21),
                                      EndDate   = new DateTime(2024, 12, 23),
                                      Content = new TaskContent()
                                                {
                                                    HeaderImg = new Bitmap(AssetLoader.Open(new Uri("avares://XieJiang.Gantt.Avalonia.Demo/Assets/1.jpg"))),
                                                    Title     = "Open the refrigerator",
                                                }
                                  });

        ganttModel.GanttTasks.Add(new MyGanttTask()
                                  {
                                      Id        = 2,
                                      Progress  = 0.2d,
                                      StartDate = new DateTime(2024, 12, 23),
                                      EndDate   = new DateTime(2024, 12, 26),
                                      Content = new TaskContent()
                                                {
                                                    HeaderImg = new Bitmap(AssetLoader.Open(new Uri("avares://XieJiang.Gantt.Avalonia.Demo/Assets/2.jpg"))),
                                                    Title     = "Put the elephant in the refrigerator",
                                                }
                                  });

        ganttModel.GanttTasks.Add(new MyGanttTask()
                                  {
                                      Id        = 3,
                                      Progress  = 0.7d,
                                      StartDate = new DateTime(2024, 12, 27),
                                      EndDate   = new DateTime(2024, 12, 31),
                                      Content = new TaskContent()
                                                {
                                                    HeaderImg = new Bitmap(AssetLoader.Open(new Uri("avares://XieJiang.Gantt.Avalonia.Demo/Assets/3.png"))),
                                                    Title     = "Close the refrigerator",
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

        TreeDataGrid1.Source = new FlatTreeDataGridSource<MyGanttTask>(ganttModel.GanttTasks.Cast<MyGanttTask>())
                               {
                                   Columns =
                                   {
                                       new TextColumn<MyGanttTask, int>("ID",
                                                                        x => x.Id,
                                                                        new GridLength(1, GridUnitType.Auto),
                                                                        new()
                                                                        {
                                                                            TextAlignment = TextAlignment.Center,
                                                                        }
                                                                       ),

                                       new TemplateColumn<MyGanttTask>("Header",
                                                                       "HeaderCell"
                                                                      ),

                                       new TextColumn<MyGanttTask, string>("Title",
                                                                           x => ((TaskContent)x.Content!).Title,
                                                                           new GridLength(1, GridUnitType.Auto)
                                                                          ),


                                       new TemplateColumn<MyGanttTask>("Progress",
                                                                       "ProgressCell",
                                                                       "ProgressCellEditing"
                                                                      ),

                                       new TextColumn<MyGanttTask, DateTime>("StartDate",
                                                                             x => x.StartDate,
                                                                             new GridLength(1, GridUnitType.Auto),
                                                                             new()
                                                                             {
                                                                                 TextAlignment = TextAlignment.Right,
                                                                                 MaxWidth      = new GridLength(150)
                                                                             }
                                                                            ),

                                       new TextColumn<MyGanttTask, DateTime>("EndDate",
                                                                             x => x.EndDate,
                                                                             new GridLength(1, GridUnitType.Auto),
                                                                             new()
                                                                             {
                                                                                 TextAlignment = TextAlignment.Right,
                                                                                 MaxWidth      = new GridLength(150)
                                                                             }
                                                                            ),
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