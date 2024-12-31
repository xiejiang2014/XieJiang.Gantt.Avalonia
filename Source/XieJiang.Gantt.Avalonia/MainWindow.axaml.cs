using System;
using Avalonia.Controls;
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
                                          Progress  = 0.5d,
                                          StartDate = new DateTime(2024, 12, 21),
                                          EndDate   = new DateTime(2024, 12, 23),
                                      });

            ganttModel.GanttTasks.Add(new GanttTask()
                                      {
                                          Progress  = 0.2d,
                                          StartDate = new DateTime(2024, 12, 23),
                                          EndDate   = new DateTime(2024, 12, 26),
                                      });

            ganttModel.GanttTasks.Add(new GanttTask()
                                      {
                                          Progress  = 0.7d,
                                          StartDate = new DateTime(2024, 12, 27),
                                          EndDate   = new DateTime(2024, 12, 31),
                                      });

            ganttModel.GanttTasks[0].Child     = ganttModel.GanttTasks[1];
            ganttModel.GanttTasks[1].Parent = ganttModel.GanttTasks[0];
            ganttModel.GanttTasks[1].Child     = ganttModel.GanttTasks[2];
            ganttModel.GanttTasks[2].Parent = ganttModel.GanttTasks[1];


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


    }
}