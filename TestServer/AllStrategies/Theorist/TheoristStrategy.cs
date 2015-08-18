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

namespace Theorist
{
    [RegisterAdvisor(Name = "TheoristStrategy")]
    public class TheoristStrategy : FxAdvisorCore.SimpleAdvisor
    {
        DateTime currTime = DateTime.MinValue;

        SpreadAnalyzer spreadAnalyzer;

        public TheoristStrategy() 
        {
        }

        public override bool onStart(int ticks)
        {
            spreadAnalyzer = new SpreadAnalyzer(100);
            return true;
        }
        public override void onTick(Tick<int> currTick)
        {
            spreadAnalyzer.EvaluateSpread(currTick);
			
            ShowComment(currTick);

            if (currTime.Ticks / TickHistory.tickInOneMinute == currTick.DateTime.Ticks / TickHistory.tickInOneMinute)
                return;
            currTime = currTick.DateTime;

            int orderID = Meta.OrderSend(base.Symbol, OrderType.Market, OrderSide.Buy, 100, currTick.Ask, 0, 0, "");
            Meta.OrderClose(orderID, 100, currTick.Bid, 0);

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

