using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics
{
	public class DateTimeParser
	{
		private static readonly int[] s_daysInMonth = new int[] { 0, 31, 0, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
		public static DateTime Parse(string st)
		{
			const string format = "yyyy.MM.dd HH:mm:ss";
			if (st.Length != format.Length)
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			char ch = '\0';
			int year = 0;
			#region year parsing
			ch = st[0];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			year += (ch - '0') * 1000;
			ch = st[1];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			year += (ch - '0') * 100;
			ch = st[2];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			year += (ch - '0') * 10;

			ch = st[3];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			year += (ch - '0');
			#endregion
			if (st[4] != '.')
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			int month = 0;
			#region month parsing
			ch = st[5];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			month += (ch - '0') * 10;

			ch = st[6];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			month += (ch - '0');
			#endregion
			if (st[7] != '.')
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			int day = 0;
			#region day parsing
			ch = st[8];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			day += (ch - '0') * 10;
			ch = st[9];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			day += (ch - '0');
			#endregion
			if (st[10] != ' ')
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			int hour = 0;
			#region hour parsing
			ch = st[11];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			hour += (ch - '0') * 10;
			ch = st[12];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			hour += (ch - '0');
			#endregion

			if (st[13] != ':')
			{
				throw new FormatException("Invalid date time format: " + st);
			}

			int minute = 0;
			#region minute parsing
			ch = st[14];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			minute += (ch - '0') * 10;
			ch = st[15];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			minute += (ch - '0');
			#endregion
			if (st[16] != ':')
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			int second = 0;
			#region second parsing
			ch = st[17];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			second += (ch - '0') * 10;
			ch = st[18];
			if ((ch < '0') || (ch > '9'))
			{
				throw new FormatException("Invalid date time format: " + st);
			}
			second += (ch - '0');
			#endregion
			DateTime result = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
			return result;
		}
	}
}
