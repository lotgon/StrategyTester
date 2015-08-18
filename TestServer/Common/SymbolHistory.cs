using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class SymbolHistory
    {
        private object _sync = new object();
        private Queue<Tick<decimal>> history = new Queue<Tick<decimal>>();
        private int limit;
        private decimal lastBid=0, lastAsk=0;

        public SymbolHistory(Symbol symbol, int limit)
        {
            this.Symbol = symbol;
            this.limit = limit;
            IsFull = false;
            //Symbol.Changed += new EventHandler(Symbol_Changed);
        }

        public object _Sync { get { return _sync; } }
        public Symbol Symbol { get; private set; }
        public bool IsFull { get; private set; }

        protected void AddNewTick(Tick<decimal> tick)
        {
            lock (_sync)
            {
                history.Enqueue(tick);

                while (tick.DateTime - history.Peek().DateTime > TimeSpan.FromSeconds(limit))
                {
                    IsFull = true;
                    history.Dequeue();
                }
            }
        }
        public bool Add(decimal ask, decimal bid)
        {
            if (lastBid == bid && lastAsk == ask)
                return false;
            
            Tick<decimal> newTick = new Tick<decimal>();
            newTick.DateTime = DateTime.Now;
            newTick.Ask = ask;
            newTick.Bid = bid;
            newTick.volume = 0;

            lock (_sync)
            {
                AddNewTick(newTick);
            }
            return true;
        }

        public IEnumerable<Tick<decimal>> Ticks
        {
            get { return history; }
        }

        //private void Symbol_Changed(object sender, EventArgs e)
        //{
        //    lock (_sync)
        //    {
        //        Update(new QuoteStamp(Symbol));
        //    }
        //}
    }
}
