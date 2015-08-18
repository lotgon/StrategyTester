using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FxAdvisorCore.Logging
{
    public class Logger
    {
        private static Logger instance = new Logger();

        private string className;
        private string sourceId = "FxLogger";
        private LogInputNode error;
        private LogInputNode warning;
        private LogExInputNode info;
        private LogExInputNode debug;

        protected Logger(string className)
        {
            this.className = className;

            error = new LogInputNode(this, LogLevel.Error);
            warning = new LogInputNode(this, LogLevel.Warning);
            info = new LogExInputNode(this, LogLevel.Info);
            debug = new LogExInputNode(this, LogLevel.Debug);
        }

        protected Logger() : this(null)
        {
        }

        public static Logger Get(String className)
        {
            return new Logger(className);
        }

        public static Logger Instance
        {
            get { return instance; }
        }

        public LogInputNode Error
        {
            get { return error; }
        }

        public LogInputNode Warning
        {
            get { return warning; }
        }

        public LogExInputNode Info
        {
            get { return info; }
        }

        public LogExInputNode Debug
        {
            get { return debug; }
        }

        internal string ClassName
        {
            get { return className; }
        }

        public string SourceId
        {
            get { return sourceId; }
            set { this.sourceId = value; }
        }
    }
}
