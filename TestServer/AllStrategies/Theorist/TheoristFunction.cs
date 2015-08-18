using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Genetic.Fitness_Functions;
using AForge.Genetic;
using EngineTest;
using Common;
using System.IO;
using System.Xml;
using System.Configuration;
using System.Runtime.Serialization;
using StrategyCommon;

namespace Theorist
{
    public class TheoristFunction : CommonStrategyFunction
    {
        static object syn = new object();


        internal TheoristFunction(InputData inSampleData, DateTime startTime, EstimationFunctionType estFunctionType, Account acc, StratergyParameterRange range)
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
            return StandardOptimizationFunction(sParam, directory, new TheoristStrategy(), new TheoristConfigReader(), out srResult);
        }
    }
}
