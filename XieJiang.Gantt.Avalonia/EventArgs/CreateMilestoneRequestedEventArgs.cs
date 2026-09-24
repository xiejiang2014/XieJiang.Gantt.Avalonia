using System;
using Avalonia.Interactivity;

namespace XieJiang.Gantt.Avalonia;

public class CreateMilestoneRequestedEventArgs(RoutedEvent routedEvent, DateOnly date) : RoutedEventArgs(routedEvent)
{
    public DateOnly Date { get; } = date;
}
