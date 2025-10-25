using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using System;
using System.Linq;
using Avalonia;
using Avalonia.Media;
using XieJiang.Gantt.Avalonia.Models;

namespace XieJiang.Gantt.Avalonia.Demo.Views;

public partial class MainView : UserControl
{
    private ScrollViewer _treeDataGridScrollViewer;
    private ScrollBar    _ganttControlHScrollBar;
    private ScrollBar    _ganttControlVScrollBar;

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
                    HeaderImg = new Bitmap(AssetLoader.Open(new Uri("avares://XieJiang.Gantt.Avalonia.Demo/Assets/3.png"))),
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

        GanttControl.DataContext = ganttModel;

        var flatTreeDataGridSource = new FlatTreeDataGridSource<MyGanttTask>(ganttModel.GanttTasks.Cast<MyGanttTask>())
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

                                             //new TextColumn<MyGanttTask, string>("Title",
                                             //                                    x => ((TaskContent)x.Content!).Title,
                                             //                                    new GridLength(1, GridUnitType.Auto)
                                             //                                   ),

                                             new TemplateColumn<MyGanttTask>("Title",
                                                                             "TextCell",
                                                                             "TextEditCell"
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

        flatTreeDataGridSource.RowSelection!.SingleSelect    =  true;
        flatTreeDataGridSource.RowSelection.SelectionChanged += RowSelection_SelectionChanged;
        TreeDataGrid1.Source                                 =  flatTreeDataGridSource;
    }



    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        _treeDataGridScrollViewer = TreeDataGrid1.Scroll as ScrollViewer;
        _treeDataGridScrollViewer.ScrollChanged += ScrollViewer1_ScrollChanged;

        _ganttControlHScrollBar = GanttControl.HScrollBar;
        _ganttControlVScrollBar = GanttControl.VScrollBar;
        _ganttControlVScrollBar.ValueChanged += GanttControlVScrollBar_ValueChanged;

        GanttControl.Reload();
    }

    private void GanttControlVScrollBar_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        _treeDataGridScrollViewer.SetCurrentValue(ScrollViewer.OffsetProperty, new Vector(_treeDataGridScrollViewer.Offset.X, e.NewValue));
    }

    private void ScrollViewer1_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        var temp = _ganttControlVScrollBar.Transitions;
        _ganttControlVScrollBar.Transitions = null;
        _ganttControlVScrollBar.SetCurrentValue(RangeBase.ValueProperty, _treeDataGridScrollViewer.Offset.Y);
        _ganttControlVScrollBar.Transitions = temp;

        e.Handled = true;
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

    private void RowSelection_SelectionChanged(object? sender, global::Avalonia.Controls.Selection.TreeSelectionModelSelectionChangedEventArgs<MyGanttTask> e)
    {
        var selectedItem = e.SelectedItems.FirstOrDefault();

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