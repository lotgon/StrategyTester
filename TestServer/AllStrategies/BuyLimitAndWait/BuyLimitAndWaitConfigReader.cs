using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrategyCommon;

namespace BuyLimitAndWait
{
    public class BuyLimitAndWaitConfigReader : ConfigReader
    {
        public override string GetConfigName()
        {
            return "BuyLimitAndWait.dll.config";
        }
    }
}
