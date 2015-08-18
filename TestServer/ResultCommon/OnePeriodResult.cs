using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using Common;
using System.Globalization;

namespace ResultBusinessEntity
{
    public class OnePeriodResult
    {
        //public SortedDictionary<DateTime, int> dictDayEquity = new SortedDictionary<DateTime, int>();
        public int Volume, Orders, MinimumEquity, MaximumMargin;

        //public IEnumerable<KeyValuePair<StrategyParameter, StrategyResultStatistics>> OOSResult;
        public IEnumerable<KeyValuePair<StrategyParameter, StrategyResultStatistics>> ISResult;
        public string RoorDirectory;
        public DateTime StartDateTime;

        static public OnePeriodResult Load(string rootPath, string symbol)
        {
            OnePeriodResult result = new OnePeriodResult();
            result.RoorDirectory = rootPath;
            result.StartDateTime = GetFirstFileDate(rootPath, symbol);
            result.ISResult = OnePeriodResult.LoadResultsFromFile(Path.Combine(rootPath, GetISResultName(symbol, result.StartDateTime)));

            return result;
        }

        private static DateTime GetFirstFileDate(string path, string symbol)
        {
            string startString = "IS_" + symbol.ToUpper() + "_";
            string endString = ".txt";
            foreach (string filePaths in Directory.GetFiles(path,  startString+ "*" + endString))
            {
                string name = Path.GetFileName(filePaths).Replace(startString, "").Replace(endString, "");
                return DateTime.ParseExact(name, "yyyyMMdd", CultureInfo.InvariantCulture);
            }
            throw new ApplicationException("There is no result data files in folder: " + path);
        }

        private static string GetOOSResultName(string symbol, DateTime startDateTime)
        {
            return string.Format("OOS_{0}_{1}.txt", symbol, startDateTime.ToString("yyyyMMdd"));
        }

        private static string GetISResultName(string symbol, DateTime startDateTime)
        {
            return string.Format("IS_{0}_{1}.txt", symbol, startDateTime.ToString("yyyyMMdd"));
        }

        static public IEnumerable<KeyValuePair<StrategyParameter, StrategyResultStatistics>> LoadResultsFromFile(string fileName)
        {
             using (FileStream fs = new FileStream(fileName, FileMode.Open))
             using (XmlDictionaryReader reader = XmlDictionaryReader.CreateTextReader(fs, new XmlDictionaryReaderQuotas()))
             {

                 DataContractSerializer ser = new DataContractSerializer(typeof(IEnumerable<KeyValuePair<StrategyParameter, StrategyResultStatistics>>));
                 return (IEnumerable<KeyValuePair<StrategyParameter, StrategyResultStatistics>>)ser.ReadObject(reader, true);
             }

        }
    }
}
