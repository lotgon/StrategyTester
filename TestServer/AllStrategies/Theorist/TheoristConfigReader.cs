using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrategyCommon;

namespace Theorist
{
    public class TheoristConfigReader : ConfigReader
    {
        public override string GetConfigName()
        {
            return "Theorist.dll.config";
        }
    }
}
