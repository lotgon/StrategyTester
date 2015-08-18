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

namespace Pipsovik
{
    public class PipsovikFunction : CommonStrategyFunction
    {
        static object syn = new object();


        internal PipsovikFunction(InputData inSampleData, DateTime startTime, EstimationFunctionType estFunctionType, Account acc, StratergyParameterRange range)
            : base(inSampleData, startTime, estFunctionType, acc, range)
        {
        }

        public override bool IsValid(StrategyParameter param)
        {
            //if (param["TP"] > param["OpenOrderShift"])
            //    return false;

            param.NewsFilePath = System.Configuration.ConfigurationManager.AppSettings["NewsFilePath"];
            return true;
        }

        public override double OptimizationFunction(StrategyParameter sParam, string directory)
        {
            StrategyResultStatistics srResult;
            return StandardOptimizationFunction(sParam, directory, new PipsovikStrategy(), new PipsovikConfigReader(), out srResult);
        }
        public override double OptimizationFunction(StrategyParameter sParam, out StrategyResultStatistics srResult)
        {
            return StandardOptimizationFunction(sParam, string.Empty, new PipsovikStrategy(), new PipsovikConfigReader(), out srResult);
        }
        public override double OptimizationFunction(StrategyParameter sParam, out StrategyResultStatistics srResult, string directory)
        {
            return StandardOptimizationFunction(sParam, directory, new PipsovikStrategy(), new PipsovikConfigReader(), out srResult);
        }

    }
}
