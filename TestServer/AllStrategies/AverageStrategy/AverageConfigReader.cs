using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrategyCommon;

namespace Average
{
    public class AverageConfigReader : ConfigReader
    {
        public override string GetConfigName()
        {
            return "Average.dll.config";
        }
    }
}
