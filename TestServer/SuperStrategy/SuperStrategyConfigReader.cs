using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrategyCommon;

namespace SuperStrategy
{
    public class SuperStrategyConfigReader : StrategyCommon.ConfigReader
    {
        public override string GetConfigName()
        {
            return "SuperStrategy.dll.config";
        }
    }
}
