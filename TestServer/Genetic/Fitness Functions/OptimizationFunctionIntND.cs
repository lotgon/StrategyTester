using System;
using System.Collections.Generic;
using System.Text;
using Common;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Configuration;

namespace AForge.Genetic
{
    using System;
    using AForge;

    public abstract class OptimizationFunctionIntND : IFitnessFunction
    {
        private StratergyParameterRange sparamRange;

        /// <summary>
        /// OptimizationFunctionIntND
        /// </summary>
        /// <param name="sparamRange"></param>
        public OptimizationFunctionIntND(StratergyParameterRange sparamRange)
        {
            this.sparamRange = sparamRange;
        }
        public OptimizationFunctionIntND() { }

        public double Evaluate(IChromosome chromosome)
        {
            StrategyParameter sParam;

            // do native translation first
            sParam = Translate(chromosome);
            // get function value
            return OptimizationFunction(sParam);
        }
        public StrategyParameter Translate(IChromosome chromosome)
        {
            // get chromosome's value
            ushort[] val = ((ShortArrayChromosome)chromosome).Value;
            StrategyParameter sParam = new StrategyParameter();

            int i = 0;
            foreach (KeyValuePair<string, AForge.IntStepRange> kvp in sparamRange.ranges)
            {
                int result = val[i++] * (kvp.Value.Max - kvp.Value.Min + 1)  / ((ShortArrayChromosome)chromosome).MaxValue;
                result = result - result % kvp.Value.Step + kvp.Value.Min;
                sParam.Add(kvp.Key, result);
            }

            return sParam;
        }
        public StratergyParameterRange StratergyParameterRange
        {
            get
            {
                return sparamRange;
            }
        }


        public abstract double OptimizationFunction(StrategyParameter sParam);
        public abstract double OptimizationFunction(StrategyParameter sParam, out StrategyResultStatistics srResult);
        public abstract double OptimizationFunction(StrategyParameter sParam, out StrategyResultStatistics srResult, string directory);
        public abstract double OptimizationFunction(StrategyParameter sParam, string directory);
        public abstract void EstimateAll(string directory);


    }
}
