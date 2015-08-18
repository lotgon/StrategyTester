using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using EngineTest;
using System.IO;
using System.Collections.Concurrent;

namespace StrategyCommon
{
    public abstract class CommonStrategyFunction : AForge.Genetic.OptimizationFunctionIntND
    {
        InputData SampleData;
        DateTime StartTime;
        readonly Account DefaultAccount;
        EstimationFunctionType EstimationFunctionType { get; set; }

        protected CommonStrategyFunction( InputData inSampleData, DateTime startTime, EstimationFunctionType estFunctionType, Account acc, StratergyParameterRange range)
            : base(range)
        {
            this.SampleData = inSampleData;
            this.StartTime = startTime;
            this.EstimationFunctionType = estFunctionType;
            this.DefaultAccount = acc;
        }

        abstract public bool IsValid(StrategyParameter param);

        public override double OptimizationFunction(StrategyParameter sParam)
        {
            return OptimizationFunction(sParam, string.Empty);
        }

        public double StandardOptimizationFunction(StrategyParameter sParam, string directory,
            FxAdvisorCore.SimpleAdvisor advisor, ConfigReader configReader, out StrategyResultStatistics srResult)
        {
            srResult = new StrategyResultStatistics();
            if (!IsValid(sParam))
                return Double.MinValue;

            string cacheKey = CreateKey(SampleData.Symbol, sParam, SampleData.Data.First().DateTime, SampleData.Data.Last().DateTime);
            if (cacheResult.ContainsKey(cacheKey))
                return cacheResult[cacheKey];

            using (Log4Smart.Logger logger = new Log4Smart.Logger(false))
            {
                string strategyName = System.Configuration.ConfigurationManager.AppSettings["ExportAdvisorName"];
                try
                {
                    Engine engine = new Engine(logger, logger);
                    advisor.TesterInit(sParam, logger);
                    Account acc = new Account(DefaultAccount.Balance, DefaultAccount.Commission);
                    engine.StartTest(SampleData, advisor, acc);
                    srResult = acc.Statistics;
                    if (!engine.IsTestSuccessfull)
                    {
                        if (string.IsNullOrEmpty(directory))
                            directory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

                        string fullDir = directory + string.Format("/errors/{0}/{1}/{2}/", SampleData.Symbol, Guid.NewGuid().ToString(), StartTime.ToString("yyyyMMdd"));
                        configReader.SaveCurrentParameters(strategyName, sParam, fullDir);
                        logger.SaveLogToFile(fullDir, "error");
                        cacheResult[cacheKey] = Double.MinValue;

                        return Double.MinValue;
                    }
                    if (sParam.Contains("MinAmountOrdersFLP") && acc.Statistics.NumberOrders < sParam["MinAmountOrdersFLP"] && string.IsNullOrEmpty(directory))
                    {
                        cacheResult[cacheKey] = Double.MinValue;
                        return Double.MinValue;
                    }

                    if (!string.IsNullOrEmpty(directory))
                    {
                        string fullDir = directory + "/" + this.StartTime.ToString("yyyyMMdd") + "_" + (acc.Balance.ToString()) + "$";
                        configReader.SaveCurrentParameters(strategyName, sParam, fullDir);
                        logger.SaveLogToFile(fullDir, "allData");
                    }
                    if( DefaultAccount.Commission > 0 )
                        cacheResult[cacheKey] = acc.Statistics.CalculateConfidenceIntervalLow;
                    else
                        cacheResult[cacheKey] = 10 - acc.Statistics.CalculateConfidenceIntervalHigh;

                    return cacheResult[cacheKey];
                }
                catch (Exception exc)
                {
                    (logger as Log4Smart.IStrategyLogger).AddMessage("{0}", exc.ToString());
                    if (string.IsNullOrEmpty(directory))
                        directory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

                    string fullDir = directory + string.Format("/errors/{0}/{1}/", SampleData.Symbol, StartTime.ToString("yyyyMMdd"));
                    configReader.SaveCurrentParameters(strategyName, sParam, fullDir);
                    logger.SaveLogToFile(fullDir, "error");
                    cacheResult[cacheKey] = Double.MinValue;

                    return Double.MinValue;
                }
            }

    }
        public override void EstimateAll(string directory)
        {
            //StrategyParameter param = new StrategyParameter();

            //foreach (KeyValuePair<string, AForge.IntStepRange> range in Range.ranges)
            //{
            //    param.Add(range.Key, range.Value.Max);
            //}
            //OptimizationFunction(param, directory);
        }

        #region Cache
        static string CreateKey(string symbol, StrategyParameter sParam, DateTime start, DateTime end)
        {
            return symbol + sParam.ToString() + start.Ticks.ToString() + end.Ticks.ToString();
        }
        static private ConcurrentDictionary<string, double> cacheResult = new ConcurrentDictionary<string, double>();
        #endregion

        public override double OptimizationFunction(StrategyParameter sParam, out StrategyResultStatistics srResult)
        {
            throw new NotImplementedException();
        }

        public override double OptimizationFunction(StrategyParameter sParam, out StrategyResultStatistics srResult, string directory)
        {
            throw new NotImplementedException();
        }

        public override double OptimizationFunction(StrategyParameter sParam, string directory)
        {
            throw new NotImplementedException();
        }
    }
}
