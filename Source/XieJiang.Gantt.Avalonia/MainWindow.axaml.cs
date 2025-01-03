using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace XieJiang.Gantt.Avalonia
{
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
                                      });

            ganttModel.GanttTasks.Add(new GanttTask()
                                      {
                                          Id        = 2,
                                          Progress  = 0.2d,
                                          StartDate = new DateTime(2024, 12, 23),
                                          EndDate   = new DateTime(2024, 12, 26),
                                      });

            ganttModel.GanttTasks.Add(new GanttTask()
                                      {
                                          Id        = 3,
                                          Progress  = 0.7d,
                                          StartDate = new DateTime(2024, 12, 27),
                                          EndDate   = new DateTime(2024, 12, 31),
                                      });

            ganttModel.GanttTasks[0].AddingDependentTask(ganttModel.GanttTasks[1]);
            //ganttModel.GanttTasks[1].AddingDependentTask(ganttModel.GanttTasks[2]);

            GanttControl.DataContext = ganttModel;
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);

            GanttControl.Reload();
        }


        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            GanttControl.Reload();
        }

        private void GanttControl_OnDependencyLinePointerPressed(object? sender, DependencyLinePointerPressedEventArgs e)
        {
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
}