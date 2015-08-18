using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrategyCommon
{
    public class BasicParam
    {
        public int ID;

        public int InitHistoryMinutes;
        public string Symbol;
        public int BasicVolume;
        public int ReadOnly;
        public string RawStrategyParam;

        public string NewUniqueComment()
        {
            return string.Format("{0}_{1}", this.ID, Guid.NewGuid().ToString());
        }
        public string IdentityComment
        {
            get
            {
                return this.ID.ToString() + "_";
            }
        }
    }
}
