using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EngineTest;
using Common;

namespace AForge.Genetic.Fitness_Functions
{
    public abstract class OptimizationFunctionIntNDFactory
    {
        public StratergyParameterRange StratergyParameterRange { get; protected set; }

        public abstract OptimizationFunctionIntND CreateOptimizationFunctionIntND(InputData inSampleData, DateTime startTime, EstimationFunctionType estFunctionType, Account acc);
    }
}
