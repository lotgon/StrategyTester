using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Genetic.Fitness_Functions;
using AForge.Genetic;
using EngineTest;
using Common;

namespace Average
{
    public class AverageFunctionFactory : OptimizationFunctionIntNDFactory
    {
        public AverageFunctionFactory(StratergyParameterRange spRange)
        {
            base.StratergyParameterRange = spRange;
        }
        public override OptimizationFunctionIntND CreateOptimizationFunctionIntND(InputData inSampleData, DateTime startTime, EstimationFunctionType estFunctionType, Account acc)
        {
            return new AverageFunction(inSampleData, startTime, estFunctionType, acc, base.StratergyParameterRange);
        }
    }
}
