using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public static class WorkDateTime
    {
        public static DateTime AddWorkingDay(this DateTime date, int day)
        {
            int actualNumberDay = Math.Abs(day);
            int actualOneDay = day > 0 ? 1 : -1;

            while (actualNumberDay > 0)
            {
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    actualNumberDay--;
                date = date.AddDays(actualOneDay);
            }
            return date;
        }
    }
}
