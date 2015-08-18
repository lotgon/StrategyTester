using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SuperAdaptStrategy;
using EngineTest;
using Common;
using AForge.Genetic;
using AForge.Genetic.Fitness_Functions;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;

namespace TestServer
{
    public class TestManager
    {
        private SemaphoreEx semaphore;
        private class TestParameters
        {
            public TestParameters(InputData inData, DateTime startDate, int daysInterval, int daysTestInterval, bool IsFullStatistics, OptimizationFunctionIntNDFactory optimizationFunctionFactory, string dir)
            {
                this.inData = inData;
                this.startDate = startDate;
                this.daysInterval = daysInterval;
                this.daysTestInterval = daysTestInterval;
                this.IsFullStatistics = IsFullStatistics;
                this.OptimizationFunctionFactory = optimizationFunctionFactory;
                this.DefaultDirectory = dir;
            }
            public InputData inData;
            public DateTime startDate;
            public int daysInterval;
            public int daysTestInterval;
            public bool IsFullStatistics;
            public OptimizationFunctionIntNDFactory OptimizationFunctionFactory;
            public string DefaultDirectory;

        }
        object objSync = new object();

        public TestManager(int numberThreads)
        {
            semaphore = new SemaphoreEx(numberThreads, numberThreads);
        }
        public void RunGeneticTests(string defaultDir, InputData inData, DateTime startDate, int daysInterval, int daysTestInterval, OptimizationFunctionIntNDFactory optFuncFactory)
        {
            if (!System.IO.Directory.Exists(defaultDir))
                System.IO.Directory.CreateDirectory(defaultDir);

            DateTime currentDate = inData.Data.Last().DateTime;
            while( currentDate.DayOfWeek != DayOfWeek.Saturday )
                currentDate = currentDate.AddDays(-1);
            currentDate = currentDate.Date.AddWorkingDay(-1).AddWorkingDay(1);
            int i = 0;
            do
            {
                semaphore.WaitOne();

                System.Threading.Thread t = new Thread(new ParameterizedThreadStart(RunSinglePeriodBruteForceTest));
                t.Start(new TestParameters(inData, currentDate.AddWorkingDay(-daysInterval + 1), daysInterval, daysTestInterval, i++ == 0, optFuncFactory, defaultDir));
                //RunSingleTest(new TestParameters(inData, currentDate.AddWorkingDay(-daysInterval + 1), daysInterval, daysTestInterval, i++ == 0, optFuncFactory, defaultDir));

                currentDate = currentDate.AddWorkingDay(-daysTestInterval);
            }
            while(startDate < currentDate);
        }
        public void EstimateAll(InputData inData, OptimizationFunctionIntNDFactory optFuncFactory)
        {
            OptimizationFunctionIntND optTestFunction = optFuncFactory.CreateOptimizationFunctionIntND(
                inData, inData.Data[0].DateTime, EstimationFunctionType.RelativeStudent, new Account(Settings1.Default.Balance, Settings1.Default.Commision));
            optTestFunction.EstimateAll(string.Format("{0}-{1}-{2}", inData.Symbol, inData.Data.First().DateTime.ToString("yyyyMMdd"), inData.Data.Last().DateTime.ToString("yyyyMMdd")));


        }

        public void WaitJobFinished()
        {
            semaphore.WaitJobFinished();
        }

