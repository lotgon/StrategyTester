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
using News;
using FxAdvisorCore;

namespace SuperAdaptStrategy
{
    [RegisterAdvisor(Name = "SuperAdaptStrategy")]
    internal class SuperAdaptStrategy : FxAdvisorCore.SimpleAdvisor
    {
        public SuperAdaptStrategy()
        {
            TestingMode = false;
        }

        class SuperAdaptParam : BasicParam
        {
            public int TP;
            public int SL;
            public int HappyHour;
            public int HappyMinute;
            public int TradeDuration;
        };

        class StrategyState
        {
            public SuperAdaptParam cparam;
            public StrategyTime tradeTime = new StrategyTime();
            public ExtremumPointStrategy ExtremumPoint;
            public News.NewsPeriods newsPeriod ;
            public SpreadAnalyzer spreadAnalyzer;
            public DateTime currTime = DateTime.MinValue;
            public int TralOPEN = 0;
            public bool LimitsOFFNew;
            public bool FlagUP = true;
            public int Max, Min;
            public int Spread;
        }
        List<StrategyState> ssList = new List<StrategyState>();

        public override bool onStart(int ticks)
        {
            this.logger.AddMessage("Start strategy with parameter {0}", param.ToString());

            for (int i = 1; i < 100; i++)
            {
                string paramName = string.Format("P{0}String", i);
                if (this.param.ContainsString(paramName))
                {
                    StrategyState ssNew = new StrategyState();

                    ssNew.cparam = new SuperAdaptParam();
                    string[] paramStr = this.param.GetStringParameter(paramName).Split(new char[] { ',' });
                    ssNew.cparam.Symbol = paramStr[0];
                    ssNew.cparam.TP = Int32.Parse(paramStr[1]);
                    ssNew.cparam.SL = Int32.Parse(paramStr[2]);
                    ssNew.cparam.BasicVolume = Int32.Parse(paramStr[3]);
                    ssNew.cparam.HappyHour = Int32.Parse(paramStr[4]);
                    ssNew.cparam.HappyMinute = Int32.Parse(paramStr[5]);
                    ssNew.cparam.TradeDuration = Int32.Parse(paramStr[6]);
                    ssNew.cparam.InitHistoryMinutes = Int32.Parse(paramStr[7]);
                    ssNew.cparam.ID = i;

                    ssNew.tradeTime.Init(ssNew.cparam.HappyHour, ssNew.cparam.HappyMinute, ssNew.cparam.TradeDuration, 0, 127, 0);
                    ssList.Add(ssNew);
                }
            }

            if (!ssList.Any())
            {
                StrategyState ssNew = new StrategyState();

                ssNew.cparam = new SuperAdaptParam();
                ssNew.cparam.Symbol = base.Symbol;
                ssNew.cparam.TP = param["TP"];
                ssNew.cparam.SL = param["SL"];
                ssNew.cparam.BasicVolume = param["BasicVolume"];
                ssNew.cparam.HappyHour = param["HappyHour"];
                ssNew.cparam.HappyMinute = param["HappyMinute"];
                ssNew.cparam.TradeDuration = param["TradeDuration"];
                ssNew.cparam.InitHistoryMinutes = param["InitMinutesNumber"];
                ssNew.cparam.ID = 0;

                ssNew.tradeTime.Init(ssNew.cparam.HappyHour, ssNew.cparam.HappyMinute, ssNew.cparam.TradeDuration, 0, 31, 0);
                ssList.Add(ssNew);
            }

            return true;
        }
        public override void onTick(Tick<int> tick2)
        {
            lock (ssList)
            {
                try
                {
                    foreach (StrategyState currSS in ssList)
                    {
                        if (currSS.cparam.InitHistoryMinutes > 0)
                        {
                            try
                            {

                                InitComment();
                                AddComment(currSS.cparam.Symbol, "Initializing");
                                currSS.ExtremumPoint = new ExtremumPointStrategy(currSS.cparam.TP);

                                this.logger.AddMessage("Init System");
                                InitSystem(currSS);
                                this.logger.AddMessage("Init Extremums");
                                InitExtremum(currSS);

                                this.logger.AddMessage("Init SpreadAnalyzer");
                                currSS.spreadAnalyzer = new SpreadAnalyzer(100);

                                currSS.cparam.InitHistoryMinutes = 0;
                            }
                            catch (HistoryNotAvailableExceptions exc)
                            {
                                return;
                            }
                            finally
                            {
                                ShowComment();
                            }
                        }
                    }
                    InitComment();

                    foreach (StrategyState currSS in ssList)
                    {
                        Tick<int> currSymbolTick;

                        currSymbolTick = new Tick<int>();
                        currSymbolTick.Ask = Meta.MarketInfo(currSS.cparam.Symbol, MarketInfoType.MODE_ASK);
                        currSymbolTick.Bid = Meta.MarketInfo(currSS.cparam.Symbol, MarketInfoType.MODE_BID);
                        currSymbolTick.DateTime = Convertor.SecondsToDateTime(Meta.MarketInfo(currSS.cparam.Symbol, MarketInfoType.MODE_TIME));
                        currSymbolTick.volume = 1;

                        currSS.Spread = currSS.spreadAnalyzer.EvaluateSpread(currSymbolTick);
                        if (currSS.currTime.Ticks / TickHistory.tickInOneMinute == currSymbolTick.DateTime.Ticks / TickHistory.tickInOneMinute)
                            return;
                        currSS.currTime = currSymbolTick.DateTime;

                        AddCommentLine(currSS.cparam.Symbol, currSS.spreadAnalyzer.ToString());

                        currSS.ExtremumPoint.Process(Math.Max(currSymbolTick.Bid, Meta.iHigh(currSS.cparam.Symbol, GraphPeriod.PERIOD_M1, 1)), Math.Min(currSymbolTick.Bid, Meta.iLow(currSS.cparam.Symbol, GraphPeriod.PERIOD_M1, 1)), currSS.currTime);
                        AddComment(currSS.cparam.Symbol, string.Format("FlagUP={0}   Min={1}   Max={2}   TralOpen={3}\n",
                            currSS.ExtremumPoint.FlagUP, currSS.ExtremumPoint.Min, currSS.ExtremumPoint.Max, currSS.ExtremumPoint.TralOPEN));
                        
                        bool isSpreadInvalid = (currSS.spreadAnalyzer.AverageSpread * 1.7f > currSS.cparam.TP);
                        bool isReadOnly = currSS.cparam.ReadOnly!=0;
                        bool isTradeTime = currSS.tradeTime.IsSystemON(currSymbolTick);
                        bool isWasInit = currSS.ExtremumPoint.TralOPEN > 0 && currSS.ExtremumPoint.TralOPEN < 2000;
                        if( isSpreadInvalid)
                            AddComment(currSS.cparam.Symbol, "Spread WARNING\n");
                        AddComment(currSS.cparam.Symbol, "Trade Time is " + isTradeTime.ToString() + "\n");
                        AddComment(currSS.cparam.Symbol, "isWasInit is  " + isWasInit.ToString() + "\n");

                        bool newLimitsOFFNew = isSpreadInvalid
                            || !isTradeTime
                            //|| /*(currSS.newsPeriod.IsNewsTime(currSymbolTick.DateTime))*/
                            || isReadOnly
                            || !isWasInit;
                        if (newLimitsOFFNew != currSS.LimitsOFFNew)
                        {
                            if (!TestingMode)
                                ;//AddComment(string.Format("TP {0}: LimitsOFFNew was changed to {1}\n" + newLimitsOFFNew.ToString(), currSS.cparam.TP, currSS.LimitsOFFNew));
                            else
                                logger.AddMessage("{2}. LimitsOFFNew was changed to {0}. NewsTime = {1}", newLimitsOFFNew.ToString(), /*newsPeriod.IsNewsTime(tick.DateTime)*/1, currSymbolTick.DateTime);

                            currSS.LimitsOFFNew = newLimitsOFFNew;
                        }
                        AddComment(currSS.cparam.Symbol, "newLimitsOFFNew=" + newLimitsOFFNew.ToString()+"\n");

                        System(currSS, currSymbolTick);
                    }
                }
                catch (HistoryNotAvailableExceptions exc)
                {
                    logger.AddMessage("tick = {0}\r\n {1}\n", tick2.DateTime.ToLongTimeString(), exc);
                    throw;
                }
                catch (Exception exc)
                {
                    AddComment(exc.ToString());
                }
                finally
                {
                    ShowComment();
                }
            
            }
        }
        public override void onEnd()
        {
            //logger.AddMessage(this.Account.Account.Statistics.ToString());
        }
        void System(StrategyState currSS, Tick<int> tick)
        {
            try
            {

                int PriceLow, PriceHigh;
                int open, tp, sl;

                if (currSS.LimitsOFFNew)
                {
                    foreach (Order currOrder in OrderOperation.GetLimitOrders().Where(p => p.Comment.StartsWith(currSS.cparam.IdentityComment)))
                        OrderOperation.CloseOrder(currOrder);
                }
                PriceHigh = Math.Max(tick.Bid, Meta.iHigh(currSS.cparam.Symbol, GraphPeriod.PERIOD_M1, 1));
                PriceLow = Math.Min(tick.Bid, Meta.iLow(currSS.cparam.Symbol, GraphPeriod.PERIOD_M1, 1));

                if (currSS.FlagUP)
                {
                    if (PriceHigh > currSS.Max)
                    {
                        currSS.Max = PriceHigh;

                        /***START BUYLIMIT & SELL******/
                        if (!currSS.LimitsOFFNew)
                        {

                            open = currSS.Max - currSS.TralOPEN + currSS.Spread;  // SPREAD!!!!
                            tp = currSS.Max - currSS.TralOPEN + currSS.cparam.TP;
                            sl = open - currSS.cparam.SL;

                            AddModifyLimitOrder(currSS, open, sl, tp, OrderSide.Buy);
                        }

                        tp = currSS.Max - currSS.cparam.TP + currSS.Spread;
                        ModifyMarketOrders(currSS, OrderSide.Sell, tp);
                    }
                    else if (currSS.Max - PriceLow >= currSS.cparam.TP)
                    {
                        if (!OrderOperation.GetMarketOrders().Where(p => p.Side==OrderSide.Sell&& p.Comment.StartsWith(currSS.cparam.IdentityComment)).Any())
                        {
                            currSS.TralOPEN = currSS.Max - currSS.Min;

                            currSS.FlagUP = false;
                            currSS.Min = PriceLow;

                            /***START SELLLIMIT************/
                            if (!currSS.LimitsOFFNew)
                            {
                                open = currSS.Max;
                                tp = currSS.Max - currSS.cparam.TP + currSS.Spread; // SPREAD!!!
                                sl = open + currSS.cparam.SL;

                                AddModifyLimitOrder(currSS, open, sl, tp, OrderSide.Sell);
                            }
                        }
                    }
                }
                /***END SELLLIMIT**************/
                else //if (FlagUP)
                {
                    if (PriceLow < currSS.Min)
                    {
                        currSS.Min = PriceLow;
                        if (!currSS.LimitsOFFNew)
                        {
                            open = currSS.Min + currSS.TralOPEN;
                            tp = currSS.Min + currSS.TralOPEN - currSS.cparam.TP + currSS.Spread; // SPREAD!!!
                            sl = open + currSS.cparam.SL;

                            AddModifyLimitOrder(currSS, open, sl, tp, OrderSide.Sell);
                        }

                        tp = currSS.Min + currSS.cparam.TP;
                        ModifyMarketOrders(currSS, OrderSide.Buy, tp);

                    }/***END SELLLIMIT & BUY*********/
                    else if (PriceHigh - currSS.Min >= currSS.cparam.TP)
                    {
                        if (!OrderOperation.GetMarketOrders().Where(p => p.Side == OrderSide.Buy&&p.Comment.StartsWith(currSS.cparam.IdentityComment)).Any())
                        {
                            currSS.TralOPEN = currSS.Max - currSS.Min;

                            currSS.FlagUP = true;
                            currSS.Max = PriceHigh;

                            /***START BUYLIMIT************/
                            if (!currSS.LimitsOFFNew)
                            {
                                open = currSS.Min + currSS.Spread;
                                tp = currSS.Min + currSS.cparam.TP;
                                sl = open - currSS.cparam.SL;

                                AddModifyLimitOrder(currSS, open, sl, tp, OrderSide.Buy);
                            }
                        }
                    }
                }
            }
            catch (WrongArgumentException exc)
            {
                AddComment(exc.ToString());
            }
            
        }

