using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace FxAdvisorCore.Logging
{
    public enum LogLevel
    {
        Error,
        Warning,
        Info,
        Debug
    }

    public class LogInputNode
    {
        protected Logger parent;
        protected LogLevel level;

        #region Public
        public LogInputNode(Logger parent, LogLevel level)
        {
            this.parent = parent;
            this.level = level;
        }

        public void Log(string message)
        {
            Write(null, message, null);
        }

        public void Log(string methodName, string message)
        {
            Write(methodName, message, null);
        }

        public void Log(Exception ex)
        {
            Write(null, null, ex);
        }

        public void Log(string message, Exception ex)
        {
            Write(null, message, ex);
        }

        public void Log(string methodName, string message, Exception ex)
        {
            Write(methodName, message, ex);
        }
        #endregion Public

        protected void Write(string methodName, string message, Exception ex)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(level.ToString());
            if (parent.ClassName != null)
                builder.Append(' ').Append(parent.ClassName).Append('.');
            if (methodName != null)
                builder.Append(methodName).Append("()");
            if (message != null)
                builder.Append(' ').Append(message);
            if (ex != null)
                builder.Append(Environment.NewLine).Append(ex.ToString());

            string logEntryText = builder.ToString();

            try
            {
                Debug.Write(logEntryText);
                if (level != LogLevel.Debug)
                {
                    EventLog.WriteEntry(
                        parent.SourceId,
                        logEntryText,
                        ToEventLogEventType(level));
                }
            }
            catch (Exception)
            {
                // Ignore.
                // Note: Logger should not be a source of exceptions. 
                // And we cannot log the exception because it may cause another exception.
            }
        }

        protected static EventLogEntryType ToEventLogEventType(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Error:
                    return EventLogEntryType.Error;
                case LogLevel.Info:
                    return EventLogEntryType.Information;
                case LogLevel.Warning:
                    return EventLogEntryType.Warning;
            }
            throw new ArgumentException("level");
        }
    }

    public class LogExInputNode : LogInputNode
    {
        public LogExInputNode(Logger parent, LogLevel level)
            : base(parent, level)
        {
        }

        public void Enter(string methodName)
        {
            Write(methodName, "enter", null);
        }

        public void Enter(string methodName, object[] args)
        {
            Write(methodName, "enter" + ArgsToString(args), null);
        }

        public void Leave(string methodName)
        {
            Write(methodName, "leave", null);
        }

        public void Leave(string methodName, object result)
        {
            Write(methodName, "leave (" + result.ToString() + ")", null);
        }

        private string ArgsToString(object[] args)
        {
            StringBuilder builder = new StringBuilder();
            return builder.ToString();
        }
    }
}
