using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EngineTest;
using Log4Smart;
using Common;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Configuration;
using FxAdvisorCore.Extensibility;
using StrategyCommon;

namespace SuperStrategy
{
    [RegisterAdvisor(Name = "SuperStrategy")]
    public class SuperStrategy : FxAdvisorCore.SimpleAdvisor
    {
        public const int countOrders = 1;

        ExtremumPointStrategy ExtremumPoint;
        ExtremumPointStrategy ExtremumPoint2;
        SpreadAnalyzer spreadAnalyzer;
        DateTime currTime = DateTime.MinValue;
        int Spread;
        StrategyTime strategyTime;

        public SuperStrategy()
            : this(null, new Log4Smart.Logger(false))
        {
            TestingMode = false;

        }
        public SuperStrategy(StrategyParameter param, IStrategyLogger logger)
        {
            TestingMode = true;
            this.logger = logger;
            this.param = param;
        }

        public override bool onStart(int ticks)
        {
            ExtremumPoint = new ExtremumPointStrategy((param["TP"] * param["TPCoefExtremum"]) / 10);
            ExtremumPoint2 = new ExtremumPointStrategy((param["TP"] * param["TPCoefExtremum2"]) / 10);
            strategyTime = new StrategyTime(param["ProhibitedStartHour"], param["ProhibitedStartMinute"], param["ProhibitedDuration"], (param.Keys.Contains("GMT") ? param["GMT"] : 0), 31);
            spreadAnalyzer = new SpreadAnalyzer(100);

            InitExtremum(1000);
            if (logger != null)
                this.logger.AddMessage("Start strategy with parameter {0}", param.ToString());

            return true;
        }
        public override void onTick(Tick<int> currTick)
        {
            Spread = spreadAnalyzer.EvaluateSpread(currTick);

            ExtremumPoint.Process(Math.Max(currTick.Bid, Meta.High(this.Symbol, 1)), Math.Min(currTick.Bid, Meta.Low(this.Symbol, 1)), currTick.DateTime);
            ExtremumPoint2.Process(Math.Max(currTick.Bid, Meta.High(this.Symbol, 1)), Math.Min(currTick.Bid, Meta.Low(this.Symbol, 1)), currTick.DateTime);

            if (currTime.Ticks / TickHistory.tickInOneMinute == currTick.DateTime.Ticks / TickHistory.tickInOneMinute)
                return;
            currTime = currTick.DateTime;

            if( !TestingMode )
                ShowComment();

            try
            {

                int PriceBidHighest = Math.Max(currTick.Bid, Meta.High(this.Symbol, 1));
                int PriceBidLowest = Math.Min(currTick.Bid, Meta.Low(this.Symbol, 1));
                int PriceAskHighest = PriceBidHighest + Spread;
                int PriceAskLowest = PriceBidLowest + Spread;

                IEnumerable<Order> marketOrders = this.OrderOperation.GetMarketOrders();
                IEnumerable<Order> limitOrders = this.OrderOperation.GetLimitOrders();

                foreach (Order currOrder in marketOrders)
                {
                    if (currOrder.Side == OrderSide.Buy)
                    {
                        if (currOrder.TP - PriceAskLowest > param["TP"])
                            OrderOperation.ModifyMarketOrder(currOrder.ID, currOrder.SL, PriceAskLowest + param["TP"]);
                    }
                    else
                    {
                        if (PriceBidHighest - currOrder.TP > param["TP"])
                            OrderOperation.ModifyMarketOrder(currOrder.ID, currOrder.SL, PriceBidHighest - param["TP"]);
                    }
                }

                foreach (Order currOrder in limitOrders)
                {
                    if (!strategyTime.IsSystemON(currTick))
                    {
                        OrderOperation.CloseOrder(currOrder);
                        break;
                    }

                    if (currOrder.Side == OrderSide.Buy)
                    {
                        if (PriceAskHighest - currOrder.OpenPrice > param["LimitOpen"])
                        {
                            int openPrice = PriceAskHighest - param["LimitOpen"];
                            OrderOperation.ModifyLimitOrder(currOrder.ID, openPrice, openPrice - param["SL"], openPrice + param["TP"]);
                        }
                        if (!ExtremumPoint.FlagUP)
                            OrderOperation.CloseOrder(currOrder);
                    }
                    else
                    {
                        if (currOrder.OpenPrice - PriceBidLowest > param["LimitOpen"])
                        {
                            int openPrice = PriceBidLowest + param["LimitOpen"];
                            OrderOperation.ModifyLimitOrder(currOrder.ID, openPrice, openPrice + param["SL"], openPrice - param["TP"]);
                        }
                        if (ExtremumPoint.FlagUP)
                            OrderOperation.CloseOrder(currOrder);
                    }
                }

                if (strategyTime.IsSystemON(currTick) && limitOrders.Count() == 0 && marketOrders.Count() == 0)
                {
                    if (ExtremumPoint2.FlagUP == ExtremumPoint.FlagUP)
                    {
                        OrderSide side = ExtremumPoint.FlagUP ? OrderSide.Buy : OrderSide.Sell;
                        int openPrice = side == OrderSide.Buy ? PriceAskHighest - param["LimitOpen"] : PriceBidLowest + param["LimitOpen"];

                        OrderOperation.AddOrder(Order.NewLimitOrder(Meta.Symbol(), side, GetVolume(), openPrice, param["SL"], param["TP"]));

                    }
                }
            }
            catch (Exception exc)
            {
                logger.AddMessage(exc.ToString());
            }
        }
        public override void onEnd()
        {
            if( logger != null )
                logger.AddMessage(string.Format("Balance = {0}", Account.GetEquity()));
        }

