using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace XieJiang.Gantt.Avalonia;

public class GanttHeader : Control
{
    private IPen _penGrid = new Pen(new SolidColorBrush(Colors.Black));

    public override void Render(DrawingContext dc)
    {
        var dateMode  = GetValue(GanttControl.DateModeProperty);
        var startDate = GetValue(GanttControl.StartDateProperty);
        var endDate   = GetValue(GanttControl.EndDateProperty);


        DrawMonth(dc, 0, 60, 32, 25, startDate);
    }

    private void DrawMonth(DrawingContext dc,
                           double         x,
                           double         dayWidth,
                           double         row0Height,
                           double         row1Height,
                           DateTime       firstDayOfMonth)
    {
        var daysInMonth = DateTime.DaysInMonth(firstDayOfMonth.Year, firstDayOfMonth.Month);
        var monthWidth  = daysInMonth * dayWidth;

        //上横线
        dc.DrawLine(_penGrid,
                    new Point(x,          0.5),
                    new Point(monthWidth, 0.5)
                   );

        //中横线
        dc.DrawLine(_penGrid,
                    new Point(x,          row0Height + 0.5),
                    new Point(monthWidth, row0Height + 0.5)
                   );

        //左竖线
        dc.DrawLine(_penGrid,
                    new Point(0.5, 0),
                    new Point(0.5, row0Height + row1Height)
                   );

        //右竖线
        dc.DrawLine(_penGrid,
                    new Point(monthWidth + 0.5, 0),
                    new Point(monthWidth + 0.5, row0Height)
                   );

        var fText = new FormattedText(firstDayOfMonth.ToString("Y"),
                                      CultureInfo.CurrentCulture,
                                      FlowDirection.LeftToRight,
                                      Typeface.Default,
                                      14d,
                                      Brushes.Black
                                     );

        dc.DrawText(fText,
                    new Point(4, //left margin
                              0 + (row0Height - fText.Height) / 2
                             )
                   );

        DrawDaysInMonth(dc, x, dayWidth, row0Height, row1Height, firstDayOfMonth);
    }

    private void DrawDaysInMonth(DrawingContext dc,
                                 double         x,
                                 double         dayWidth,
                                 double         row0Height,
                                 double         row1Height,
                                 DateTime       yearMonth)
    {
        var daysInMonth = DateTime.DaysInMonth(yearMonth.Year, yearMonth.Month);
        //var monthWidth  = daysInMonth * dayWidth;

        for (var i = 0; i < daysInMonth; i++)
        {
            var day = new DateTime(yearMonth.Year, yearMonth.Month, i + 1);

            var dayX = 0.5 + x + dayWidth * i;

            if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                dc.DrawRectangle(Brushes.Gray, null, new Rect(dayX, row0Height, dayWidth, row1Height));
            }

            dc.DrawLine(_penGrid, new Point(dayX + dayWidth, row0Height), new Point(dayX + dayWidth, row0Height + row1Height));

            var fText = new FormattedText((i + 1).ToString("00"),
                                          CultureInfo.CurrentCulture,
                                          FlowDirection.LeftToRight,
                                          Typeface.Default,
                                          14d,
                                          Brushes.Black
                                         );

            dc.DrawText(fText,
                        new Point(dayX       + (dayWidth   - fText.Width)  / 2,
                                  row0Height + (row1Height - fText.Height) / 2
                                 )
                       );
        }
    }


    private void DrawWeekDays(DrawingContext dc,
                              double         x,
                              double         dayWidth,
                              double         row0Height,
                              double         row1Height,
                              DateTime       firstDayOfWeek)
    {
        //days
        for (var i = 0; i < 7; i++)
        {
            var date = firstDayOfWeek.AddDays(i);

            var lineX = 0.5 + x + dayWidth * i;

            dc.DrawLine(_penGrid, new Point(lineX, row0Height), new Point(lineX, row0Height + row1Height));

            var fText = new FormattedText(date.Day.ToString("00"),
                                          CultureInfo.CurrentCulture,
                                          FlowDirection.LeftToRight,
                                          Typeface.Default,
                                          14d,
                                          Brushes.Black
                                         );

            dc.DrawText(fText,
                        new Point(lineX      + (dayWidth   - fText.Width)  / 2,
                                  row0Height + (row1Height - fText.Height) / 2
                                 )
                       );
        }
    }
}