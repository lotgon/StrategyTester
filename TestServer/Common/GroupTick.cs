using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class GroupTick : IComparable
    {
        public DateTime DateTime;
        public int OpenBid;
        public int OpenAsk;
        public int CloseAsk;
        public int CloseBid;
        public int MaxBid;
        public int MaxAsk;
        public int MinBid;
        public int MinAsk;
        public int volume;

        public GroupTick()
        {
        }

        public GroupTick(Tick<int> tick)
        {
            this.DateTime = tick.DateTime;
            this.OpenBid = tick.Bid;
            this.CloseBid = tick.Bid;
            this.MaxBid = tick.Bid;
            this.MinBid = tick.Bid;

            this.OpenAsk = tick.Ask;
            this.CloseAsk = tick.Ask;
            this.MaxAsk = tick.Ask;
            this.MinAsk = tick.Ask;

            this.volume = 1;

        }

        internal Tick<int> Tick
        {
            get
            {
                Tick<int> tick = new Tick<int>();
                tick.DateTime = this.DateTime;
                tick.Bid = this.OpenBid;
                tick.Ask = this.OpenAsk;
                tick.volume = this.volume;
                return tick;
            }
        }

        public int CompareTo(object obj)
        {
            GroupTick gt = (GroupTick)obj;
            return this.DateTime.CompareTo(gt.DateTime);
                
        }
    }
}
