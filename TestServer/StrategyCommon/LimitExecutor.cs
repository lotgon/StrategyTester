using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StrategyCommon
{
    public class LimitExecutor
    {
        internal class IntReverseComparer : Comparer<int>
        {
            public override int Compare(int x, int y)
            {
                return y.CompareTo(x);
            }
        }
        SortedList<int, int> BuyLimits = new SortedList<int, int>(new IntReverseComparer());
        SortedList<int, int> SellLimits = new SortedList<int, int>();

        public LimitExecutor()
        {
        }

        public void AddBuyLimit(int Price)
        {
            BuyLimits.Add(Price, 0);
        }
        public void AddSellLimit(int Price)
        {
            SellLimits.Add(Price, 0);
        }

        public int ProccessAsk(int Ask)
        {
            int i;
            for (i = 0; i < BuyLimits.Count && Ask <= BuyLimits.Keys[i]; i++) ;
            for (int j = 0; j < i; j++)
                BuyLimits.RemoveAt(0);
            return i;
        }
        public int ProccessBid(int Bid)
        {
            int i;
            for (i = 0; i < SellLimits.Count && Bid >= SellLimits.Keys[i]; i++) ;
            for (int j = 0; j < i; j++)
                SellLimits.RemoveAt(0);
            return i;
        }
    }
}
