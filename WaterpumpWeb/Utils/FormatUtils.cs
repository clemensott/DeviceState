using System;

namespace WaterpumpWeb.Utils
{
    static class FormatUtils
    {
        public static string FormatTimeSpan(TimeSpan span)
        {
            int days = span.Days;
            int hours = span.Hours;
            int minutes = span.Minutes;
            int seconds = span.Seconds;

            string timeText = "";
            if (days > 0) timeText += days + "d ";
            if (days > 0 || hours > 0) timeText += hours + "h ";
            if (days > 0 || hours > 0 || minutes > 0) timeText += minutes + "m ";
            timeText += seconds + "s ";

            return timeText;
        }
    }
}