        public void RunSinglePeriodBruteForceTest(object objParam)
        {
            try
            {

                TestParameters testParam = objParam as TestParameters;
                InputData inData = testParam.inData;
                DateTime startDate = testParam.startDate;
                int daysInterval = testParam.daysInterval;
                int daysTestInterval = testParam.daysTestInterval;

                string outputPath = System.IO.Path.Combine(testParam.DefaultDirectory, string.Format("{0}-{1}-{2}", inData.Symbol, daysInterval, daysTestInterval));
                if (!System.IO.Directory.Exists(outputPath))
                    System.IO.Directory.CreateDirectory(outputPath);

            
                DateTime endDate = startDate.AddWorkingDay(daysInterval);
                DateTime startTestDate = endDate;
                DateTime endTestDate = startTestDate.AddWorkingDay(daysTestInterval);

                InputData inSampleData = inData.Select(startDate, endDate);
                InputData inTestData = inData.Select(startTestDate, endTestDate);

                List<StrategyParameter> listParams = testParam.OptimizationFunctionFactory.StratergyParameterRange.GetAllParameters();
                using (System.IO.StreamWriter completionLog = new System.IO.StreamWriter(string.Format("{2}\\ProgressIS_{0}_{1}.txt", inData.Symbol, startTestDate.ToString("yyyyMMdd"), outputPath)))
                {
                    List<KeyValuePair<StrategyParameter, StrategyResultStatistics>> listResult = new List<KeyValuePair<StrategyParameter, StrategyResultStatistics>>();

                    OptimizationFunctionIntND optTestFunction = testParam.OptimizationFunctionFactory.CreateOptimizationFunctionIntND(inSampleData, startDate, EstimationFunctionType.RelativeStudent, new Account(Settings1.Default.Balance, Settings1.Default.Commision));
                    foreach (StrategyParameter strategyParam in listParams)
                    {
                        StrategyResultStatistics sr;
                        double result = optTestFunction.OptimizationFunction(strategyParam, out sr);
                        listResult.Add(new KeyValuePair<StrategyParameter, StrategyResultStatistics>(strategyParam, sr));
                        completionLog.WriteLine(listResult.Last().ToString());
                        completionLog.Flush();
                    }

                    IEnumerable<KeyValuePair<StrategyParameter, StrategyResultStatistics>> arrayResult =
                        listResult.OrderByDescending(p => p.Value.CalculateConfidenceIntervalLow); 
                    using (System.IO.StreamWriter inSampleLog = new System.IO.StreamWriter(string.Format("{2}\\IS_{0}_{1}.txt", inData.Symbol, startTestDate.ToString("yyyyMMdd"), outputPath)))
                    {

                        DataContractSerializer ser = new DataContractSerializer(typeof(IEnumerable<KeyValuePair<StrategyParameter, StrategyResultStatistics>>));
                        ser.WriteObject(inSampleLog.BaseStream, arrayResult);
                    }

                    //analyze result
                    OptimizationFunctionIntND optOOSFunction = testParam.OptimizationFunctionFactory.CreateOptimizationFunctionIntND(inTestData, startTestDate, EstimationFunctionType.RelativeStudent, new Account(Settings1.Default.Balance, Settings1.Default.Commision));
                    StrategyParameter bestParameter = arrayResult.First().Key;
                    optOOSFunction.OptimizationFunction(bestParameter, outputPath);
                }

                //temporary OOS result
                //using (System.IO.StreamWriter completionLog = new System.IO.StreamWriter(string.Format("{2}\\ProgressOOS_{0}_{1}.txt", inData.Symbol, startTestDate.ToString("yyyyMMdd"), outputPath)))
                //{
                //    List<KeyValuePair<StrategyParameter, StrategyResultStatistics>> listResult = new List<KeyValuePair<StrategyParameter, StrategyResultStatistics>>();

                //    OptimizationFunctionIntND optTestFunction = testParam.OptimizationFunctionFactory.CreateOptimizationFunctionIntND(inTestData, startTestDate, EstimationFunctionType.RelativeStudent, new Account(Settings1.Default.Balance, Settings1.Default.Commision));
                //    foreach (StrategyParameter strategyParam in listParams)
                //    {
                //        StrategyResultStatistics sr;
                //        double result = optTestFunction.OptimizationFunction(strategyParam, out sr);
                //        listResult.Add(new KeyValuePair<StrategyParameter, StrategyResultStatistics>(strategyParam, sr));
                //        completionLog.WriteLine(listResult.Last().ToString());
                //        completionLog.Flush();
                //    }
                //    using (System.IO.StreamWriter outSampleLog = new System.IO.StreamWriter(string.Format("{2}\\OOS_{0}_{1}.txt", inData.Symbol, startTestDate.ToString("yyyyMMdd"), outputPath)))
                //    {
                //        IEnumerable<KeyValuePair<StrategyParameter, StrategyResultStatistics>> arrayResult =
                //            listResult.OrderByDescending(p => p.Value.CalculateConfidenceIntervalLow);
                //        DataContractSerializer ser = new DataContractSerializer(typeof(IEnumerable<KeyValuePair<StrategyParameter, StrategyResultStatistics>>));
                //        ser.WriteObject(outSampleLog.BaseStream, arrayResult);
                //    }

                //}
              
            }
            finally
            {
                semaphore.Release();
            }
        }
        
