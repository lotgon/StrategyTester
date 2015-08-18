using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrategyCommon;

namespace Pipsovik
{
    public class PipsovikConfigReader : ConfigReader
    {
        public override string GetConfigName()
        {
            return "Pipsovik.dll.config";
        }
    }
}
