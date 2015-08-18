using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace Log4Smart
{
	public class Log : IDisposable
	{
		#region event log methods
		public static void Event(string message)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Event)) && (LogOptions.None != (s_threadOptions & LogOptions.Event)))
			{
				s_log.m_eventLog.Message(message);
				s_log.m_event.Set();
			}
		}
		public static void Event<T0>(string format, T0 a0)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Event)) && (LogOptions.None != (s_threadOptions & LogOptions.Event)))
			{
				string st = string.Format(format, a0);
				s_log.m_eventLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Event<T0, T1>(string format, T0 a0, T1 a1)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Event)) && (LogOptions.None != (s_threadOptions & LogOptions.Event)))
			{
				string st = string.Format(format, a0, a1);
				s_log.m_eventLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Event<T0, T1, T2>(string format, T0 a0, T1 a1, T2 a2)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Event)) && (LogOptions.None != (s_threadOptions & LogOptions.Event)))
			{
				string st = string.Format(format, a0, a1, a2);
				s_log.m_eventLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Event<T0, T1, T2, T3>(string format, T0 a0, T1 a1, T2 a2, T3 a3)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Event)) && (LogOptions.None != (s_threadOptions & LogOptions.Event)))
			{
				string st = string.Format(format, a0, a1, a2, a3);
				s_log.m_eventLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Event(string format, params object[] args)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Event)) && (LogOptions.None != (s_threadOptions & LogOptions.Event)))
			{
				string st = string.Format(format, args);
				s_log.m_eventLog.Message(st);
				s_log.m_event.Set();
			}
		}
		#endregion
		#region warning log methods
		public static void Warning(string message)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Warning)) && (LogOptions.None != (s_threadOptions & LogOptions.Warning)))
			{
				s_log.m_warningLog.Message(message);
				s_log.m_event.Set();
			}
		}
		public static void Warning<T0>(string format, T0 a0)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Warning)) && (LogOptions.None != (s_threadOptions & LogOptions.Warning)))
			{
				string st = string.Format(format, a0);
				s_log.m_warningLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Warning<T0, T1>(string format, T0 a0, T1 a1)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Warning)) && (LogOptions.None != (s_threadOptions & LogOptions.Warning)))
			{
				string st = string.Format(format, a0, a1);
				s_log.m_warningLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Warning<T0, T1, T2>(string format, T0 a0, T1 a1, T2 a2)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Warning)) && (LogOptions.None != (s_threadOptions & LogOptions.Warning)))
			{
				string st = string.Format(format, a0, a1, a2);
				s_log.m_warningLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Warning<T0, T1, T2, T3>(string format, T0 a0, T1 a1, T2 a2, T3 a3)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Warning)) && (LogOptions.None != (s_threadOptions & LogOptions.Warning)))
			{
				string st = string.Format(format, a0, a1, a2, a3);
				s_log.m_warningLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Warning(string format, params object[] args)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Warning)) && (LogOptions.None != (s_threadOptions & LogOptions.Warning)))
			{
				string st = string.Format(format, args);
				s_log.m_warningLog.Message(st);
				s_log.m_event.Set();
			}
		}
		#endregion
		#region error log methods
		public static void Error(string message)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Error)) && (LogOptions.None != (s_threadOptions & LogOptions.Error)))
			{
				s_log.m_errorLog.Message(message);
				s_log.m_event.Set();
			}
		}
		public static void Error<T0>(string format, T0 a0)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Error)) && (LogOptions.None != (s_threadOptions & LogOptions.Error)))
			{
				string st = string.Format(format, a0);
				s_log.m_errorLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Error<T0, T1>(string format, T0 a0, T1 a1)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Error)) && (LogOptions.None != (s_threadOptions & LogOptions.Error)))
			{
				string st = string.Format(format, a0, a1);
				s_log.m_errorLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Error<T0, T1, T2>(string format, T0 a0, T1 a1, T2 a2)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Error)) && (LogOptions.None != (s_threadOptions & LogOptions.Error)))
			{
				string st = string.Format(format, a0, a1, a2);
				s_log.m_errorLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Error<T0, T1, T2, T3>(string format, T0 a0, T1 a1, T2 a2, T3 a3)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Error)) && (LogOptions.None != (s_threadOptions & LogOptions.Error)))
			{
				string st = string.Format(format, a0, a1, a2);
				s_log.m_errorLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Error(string format, params object[] args)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Error)) && (LogOptions.None != (s_threadOptions & LogOptions.Error)))
			{
				string st = string.Format(format, args);
				s_log.m_errorLog.Message(st);
				s_log.m_event.Set();
			}
		}
		#endregion
		#region technical log methods
		public static void Technical(string message)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Technical)) && (LogOptions.None != (s_threadOptions & LogOptions.Technical)))
			{
				s_log.m_technicalLog.Message(message);
				s_log.m_event.Set();
			}
		}
		public static void Technical<T0>(string format, T0 a0)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Technical)) && (LogOptions.None != (s_threadOptions & LogOptions.Technical)))
			{
				string st = string.Format(format, a0);
				s_log.m_technicalLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Technical<T0, T1>(string format, T0 a0, T1 a1)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Technical)) && (LogOptions.None != (s_threadOptions & LogOptions.Technical)))
			{
				string st = string.Format(format, a0, a1);
				s_log.m_technicalLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Technical<T0, T1, T2>(string format, T0 a0, T1 a1, T2 a2)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Technical)) && (LogOptions.None != (s_threadOptions & LogOptions.Technical)))
			{
				string st = string.Format(format, a0, a1, a2);
				s_log.m_technicalLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Technical<T0, T1, T2, T3>(string format, T0 a0, T1 a1, T2 a2, T3 a3)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Technical)) && (LogOptions.None != (s_threadOptions & LogOptions.Technical)))
			{
				string st = string.Format(format, a0, a1, a2, a3);
				s_log.m_technicalLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Technical(string format, params object[] args)
		{
			LogOptions options = s_globalOptions;
			if ((LogOptions.None != (options & LogOptions.Technical)) && (LogOptions.None != (s_threadOptions & LogOptions.Technical)))
			{
				string st = string.Format(format, args);
				s_log.m_technicalLog.Message(st);
				s_log.m_event.Set();
			}
		}
		public static void Finalize()
		{
			s_log.Dispose();
		}
		#endregion
		#region static properties
		public static LogOptions GlobalOptions
		{
			get 
			{
				return s_globalOptions;
			}
			set
			{
				s_globalOptions = value;
			}
		}
		public static LogOptions ThreadOptions
		{
			get
			{
				return s_threadOptions;
			}
			set
			{
				s_threadOptions = value;
			}
		}
		#endregion
		#region construction
		private Log()
		{
			m_continue = true;
			m_event = new AutoResetEvent(false);
			m_eventLog = new LogStream("event");
			m_warningLog = new LogStream("warning");
			m_errorLog = new LogStream("error");
			m_technicalLog = new LogStream("technical");
			m_thread = new Thread(ThreadLoop);
			m_thread.Start();
		}
		#endregion
		#region internal methods

		public void Dispose()
		{
			m_continue = false;
			m_event.Set();
			m_thread.Join();
		}
		private void ThreadLoop()
		{
			for (; m_continue; m_event.WaitOne())
			{
				DateTime now = DateTime.UtcNow.Date;
				m_eventLog.Flush(now);
				m_warningLog.Flush(now);
				m_errorLog.Flush(now);
				m_technicalLog.Flush(now);
			}
		}
		#endregion
		#region static members
		private static LogOptions s_globalOptions = LogOptions.All;
		[ThreadStatic]
		private static LogOptions s_threadOptions = LogOptions.All;
		private static readonly Log s_log = new Log();
		#endregion
		#region members
		private bool m_continue;
		private readonly AutoResetEvent m_event;
		private readonly LogStream m_eventLog;
		private readonly LogStream m_warningLog;
		private readonly LogStream m_errorLog;
		private readonly LogStream m_technicalLog;
		private readonly Thread m_thread;
		#endregion
	}
}