        void AddModifyLimitOrder(StrategyState currSS, int open, int sl, int tp, OrderSide side)
        {
            IEnumerable<Order> limitOrders = OrderOperation.GetLimitOrders().Where(p => p.Comment.StartsWith(currSS.cparam.IdentityComment))
                .Where(p => p.Side == side && p.Comment.StartsWith(currSS.cparam.IdentityComment));

            if (limitOrders.Any())
            {
                foreach (Order currOrder in limitOrders)
                    Meta.OrderModify(currOrder.ID, open, sl, tp, currOrder.Type);
            }
            else if (OrderOperation.GetMarketOrders().Where(p => p.Side == side && p.Comment.StartsWith(currSS.cparam.IdentityComment)).Count() == 0 
                && ((side == OrderSide.Sell ^ currSS.ExtremumPoint.FlagUP)))
            {
                try
                {
                    Meta.OrderSend(currSS.cparam.Symbol, OrderType.Limit, side, GetVolume(currSS), open, sl, tp, currSS.cparam.NewUniqueComment());
                }
                catch (Exception exc)
                {
                    AddComment(exc.ToString());
                }
            }
        }
        void ModifyMarketOrders(StrategyState currSS, OrderSide side, int tp)
        {
            IEnumerable<Order> marketOrders= OrderOperation.GetMarketOrders().Where(p =>p.Side==side
                && p.Comment.StartsWith(currSS.cparam.IdentityComment));
            if (marketOrders.Any() )
            {
                try
                {
                    foreach( Order currOrder in marketOrders )
                        Meta.OrderModify(currOrder.ID, currOrder.OpenPrice, currOrder.SL, tp, currOrder.Type);
                }
                catch (Exception exc)
                {
                    AddComment(exc.ToString());
                }
            }
        }
        int GetVolume(StrategyState ss)
        {
            if (ss.cparam.BasicVolume > 0)
                return ss.cparam.BasicVolume;

            return (int)Math.Max(10, Meta.AccountEquity() * (-ss.cparam.BasicVolume) / 1000);
        }

