using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace StrategyCommon
{
    public class StrategyTime
    {
        int AdjustedProhibitedStartHour, CalculatedStartHour, CalculatedEndHour;
        int WorkingDay = 0;
        int GMT = 0;
        int DayStartHour = 0;
        //public static TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("US Eastern Standard Time");

        public StrategyTime()
        {
        }

        public StrategyTime(int ProhibitedStartHour, int ProhibitedStartMinute, int ProhibitedDuration, int _GMT, int _WorkingDay)
        {
            InitProhibit( ProhibitedStartHour, ProhibitedStartMinute, ProhibitedDuration*60, _GMT, _WorkingDay);
        }
        public void InitProhibit(int ProhibitedStartHour, int ProhibitedStartMinute, int ProhibitedDurationMinutes, int _GMT, int _WorkingDay)
        {
            this.GMT = _GMT;
            AdjustedProhibitedStartHour = ProhibitedStartHour + GMT;
            if (AdjustedProhibitedStartHour >= 24)
                AdjustedProhibitedStartHour -= 24;
            CalculatedStartHour = AdjustedProhibitedStartHour * 60 + ProhibitedStartMinute;
            CalculatedEndHour = CalculatedStartHour + ProhibitedDurationMinutes;
            WorkingDay = _WorkingDay;
        }

        public void Init(int StartHour, int StartMinute, int DurationMinute, int _GMT, int _WorkingDay, int dayStartHour)
        {
            this.WorkingDay = _WorkingDay;
            this.DayStartHour = dayStartHour;

            int ProhibitedStartHour = (StartHour*60+StartMinute+DurationMinute)/60;
            if (ProhibitedStartHour >= 24)
                ProhibitedStartHour -= 24; 
            int ProhibitedStartMinute = (StartHour * 60 + StartMinute + DurationMinute) % 60;
            int ProhibitedDurationMinutes = 24 * 60 - DurationMinute;
            InitProhibit(ProhibitedStartHour, ProhibitedStartMinute, ProhibitedDurationMinutes, _GMT, _WorkingDay);
        }
        public bool IsSystemON(Tick<int> currentTick)
        {
            DateTime tickDateTime = currentTick.DateTime;// AdjustSummerTime(currentTick.DateTime);

            if (IsDayOff(tickDateTime.AddHours(-GMT)))
                return false;

            int CurrentHour = tickDateTime.Hour * 60 + tickDateTime.Minute;
            if ((CalculatedEndHour >= 24 * 60) && (CurrentHour < AdjustedProhibitedStartHour * 60))
                CurrentHour += 24 * 60;

            return !((CalculatedStartHour <= CurrentHour) && (CurrentHour <= CalculatedEndHour));
        }

        //private DateTime AdjustSummerTime(DateTime dateTime)
        //{
        //    if( timeZoneInfo.IsDaylightSavingTime(dateTime))
        //        return dateTime.AddHours(1);
        //    return dateTime;
        //}

        private bool IsDayOff(DateTime dateTime)
        {
            DateTime dt = dateTime.AddHours(-this.DayStartHour);
            int dayOfWeek = (int)dt.DayOfWeek;
            int test = ((1 << (6-dayOfWeek)) & WorkingDay);
            return  test == 0;
        }

    }
}
