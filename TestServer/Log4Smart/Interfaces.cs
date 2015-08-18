using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GUIAnalyser
{
    public enum IAbstractLoggerMessageType
    {
        CompletionStatus,
        General,
        Error,
        Warning,
    }

    public interface IAbstractLogger
    {
        void AddLog(string message, IAbstractLoggerMessageType messageType = IAbstractLoggerMessageType.General);
    }
}
