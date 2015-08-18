using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EngineTest;
using Log4Smart;
using Common;
using System.IO;
using FxAdvisorCore.Extensibility;
using StrategyCommon;
using News;
using FxAdvisorCore;

namespace BuyLimitAndWait
{
    [RegisterAdvisor(Name = "BuyLimitAndWaitStrategy")]
    public class BuyLimitAndWaitStrategy : FxAdvisorCore.SimpleAdvisor
    {
        enum OpenMode
        {
            OnlyBuy=0, 
            OnlySell=1, 
            Both
        }
        class BuyLimitAndWaitParam : BasicParam
        {
            public int TP;
            public int OpenOrderShift;
            public OpenMode OpenMode;

            public int TralTP;
            public DateTime StartDate;
            public int MinTradeVolume;

            public HistorySimulator historySimulator;
        };
        List<BuyLimitAndWaitParam> customParams = new List<BuyLimitAndWaitParam>();
        object sObject = new object();

        DateTime currTime = DateTime.MinValue;
        List<string> listSymbols = new List<string>();
        SpreadAnalyzer spreadAnalyzer;
        int TralTP = 0;

        public BuyLimitAndWaitStrategy() 
        {
        }

        public override bool onStart(int ticks)
        {
            spreadAnalyzer = new SpreadAnalyzer(100);
            
            int maxCountParam = 1000;
            //history simulator initialize only first parameter. 
            if (this.TestingMode)
                maxCountParam = 1;
            for (int i = 1; i <= maxCountParam; i++)
            {
                string paramName = string.Format("P{0}String", i);
                if (this.param.ContainsString(paramName))
                {
                    BuyLimitAndWaitParam cparam = new BuyLimitAndWaitParam();
                    cparam.RawStrategyParam = this.param.GetStringParameter(paramName);
                    string[] paramStr = cparam.RawStrategyParam.Split(new char[] { ',' });
                    cparam.Symbol = paramStr[0];
                    cparam.TP = Int32.Parse(paramStr[1]);
                    cparam.OpenOrderShift = Int32.Parse(paramStr[2]);
                    cparam.BasicVolume = Int32.Parse(paramStr[3]);
                    cparam.OpenMode = (OpenMode)Int32.Parse(paramStr[4]);
                    cparam.StartDate = new DateTime(Int32.Parse(paramStr[5]), Int32.Parse(paramStr[6]), 1);
                    cparam.ID = i;
                    cparam.InitHistoryMinutes = this.TestingMode ? 0 : Int32.Parse(paramStr[7]);
                    cparam.ReadOnly = Int32.Parse(paramStr[8]);
                    cparam.MinTradeVolume = Int32.Parse(paramStr[9]);
                    
                    customParams.Add(cparam);
                }
            }

            if (this.customParams.Count == 0 )
            {
                BuyLimitAndWaitParam cparam = new BuyLimitAndWaitParam();
                cparam.Symbol = base.Symbol;
                cparam.TP = param["TP"];
                cparam.OpenOrderShift = param["OpenOrderShift"];
                cparam.BasicVolume = param["BasicVolume"];
                cparam.OpenMode = (OpenMode)this.param["OpenMode"];
                cparam.InitHistoryMinutes = this.param["InitHistoryMinutes"];
                cparam.StartDate = new DateTime(this.param["StartYear"], this.param["StartMonth"], 1);
                cparam.MinTradeVolume = this.param["MinTradeVolume"];
                cparam.ID = 0;

                cparam.ReadOnly = this.param[ReadOnlyParamName];
                customParams.Add(cparam);
            }
            foreach( BuyLimitAndWaitParam cparam in customParams)
                cparam.TralTP = (cparam.TP + cparam.OpenOrderShift) * 5;

            return true;
        }

