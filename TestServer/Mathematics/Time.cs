using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Runtime;

namespace Mathematics
{
    public unsafe struct Time : IComparable<Time>
    {
        #region constants and members
        private const long cDelta = 621355968000000000; // new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified).Ticks;
        private int m_time;
        #endregion
        #region construction
        public Time(int time) : this()
        {
            m_time = time;
        }
        public Time(DateTime dateTime) : this()
        {
            Construct(dateTime);
        }
        public Time(int year, int month, int day, int hour = 0, int minute = 0, int second = 0) : this()
        {
            DateTime dateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
            Construct(dateTime);
        }
        public static Time System()
        {
            return new Time(DateTime.UtcNow);
        }
        public static Time Local()
        {
            return new Time(DateTime.Now);
        }
        private void Construct(DateTime dateTime)
        {
            if (DateTimeKind.Unspecified != dateTime.Kind)
            {
                dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond, DateTimeKind.Unspecified);
            }
            long ticks = dateTime.Ticks - cDelta;
            ticks /= 100000;
            Debug.Assert(ticks < int.MaxValue);
            m_time = (int)ticks;
        }
        #endregion
        #region operators
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static int operator -(Time first, Time second)
        {
            int result = first.m_time - second.m_time;
            return result;
        }
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator <(Time first, Time second)
        {
            return (first.m_time < second.m_time);
        }
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator <=(Time first, Time second)
        {
            return (first.m_time <= second.m_time);
        }
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator >(Time first, Time second)
        {
            return (first.m_time > second.m_time);
        }
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator >=(Time first, Time second)
        {
            return (first.m_time >= second.m_time);
        }
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator ==(Time first, Time second)
        {
            return (first.m_time == second.m_time);
        }
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(Time first, Time second)
        {
            return (first.m_time != second.m_time);
        }
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static Time operator -(Time first, int seconds)
        {
            return new Time(first.m_time - seconds);
        }
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static Time operator +(Time first, int seconds)
        {
            return new Time(first.m_time + seconds);
        }
        #endregion
        #region overrode methods
        public override int GetHashCode()
        {
            return m_time; // <-> m_time.GetHashCode()
        }
        public override bool Equals(object obj)
        {
            Time time = (Time)obj;
            bool result = (m_time == time.m_time);
            return result;
        }
        public bool Equals(Time time)
        {
            bool result = (m_time == time.m_time);
            return result;
        }
        public override string ToString()
        {
            long ticks = m_time * 100000 + cDelta;
            DateTime dateTime = new DateTime(ticks);
            string result = dateTime.ToString();
            return result;
        }
        public static Time Parse(string st)
        {
            DateTime dateTime = DateTimeParser.Parse(st);
            return new Time(dateTime);
        }
        #endregion
        public int CompareTo(Time other)
        {
            int result = m_time.CompareTo(other.m_time);
            return result;
        }
    }
}
