using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrategyCommon;

namespace SuperAdaptStrategy
{
    public class SuperAdaptConfigReader : ConfigReader
    {
        public override string GetConfigName()
        {
            return "SuperAdaptStrategy.dll.config";
        }
    }
}
