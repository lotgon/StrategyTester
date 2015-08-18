﻿using System;
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
        public MartinGaleFunctionFactory(StratergyParameterRange spRange)
        {
            base.StratergyParameterRange = spRange;
        }
        public override OptimizationFunctionIntND CreateOptimizationFunctionIntND(InputData inSampleData, DateTime startTime, EstimationFunctionType estFunctionType, Account acc)
        {
            return new MartinGaleFunction(inSampleData, startTime, estFunctionType, acc, base.StratergyParameterRange);
        }

    }
}
