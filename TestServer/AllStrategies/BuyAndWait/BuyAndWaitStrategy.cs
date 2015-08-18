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

namespace BuyAndWait
{
    [RegisterAdvisor(Name = "BuyAndWaitStrategy")]
    public class BuyAndWaitStrategy : FxAdvisorCore.SimpleAdvisor
    {
        class BuyLimitAndWaitParam : BasicParam
        {
            public int TP;
            public int OpenOrderShift;

            public int TralTP;
        };
        List<BuyLimitAndWaitParam> customParams = new List<BuyLimitAndWaitParam>();

        DateTime currTime = DateTime.MinValue;
        SpreadAnalyzer spreadAnalyzer;
        int TralTP = 0;

        public BuyAndWaitStrategy() 
        {
        }

        public override bool onStart(int ticks)
        {
            spreadAnalyzer = new SpreadAnalyzer(100);
            for (int i = 0; i < 1000; i++)
            {
                string paramName = string.Format("P{0}String", i);
                if (this.param.ContainsString(paramName))
                {
                    string[] paramStr = this.param.GetStringParameter(paramName).Split(new char[] { ',' });
                    BuyLimitAndWaitParam cparam = new BuyLimitAndWaitParam();
                    cparam.Symbol = paramStr[0];
                    cparam.TP = Int32.Parse(paramStr[1]);
                    cparam.OpenOrderShift = Int32.Parse(paramStr[2]);
                    cparam.BasicVolume = Int32.Parse(paramStr[3]);
                    cparam.ID = i;
                    cparam.InitHistoryMinutes = this.param["InitHistoryMinutes"];
                    customParams.Add(cparam);
                }
            }
            if (this.customParams.Count == 0)
            {
                BuyLimitAndWaitParam cparam = new BuyLimitAndWaitParam();
                cparam.ID = 0;
                cparam.TP = param["TP"];
                cparam.OpenOrderShift = param["OpenOrderShift"];
                cparam.Symbol = base.Symbol;
                cparam.BasicVolume = param["BasicVolume"];
                cparam.InitHistoryMinutes = this.param["InitHistoryMinutes"];
                customParams.Add(cparam);
            }
            foreach (BuyLimitAndWaitParam cparam in customParams)
                TralTP = cparam.OpenOrderShift * cparam.TralTP;

            return true;
        }
        public override void onTick(Tick<int> currTick)
        {
            try
            {
                spreadAnalyzer.EvaluateSpread(currTick);
                ShowComment(currTick);

                if (currTime.Ticks / TickHistory.tickInOneMinute == currTick.DateTime.Ticks / TickHistory.tickInOneMinute)
                    return;
                currTime = currTick.DateTime;

                //foreach (BuyLimitAndWaitParam cparam in customParams)
                //{
                //    if (cparam.InitHistoryMinutes > 0)
                //    {
                //        if (this.OrderOperation.GetLimitOrders().Where(p => p.Comment.StartsWith(cparam.IdentityComment)).Count() > 0
                //            || this.OrderOperation.GetMarketOrders().Where(p => p.Comment.StartsWith(cparam.IdentityComment)).Count() > 0
                //            )
                //        {
                //            cparam.InitHistoryMinutes = 0;
                //            continue;
                //        }
                //        List<string> logString;
                //        IEnumerable<Order> newOrders = new HistorySimulator(cparam, base.TestingMode).Run(new BuyAndWaitStrategy(),
                //            currTick.DateTime, out logString, this.param, cparam.Symbol);
                //        AddOrders(currTick, newOrders.Where(p => p.Comment.StartsWith(cparam.IdentityComment)));
                //        cparam.InitHistoryMinutes = 0;
                //    }
                //}

                //EURUSD, GBPUSD, GBPCHF, EURCHF, CADJPY, AUDJPY, AUDNZD
                IEnumerable<Order> allBuyOrders = OrderOperation.GetBuyMarketOrders();
                IEnumerable<Order> allSellOrders = OrderOperation.GetSellMarketOrders();

                foreach (BuyLimitAndWaitParam cparam in customParams)
                {
                    Tick<int> currSymbolTick = new Tick<int>();
                    currSymbolTick.Ask = Meta.MarketInfo(cparam.Symbol, MarketInfoType.MODE_ASK);
                    currSymbolTick.Bid = Meta.MarketInfo(cparam.Symbol, MarketInfoType.MODE_BID);
                    currSymbolTick.DateTime = currTick.DateTime;
                    currSymbolTick.volume = 1;

                    Order minBuyOrder=null; 
                    Order maxSellOrder=null; 
                    if( allBuyOrders != null )
                        minBuyOrder = allBuyOrders.Where(p => p.Comment.StartsWith(cparam.IdentityComment)).MinElement(p => p.OpenPrice);
                    if( allSellOrders != null )
                        maxSellOrder = allSellOrders.Where(p => p.Comment.StartsWith(cparam.IdentityComment)).MaxElement(p => p.OpenPrice);

                    if (minBuyOrder != null && minBuyOrder.OpenPrice - currSymbolTick.Ask > cparam.OpenOrderShift
                        || maxSellOrder != null && currSymbolTick.Bid - maxSellOrder.OpenPrice > cparam.OpenOrderShift
                        || minBuyOrder == null && maxSellOrder == null)
                    {
                        string comment = cparam.NewUniqueComment();
                        int buyOrderID = Meta.OrderSend(cparam.Symbol, OrderType.Market, OrderSide.Buy, cparam.BasicVolume, currSymbolTick.Ask, 0, 0, comment);
                        if (buyOrderID <= 0)
                            continue;
                        int sellOrderID;
                        do
                        {
                            sellOrderID = Meta.OrderSend(cparam.Symbol, OrderType.Market, OrderSide.Sell, cparam.BasicVolume, currSymbolTick.Bid, 0, 0, comment);
                            if (sellOrderID <= 0)
                            {
                                Meta.Print("Error of adding sell order. Trying to send again.");
                                Meta.Comment("Error of adding sell order. Trying to send again.");
                            }

                        } while (sellOrderID <= 0);

                        ModifyMarketOrders(OrderSide.Buy, currSymbolTick.Ask + cparam.TP, comment, cparam.Symbol);
                        ModifyMarketOrders(OrderSide.Sell, currSymbolTick.Bid - cparam.TP, comment, cparam.Symbol);
                    }

                    if (TralTP > 0)
                    {
                        //trailing tp
                        foreach (Order currOrder in allBuyOrders)
                        {
                            if (currOrder.TP - currSymbolTick.Bid > TralTP)
                                Meta.OrderModify(currOrder.ID, currOrder.OpenPrice, currOrder.SL, currSymbolTick.Bid + TralTP, currOrder.Type);
                        }
                        foreach (Order currOrder in allSellOrders)
                        {
                            if (currSymbolTick.Ask - currOrder.TP > TralTP)
                                Meta.OrderModify(currOrder.ID, currOrder.OpenPrice, currOrder.SL, currSymbolTick.Ask - TralTP, currOrder.Type);
                        }
                    }

                }
            }
            catch (HistoryNotAvailableExceptions exc)
            {
                return;
            }
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

        private void ShowComment(Tick<int> currTick)
        {
            if (base.TestingMode)
                return;

            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine(param.ToString());
            strBuilder.Append("\n Spread = " + spreadAnalyzer.AverageSpread);

            Meta.Comment(strBuilder.ToString());

        }
        public override void onEnd()
        {
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
                    int openPrice = currOrder.OpenPrice;
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

