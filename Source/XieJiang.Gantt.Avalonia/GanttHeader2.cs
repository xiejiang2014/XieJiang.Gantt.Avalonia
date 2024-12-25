using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace XieJiang.Gantt.Avalonia;

public class GanttHeader2 : Control
{
    private IPen _penGrid = new Pen(new SolidColorBrush(Colors.Black));

    public void Reload()
    {
        InvalidateVisual();
    }

    public override void Render(DrawingContext dc)
    {
        var dayWidth  = GetValue(GanttControl.DayWidthProperty);
        var dateMode  = GetValue(GanttControl.DateModeProperty);
        var startDate = GetValue(GanttControl.StartDateProperty);
        var endDate   = GetValue(GanttControl.EndDateProperty);

        var row1Height = GetValue(GanttControl.HeaderRow1HeightProperty);
        var row2Height = GetValue(GanttControl.HeaderRow2HeightProperty);

        var lightGridBrush = Application.Current?.Resources["LightGridBrush"] as IBrush;
        var lightGridPen   = new Pen(lightGridBrush);

        var darkGridBrush = Application.Current?.Resources["DarkGridBrush"] as IBrush;
        var darkGridPen   = new Pen(darkGridBrush);

        var x = 0d;

        if (dateMode == DateModes.Weekly)
        {
            if (startDate.Year  == endDate.Year &&
                startDate.Month == endDate.Month)
            {
                var monthItem = new MonthItem(startDate, endDate);
                DrawMonth(dc, monthItem, x, dayWidth, row1Height, row2Height, lightGridPen, darkGridPen, out var width);
                x += width;

                //DateItems.Add(monthItem);
                //result.AddRange(monthItem.DayItems);
            }
            else
            {
                while (true)
                {
                    var lastDayOfMonth = new DateOnly(startDate.Year, startDate.Month, 1).AddMonths(1).AddDays(-1);

                    if (lastDayOfMonth >= endDate)
                    {
                        var monthItem = new MonthItem(startDate, endDate);
                        DrawMonth(dc, monthItem, x, dayWidth, row1Height, row2Height, lightGridPen, darkGridPen, out var width);
                        x += width;
                        break;
                    }
                    else
                    {
                        var monthItem = new MonthItem(startDate, lastDayOfMonth);
                        DrawMonth(dc, monthItem, x, dayWidth, row1Height, row2Height, lightGridPen, darkGridPen, out var width);
                        x += width;

                        startDate = new DateOnly(startDate.Year, startDate.Month, 1).AddMonths(1);
                    }
                }
            }

            //Width = x;
        }
    }


    private void DrawMonth(DrawingContext dc,
                           MonthItem      monthItem,
                           double         x,
                           double         dayWidth,
                           double         row1Height,
                           double         row2Height,
                           Pen            lightGridPen,
                           Pen            darkGridPen,
                           out double     width)
    {
        width = monthItem.DayItems.Count * dayWidth;


        //中横线
        dc.DrawLine(lightGridPen,
                    new Point(x, row1Height + 0.5),
                    new Point(x             + width, row1Height + 0.5)
                   );

        for (var i = 0; i < monthItem.DayItems.Count; i++)
        {
            var dayItem = monthItem.DayItems[i];
            DrawDay(dc, dayItem, x + i * dayWidth, dayWidth, row1Height, row2Height, lightGridPen, darkGridPen);
        }
    }

    private void DrawDay(DrawingContext dc,
                         DayItem        dayItem,
                         double         x,
                         double         dayWidth,
                         double         row1Height,
                         double         row2Height,
                         Pen            lightGridPen,
                         Pen            darkGridPen)
    {
        //左线
        var lineX = 0.5 + x;
        dc.DrawLine(lightGridPen, new Point(lineX, row1Height), new Point(lineX, row1Height + row2Height));

        var fText = new FormattedText(dayItem.Date.Day.ToString("00"),
                                      CultureInfo.CurrentCulture,
                                      FlowDirection.LeftToRight,
                                      Typeface.Default,
                                      14d,
                                      Brushes.Black
                                     );

        dc.DrawText(fText,
                    new Point(lineX      + (dayWidth   - fText.Width)  / 2,
                              row1Height + (row2Height - fText.Height) / 2
                             )
                   );
    }