        private void RunSinglePeriodGeneticTest(object objParam)
        {
            TestParameters testParam = objParam as TestParameters;
            InputData inData = testParam.inData;
            DateTime startDate = testParam.startDate;
            int daysInterval = testParam.daysInterval;
            int daysTestInterval = testParam.daysTestInterval;

            string outputPath = System.IO.Path.Combine(testParam.DefaultDirectory, string.Format("{0}-{1}-{2}", inData.Symbol, daysInterval, daysTestInterval));
            if (!System.IO.Directory.Exists(outputPath))
                System.IO.Directory.CreateDirectory(outputPath);

            try
            {
                DateTime endDate = startDate.AddWorkingDay(daysInterval);
                DateTime startTestDate = endDate;
                DateTime endTestDate = startTestDate.AddWorkingDay(daysTestInterval);

                InputData inSampleData = inData.Select(startDate, endDate);
                InputData inTestData = inData.Select(startTestDate, endTestDate);

                OptimizationFunctionIntND optFunction = testParam.OptimizationFunctionFactory.CreateOptimizationFunctionIntND(
                    inSampleData, startDate, (EstimationFunctionType)Settings1.Default.EstimationFunctionType, new Account(Settings1.Default.Balance, Settings1.Default.Commision));
                // create population
                Population population = new Population(Settings1.Default.GeneticPopulationSize,
                    new ShortArrayChromosome(optFunction.StratergyParameterRange.ranges.Count),
                    optFunction,
                    (ISelectionMethod)new RankSelection()
                    );

                using (System.IO.StreamWriter completionLog = new System.IO.StreamWriter(string.Format("{2}\\Progress_{0}_{1}.txt", inData.Symbol, startTestDate.ToString("yyyyMMdd"), outputPath)))
                {
                    try
                    {
                        int epochCount = Settings1.Default.EpochCount;
                        while (--epochCount != 0)
                        {
                            population.RunEpoch();
                            if (population.BestChromosome != null)
                            {
                                completionLog.WriteLine("{0}: Best fitness {1}. Parameters {2}", DateTime.Now.ToString(),
                                   population.BestChromosome.Fitness, (optFunction.Translate(population.BestChromosome)).ToString());
                            }
                            else
                            {
                                completionLog.WriteLine("{0}: Best fitness null.", DateTime.Now.ToString());
                            }
                            completionLog.Flush();
                        }

                        completionLog.WriteLine("Write results:");
                        if (population.BestChromosome != null)
                        {
                            OptimizationFunctionIntND optTestFunction = testParam.OptimizationFunctionFactory.CreateOptimizationFunctionIntND(inTestData, startTestDate, EstimationFunctionType.RelativeStudent, new Account(Settings1.Default.Balance, Settings1.Default.Commision));
                            StrategyParameter bestParameter = optFunction.Translate(population.BestChromosome);
                            bestParameter["FixedVolume"] = 1;
                            optTestFunction.OptimizationFunction(bestParameter, outputPath);
                        }
                    }
                    catch (Exception exc)
                    {
                        completionLog.WriteLine(exc.ToString());
                    }
                }


            }
            finally
            {
                semaphore.Release();
            }
        }



    }
}
