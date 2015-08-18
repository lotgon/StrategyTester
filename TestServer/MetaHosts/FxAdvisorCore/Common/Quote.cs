using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace FxAdvisorCore.Common
{
    public class Quote : IQuoteValueProvider
    {
        private Tick<decimal> lastTick;

        public Quote(Symbol s, string marketId)
        {
            this.Symbol = s;
            this.Market = marketId;
        }

        public string Market { get; private set; }

        public Symbol Symbol { get; private set; }

        public Tick<decimal> LastTick
        {
            get { return lastTick; }
            set
            {
                this.lastTick = value;
                OnNewTick();
            }
        }

        public event EventHandler NewTick;

        protected void OnNewTick()
        {
            if (this.NewTick != null)
                NewTick(this, EventArgs.Empty);
        }

        #region IQuoteValueProvider Members

        public decimal Ask
        {
            get { return LastTick.Ask; }
        }

        public decimal Bid
        {
            get { return LastTick.Bid; }
        }

        #endregion
    }
}
