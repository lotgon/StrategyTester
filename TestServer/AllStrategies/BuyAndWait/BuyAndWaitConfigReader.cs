using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrategyCommon;

namespace BuyAndWait
{
    public class BuyAndWaitConfigReader : ConfigReader
    {
        public override string GetConfigName()
        {
            return "BuyAndWait.dll.config";
        }
    }
}
