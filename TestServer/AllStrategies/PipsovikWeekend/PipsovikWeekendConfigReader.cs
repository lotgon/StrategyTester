using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrategyCommon;

namespace PipsovikWeekend
{
    public class PipsovikWeekendConfigReader : ConfigReader
    {
        public override string GetConfigName()
        {
            return "PipsovikWeekend.dll.config";
        }
    }
}
