using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using StrategyCommon;
using Common;
using Mathematics.Containers;

namespace BuyLimitAndWait
{
    public class BuyLimitAndWaitAnalyzer
    {
        int[] StartegyStart;
        int[] BuyOrder;
        int[] SellOrder;
        readonly int TP = 0;
        readonly int Shift = 0;
        readonly List<GroupTick> Ticks;
		UnilateralTickCollection<int> BidsIndexCollection;
		UnilateralTickCollection<int> AsksIndexCollection;


        public BuyLimitAndWaitAnalyzer(List<GroupTick> ticks, int tp, int shift)
        {
            this.Ticks = ticks;
            StartegyStart = new int[Ticks.Count];
            StartegyStart[0] = 1;
            //Array.ForEach(StartegyStart, p => p = 1);

            BuyOrder = new int[Ticks.Count];
            SellOrder = new int[Ticks.Count];

            this.TP = tp;
            this.Shift = shift;
			MakeIndex(ticks);
        }

        public void Calculate()
        {
            for (int Current = 0; Current < Ticks.Count-1; Current++)
            {
                if (StartegyStart[Current] > 0)
                {
                    int UpLevel = GetLevelHitInAbsoluteTime(Current, TP);
                    int DownLevel = GetLevelHitInAbsoluteTime(Current, -TP);
                    if (UpLevel < DownLevel)
                        SellOrder[UpLevel] += StartegyStart[Current];
                    else
                        BuyOrder[DownLevel] += StartegyStart[Current];
                }

                if (BuyOrder[Current] > 0)
                {
                    int HitTp = GetLevelHitInAbsoluteTime(Current, 2 * TP);
                    int HitShift = GetLevelHitInAbsoluteTime(Current, -Shift);
                    if (HitShift < HitTp)
                        StartegyStart[HitShift] += BuyOrder[Current];
                }
                if (SellOrder[Current] > 0)
                {
                    int HitTp = GetLevelHitInAbsoluteTime(Current, -2 * TP);
                    int HitShift = GetLevelHitInAbsoluteTime(Current, Shift);
                    if (HitShift < HitTp)
                        StartegyStart[HitShift] += SellOrder[Current];
                }
            }
            SellOrder[Ticks.Count - 1] = 0;
            BuyOrder[Ticks.Count - 1] = 0;
        }
        private void MakeIndex(List<GroupTick> ticks)
		{
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
        //must return not more than element counts
        public int GetLevelHitInAbsoluteTime(int startTime, int distance)
        {
            if (distance > 0)
            {
                KeyValuePair<int, double>? entry = BidsIndexCollection.FirstEventWhenMore(startTime, Ticks[startTime].OpenAsk+ distance);
                if (entry == null)
                    return Ticks.Count - 1;
                return entry.Value.Key;
            }
            else
            {
                KeyValuePair<int, double>? entry = AsksIndexCollection.FirstEventWhenLess(startTime, Ticks[startTime].OpenBid+ distance);
                if (entry == null)
                    return Ticks.Count - 1;
                return entry.Value.Key;
            }
        }
    }
}
