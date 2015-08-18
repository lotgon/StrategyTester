using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Genetic.Fitness_Functions;
using AForge.Genetic;
using EngineTest;
using Common;

namespace SuperStrategy
{
    public class SuperStrategyFunctionFactory : OptimizationFunctionIntNDFactory
    {
        public SuperStrategyFunctionFactory(StratergyParameterRange spRange)
        {
            base.StratergyParameterRange = spRange;
        }
        public override OptimizationFunctionIntND CreateOptimizationFunctionIntND(InputData inSampleData, DateTime startTime, EstimationFunctionType estFunctionType, Account acc)
        {
            return new SuperStrategyFunction(inSampleData, startTime, estFunctionType, acc, base.StratergyParameterRange);
        }
    }
}