        void InitSystem(StrategyState ss)
        {
            int PriceLow, PriceHigh;
            int i;

            PriceHigh = Math.Max(Meta.iHigh(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, ss.cparam.InitHistoryMinutes), Meta.iHigh(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, ss.cparam.InitHistoryMinutes + 1));
            PriceLow = Math.Min(Meta.iLow(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, ss.cparam.InitHistoryMinutes), Meta.iLow(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, ss.cparam.InitHistoryMinutes + 1));

            bool FlagUP = true;
            ss.Min = PriceHigh;
            ss.Max = PriceLow;

            for (i = ss.cparam.InitHistoryMinutes; i >= 1; i--)
            {
                PriceHigh = Math.Max(Meta.iHigh(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, i), Meta.iHigh(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, i + 1));
                PriceLow = Math.Min(Meta.iLow(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, i), Meta.iLow(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, i + 1));

                if (FlagUP)
                {
                    if (PriceHigh > ss.Max)
                        ss.Max = PriceHigh;
                    else if (ss.Max - PriceLow >= ss.cparam.TP)
                    {
                        ss.TralOPEN = ss.Max - ss.Min;

                        FlagUP = false;
                        ss.Min = PriceLow;
                    }
                }
                else // (FlagUP[NSys] == FALSE)
                {
                    if (PriceLow < ss.Min)
                        ss.Min = PriceLow;
                    else if (PriceHigh - ss.Min >= ss.cparam.TP)
                    {
                        ss.TralOPEN = ss.Max - ss.Min;
                        FlagUP = true;

                        ss.Max = PriceHigh;
                    }
                }
            }
        }
        void InitExtremum(StrategyState ss)
        {
            ss.ExtremumPoint.Min = Math.Max(Meta.iHigh(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, ss.cparam.InitHistoryMinutes), Meta.iHigh(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, ss.cparam.InitHistoryMinutes + 1)); ;
            ss.ExtremumPoint.Max = Math.Min(Meta.iLow(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, ss.cparam.InitHistoryMinutes), Meta.iLow(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, ss.cparam.InitHistoryMinutes + 1));

            for (int i = ss.cparam.InitHistoryMinutes; i >= 1; i--)
                ss.ExtremumPoint.Process(Math.Max(Meta.iHigh(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, i), Meta.iHigh(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, i + 1)), Math.Min(Meta.iLow(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, i), Meta.iLow(ss.cparam.Symbol, GraphPeriod.PERIOD_M1, i + 1)), DateTime.MaxValue);
        }

    }
}
