using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace ResultBusinessEntity
{
    public class TestResult
    {
        public List<OnePeriodResult> listOnePeriodresult = new List<OnePeriodResult>();
        //public SortedDictionary<DateTime, int> dictDayEquity = new SortedDictionary<DateTime, int>();
        public string Name;

        static public TestResult Load(string name, string path, string symbol)
        {
            TestResult result = new TestResult();
            result.Name = name;
            result.listOnePeriodresult.Add(OnePeriodResult.Load(path, symbol));

            //foreach (string periodPath in Directory.GetDirectories(path))
            //{
            //    Console.WriteLine("Loading " + periodPath);
            //    string dirFileName = Path.GetFileName(periodPath);
            //    string[] testParam = dirFileName.Split(new char[] { '_' });
            //    try
            //    {
            //        DateTime dateTime = DateTime.ParseExact(testParam[0], "yyyyMMdd", CultureInfo.InvariantCulture);
            //        result.listOnePeriodresult.Add(OnePeriodResult.Load(path, dirFileName, dateTime, testPeriod, symbol));
            //    }
            //    catch (FormatException exc)
            //    {
            //        throw new FormatException("Error of parsing " + testParam[0] + ".", exc);
            //    }
            //}
            //foreach (OnePeriodResult onePeriod in result.listOnePeriodresult)
            //{
            //    foreach (KeyValuePair<DateTime, int> currDayEquity in onePeriod.dictDayEquity)
            //    {
            //        int currEquity = 0;
            //        result.dictDayEquity.TryGetValue(currDayEquity.Key, out currEquity);
            //        if (currEquity == 0)
            //            result.dictDayEquity[currDayEquity.Key] = currDayEquity.Value;
            //    }
            //}
            return result;
        }

        //public Mathematics.Interval GetConfidenceInterval
        //{
        //    get
        //    {
        //        return Mathematics.Statistics.CalculateConfidenceInterval(dictDayEquity.Values.Select(i => (double)i).ToArray(), 0.9);
        //    }
        //}
        //public int GetProfit
        //{
        //    get
        //    {
        //        return dictDayEquity.Values.Sum();
        //    }
        //}
        

    }
}
