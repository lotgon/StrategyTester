using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using EngineTest;
using System.IO;

namespace StrategyCommon
{
    public class HistorySimulator :IDisposable
    {
        public BasicParam CustomParam { get; private set; }
        public bool IsSuccessful { get;  private set;}
        string rootPath = "./sourcedata";
        FxAdvisorCore.SimpleAdvisor Advisor;
        StrategyParameter CurrentStrategyParam;
        string Symbol;
        IMeta Meta;
        const GraphPeriod CurrentGraphPeriod = Common.GraphPeriod.PERIOD_M1;
        Log4Smart.Logger Logger;
        Engine Engine;

        public HistorySimulator(BasicParam customParam, bool IsTestingMode, FxAdvisorCore.SimpleAdvisor advisor, StrategyParameter currentStrategyParam, string symbol, IMeta meta)
        {
            this.CustomParam = customParam;
            this.IsSuccessful = false;
            if (IsTestingMode)
            {
                rootPath = System.Configuration.ConfigurationManager.AppSettings["QuotesFullPath"];
            }
            this.Advisor = advisor;
            this.CurrentStrategyParam = currentStrategyParam;
            this.Symbol = InputData.ConvertToStandardName(symbol);
            this.Meta = meta;
        }

        public IEnumerable<Order> Init(DateTime currentDateTime, DateTime startSimulate, out List<string> logRecords)
        {
            logRecords = new List<string>();
            string swapFilePath = Path.Combine(rootPath, "swap.txt");
            string symbolPath = Path.Combine(rootPath, Symbol);
            
            InputData inData = new InputData();
            inData.LoadFromDirectory(symbolPath, new SwapCollection().LoadSwap(swapFilePath), 
                /*currentDateTime.AddMinutes( - CustomParam.InitHistoryMinutes)*/
                startSimulate, currentDateTime);
            DateTime dateTemp = inData.Data.Count == 0 ? startSimulate : inData.Data.Last().DateTime;
            int shift = Meta.iBarShift(Symbol, CurrentGraphPeriod, dateTemp, false);

            for (int i = shift; i > 0; i--)
            {
                GroupTick gt = new GroupTick();
                gt.DateTime = currentDateTime.AddMinutes(-i);
                gt.OpenBid = gt.OpenAsk = Meta.iOpen(CustomParam.Symbol, CurrentGraphPeriod, i);
                gt.MaxBid = gt.MaxAsk = Meta.iHigh(CustomParam.Symbol, CurrentGraphPeriod, i);
                gt.MinBid = gt.MinAsk = Meta.iLow(CustomParam.Symbol, CurrentGraphPeriod, i);
                if (gt.OpenBid == 0 || gt.MaxBid == 0 || gt.MinBid == 0)
                    continue;

                inData.AddGroupTick(gt);
            }
            logRecords.Add(string.Format("{0} {1} were loaded.", Symbol, inData.Data.Count));
            
            CustomParam.InitHistoryMinutes = 0;
             
            InputData inputData = inData;
            Logger = new Log4Smart.Logger(false);
            Engine = new Engine(Logger, Logger);
            
            //save state of CurrentStrategyParam
            int temp = CurrentStrategyParam["InitHistoryMinutes"];
            CurrentStrategyParam["InitHistoryMinutes"] = 0;
            string firstRawParam = CurrentStrategyParam.GetStringParameter("P1String");
            CurrentStrategyParam.SetStringParameter("P1String", CustomParam.RawStrategyParam);


            Advisor.TesterInit(CurrentStrategyParam, Logger);
            Account acc = new Account(10000000, 4);
            Engine.StartTest(inputData, Advisor, acc, false);

            //restor state of CurrentStrategyParam
            CurrentStrategyParam["InitHistoryMinutes"] = temp;
            CurrentStrategyParam.SetStringParameter("P1String", firstRawParam);

            IsSuccessful = Engine.IsTestSuccessfull;
            if (!Engine.IsTestSuccessfull)
            {
                string directory = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                string fullDir = directory + string.Format("/errors/{0}/{1}/{2}/", inputData.Symbol, Guid.NewGuid().ToString(), inputData.Data.First().DateTime.ToString("yyyyMMdd"));
                Logger.SaveLogToFile(fullDir, "error");
            }
            return Engine.GetLimitOrders().Union(Engine.GetMarketOrders());
        }

        public IEnumerable<Order> AddTick(Tick<int> tick)
        {
            Engine.TickTack(new GroupTick(tick));
            return Engine.GetMarketOrders();
        }

        //public bool IsEnabled
        //{
        //    get
        //    {
        //        return CustomParam.InitHistoryMinutes > 0;
        //    }
        //}
        //public void Disable()
        //{
        //    CustomParam.InitHistoryMinutes = 0;
        //}

        public IEnumerable<Order> LimitOrders
        {
            get
            {
                return Engine.GetLimitOrders();
            }

        }
        public IEnumerable<Order> MarketOrders
        {
            get
            {
                return Engine.GetMarketOrders();
            }

        }

        public void Dispose()
        {
            Logger.Dispose();
        }
    }
}
