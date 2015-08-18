using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using EngineTest;
using BuyLimitAndWait;
using Common;
using ForexSuite.Analyzers.ResetTime;
using Analysis;

namespace GUIAnalyser.ViewModel
{
        internal class ResetTimeViewModel
    {

        public string SourceDirectory { get; private set; }
        public string SymbolName { get; private set; }
        public ResetTimeAnalysis.SideAnalyser Side { get; private set; }
        int NotReadOnlyTimePercent = 100; //0..100
        SwapCollection SwapCollection = null;

        //public ResetTimeViewModel(string sourceDirectory, string symbolName, SwapCollection swapCollection)
        //    : this(sourceDirectory, symbolName, ResetTimeAnalysis.SideAnalyser.Both, 100, swapCollection) 
        //{
        //}

        public ResetTimeViewModel(string sourceDirectory, string symbolName, ResetTimeAnalysis.SideAnalyser sa, int notReadOnlyTimePercent, SwapCollection swapCollection)
        {
            this.SourceDirectory = sourceDirectory;
            this.SymbolName = symbolName;
            this.Side = sa;
            this.NotReadOnlyTimePercent = notReadOnlyTimePercent;
            this.SwapCollection = swapCollection;
        }
        public void AnalyseStrategy(DateTime? fromNullable, DateTime? toNullable)
        {
            Dictionary<int, ResetTimeAnalyzer> dictResp = new Dictionary<int, ResetTimeAnalyzer>();

            double swap = 0;
            if (ResetTimeAnalysis.SideAnalyser.Buy == Side && SwapCollection != null)
                swap = SwapCollection.GetBuySwap(SymbolName);
            if (ResetTimeAnalysis.SideAnalyser.Sell == Side && SwapCollection != null)
                swap = SwapCollection.GetSellSwap(SymbolName);

            InputData inDataAll = new InputData();
            inDataAll.LoadFromDirectory(this.SourceDirectory, null);
            DateTime from = fromNullable.HasValue ? fromNullable.Value : inDataAll.Data.First().DateTime;
            DateTime to = toNullable.HasValue ? toNullable.Value : inDataAll.Data.Last().DateTime;
            inDataAll = inDataAll.Select(from, to);

            while (from < to)
            {
                InputData inData = inDataAll.Select(from, to);
                if (inData.Data.Count == 0)
                    continue;
                int countDiff = inDataAll.Data.Count - inData.Data.Count; 
                FuturePredictor fPredictor = new FuturePredictor(inData.Data);
                List<StratergyParameterRange> ranges = new BuyLimitAndWaitConfigReader().GetRanges(new List<string>());

                Dictionary<int, ResetTimeAnalyzer> dictRespTest = new Dictionary<int, ResetTimeAnalyzer>();
                foreach (StrategyParameter currParam in ranges[0].GetAllParameters())
                {
                    if (!dictResp.ContainsKey(currParam["TP"]))
                    {
                        dictResp.Add(currParam["TP"], new ResetTimeAnalyzer(currParam["TP"], 1 / 60d, fPredictor.Bids, fPredictor.Asks, 1, swap));
                    }
                    IEnumerable<int> buyArray, sellArray;

                    ResetTimeAnalysis.CalculatePoints(currParam, inData, fPredictor, countDiff, out buyArray, out sellArray);
                    dictResp[currParam["TP"]].Process(buyArray, sellArray);
                }

                from = from.AddDays(14);
            }
            ResetTimeScript rts = new ResetTimeScript(dictResp.Values.ToList(), "",
                GetScriptDirectory(this.SymbolName, "", fromNullable, toNullable)); ;
            rts.Save();
        }


        public void AnalyseAllPoints(DateTime? fromNullable, DateTime? toNullable)
        {
            InputData inDataAll = new InputData();
            inDataAll.LoadFromDirectory(this.SourceDirectory, null);
            DateTime from = fromNullable.HasValue ? fromNullable.Value : inDataAll.Data.First().DateTime;
            DateTime to = toNullable.HasValue ? toNullable.Value : inDataAll.Data.Last().DateTime;
            InputData inData = inDataAll.Select(from, to);

            FuturePredictor fPredictor = new FuturePredictor(inData.Data);
            List<StratergyParameterRange> ranges = new BuyLimitAndWaitConfigReader().GetRanges(new List<string>());


            Dictionary<int, ResetTimeAnalyzer> dictResp = new Dictionary<int, ResetTimeAnalyzer>();
            foreach (StrategyParameter currParam in ranges[0].GetAllParameters())
            {
                if (!dictResp.ContainsKey(currParam["TP"]) /*&& i++ < inData.Data.Count / 2*/)
                {
                    string profitSymbol = ForexSuite.SymbolsManager.GetProfitSymbol(this.SymbolName);
                    double coefToUSD = ForexSuite.QuotesManager.ConvertCurrency(profitSymbol, "USD", inData.Data.Last().DateTime, ForexSuite.SymbolsManager.ValueFromPips(profitSymbol, 1));
                    dictResp.Add(currParam["TP"], new ResetTimeAnalyzer(currParam["TP"], 1 / 60d, fPredictor.Bids, fPredictor.Asks, coefToUSD));

                    IEnumerable<int> buyArray = null, sellArray = null;
                    switch (Side)
                    {
                        case ResetTimeAnalysis.SideAnalyser.Buy:
                            buyArray = Enumerable.Range(0, (inData.Data.Count - 1));
                            break;
                        default:
                            sellArray = Enumerable.Range(0, (inData.Data.Count - 1));
                            break;
                    }
                    dictResp[currParam["TP"]].Process(buyArray, sellArray);
                }
            }
            ResetTimeScript rts = new ResetTimeScript(dictResp.Values.ToList(), "",
                GetScriptDirectory(this.SymbolName, "AllPoints", fromNullable, toNullable)); ;
            rts.Save();
        }

        public bool Test()
        {
            InputData inData = new InputData();
            inData.LoadFromDirectory(this.SourceDirectory, null);

            List<StratergyParameterRange> ranges = new BuyLimitAndWaitConfigReader().GetRanges(new List<string>());

            BuyLimitAndWaitStrategy blwStrategy = new BuyLimitAndWaitStrategy();
            StrategyParameter sParam = ranges[0].GetAllParameters()[0];
            sParam["InitHistoryMinutes"] = 0;
            blwStrategy.TesterInit(sParam, null);

            FuturePredictor fPredictor = new FuturePredictor(inData.Data);
            EngineFast engineFast = new EngineFast();
            engineFast.StartTest(inData, blwStrategy, fPredictor, true, 100);

            blwStrategy = new BuyLimitAndWaitStrategy();
            sParam = ranges[0].GetAllParameters()[0];
            sParam["InitHistoryMinutes"] = 0;
            blwStrategy.TesterInit(sParam, null);
            Engine engine = new Engine(null, null);
            engine.StartTest(inData, blwStrategy, new Account(10000000, 3));

            return engineFast.HistoryOpenBuyPoints.Count() + engineFast.HistoryOpenSellPoints.Count() 
                == engine.ClosedOrders.Count();
        }

        private string GetScriptDirectory(string symbol, string folderComment, DateTime? from, DateTime? to)
        {
            string ExecPath =
                System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            string baseString = System.IO.Path.Combine(ExecPath, @"scripts", typeof(ResetTimeScript).Name, symbol,
                folderComment, DateTime.Now.ToString("MM_dd_HH_mm"));
            string param = string.Format("_Side={0}_from={1}_to={2}_nrp={3}", Side.ToString(), from, to, this.NotReadOnlyTimePercent);
            return baseString + param;
        }
    }
}
