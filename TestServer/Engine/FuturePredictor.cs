using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Mathematics.Containers;

namespace EngineTest
{
    public class FuturePredictor
    {
        UnilateralTickCollection<int> BidsIndexCollection;
        UnilateralTickCollection<int> AsksIndexCollection;
        internal OrdersExecutor Orders;
        List<GroupTick> Ticks;

        public FuturePredictor(List<GroupTick> ticks)
        {
            this.Ticks = ticks;
            List<KeyValuePair<int, double>> data = new List<KeyValuePair<int, double>>();
            int count = ticks.Count;
            // prepare bids
            for (int index = 0; index < count; ++index)
            {
                data.Add(new KeyValuePair<int, double>(index, ticks[index].OpenBid));
            }
            BidsIndexCollection = new UnilateralTickCollection<int>(data);
            // prepare asks
            data.Clear();
            for (int index = 0; index < count; ++index)
            {
                data.Add(new KeyValuePair<int, double>(index, ticks[index].OpenAsk));
            }
            AsksIndexCollection = new UnilateralTickCollection<int>(data);
        }
        public int CalculateNextInterruption(int step)
        {
            //return step + 1;

            int min = this.Ticks.Count - 1;
            if (step == min)
                return step + 1;

            bool isEmpty = true; 

            if (Orders.BuyLimits.Count > 0)
            {
                isEmpty = false;
                KeyValuePair<int, double>? entry = AsksIndexCollection.FirstEventWhenLessOrEqual(step+1, Orders.BuyLimits.Keys[0]);
                if (entry != null)
                {
                    min = Math.Min(min, entry.Value.Key);
                }
            }
            if (Orders.BuyStops.Count > 0)
            {
                isEmpty = false;
                KeyValuePair<int, double>? entry = AsksIndexCollection.FirstEventWhenMoreOrEqual(step + 1, Orders.BuyStops.Keys[0]);
                if (entry != null)
                {
                    min = Math.Min(min, entry.Value.Key);
                }
            }
            if (Orders.SellLimits.Count > 0)
            {
                isEmpty = false;
                KeyValuePair<int, double>? entry = BidsIndexCollection.FirstEventWhenMoreOrEqual(step + 1, Orders.SellLimits.Keys[0]);
                if (entry != null)
                {
                    min = Math.Min(min, entry.Value.Key);
                }
            }
            if (Orders.SellStops.Count > 0)
            {
                isEmpty = false;
                KeyValuePair<int, double>? entry = BidsIndexCollection.FirstEventWhenLessOrEqual(step + 1, Orders.SellStops.Keys[0]);
                if (entry != null)
                {
                    min = Math.Min(min, entry.Value.Key);
                }
            }
            if (isEmpty)
                return step + 1;

            return min;
        }

        public UnilateralTickCollection<int> Bids
        {
            get
            {
                return BidsIndexCollection;
            }
        }
        public UnilateralTickCollection<int> Asks
        {
            get
            {
                return AsksIndexCollection;
            }
        }
    }
}
