using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FxAdvisorHost
{
    public enum EventSeverity
    {
        Error,
        Info
    }

    class EventLog
    {
        private object _sync = new object();
        private Queue<string> eventQueue = new Queue<string>();  

        public void AddEntry(string text, EventSeverity severity)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(DateTime.Now).Append(" | ");
            builder.Append(severity.ToString()).Append(" | ");
            builder.Append(text);
            string record = builder.ToString();

            lock (_sync)
            {
                eventQueue.Enqueue(record);
            }
        }

        public string GetNextEvent()
        {
            lock (_sync)
            {
                if (eventQueue.Count == 0)
                    return null;
                return eventQueue.Dequeue();
            }
        }
    }
}
