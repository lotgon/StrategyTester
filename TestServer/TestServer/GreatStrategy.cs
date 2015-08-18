using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EngineTest;
using Log4Smart;

namespace TestServer
{
    public class Parameters
    {
        public int TP = 40;
        public int SL = 40;
        public int BasicVolume = 10;
        public double VolCoef = 1;

        public override string ToString()
        {
            return string.Format("TP={0}_SL={1}_Coef={2}", TP, SL, VolCoef);
        }

        static public List<Parameters> AllParameters
        {
            get
            {
                List<Parameters> paramList = new List<Parameters>();

                //Parameters p = new Parameters();
                //p.TP = 10;
                //p.SL = 30;
                //p.BasicVolume = 10;
                //p.VolCoef = 4;
                //paramList.Add(p);
                for (int i = 10; i <= 200; i += 10)
                    for (int j = i; j <= 200; j += 10)
                        for (double k = 1; k < 3; k += 0.2)
                        {
                            Parameters p = new Parameters();
                            p.TP = i;
                            p.SL = j;
                            p.BasicVolume = 10;
                            p.VolCoef = k;
                            paramList.Add(p);
                        }

                return paramList;
            }
        }
    }
    class GreatStrategy : EngineTest.Strategy
    {
        Parameters param;
        IStrategyLogger logger;
        public GreatStrategy(Parameters param, IStrategyLogger logger)
        {
            this.param = param;
            this.logger = logger;
        }

        int lastOrderID = 0;

        public override void onTick(Tick tick)
        {
            if (lastOrderID <= 0)
            {
                lastOrderID = OrderOperation.AddOrder(new MarketOrder(OrderSide.Buy, param.BasicVolume, tick.Ask - param.SL, tick.Ask + param.TP));
                return;
            }
            if (!History.Contains(lastOrderID))
                return;

            MarketOrder mOrder = History.GetMarketOrderByID(lastOrderID);
            if (mOrder.Profit > 0)
            {
                lastOrderID = OrderOperation.AddOrder(LimitOrder.SmartConstructor(tick.Ask, tick.Bid, Order.RevertSide(mOrder.Side), param.BasicVolume, 10, param.SL, param.TP));
            }
            else
            {
                lastOrderID = OrderOperation.AddOrder(LimitOrder.SmartConstructor(tick.Ask, tick.Bid, mOrder.Side, ((int)(((double)mOrder.Volume * param.VolCoef))), 10, param.SL, param.TP));
            }
            logger.AddMessage(string.Format("Equity = {0}", Account.GetEquity()));
        }
        public override void onEnd()
        {
            logger.AddMessage(string.Format("Balance = {0}", Account.GetEquity()));
        }
    }
}
