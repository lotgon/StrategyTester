using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ResultBusinessEntity
{
    public class SymbolResult
    {
        public List<TestResult> testResults = new List<TestResult>();
        public string Symbol;

        static public SymbolResult Load(string path)
        {
            SymbolResult symResult = new SymbolResult();
            foreach (string testPath in Directory.GetDirectories(path))
            {
                foreach (string testPath2 in Directory.GetDirectories(testPath))
                {
                    string dirName = Path.GetFileName(testPath2);
                    string[] testParam = dirName.Split(new char[] { '-' });
                    symResult.Symbol = testParam[0];
                    symResult.testResults.Add(TestResult.Load(Path.GetFileName(testPath), testPath2, symResult.Symbol));
                }
            }

            return symResult;
        }
        //public void SaveStatistics(string path)
        //{
            //List<Mathematics.Interval> listIntervals = new List<Mathematics.Interval>();
            //List<int> listEquity = new List<int>();

            //foreach (TestResult currTestResult in testResults)
            //{
            //    Mathematics.Interval interval = currTestResult.GetConfidenceInterval;
            //    listIntervals.Add(interval);
            //    listEquity.Add(currTestResult.GetProfit);
            //    File.Create(Path.Combine(path, currTestResult.Name + "_" + currTestResult.GetProfit.ToString() +
            //        "_"+ interval.Minimum.ToString("#")+ "_"+ interval.Maximum.ToString("#")));
            //}
            //File.Create(Path.Combine(path, "Summary_" + listEquity.Sum().ToString() +
            //    "_" + listIntervals.Average(p => p.Minimum).ToString("#") + "_" + listIntervals.Average(p => p.Maximum).ToString("#")));

            //foreach (TestResult currTestResult in testResults)
            //    foreach (OnePeriodResult currOnePeriodresult in currTestResult.listOnePeriodresult)
            //    {
            //        using (System.IO.StreamWriter TPShiftLog = new System.IO.StreamWriter(Path.Combine(currOnePeriodresult.ResultDirectory, 
            //            "TPShift_IS.txt")))
            //            {
            //                foreach (KeyValuePair<StrategyParameter, StrategyResultStatistics> currKeyValue in currOnePeriodresult.ISResult)
            //                    //if( currKeyValue.Value. > 1 )
            //                    TPShiftLog.WriteLine(string.Format("{0}      {1}      {2}", currKeyValue.Key["TP"], currKeyValue.Key["OpenOrderShift"], currKeyValue.Value));
            //            }
            //        //using (System.IO.StreamWriter TPShiftLog = new System.IO.StreamWriter(Path.Combine(currOnePeriodresult.ResultDirectory,
            //        //    "TPShift_OOS.txt")))
            //        //{
            //        //    foreach (KeyValuePair<StrategyParameter, StrategyResultStatistics> currKeyValue in currOnePeriodresult.OOSResult)
            //        //            TPShiftLog.WriteLine(string.Format("{0}      {1}      {2}", currKeyValue.Key["TP"], currKeyValue.Key["OpenOrderShift"], currKeyValue.Value));
            //        //}                    

            //    }

        //}
    }
}
