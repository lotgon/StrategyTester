﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Genetic.Fitness_Functions;
using AForge.Genetic;
using EngineTest;
using Common;

namespace PipsovikWeekend
{
    public class PipsovikWeekendFunctionFactory : OptimizationFunctionIntNDFactory
    {
        public PipsovikWeekendFunctionFactory(StratergyParameterRange spRange)
        {
            base.StratergyParameterRange = spRange;
        }
        public override OptimizationFunctionIntND CreateOptimizationFunctionIntND(InputData inSampleData, DateTime startTime, EstimationFunctionType estFunctionType, Account acc)
        {
            return new PipsovikWeekendFunction(inSampleData, startTime, estFunctionType, acc, base.StratergyParameterRange);
        }
    }
}
