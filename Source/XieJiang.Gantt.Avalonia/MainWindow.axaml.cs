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
                                                Width     = 200,
                                                Progress  = 0.5d,
                                                StartDate = new DateTime(2024, 12, 21),
                                                EndDate = new DateTime(2024, 12, 23),

                                            },
                                            new TaskBar()
                                            {
                                                Width     = 200,
                                                Progress  = 0.2d,
                                                StartDate = new DateTime(2024, 12, 23),
                                                EndDate   = new DateTime(2024, 12, 26),
                                            },
                                            new TaskBar()
                                            {
                                                Width     = 200,
                                                Progress  = 0.7d,
                                                StartDate = new DateTime(2024, 12, 27),
                                                EndDate   = new DateTime(2024, 12, 31),
                                            },
                                        };
        }

        private void GanttControl_OnEndDateChanged(object? sender, RoutedEventArgs e)
        {

        }

        private void Button_OnClick(object? sender, RoutedEventArgs e)
        {

            GanttControl.Reload();
        }
    }
}