        private void AddOrders(Tick<int> currTick, IEnumerable<Order> Orders)
        {
            foreach (Order currOrder in Orders)
            {
                try
                {
                    string output = string.Format("Adding init order: {0}.", currOrder.ToString());
                    Meta.Print(output);
                    Meta.Comment(output);
                    int openPrice  = currOrder.OpenPrice;
                    if (currOrder.Type == OrderType.Market)
                        openPrice = currOrder.Side == OrderSide.Buy ? currTick.Ask : currTick.Bid;
                    int result = Meta.OrderSend(currOrder.Symbol, currOrder.Type, currOrder.Side, currOrder.Volume, openPrice, 0, 0, currOrder.Comment);
                    if (result <= 0)
                    {
                        Meta.Print("Error of adding last order");
                        continue;
                    }
                    if (!Meta.OrderModify(result, currOrder.OpenPrice, currOrder.SL, currOrder.TP, currOrder.Type))
                    {
                        Meta.Print("Error of modifing last order");
                    }
                }
                catch (OpenPricetooCloseExceptions exc)
                {
                    Meta.Print(exc.ToString());
                }
                catch (SLTPtooCloseExceptions exc)
                {
                    Meta.Print(exc.ToString());
                }
            }
        }
        public override void onTick(Tick<int> currTick)
        {
            lock (sObject)
            {
                try
                {
                    base.InitComment();

                    if (!this.TestingMode)
                        spreadAnalyzer.EvaluateSpread(currTick);

                    if (currTime.Ticks / (TickHistory.tickInOneMinute) == currTick.DateTime.Ticks / (TickHistory.tickInOneMinute))
                        return;
                    currTime = currTick.DateTime;

                    foreach (BuyLimitAndWaitParam cparam in customParams)
                    {
                        if (cparam.InitHistoryMinutes > 0)
                        {
                            cparam.historySimulator = new HistorySimulator(cparam, base.TestingMode, new BuyLimitAndWaitStrategy(), this.param, cparam.Symbol, this.Meta);

                            if (TestingMode)
                                cparam.StartDate = currTick.DateTime.AddMinutes(-cparam.InitHistoryMinutes);

                            List<string> logString;
                            base.AddComment(cparam.ID, string.Format("Start simulate from {0} to {1}\n", cparam.StartDate.ToShortDateString(), currTick.DateTime.ToShortDateString()));
                            IEnumerable<Order> newOrders = cparam.historySimulator.Init(currTick.DateTime, cparam.StartDate, out logString);
                            if (cparam.historySimulator.IsSuccessful)
                            {
                                base.AddComment(cparam.ID, "Simulation was successful");
                                if (TestingMode)
                                {
                                    Tick<int> currSymbolTick = new Tick<int>();
                                    currSymbolTick.Ask = Meta.MarketInfo(cparam.Symbol, MarketInfoType.MODE_ASK);
                                    currSymbolTick.Bid = Meta.MarketInfo(cparam.Symbol, MarketInfoType.MODE_BID);
                                    currSymbolTick.DateTime = currTick.DateTime;
                                    currSymbolTick.volume = 1;
                                    AddOrders(currSymbolTick, newOrders.Where(p => p.Comment.StartsWith(cparam.IdentityComment)));
                                }

                            }
                            else
                            {
                                base.AddComment(cparam.ID, "History Simulator return error. Init state is not valid.\n");
                            }
                            cparam.InitHistoryMinutes = 0;
                        }
                    }

                    IEnumerable<Order> allBuyOrders = OrderOperation.GetBuyMarketOrders();
                    IEnumerable<Order> allSellOrders = OrderOperation.GetSellMarketOrders();
                    IEnumerable<Order> allBuyLimitOrders;
                    IEnumerable<Order> allSellLimitOrders;

                    if (!TestingMode)
                    {
                        PositionAggregator pa = new PositionAggregator();

                        foreach (BuyLimitAndWaitParam cparam in customParams)
                        {
                            Tick<int> currSymbolTick = new Tick<int>();
                            currSymbolTick.Ask = Meta.MarketInfo(cparam.Symbol, MarketInfoType.MODE_ASK);
                            currSymbolTick.Bid = Meta.MarketInfo(cparam.Symbol, MarketInfoType.MODE_BID);
                            currSymbolTick.DateTime = currTick.DateTime;
                            currSymbolTick.volume = 1;

                            IEnumerable<Order> requiredOrders = cparam.historySimulator.AddTick(currSymbolTick);
                            //IEnumerable<Order> openedOrders = allBuyOrders.Where(p => p.Comment.StartsWith(cparam.IdentityComment))
                            //    .Union(allSellOrders.Where(p => p.Comment.StartsWith(cparam.IdentityComment)));

                            pa.AddRequrementPosition(requiredOrders);
                            base.AddCommentLine(cparam.Symbol, string.Format("{0} - openPrices={1}, tpPrices={2}",
                                currSymbolTick.DateTime.ToShortTimeString(), 
                                String.Join(", ", cparam.historySimulator.LimitOrders.Select(p=>p.OpenPrice)),
                                String.Join(", ", cparam.historySimulator.MarketOrders.Select(p=>p.TP))
                                ));

                        }
                        
                        foreach (BuyLimitAndWaitParam cparam in customParams)
                            pa.CheckMinTradeVolumeSize(cparam.Symbol, cparam.MinTradeVolume);
                        pa.AddCurrentPositions(allBuyOrders);
                        pa.AddCurrentPositions(allSellOrders);

                        foreach (KeyValuePair<string, int> kv in pa.ResultDiff.Where(p => p.Value != 0))
                        {
                            Tick<int> currSymbolTick = new Tick<int>();
                            currSymbolTick.Ask = Meta.MarketInfo(kv.Key, MarketInfoType.MODE_ASK);
                            currSymbolTick.Bid = Meta.MarketInfo(kv.Key, MarketInfoType.MODE_BID);
                            currSymbolTick.DateTime = currTick.DateTime;
                            currSymbolTick.volume = 1;

                            AddComment(string.Format("Adding new order for symbol {0} with volume {1}", kv.Key, kv.Value));
                            Meta.OrderSend(kv.Key, OrderType.Market, kv.Value > 0 ? OrderSide.Buy : OrderSide.Sell, Math.Abs(kv.Value),
                                kv.Value > 0 ? currSymbolTick.Ask : currSymbolTick.Bid, 0, 0, "");
                        }

                        allBuyOrders = OrderOperation.GetBuyMarketOrders();
                        allSellOrders = OrderOperation.GetSellMarketOrders();
                        var elem = allBuyOrders.Join(allSellOrders, bO => bO.Symbol, sO => sO.Symbol, (bO, sO) => new { t1 = bO.ID, t2 = sO.ID }).FirstOrDefault();
                        if (elem != null)
                        {
                            AddComment(string.Format("Trying to close by two orders: {0} and {1}.", elem.t1, elem.t2));
                            Meta.OrderCloseBy(elem.t1, elem.t2, 0);
                            AddComment(string.Format("Successfull close by two orders: {0} and {1}.", elem.t1, elem.t2));
                        }
                        return;
                    }


                    BuyLimitAndWaitParam firstParam = customParams[0];

                    RemoveAllLimitWithoutPair(OrderOperation.GetBuyLimitOrders(), OrderOperation.GetSellLimitOrders(),
                        firstParam, currTick);

                    allBuyLimitOrders = OrderOperation.GetBuyLimitOrders();
                    allSellLimitOrders = OrderOperation.GetSellLimitOrders();

                    Order minBuyOrder = null;
                    Order maxSellOrder = null;
                    if (allBuyOrders != null)
                        minBuyOrder = allBuyOrders
                            .Where(p => p.Comment.StartsWith(firstParam.IdentityComment))
                            //.MinElement(p => p.OpenPrice);
                            .MinElement(p => p.TP);
                    if (allSellOrders != null)
                        maxSellOrder = allSellOrders
                            .Where(p => p.Comment.StartsWith(firstParam.IdentityComment))
                            //.MaxElement(p => p.OpenPrice);
                            .MaxElement(p => p.TP);

                    bool isLimitExist = allSellLimitOrders.Where(p => p.Comment.StartsWith(firstParam.IdentityComment)).Count() != 0
                                        || allBuyLimitOrders.Where(p => p.Comment.StartsWith(firstParam.IdentityComment)).Count() != 0;

                    if (this.TestingMode && minBuyOrder != null && !isLimitExist)
                        RegisterTickToHandle.RegisterBuyLimit((minBuyOrder.TP - 2 * firstParam.TP) - firstParam.OpenOrderShift);
                    if (this.TestingMode && maxSellOrder != null && !isLimitExist)
                        RegisterTickToHandle.RegisterSellLimit(firstParam.OpenOrderShift + (maxSellOrder.TP + 2 * firstParam.TP));

                    if (!isLimitExist &&
                        //(minBuyOrder != null && minBuyOrder.OpenPrice - currSymbolTick.Ask > firstParam.OpenOrderShift
                        //|| maxSellOrder != null && currSymbolTick.Bid - maxSellOrder.OpenPrice > firstParam.OpenOrderShift
                        (minBuyOrder != null && (minBuyOrder.TP - 2 * firstParam.TP) - currTick.Ask >= firstParam.OpenOrderShift
                        || maxSellOrder != null && currTick.Bid - (maxSellOrder.TP + 2 * firstParam.TP) >= firstParam.OpenOrderShift
                        || minBuyOrder == null && maxSellOrder == null)
                        )
                    {
                        string comment = firstParam.NewUniqueComment();
                        int openPrice = currTick.Ask - firstParam.TP;
                        if (firstParam.OpenMode != OpenMode.OnlySell
                            && (minBuyOrder == null || (minBuyOrder.TP - 2 * firstParam.TP) - openPrice >= firstParam.OpenOrderShift))
                        {
                            int buyOrderID = Meta.OrderSend(firstParam.Symbol, OrderType.Limit, OrderSide.Buy, firstParam.BasicVolume, openPrice, 0, openPrice + 2 * firstParam.TP, comment);
                            if (buyOrderID <= 0)
                            {
                                Meta.Print(string.Format("Error of adding buy order. Price = {0} Symbol = {1}", openPrice, firstParam.Symbol));
                                Meta.Comment(string.Format("Error of adding buy order. Price = {0} Symbol = {1}", openPrice, firstParam.Symbol));
                                return;
                            }
                            this.RegisterTickToHandle.RegisterBuyStop(openPrice + 2 * firstParam.TP);
                        }

                        openPrice = currTick.Bid + firstParam.TP;
                        if (firstParam.OpenMode != OpenMode.OnlyBuy
                            && (maxSellOrder == null || openPrice - (maxSellOrder.TP + 2 * firstParam.TP) >= firstParam.OpenOrderShift))
                        {
                            int sellOrderID = Meta.OrderSend(firstParam.Symbol, OrderType.Limit, OrderSide.Sell, firstParam.BasicVolume, openPrice, 0, openPrice - 2 * firstParam.TP, comment);
                            if (sellOrderID <= 0)
                            {
                                Meta.Print(string.Format("Error of adding sell order. Price = {0} Symbol = {1}", openPrice, firstParam.Symbol));
                                Meta.Comment(string.Format("Error of adding sell order. Price = {0} Symbol = {1}", openPrice, firstParam.Symbol));
                            }
                            this.RegisterTickToHandle.RegisterSellStop(openPrice - 2 * firstParam.TP);

                        }
                    }

                }
                catch (HistoryNotAvailableExceptions exc)
                {
                    logger.AddMessage("tick = {0}\r\n {1}", currTick.DateTime.ToLongTimeString(), exc);
                    throw;
                }
                finally
                {
                    ShowComment();
                }
            }
        }

