using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Log4Smart
{
	[Flags]
	public enum LogOptions
	{
		None = 0,
		Event = 1,
		Warning = 2,
		Error = 4,
		Technical = 8,
		All = Event | Warning | Error | Technical
	}
}
