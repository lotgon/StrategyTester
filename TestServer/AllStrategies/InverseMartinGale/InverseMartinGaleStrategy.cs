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

namespace InverseMartinGale
{
    [RegisterAdvisor(Name = "InverseMartinGaleStrategy")]
    public class InverseMartinGaleStrategy : FxAdvisorCore.SimpleAdvisor
    {
        DateTime currTime = DateTime.MinValue;

        SpreadAnalyzer spreadAnalyzer;
        StrategyTime strategyTime;
        int martinStep = 0;

        bool virtualBuySide = true;
        int virtualLastMarketOpenPrice = 0;

        public InverseMartinGaleStrategy() 
        {
        }

        public override bool onStart(int ticks)
        {
            spreadAnalyzer = new SpreadAnalyzer(100);
            strategyTime = new StrategyTime(param["ProhibitedStartHour"], param["ProhibitedStartMinute"], param["ProhibitedDuration"], (param.Keys.Contains("GMT") ? param["GMT"] : 0), 31);

            return true;
        }
        public override void onTick(Tick<int> currTick)
        {
            try
            {
                spreadAnalyzer.EvaluateSpread(currTick);
                ShowComment(currTick);

                if (!strategyTime.IsSystemON(currTick))
                {
                    martinStep = 0;
                    OrderOperation.CloseAllOrders();
                    return;
                }

                if (martinStep < 99999)
                {
                    if (virtualLastMarketOpenPrice == 0)
                    {
                        virtualLastMarketOpenPrice = currTick.Ask;
                        Meta.Print("New wave was started. virtualLastMarketOpenPrice = " + virtualLastMarketOpenPrice.ToString() + ", virtualBuySide = " + virtualBuySide.ToString());
                        return;
                    }

                    if (virtualBuySide)//BUY
                    {
                        if (currTick.Bid >= virtualLastMarketOpenPrice + param["TP"])
                        {
                            Meta.Print("TP was hitted. martinStep was " + martinStep.ToString());
                            virtualLastMarketOpenPrice = 0;
                            martinStep = 0;
                            return;
                        }
                        if (currTick.Bid <= virtualLastMarketOpenPrice - param["BuySellDistance"])
                        {
                            virtualLastMarketOpenPrice = currTick.Bid;
                            martinStep++;
                            virtualBuySide = !virtualBuySide;
                            Meta.Print(string.Format("New order should be open. VirtualBuySide = {2}, VirtualLastMarketOpenPrice = {0}, marginStep = {1}."
                                , virtualLastMarketOpenPrice, martinStep, virtualBuySide));

                            if (martinStep >= param["StartStep"])
                            {
                                Meta.OrderSend(base.Symbol, OrderType.Market, OrderSide.Buy, GetVolumeForStep(martinStep), currTick.Ask, 0, 0, "");
                                ModifyMarketOrders(OrderSide.Buy, currTick.Ask + param["TP"], 0);
                                ModifyMarketOrders(OrderSide.Sell, 0, currTick.Ask + param["TP"]);
                            }
                        }
                    }
                    else //SELL
                    {
                        if (currTick.Ask <= virtualLastMarketOpenPrice - param["TP"])
                        {
                            Meta.Print("TP was hitted. martinStep was " + martinStep.ToString());
                            virtualLastMarketOpenPrice = 0;
                            martinStep = 0;
                            return;
                        }
                        if (currTick.Ask >= virtualLastMarketOpenPrice + param["BuySellDistance"])
                        {
                            virtualLastMarketOpenPrice = currTick.Ask;
                            martinStep++;
                            virtualBuySide = !virtualBuySide;
                            Meta.Print(string.Format("New order should be open. VirtualBuySide = {2}, VirtualLastMarketOpenPrice = {0}, marginStep = {1}."
                                , virtualLastMarketOpenPrice, martinStep, virtualBuySide));

                            if (martinStep >= param["StartStep"])
                            {
                                Meta.OrderSend(base.Symbol, OrderType.Market, OrderSide.Sell, GetVolumeForStep(martinStep), currTick.Bid, 0, 0, "");
                                ModifyMarketOrders(OrderSide.Sell, currTick.Bid - param["TP"], 0);
                                ModifyMarketOrders(OrderSide.Buy, 0, currTick.Bid - param["TP"]);
                            }
                        }
                    }

                    return;
                }
            }
            catch (HistoryNotAvailableExceptions exc)
            {
                return;
            }
        }

        public int GetVolumeForStep(int step)
        {
            return (int)(Math.Pow((double)(param["CoefVolume"])/10, step-param["StartStep"]-1) * param["BasicVolume"]);
        }
        public int GetPriceForStep(int step, int previousPrice, OrderSide side)
        {
            if (side == OrderSide.Buy)
                return previousPrice - param["OpenOrderShift"];
            else
                return previousPrice + param["OpenOrderShift"];
        }

        void ModifyMarketOrders(OrderSide side, int tp, int sl)
        {
            tp = MathPrice.RoundToDown(tp);

            IEnumerable<Order> marketOrders = OrderOperation.GetMarketOrders(side);
            if (marketOrders.Count() != 0)
            {
                try
                {
                    foreach (Order currOrder in marketOrders)
                        Meta.OrderModify(currOrder.ID, currOrder.OpenPrice, sl==0?currOrder.SL:sl, tp==0?currOrder.TP:tp, currOrder.Type);
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
            this.logger.AddMessage("The end");
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

