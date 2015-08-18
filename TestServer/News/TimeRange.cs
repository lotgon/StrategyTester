using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace News
{
    public class TimeRange
    {
        DateTime startDateTime;
        DateTime endDateTime;
        string description;
 
        public TimeRange(DateTime dt1, DateTime dt2, string description)
        {
            startDateTime = dt1;
            endDateTime = dt2;
            this.description = description;
        }
        public bool IsInRange( DateTime current)
        {
            return !IsEarlier(current) && !IsLater(current);
        }
        public bool IsEarlier(DateTime current)
        {
            return current < startDateTime;
        }
        public bool IsLater(DateTime current)
        {
            return current > endDateTime;
        }

        public DateTime StartDateTime
        {
            get
            {
                return startDateTime;
            }
        }
        public DateTime EndDateTime
        {
            get
            {
                return endDateTime;
            }
        }
        public string Description
        {
            get
            {
                return description;
            }
        } 
    }
}
