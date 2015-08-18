using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Log4Smart;
using FxAdvisorCore;
using Common;

namespace FxAdvisorCore.Logging
{
    public class MetaStrategyLogger : IStrategyLogger
    {
        IMeta meta;

        public MetaStrategyLogger(IMeta m)
        {
            this.meta = m;
        }
        #region IStrategyLogger Members

        public void AddMessage(string format, params object[] args)
        {
            string result;
            try
            {
                result = string.Format( format, args);
            }
            catch
            {
                result = "Wrong arguments for " + format;
            }

            meta.Print(result);
        }

        #endregion
    }
}
