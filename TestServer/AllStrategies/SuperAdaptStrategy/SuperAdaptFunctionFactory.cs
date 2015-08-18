using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Genetic.Fitness_Functions;
using AForge.Genetic;
using EngineTest;
using Common;

namespace SuperAdaptStrategy
{
    public class SuperAdaptFunctionFactory : OptimizationFunctionIntNDFactory
    {
        public SuperAdaptFunctionFactory(StratergyParameterRange spRange)
        {
            base.StratergyParameterRange = spRange;
        }
        public override OptimizationFunctionIntND CreateOptimizationFunctionIntND(InputData inSampleData, DateTime startTime, EstimationFunctionType estFunctionType, Account acc)
        {
            return new SuperAdaptFunction(inSampleData, startTime, estFunctionType, acc, base.StratergyParameterRange);
        }
    }
}
