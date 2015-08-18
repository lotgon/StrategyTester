using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EngineTest;
using Log4Smart;
using Common;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Configuration;
using StrategyCommon;

namespace SuperAdaptStrategy
{
    public class SuperAdaptFunction : CommonStrategyFunction
    {
        static object syn = new object();

        internal SuperAdaptFunction(InputData inSampleData, DateTime startTime, EstimationFunctionType estFunctionType, Account acc, StratergyParameterRange range)
            : base(inSampleData, startTime, estFunctionType, acc, range)
        {
        }

        public override bool IsValid(StrategyParameter param)
        {
            param.NewsFilePath = System.Configuration.ConfigurationManager.AppSettings["NewsFilePath"];
            return true;
        }

        public override double OptimizationFunction(StrategyParameter sParam, string directory)
        {
            StrategyResultStatistics srResult;
            return StandardOptimizationFunction(sParam, directory, new SuperAdaptStrategy(), new SuperAdaptConfigReader(), out srResult);
        }
        public override double OptimizationFunction(StrategyParameter sParam, out StrategyResultStatistics srResult)
        {
            return StandardOptimizationFunction(sParam, string.Empty, new SuperAdaptStrategy(), new SuperAdaptConfigReader(), out srResult);
        }
        public override double OptimizationFunction(StrategyParameter sParam, out StrategyResultStatistics srResult, string directory)
        {
            return StandardOptimizationFunction(sParam, directory, new SuperAdaptStrategy(), new SuperAdaptConfigReader(), out srResult);
        }
    }
}
