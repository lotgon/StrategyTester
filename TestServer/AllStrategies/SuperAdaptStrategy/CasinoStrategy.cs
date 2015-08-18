using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Log4Smart;
using EngineTest;
using Common;

namespace SuperAdaptStrategy
{
    public class CasinoParameters
    {
        public int TP = 10;
        public int SL = 49;
        public int BasicVolume = 10;
        public double VolCoef = 1.6;

        public override string ToString()
        {
            return string.Format("TP={0}_SL={1}_VolCoef={2}", TP, SL, VolCoef);
        }

        static public List<CasinoParameters> AllParameters
        {
            get
            {
                List<CasinoParameters> paramList = new List<CasinoParameters>();
                for (int i = 10; i <= 200; i += 10)
                    for (int j = i; j <= 200; j += 10)
                        for( double volCoef = 1.6; volCoef<=2.6; volCoef+=0.2)
                    {
                        CasinoParameters p = new CasinoParameters();
                        p.TP = i;
                        p.SL = j;
                        p.BasicVolume = 10;
                        p.VolCoef = volCoef;
                        paramList.Add(p);
                    }

            return paramList;
            }
        }
    }
    class CasinoStrategy : Strategy
    {
        CasinoParameters param;
        IStrategyLogger logger;
        public CasinoStrategy(CasinoParameters param, IStrategyLogger logger)
        {
            this.param = param;
            this.logger = logger;
        }

        int lastOrderID = 0;

        public override void onTick(Tick<int> tick)
        {

            if (lastOrderID <= 0)
            {
                lastOrderID = OrderOperation.AddOrder(Order.NewMarketOrder(tick.Ask, tick.Bid, OrderSide.Buy, param.BasicVolume, param.SL, param.TP)).ID;
                return;
            }
            if (!History.Contains(lastOrderID))
                return;

            Order mOrder = History.GetOrderByID(lastOrderID);
            if (mOrder.Profit > 0)
            {
                lastOrderID = OrderOperation.AddOrder(Order.NewMarketOrder(tick.Ask, tick.Bid, mOrder.RevertSide, param.BasicVolume, param.SL, param.TP)).ID;
            }
            else
            {
                lastOrderID = OrderOperation.AddOrder(Order.NewMarketOrder(tick.Ask, tick.Bid, mOrder.Side, ((int)(((double)mOrder.Volume * param.VolCoef))), param.SL, param.TP)).ID;
            }
            logger.AddMessage(string.Format("Equity = {0}", Account.GetEquity()));
        }
    }
}
