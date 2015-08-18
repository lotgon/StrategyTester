using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Log4Smart
{
    public interface IStrategyLogger
    {
        void AddMessage(string format, params object[] args);
    }
    public interface IOrderLogger
    {
        void AddMessage(string format, params object[] args);
    }
    public interface IBalanceLogger
    {
        void AddMessage(string format, params object[] args);
    }

    public class Logger : IStrategyLogger, IOrderLogger, IBalanceLogger, IDisposable, ICloneable    
    {
        StringWriter orderLog = new StringWriter();
        StringWriter strategyLog = new StringWriter();
        StringWriter balanceLog = new StringWriter();
        public bool isShortLog = false;

        public Logger(bool isShortLog)
        {
            this.isShortLog = isShortLog;
            System.IO.Directory.CreateDirectory("result");
        }
        public void SaveLogToFile(string directory, string fileName)
        {
            if( !Directory.Exists(directory) )
                Directory.CreateDirectory(directory);

            using (StreamWriter strWriter = new StreamWriter(Path.Combine(directory, string.Format("strategy_{0}.csv", fileName))))
            {
                strWriter.WriteLine(strategyLog.ToString());
            }
            using (StreamWriter strWriter = new StreamWriter(Path.Combine(directory, string.Format(@"balance_{0}.csv", fileName))))
            {
                strWriter.WriteLine(balanceLog.ToString());
            }
            if (isShortLog)
                return;

            using (StreamWriter strWriter = new StreamWriter(Path.Combine(directory, string.Format(@"order_{0}.csv", fileName))))
            {
                strWriter.WriteLine(orderLog.ToString());
            }

        }

        public static void ClearAllResult()
        {
            if (Directory.Exists(@"result"))
            {
                Array.ForEach(Directory.GetFiles(@"result/"), delegate(string path) { File.Delete(path); });
                Array.ForEach(Directory.GetDirectories(@"result/"), delegate(string path) { Directory.Delete(path, true); });
            }
            if (Directory.Exists(@"errors"))
            {
                Array.ForEach(Directory.GetFiles(@"errors/"), delegate(string path) { File.Delete(path); });
                Array.ForEach(Directory.GetDirectories(@"errors/"), delegate(string path) { Directory.Delete(path, true); });
            }
        }

        #region IOrderLogger Members

        void IOrderLogger.AddMessage(string format, params object[] args)
        {
            if (isShortLog)
                return;

            orderLog.WriteLine(format, args);
        }

        #endregion

        #region IStrategyLogger Members

        void IStrategyLogger.AddMessage(string format, params object[] args)
        {
            strategyLog.WriteLine(format, args);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            orderLog.Close();
            strategyLog.Close();
        }

        #endregion

        #region IBalanceLogger Members

        void IBalanceLogger.AddMessage(string format, params object[] args)
        {
            balanceLog.WriteLine(format, args);
        }

        #endregion

        //#region ICloneable Members

        //object ICloneable.Clone()
        //{
        //    return this.MemberwiseClone();
        //}

        //#endregion

        #region ICloneable Members

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
