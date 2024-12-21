using System;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace XieJiang.Gantt.Avalonia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            GanttControl.TaskBars = new ObservableCollection<TaskBar>()
                                        {
                                            new TaskBar()
                                            {
                                                Width = 200,
                                                Progress = 0.5d,
                                            },
                                            new TaskBar()
                                            {
                                                Width = 200,
                                                Progress = 0.2d,
                                            },
                                            new TaskBar()
                                            {
                                                Width    = 200,
                                                Progress = 0.7d,
                                            },
                                        };
        }

        private void GanttControl_OnEndDateChanged(object? sender, RoutedEventArgs e)
        {

        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {
            GanttControl.TaskBars[0].EndDate = DateTime.Now;
        }
    }
}