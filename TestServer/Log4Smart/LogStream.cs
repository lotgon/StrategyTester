using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;


namespace Log4Smart
{
	internal class LogStream : IDisposable
	{
		internal LogStream(string filename)
		{
			m_filename = filename;
			Construct();
		}
		internal void Flush(DateTime dateNow)
		{
			List<LogEntry> entries = null;
			lock(m_synchronizer)
			{
				entries = m_addingEntreis;
				m_addingEntreis = m_writingEntries;
				m_writingEntries = entries;
			}

			foreach (var element in entries)
			{
				element.ToStream(m_stream);
			}
			m_stream.Flush();	
			if (m_createdDateTime != dateNow)
			{
				Construct();
			}
		}
		internal void Message(string message)
		{
			LogEntry entry = new LogEntry(message);
			lock (m_synchronizer)
			{
				m_addingEntreis.Add(entry);
			}
		}
		#region private methods
		private void Construct()
		{
 			if (null != m_stream)
 			{
				m_stream.Close();
				m_stream = null;
 			}
			string logsDirectoryPath = Process.GetCurrentProcess().MainModule.FileName;
			logsDirectoryPath = Path.GetDirectoryName(logsDirectoryPath);
			logsDirectoryPath = Path.Combine(logsDirectoryPath, "logs");
			if (!Directory.Exists(logsDirectoryPath))
			{
				Directory.CreateDirectory(logsDirectoryPath);
			}
			m_createdDateTime = DateTime.UtcNow.Date;
			string st = m_createdDateTime.ToString("yyyy-MM-dd");
			string path = string.Format("{0}\\{1} {2}{3}", logsDirectoryPath, m_filename, st, ".log");
			m_stream = new StreamWriter(path, true);
		}
		public void Dispose()
		{
			m_stream.Close();
			m_stream = null;
		}
		#endregion
		#region members
		private readonly object m_synchronizer = new object();
		private readonly string m_filename;
		private StreamWriter m_stream;
		private DateTime m_createdDateTime;
		private List<LogEntry> m_addingEntreis = new List<LogEntry>();
		private List<LogEntry> m_writingEntries = new List<LogEntry>();
		#endregion		
	}
}