    //private void DrawMonth(DrawingContext dc,
    //                       double         x,
    //                       double         dayWidth,
    //                       double         row0Height,
    //                       double         row1Height,
    //                       DateOnly       firstDayOfMonth)
    //{
    //    var daysInMonth = DateTime.DaysInMonth(firstDayOfMonth.Year, firstDayOfMonth.Month);
    //    var monthWidth  = daysInMonth * dayWidth;

    //    //上横线
    //    dc.DrawLine(_penGrid,
    //                new Point(x,          0.5),
    //                new Point(monthWidth, 0.5)
    //               );

    //    //中横线
    //    dc.DrawLine(_penGrid,
    //                new Point(x,          row0Height + 0.5),
    //                new Point(monthWidth, row0Height + 0.5)
    //               );

    //    //左竖线
    //    dc.DrawLine(_penGrid,
    //                new Point(0.5, 0),
    //                new Point(0.5, row0Height + row1Height)
    //               );

    //    //右竖线
    //    dc.DrawLine(_penGrid,
    //                new Point(monthWidth + 0.5, 0),
    //                new Point(monthWidth + 0.5, row0Height)
    //               );

    //    var fText = new FormattedText(firstDayOfMonth.ToString("Y"),
    //                                  CultureInfo.CurrentCulture,
    //                                  FlowDirection.LeftToRight,
    //                                  Typeface.Default,
    //                                  14d,
    //                                  Brushes.Black
    //                                 );

    //    dc.DrawText(fText,
    //                new Point(4, //left margin
    //                          0 + (row0Height - fText.Height) / 2
    //                         )
    //               );

    //    DrawDaysInMonth(dc, x, dayWidth, row0Height, row1Height, firstDayOfMonth);
    //}

    //private void DrawDaysInMonth(DrawingContext dc,
    //                             double         x,
    //                             double         dayWidth,
    //                             double         row0Height,
    //                             double         row1Height,
    //                             DateOnly       yearMonth)
    //{
    //    var daysInMonth = DateTime.DaysInMonth(yearMonth.Year, yearMonth.Month);
    //    //var monthWidth  = daysInMonth * dayWidth;

    //    for (var i = 0; i < daysInMonth; i++)
    //    {
    //        var day = new DateTime(yearMonth.Year, yearMonth.Month, i + 1);

    //        var dayX = 0.5 + x + dayWidth * i;

    //        if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
    //        {
    //            dc.DrawRectangle(Brushes.Gray, null, new Rect(dayX, row0Height, dayWidth, row1Height));
    //        }

    //        dc.DrawLine(_penGrid, new Point(dayX + dayWidth, row0Height), new Point(dayX + dayWidth, row0Height + row1Height));

    //        var fText = new FormattedText((i + 1).ToString("00"),
    //                                      CultureInfo.CurrentCulture,
    //                                      FlowDirection.LeftToRight,
    //                                      Typeface.Default,
    //                                      14d,
    //                                      Brushes.Black
    //                                     );

    //        dc.DrawText(fText,
    //                    new Point(dayX       + (dayWidth   - fText.Width)  / 2,
    //                              row0Height + (row1Height - fText.Height) / 2
    //                             )
    //                   );
    //    }
    //}


    //private void DrawWeekDays(DrawingContext dc,
    //                          double         x,
    //                          double         dayWidth,
    //                          double         row0Height,
    //                          double         row1Height,
    //                          DateTime       firstDayOfWeek)
    //{
    //    //days
    //    for (var i = 0; i < 7; i++)
    //    {
    //        var date = firstDayOfWeek.AddDays(i);

    //        var lineX = 0.5 + x + dayWidth * i;

    //        dc.DrawLine(_penGrid, new Point(lineX, row0Height), new Point(lineX, row0Height + row1Height));

    //        var fText = new FormattedText(date.Day.ToString("00"),
    //                                      CultureInfo.CurrentCulture,
    //                                      FlowDirection.LeftToRight,
    //                                      Typeface.Default,
    //                                      14d,
    //                                      Brushes.Black
    //                                     );

    //        dc.DrawText(fText,
    //                    new Point(lineX      + (dayWidth   - fText.Width)  / 2,
    //                              row0Height + (row1Height - fText.Height) / 2
    //                             )
    //                   );
    //    }
    //}
}