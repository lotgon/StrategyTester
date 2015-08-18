using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    //public class Portfolio
    //{
    //    public class PortfolioRecord
    //    {
    //        public PortfolioRecord( string Symbol, string TestID, double Coef)
    //        {
    //            this.Symbol =Symbol;
    //            this.TestID = TestID;
    //            this.Coef = Coef;
    //        }
    //        public string Symbol {get; private set;}
    //        public string TestID { get; private set; }
    //        public double Coef { get; private set; }

    //        public string ToString()
    //        {
    //            return string.Format("{0} {1} {2}", this.Symbol, this.TestID, this.Coef);
    //        }
    //    }

    //    public List<SymbolResult> SymbolResults { get; protected set; }
    //    public List<PortfolioRecord> PortfolioRecords { get; protected set; }
    //    public double DailyProfit = 0;

    //    public Portfolio(List<SymbolResult> symResults)
    //    {
    //        this.SymbolResults = symResults;
    //        this.PortfolioRecords = new List<PortfolioRecord>();
    //    }
    //    public void CalculatePortfolio(int initialSum, double reliability, double maximumLoss, double minVolume, int maximumTests)
    //    {
    //        double[,] equityForEachTest = new double[SymbolResults.Count, SymbolResults[0].testResults[0].dictDayEquity.Count];

    //        int indexSymbol = 0;
    //        foreach (SymbolResult symResult in SymbolResults)
    //        {
    //            int testCount = Math.Min(symResult.testResults.Count, maximumTests);
    //            SortedDictionary<DateTime, int> dictSumDayEquity = new SortedDictionary<DateTime, int>();
    //            for (int i = 0; i < testCount; i++)
    //            {
    //                TestResult currTestResult = symResult.testResults[i];
    //                foreach (KeyValuePair<DateTime, int> currKeyValue in currTestResult.dictDayEquity)
    //                    if (dictSumDayEquity.Keys.Contains(currKeyValue.Key))
    //                        dictSumDayEquity[currKeyValue.Key] += currKeyValue.Value / testCount;
    //                    else
    //                        dictSumDayEquity[currKeyValue.Key] = currKeyValue.Value / testCount;
    //            }
    //            if (dictSumDayEquity.Count != SymbolResults[0].testResults[0].dictDayEquity.Count)
    //                throw new ApplicationException("dictSumDayEquity.Count != SymbolResults[0].testResults[0].dictDayEquity.Count");

    //            int indexEquity = 0;
    //            int prevousValue = initialSum;
    //            foreach (KeyValuePair<DateTime, int> currKeyValue in dictSumDayEquity)
    //            {
    //                int temp = currKeyValue.Value + prevousValue;
    //                equityForEachTest[indexSymbol, indexEquity++] = prevousValue = temp;
    //            }
    //            indexSymbol++;
    //        }

    //        Mathematics.Portfolio port;
    //        if( minVolume <= 0)
    //            port = Mathematics.Portfolio.CalculateLinear(equityForEachTest, reliability, maximumLoss);
    //        else
    //            port = Mathematics.Portfolio.CalculateLinearFast(equityForEachTest, reliability, maximumLoss, minVolume);

    //        indexSymbol = 0;
    //        foreach (SymbolResult symResult in SymbolResults)
    //            PortfolioRecords.Add(new PortfolioRecord(symResult.Symbol, "", port.Coefficients[indexSymbol++]));
    //        this.DailyProfit = port.Profit;
    //    }

    //}
}
