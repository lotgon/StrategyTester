using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using EngineTest;
using BuyLimitAndWait;
using ForexSuite.Analyzers.ResetTime;

namespace Analysis
{
    public class ResetTimeAnalysis
    {
        public enum SideAnalyser
        {
            Buy,
            Sell
        }

        static public double CalculateTimeOfOnePips(string sourceDirectory, string symbolName, ResetTimeAnalysis.SideAnalyser side)
        {
            InputData inData = new InputData();
            inData.LoadFromDirectory(sourceDirectory, null);
            
            double swap = 0;
            if (ResetTimeAnalysis.SideAnalyser.Buy == side )
                swap = inData.SwapBuy;
            if (ResetTimeAnalysis.SideAnalyser.Sell == side )
                swap = inData.SwapSell;

            FuturePredictor fPredictor = new FuturePredictor(inData.Data);
            List<StratergyParameterRange> ranges = new BuyLimitAndWaitConfigReader().GetRanges(new List<string>());

            StrategyParameter currParam = ranges[0].GetAllParameters().Last();
            IEnumerable<int> buyArray, sellArray;
            ResetTimeAnalysis.CalculatePoints(currParam, inData, out buyArray, out sellArray);

            string profitSymbol = ForexSuite.SymbolsManager.GetProfitSymbol(symbolName);
            double coefToUSD = ForexSuite.QuotesManager.ConvertCurrency(profitSymbol, "USD", DateTime.Now.AddMonths(-7), ForexSuite.SymbolsManager.ValueFromPips(profitSymbol, 1));
            ResetTimeAnalyzer rta = new ResetTimeAnalyzer(currParam["TP"], 1 / 60d, fPredictor.Bids, fPredictor.Asks, coefToUSD, swap);
            if (side == SideAnalyser.Buy)
                rta.Process(buyArray, null);
            else
                rta.Process(null, sellArray);
            if (rta.RealProfit == 0)
                throw new ApplicationException("RealProfit=0");
            return rta.AverageTime / rta.RealProfit;
        }


        static public void CalculatePoints(StrategyParameter currParam, InputData inData,
            out IEnumerable<int> buyArray, out IEnumerable<int> sellArray)
        {
            CalculatePoints(currParam, inData, new FuturePredictor(inData.Data), 0, out buyArray, out sellArray);
        }

        static public void CalculatePoints(StrategyParameter currParam, InputData inData, FuturePredictor fPredictor, int countDiff,
            out IEnumerable<int> buyArray, out IEnumerable<int> sellArray)
        {
            buyArray = null;
            sellArray = null;

            BuyLimitAndWaitStrategy blwStrategy = new BuyLimitAndWaitStrategy();
            currParam["InitHistoryMinutes"] = 0;
            blwStrategy.TesterInit(currParam, null);

            EngineFast engineFast = new EngineFast();
            engineFast.StartTest(inData, blwStrategy, fPredictor, false, 100);

            buyArray = Enumerable.Select(engineFast.HistoryOpenBuyPoints, p => p + countDiff);
            sellArray = Enumerable.Select(engineFast.HistoryOpenSellPoints, p => p + countDiff);
        }
    }
}
