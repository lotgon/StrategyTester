using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Log4Smart
{
	public class LogOptionsSwitch : IDisposable
	{
		public LogOptionsSwitch(LogOptions newThreadOptions)
		{
			m_oldLogThreadOptions = Log.ThreadOptions;
			Log.ThreadOptions = newThreadOptions;
		}
		public void Dispose()
		{
			Log.ThreadOptions = m_oldLogThreadOptions;
		}
		private readonly LogOptions m_oldLogThreadOptions;
	}
}
