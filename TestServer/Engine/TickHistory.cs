using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace EngineTest
{
    public class TickHistory
    {
        public static readonly  long tickInOneMinute = new TimeSpan(0, 1, 0).Ticks;

        List<GroupTick> historyTicks = new List<GroupTick>();
        Dictionary<long, int> timeToIndexDictionary = new Dictionary<long, int>();

        public GroupTick this[DateTime dateTime]
        {
            get
            {
                return historyTicks[timeToIndexDictionary[dateTime.Ticks / tickInOneMinute]];
            }
        }

        public GroupTick GetRelativeGroupTick(DateTime dateTime, int stepBehind)
        {
            if (stepBehind <= 0)
                throw new ApplicationException("Incorrect operation");
            int index = timeToIndexDictionary[dateTime.Ticks / tickInOneMinute];
            if (index - stepBehind < 0)
                throw new HistoryNotAvailableExceptions();
            return historyTicks[index - stepBehind];
        }

        //internal TickHistory Select( fromDate, DateTime toDate)
        //{
        //    TickHistory newTickHistory = new TickHistory();

        //        var a = from currGT in historyTicks
        //                where currGT.Key > fromDate.Ticks / tickInOneMinute && currGT.Key < toDate.Ticks / tickInOneMinute
        //                select currGT;
        //        foreach (KeyValuePair<long, GroupTick> curr in a)
        //            newTickHistory.historyTicks.Add(curr.Key, curr.Value);
        //    return newTickHistory;

        //}

        internal void AddTick(Tick<int> tick)
        {
            GroupTick gt = null;
            int index = 0;

            if (!timeToIndexDictionary.TryGetValue(tick.DateTime.Ticks / tickInOneMinute, out index))
            {
                gt = new GroupTick();
                gt.DateTime = tick.DateTime;
                gt.CloseAsk = tick.Ask;
                gt.CloseBid = tick.Bid;
                gt.MaxAsk = tick.Ask;
                gt.MaxBid = tick.Bid;
                gt.MinAsk = tick.Ask;
                gt.MinBid = tick.Bid;
                gt.OpenAsk = tick.Ask;
                gt.OpenBid = tick.Bid;

                historyTicks.Add(gt);
                timeToIndexDictionary[tick.DateTime.Ticks / tickInOneMinute] = historyTicks.Count - 1;
                return;
            }
            gt = historyTicks[index];

            gt.CloseAsk = tick.Ask;
            gt.CloseBid = tick.Bid;
            if (gt.MaxAsk < tick.Ask)
                gt.MaxAsk = tick.Ask;
            if (gt.MaxBid < tick.Bid)
                gt.MaxBid = tick.Bid;
            if (gt.MinAsk > tick.Ask)
                gt.MinAsk = tick.Ask;
            if (gt.MinBid > tick.Bid)
                gt.MinBid = tick.Bid;
        }
        internal void AddGroupTick(GroupTick groupTick)
        {
            int index = 0;
            if (!timeToIndexDictionary.TryGetValue(groupTick.DateTime.Ticks / tickInOneMinute, out index))
            {
                historyTicks.Add(groupTick);
                timeToIndexDictionary[groupTick.DateTime.Ticks / tickInOneMinute] = historyTicks.Count - 1;
                return;
            }
            historyTicks[index] = groupTick;
        }

    }
}
