using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace FxAdvisorCore.Common
{
    class QuoteUpdater
    {
        private RemoteAdvisor rmAdvisor;
        private List<Quote> list = new List<Quote>();

        public QuoteUpdater(RemoteAdvisor rmAdvisor)
        {
            this.rmAdvisor = rmAdvisor;
        }

        public void AddQuote(Quote q)
        {
            list.Add(q);
        }

        public void UpdateAll()
        {
            foreach (Quote q in list)
            {
                decimal ask = rmAdvisor.MarketInfo(q.Symbol.Name, MarketInfoType.MODE_ASK);
                decimal bid = rmAdvisor.MarketInfo(q.Symbol.Name, MarketInfoType.MODE_BID);
                if (ask != q.LastTick.Ask || bid != q.LastTick.Bid)
                {
                    Tick<decimal> newTick = new Tick<decimal>();
                    newTick.Ask = ask;
                    newTick.Bid = bid;
                    newTick.DateTime = DateTime.Now;
                    q.LastTick = new Tick<decimal>();
                }
            }
        }
    }
}
