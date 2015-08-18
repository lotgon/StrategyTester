using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    //we can use common object for strategy. Gil you can add add arrays for interfaces and use this object instead of Advisor.
    public abstract class Strategy
    {
        protected IOrderOperation _OrderOperation;
        protected IAccount _Account;
        protected IHistory _History;
        protected IMeta _Meta;
        protected int _maxTicks = 0;
        protected int _currentTicks = 0;
        protected string Symbol { get; set; }

        internal bool Start(int ticks, IOrderOperation OrderOperation, IAccount Account, IHistory History, IMeta Meta)
        {
            _maxTicks = ticks;
            this._OrderOperation = OrderOperation;
            this._Account = Account;
            this._History = History;
            this._Meta = Meta;
            return onStart(ticks);
        }
        internal void Tick(Tick<int> tick)
        {
            onTick(tick);
            _currentTicks++;
        }
        internal void End()
        {
            onEnd();
        }

        public virtual bool onStart(int ticks)
        {
            return true;
        }
        public abstract void onTick(Tick<int> tick);
        public virtual void onEnd()
        {
        }



        protected IOrderOperation OrderOperation
        {
            get
            {
                return _OrderOperation;
            }
        }
        protected IAccount Account
        {
            get
            {
                return _Account;
            }
        }
        protected IHistory History
        {
            get
            {
                return _History;
            }
        }
        protected IMeta Meta
        {
            get
            {
                return _Meta;
            }
        }
        
    

    }
}
