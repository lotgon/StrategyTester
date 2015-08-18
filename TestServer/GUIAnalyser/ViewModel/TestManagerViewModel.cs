using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using EngineTest;
using AForge.Genetic.Fitness_Functions;
using System.Threading;
using AForge.Genetic;
using System.Runtime.Serialization;

namespace GUIAnalyser.ViewModel
{
    public class TestManagerViewModel
    {
        private SemaphoreEx semaphore;
        private class TestParameters
        {
            public TestParameters(InputData inData, DateTime startDate, DateTime endDate, OptimizationFunctionIntNDFactory optimizationFunctionFactory, string dir)
            {
                this.inData = inData;
                this.startDate = startDate;
                this.endDate = endDate;
                this.OptimizationFunctionFactory = optimizationFunctionFactory;
                this.DefaultDirectory = dir;
            }
            public InputData inData;
            public DateTime startDate;
            public DateTime endDate;
            public OptimizationFunctionIntNDFactory OptimizationFunctionFactory;
            public string DefaultDirectory;

        }
        object objSync = new object();

        public TestManagerViewModel(int numberThreads)
        {
            semaphore = new SemaphoreEx(numberThreads, numberThreads);
        }
        public void RunBruteForceTest(string defaultDir, InputData inData, OptimizationFunctionIntNDFactory optFuncFactory)
        {
            if (!System.IO.Directory.Exists(defaultDir))
                System.IO.Directory.CreateDirectory(defaultDir);

            semaphore.WaitOne();

            System.Threading.Thread t = new Thread(new ParameterizedThreadStart(RunSinglePeriodBruteForceTest));
            t.Start(new TestParameters(inData, inData.Data[0].DateTime, inData.Data.Last().DateTime, optFuncFactory, defaultDir));
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
                DateTime endDate = testParam.endDate;

                string outputPath = System.IO.Path.Combine(testParam.DefaultDirectory, inData.Symbol);
                if (!System.IO.Directory.Exists(outputPath))
                    System.IO.Directory.CreateDirectory(outputPath);

                List<StrategyParameter> listParams = testParam.OptimizationFunctionFactory.StratergyParameterRange.GetAllParameters();
                using (System.IO.StreamWriter completionLog = new System.IO.StreamWriter(string.Format("{2}\\ProgressIS_{0}_{1}.txt", inData.Symbol, startDate.ToString("yyyyMMdd"), outputPath)))
                {
                    List<KeyValuePair<StrategyParameter, StrategyResultStatistics>> listResult = new List<KeyValuePair<StrategyParameter, StrategyResultStatistics>>();

                    OptimizationFunctionIntND optTestFunction = testParam.OptimizationFunctionFactory.CreateOptimizationFunctionIntND(inData, startDate, EstimationFunctionType.RelativeStudent, new Account(10000000, 4));
                    bool IsCreateLogFiles = true;
                    foreach (StrategyParameter strategyParam in listParams)
                    {
                        StrategyResultStatistics sr;
                        double result;
                        if (IsCreateLogFiles)
                        {
                            optTestFunction.OptimizationFunction(strategyParam, out sr, outputPath);
                            IsCreateLogFiles = false;
                        }
                        else 
                            result = optTestFunction.OptimizationFunction(strategyParam, out sr);

                        listResult.Add(new KeyValuePair<StrategyParameter, StrategyResultStatistics>(strategyParam, sr));
                        completionLog.WriteLine(listResult.Last().ToString());
                        completionLog.Flush();
                    }

                    IEnumerable<KeyValuePair<StrategyParameter, StrategyResultStatistics>> arrayResult =
                        listResult.OrderByDescending(p => p.Value.CalculateConfidenceIntervalLow);
                    using (System.IO.StreamWriter inSampleLog = new System.IO.StreamWriter(string.Format("{2}\\IS_{0}_{1}.txt", inData.Symbol, startDate.ToString("yyyyMMdd"), outputPath)))
                    {

                        DataContractSerializer ser = new DataContractSerializer(typeof(IEnumerable<KeyValuePair<StrategyParameter, StrategyResultStatistics>>));
                        ser.WriteObject(inSampleLog.BaseStream, arrayResult);
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