        void RemoveAllLimitWithoutPair(IEnumerable<Order> allBuyLimitOrders, IEnumerable<Order> allSellLimitOrders,
            BuyLimitAndWaitParam cparam, Tick<int> currSymbolTick)
        {
            //var orderToRemoved = from o in allBuyLimitOrders.Union(allSellLimitOrders)
            //        group o by o.Comment into g
            //        where g.Count() == 1
            //        select g.FirstOrDefault();
            List<Order> orderToRemoved = new List<Order>();
            foreach (Order currOrder in allBuyLimitOrders)
            {

                if (currOrder.Comment.StartsWith(cparam.IdentityComment) && Math.Abs(currOrder.OpenPrice - currSymbolTick.Ask) >= 2 * cparam.TP)
                    orderToRemoved.Add(currOrder); 
            }
            foreach (Order currOrder in allSellLimitOrders)
            {

                if (currOrder.Comment.StartsWith(cparam.IdentityComment) && Math.Abs(currOrder.OpenPrice - currSymbolTick.Bid) >= 2 * cparam.TP)
                    orderToRemoved.Add(currOrder);
            }
            foreach (Order currOrder in orderToRemoved)
                this.Meta.OrderDelete(currOrder.ID);
        }

        void ModifyMarketOrders(OrderSide side, int tp, string comment, string symbol)
        {
            tp = MathPrice.RoundToDown(tp);

            IEnumerable<Order> marketOrders = OrderOperation.GetMarketOrders(side);
            if (marketOrders.Count() != 0)
            {
                try
                {
                    foreach (Order currOrder in marketOrders)
                    {
                        if ((currOrder.Comment == comment ||TralTP<=0) && symbol.ToLowerInvariant() == currOrder.Symbol.ToLowerInvariant())
                            Meta.OrderModify(currOrder.ID, currOrder.OpenPrice, currOrder.SL, tp, currOrder.Type);
                    }
                }
                catch (Exception exc)
                {
                    logger.AddMessage(exc.ToString());
                }
            }
        }