        public int GetVolume()
        {
            if (_currentTicks > _maxTicks / 2 && param["FixedVolume"] == 0)
                return (int) (param["BasicVolume"]*1.5);
            return param["BasicVolume"];
        }
        internal void InitExtremum(int AmountBars)
        {
            ExtremumPoint.Min = Math.Max(Meta.Open(this.Symbol, AmountBars), Meta.High(this.Symbol, AmountBars + 1)); ;
            ExtremumPoint.Max = Math.Min(Meta.Open(this.Symbol, AmountBars), Meta.Low(this.Symbol, AmountBars + 1));
            
            ExtremumPoint2.Min = Math.Max(Meta.Open(this.Symbol, AmountBars), Meta.High(this.Symbol, AmountBars + 1)); ;
            ExtremumPoint2.Max = Math.Min(Meta.Open(this.Symbol, AmountBars), Meta.Low(this.Symbol, AmountBars + 1));

            for (int i = AmountBars; i >= 1; i--)
            {
                ExtremumPoint.Process(Math.Max(Meta.Open(this.Symbol, i), Meta.High(this.Symbol, i + 1)), Math.Min(Meta.Open(this.Symbol, i), Meta.Low(this.Symbol, i + 1)), DateTime.MaxValue);
                ExtremumPoint2.Process(Math.Max(Meta.Open(this.Symbol, i), Meta.High(this.Symbol, i + 1)), Math.Min(Meta.Open(this.Symbol, i), Meta.Low(this.Symbol, i + 1)), DateTime.MaxValue);
            }

        }
        private void ShowComment()
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine(param.ToString());
            strBuilder.AppendLine("\nExtremum 1");
            strBuilder.Append("Min1 = " + ExtremumPoint.Min);
            strBuilder.Append("\n Max1 = "+ ExtremumPoint.Max);
            strBuilder.Append("\n TralOPEN1 = "+ ExtremumPoint.TralOPEN);
            strBuilder.Append("\n FlagUP1 = "+ ExtremumPoint.FlagUP);
            strBuilder.AppendLine("\n Extremum 2");
            strBuilder.Append("\nMin2 = " + ExtremumPoint2.Min);
            strBuilder.Append("\n Max2 = " + ExtremumPoint2.Max);
            strBuilder.Append("\n TralOPEN2 = " + ExtremumPoint2.TralOPEN);
            strBuilder.Append("\n FlagUP2 = " + ExtremumPoint2.FlagUP);

            Meta.Comment(strBuilder.ToString());

        }
    }

}
