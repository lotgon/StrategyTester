using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrategyCommon;

namespace MartinGale
{
    public class MartinGaleConfigReader : ConfigReader
    {
        public override string GetConfigName()
        {
            return "MartinGale.dll.config";
        }
    }
}
