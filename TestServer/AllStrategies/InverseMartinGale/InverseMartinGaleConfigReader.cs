using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrategyCommon;

namespace InverseMartinGale
{
    public class InverseMartinGaleConfigReader : ConfigReader
    {
        public override string GetConfigName()
        {
            return "InverseMartinGale.dll.config";
        }
    }
}
