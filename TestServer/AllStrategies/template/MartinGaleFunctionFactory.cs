using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Genetic.Fitness_Functions;
using AForge.Genetic;
using EngineTest;
using Common;

namespace MartinGale
{
    public class MartinGaleFunctionFactory : OptimizationFunctionIntNDFactory
    {
        public override OptimizationFunctionIntND CreateOptimizationFunctionIntND(InputData inSampleData, DateTime startTime, EstimationFunctionType estFunctionType)
        {
            return new MartinGaleFunction(inSampleData, startTime, estFunctionType);
        }
    }
}
