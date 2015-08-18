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


namespace MartinGale
{
    public class MartinGaleFunction : AForge.Genetic.OptimizationFunctionIntND
    {
        InputData SampleData;
        DateTime StartTime;
        Dictionary<string, double> previosResult = new Dictionary<string, double>();
        static object syn = new object();
        EstimationFunctionType EstimationFunctionType { get; set; }

        internal MartinGaleFunction(InputData inSampleData, DateTime startTime, EstimationFunctionType estFunctionType)
            : base(Range)
        {
            this.SampleData = inSampleData;
            this.StartTime = startTime;
            this.EstimationFunctionType = estFunctionType;
        }

        public bool IsValid(StrategyParameter param)
        {
            return true;
        }
        static public StratergyParameterRange Range
        {
            get
            {
                lock (syn)
                {
                    using (FileStream fs = new FileStream("MartinGale.dll.config", FileMode.Open))
                    using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas()))
                    {
                        DataContractSerializer ser2 = new DataContractSerializer(typeof(StratergyParameterRange));
                        StratergyParameterRange deserializedStratergyParameterRange = (StratergyParameterRange)ser2.ReadObject(reader, true);
                        return deserializedStratergyParameterRange;
                    }
                }
            }
        }
        public override double OptimizationFunction(StrategyParameter sParam)
        {
            return OptimizationFunction(sParam, string.Empty);
        }
        public override double OptimizationFunction(StrategyParameter sParam, string directory)
        {
            if (!IsValid(sParam))
                return 0;

            if (previosResult.ContainsKey(sParam.ToString()))
                return previosResult[sParam.ToString()];

            using (Log4Smart.Logger logger = new Log4Smart.Logger(false))
            {
                Engine engine = new Engine(logger, logger);
                MartinGaleStrategy strategy = new MartinGaleStrategy(sParam, logger);
                int initialMoney = 10000000;
                Account acc = new Account(initialMoney);
                engine.StartTest(SampleData, strategy, acc);
                if (!engine.IsTestSuccessfull)
                {
                    logger.SaveLogToFile(directory + "/errors", SampleData.Symbol + "_" + StartTime.ToString("yyyyMMdd"));
                    previosResult[sParam.ToString()] = 0;
                    return 0;
                }

                if (directory != string.Empty)
                    logger.SaveLogToFile(directory + "/" + this.StartTime.ToString("yyyyMMdd") + "_" + (acc.Balance.ToString()) + "$", "allData");

                if (acc.Balance > initialMoney && this.EstimationFunctionType == EstimationFunctionType.MostMoney8Deviation)
                {
                    previosResult[sParam.ToString()] = initialMoney + (acc.Balance - initialMoney) * (1 - acc.Statistics.GetDeviation);
                }
                else
                {
                    previosResult[sParam.ToString()] = acc.Balance;
                }
                return previosResult[sParam.ToString()];
            }
        }
        public override void EstimateAll(string directory)
        {
            StrategyParameter param = new StrategyParameter();

            foreach (KeyValuePair<string, AForge.IntStepRange> range in Range.ranges)
            {
                param.Add(range.Key, range.Value.Max);
            }
            OptimizationFunction(param, directory);

        }
    }
}