        //private void ShowComment(Tick<int> currTick)
        //{
        //    if (base.TestingMode)
        //        return;

        //    StringBuilder strBuilder = new StringBuilder();
        //    strBuilder.AppendLine(param.ToString());
        //    strBuilder.Append("\n Spread = " + spreadAnalyzer.AverageSpread);

        //    Meta.Comment(strBuilder.ToString());

        //}
        public override void onEnd()
        {
        }
    }

}

public static class EnumerableExtensions
{
    public static T MaxElement<T, TCompare>(this IEnumerable<T> collection, Func<T, TCompare> func) where TCompare : IComparable<TCompare>
    {
        T maxItem = default(T);
        TCompare maxValue = default(TCompare);
        foreach (var item in collection)
        {
            TCompare temp = func(item);
            if (maxItem == null || temp.CompareTo(maxValue) > 0)
            {
                maxValue = temp;
                maxItem = item;
            }
        }
        return maxItem;
    }
    public static T MinElement<T, TCompare>(this IEnumerable<T> collection, Func<T, TCompare> func) where TCompare : IComparable<TCompare>
    {
        T minItem = default(T);
        TCompare minValue = default(TCompare);
        foreach (var item in collection)
        {
            TCompare temp = func(item);
            if (minItem == null || temp.CompareTo(minValue) < 0)
            {
                minValue = temp;
                minItem = item;
            }
        }
        return minItem;
    }
    public static bool IsEmpty<T>(this IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            return false;
        }
        return true;
    }
}

