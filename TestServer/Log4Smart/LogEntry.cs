using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Log4Smart
{
	internal class LogEntry
	{
		internal LogEntry(string message)
		{
			m_threadId = AppDomain.GetCurrentThreadId();
			m_when = DateTime.UtcNow;
			m_message = message;
		}
		internal void ToStream(StreamWriter stream)
		{
			stream.WriteLine("UTC: {0}> {1}", m_when, m_message);
		}
		#region members
		private readonly int m_threadId;
		private readonly DateTime m_when;
		private readonly string m_message;
		#endregion
	}
}
