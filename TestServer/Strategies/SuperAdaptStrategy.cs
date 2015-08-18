using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EngineTest;
using Log4Smart;
using Common;

namespace Strategies
{
    //public class SuperAdaptParameter : StrategyParameter
    //{
    //    public int MinTralOpen = 0;
    //    public int MaxTralOpen = Int32.MaxValue;

    //    public SuperAdaptParameters()
    //    {
 
    //        //this.Add("MinTralOpen", 0);
    //        //this.Add("MaxTralOpen", 0);
        
    //    }
    //    //public SuperAdaptParameters(int TP, int SL)
    //    //{
    //    //    this.TP = TP;
    //    //    this.SL = SL;
    //    //}

    //    //static public List<SuperAdaptParameters> AllParameters
    //    //{
    //    //    get
    //    //    {
    //    //        List<SuperAdaptParameters> paramList = new List<SuperAdaptParameters>();


    //    //        for (int sl = 1100; sl < 2000; sl += 3000)
    //    //            for (int tp = 210; tp < 600; tp += 30)
    //    //                for (int startHour = 0; startHour <= 21; startHour += 3)
    //    //                    for (int duration = 12; duration <= 21; duration += 3)
    //    //                        for (int MinTralOpen = tp; MinTralOpen <= tp * 2; MinTralOpen += tp / 5)
    //    //                            for (int MaxTralOpen = 2*MinTralOpen; MaxTralOpen <= MinTralOpen * 3; MaxTralOpen += MinTralOpen/2)
    //    //                        //for (int TralOpenCorrection = 0; duration <= 20; duration += 4)
    //    //            {
    //    //                SuperAdaptParameters p = new SuperAdaptParameters();
    //    //                p.TP = tp;
    //    //                p.SL = sl;
    //    //                p.BasicVolume = 10;
    //    //                p.StartHour = startHour;
    //    //                p.Duration = duration;
    //    //                p.MinTralOpen = MinTralOpen;
    //    //                p.MaxTralOpen = MaxTralOpen;
    //    //                //p.TralOpenCorrection = TralOpenCorrection;
    //    //                paramList.Add(p);
    //    //            }
    //    //        return paramList;
    //    //    }
    //    //}
    //}

    public class SuperAdaptFunction : AForge.Genetic.OptimizationFunctionIntND
    {
        InputData SampleData;
        DateTime StartTime;
        Dictionary<string, double> previosResult = new Dictionary<string, double>();

        static public StratergyParameterRange Range
        {
            get
            {
                StratergyParameterRange range = new StratergyParameterRange();
                range.AddRange("TP", new AForge.IntStepRange(Settings1.Default.TP_Start, Settings1.Default.TP_End, Settings1.Default.TP_Step));
                range.AddRange("SL", new AForge.IntStepRange(Settings1.Default.SL_Start, Settings1.Default.SL_End, Settings1.Default.SL_Step));
                range.AddRange("StartHour", new AForge.IntStepRange(Settings1.Default.StartHour_Start, Settings1.Default.StartHour_End, Settings1.Default.StartHour_Step));
                range.AddRange("Duration", new AForge.IntStepRange(Settings1.Default.Duration_Start, Settings1.Default.Duration_End, Settings1.Default.StartHour_Step));
                range.AddRange("BasicVolume", new AForge.IntStepRange(10, 10, 1));
                range.AddRange("FixedVolume", new AForge.IntStepRange(0, 0, 1));
                return range;
            }
        }
        public SuperAdaptFunction(InputData inSampleData, DateTime startTime) : base (Range)
        {
            this.SampleData = inSampleData;
            this.StartTime = startTime;
        }
        public override double OptimizationFunction(StrategyParameter sParam)
        {
            return EstimateFunction(sParam, string.Empty);            
        }
        public double EstimateFunction(StrategyParameter sParam, string directory)
        {
            if (previosResult.ContainsKey(sParam.ToString()))
                return previosResult[sParam.ToString()];

            using (Log4Smart.Logger logger = new Log4Smart.Logger(false))
            {
                Engine engine = new Engine(logger, logger);
                SuperAdaptStrategy strategy = new SuperAdaptStrategy(sParam, logger);
                int initialMoney = 10000000;
                Account acc = new Account(initialMoney);

                engine.StartTest(SampleData, strategy, acc);
                if (!engine.IsTestSuccessfull)
                {
                    logger.SaveLogToFile("errors", SampleData.Symbol + "_" + StartTime.ToString("yyyyMMdd"));
                    previosResult[sParam.ToString()] = 0;
                    return 0;
                }
                if (directory!=string.Empty)
                    logger.SaveLogToFile(directory + "/" + this.StartTime.ToString("yyyyMMdd") + "_" + (acc.Balance.ToString()) + "$", "allData");

                previosResult[sParam.ToString()] = acc.Balance;
                return acc.Balance;
            }
        }
    }

    public class SuperAdaptStrategy : Common.Strategy
    {
        StrategyParameter param;
        IStrategyLogger logger;
        int TralOPEN = 0;
        bool LimitsOFF;
        bool FlagUP = true;
        int Max, Min;
        int Spread;
        DateTime currTime = DateTime.MinValue;

        public SuperAdaptStrategy(StrategyParameter param, IStrategyLogger logger)
        {
            this.logger = logger;
            this.param = param;
            this.logger.AddMessage("Start strategy with parameter {0}", param.ToString());
        }
        public override bool onStart(int ticks)
        {
            InitSystem(10000);
            return true;
        }
        public override void onTick(Tick<int> tick)
        {
            if (currTime.Ticks / TickHistory.tickInOneMinute == tick.DateTime.Ticks / TickHistory.tickInOneMinute)
                return;
            currTime = tick.DateTime;

            Spread = EvaluateSpread(tick);
            LimitsOFF = (Spread > 200) || Spread > param["TP"];
            System(tick);
        }
        #region Spread
        int EvaluateSpreadPos = 0;
        int EvaluateSpreadSum = 0;
        int EvaluateSpreadAmount = 0;
        const int AmountTicks = 500;
        int[] Spreads = new int[AmountTicks];
        public int EvaluateSpread(Tick<int> tick)
        {
            try
            {
                int Spr = tick.Ask - tick.Bid;

                EvaluateSpreadSum += Spr - Spreads[EvaluateSpreadPos];
                Spreads[EvaluateSpreadPos] = Spr;

                EvaluateSpreadPos++;
                EvaluateSpreadAmount++;

                if (EvaluateSpreadPos == AmountTicks)
                    EvaluateSpreadPos = 0;

                if (EvaluateSpreadAmount > AmountTicks)
                    EvaluateSpreadAmount = AmountTicks;

                return (int)EvaluateSpreadSum / EvaluateSpreadAmount;

            }
            catch
            {
                throw;
            }
        }
        #endregion
        Order SelectSingle(OrderSide side, Order[] orders)
        {
            Order myOrder = null;
            foreach (Order order in orders)
            {
                if (order.Side == side)
                {
                    if (myOrder == null)
                        myOrder = order;
                    else
                        logger.AddMessage("More than one order");
                        //throw new ApplicationException("SelectSingle find more than one order.");
                }
            }
            return myOrder;
        }
        public void System(Tick<int> tick)
        {
            try
            {

                int PriceLow, PriceHigh;
                int open, tp, sl;
                bool LimitsOFFNew;

                LimitsOFFNew = LimitsOFF || (!IsSystemON(tick));

                if (LimitsOFFNew)
                {
                    foreach (Order currOrder in OrderOperation.GetLimitOrders())
                        OrderOperation.CloseOrder(currOrder);
                }
                PriceHigh = Math.Max(tick.Bid, Meta.High(1));
                PriceLow = Math.Min(tick.Bid, Meta.Low(1));

                if (FlagUP)
                {
                    if (PriceHigh > Max)
                    {
                        Max = PriceHigh;

                        /***START BUYLIMIT & SELL******/
                        if (!LimitsOFFNew)
                        {

                            open = Max - TralOPEN + Spread;  // SPREAD!!!!
                            tp = Max - TralOPEN + param["TP"];
                            sl = open - param["SL"];

                            Order myOrder = SelectSingle(OrderSide.Buy, OrderOperation.GetLimitOrders());
                            if (myOrder != null)
                            {
                                Meta.OrderModify(myOrder.ID, open, sl, tp, myOrder.Type);
                            }
                            else
                            {
                                try
                                {
                                    if (null == SelectSingle(OrderSide.Buy, OrderOperation.GetMarketOrders()))
                                        Meta.OrderSend(OrderType.Limit, OrderSide.Buy, LotFunction(), open, sl, tp);
                                }
                                catch (Exception exc)
                                {
                                    logger.AddMessage(exc.ToString());
                                }
                            }
                        }

                        Order myOrder2 = SelectSingle(OrderSide.Sell, OrderOperation.GetMarketOrders());
                        if (myOrder2 != null)
                        {
                            tp = Max - param["TP"] + Spread;
                            try
                            {
                                Meta.OrderModify(myOrder2.ID, myOrder2.OpenPrice, myOrder2.SL, tp, myOrder2.Type);
                            }
                            catch (Exception exc)
                            {
                                logger.AddMessage(exc.ToString());
                            }
                        }
                    }
                    else if (Max - PriceLow >= param["TP"])
                    {
                        if (null == SelectSingle(OrderSide.Sell, OrderOperation.GetMarketOrders()))
                        {

                            //TralOPEN = Max - Min;
                            //if (TralOPEN > param.MaxTralOpen)
                            //    TralOPEN = param.MaxTralOpen;
                            //if (TralOPEN < param.MinTralOpen)
                            //    TralOPEN = param.MinTralOpen;

                            FlagUP = false;
                            Min = PriceLow;

                            /***START SELLLIMIT************/
                            if (!LimitsOFFNew)
                            {
                                open = Max;
                                tp = Max - param["TP"] + Spread; // SPREAD!!!
                                sl = open + param["SL"];

                                Order myOrder = SelectSingle(OrderSide.Sell, OrderOperation.GetLimitOrders());

                                if (myOrder != null)
                                {
                                    Meta.OrderModify(myOrder.ID, open, sl, tp, myOrder.Type);
                                }
                                else
                                {
                                    try
                                    {
                                        Meta.OrderSend(OrderType.Limit, OrderSide.Sell, LotFunction(), open, sl, tp);
                                    }
                                    catch (Exception exc)
                                    {
                                        logger.AddMessage(exc.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
                /***END SELLLIMIT**************/
                else //if (FlagUP)
                {
                    if (PriceLow < Min)
                    {
                        Min = PriceLow;
                        if (!LimitsOFFNew)
                        {
                            open = Min + TralOPEN;
                            tp = Min + TralOPEN - param["TP"] + Spread; // SPREAD!!!
                            sl = open + param["SL"];

                            Order myOrder = SelectSingle(OrderSide.Sell, OrderOperation.GetLimitOrders());
                            if (myOrder != null)
                            {
                                Meta.OrderModify(myOrder.ID, open, sl, tp, myOrder.Type);
                            }
                            else
                            {
                                try
                                {
                                    if (null == SelectSingle(OrderSide.Sell, OrderOperation.GetMarketOrders()))
                                        Meta.OrderSend(OrderType.Limit, OrderSide.Sell, LotFunction(), open, sl, tp);
                                }
                                catch (Exception exc)
                                {
                                    logger.AddMessage(exc.ToString());
                                }
                            }
                        }
                        Order myOrder2 = SelectSingle(OrderSide.Buy, OrderOperation.GetMarketOrders());
                        if (myOrder2 != null)
                        {
                            tp = Min + param["TP"];
                            try
                            {
                                Meta.OrderModify(myOrder2.ID, myOrder2.OpenPrice, myOrder2.SL, tp, myOrder2.Type);
                            }
                            catch (Exception exc)
                            { }
                        }
                    }/***END SELLLIMIT & BUY*********/
                    else if (PriceHigh - Min >= param["TP"])
                    {
                        if (null == SelectSingle(OrderSide.Buy, OrderOperation.GetMarketOrders()))
                        {
                            //TralOPEN = Max - Min;
                            //if (TralOPEN > param.MaxTralOpen)
                            //    TralOPEN = param.MaxTralOpen;
                            //if (TralOPEN < param.MinTralOpen)
                            //    TralOPEN = param.MinTralOpen;

                            FlagUP = true;
                            Max = PriceHigh;


                            /***START BUYLIMIT************/
                            if (!LimitsOFFNew)
                            {
                                open = Min + Spread;
                                tp = Min + param["TP"];
                                sl = open - param["SL"];

                                Order myOrder = SelectSingle(OrderSide.Buy, OrderOperation.GetLimitOrders());

                                if (myOrder != null)
                                {
                                    Meta.OrderModify(myOrder.ID, open, sl, tp, myOrder.Type);
                                }
                                else
                                {
                                    try
                                    {
                                        Order myOrder2 = SelectSingle(OrderSide.Buy, OrderOperation.GetMarketOrders());
                                        if (myOrder2 == null)
                                            Meta.OrderSend(OrderType.Limit, OrderSide.Buy, LotFunction(), open, sl, tp);
                                    }
                                    catch (Exception exc)
                                    {
                                        logger.AddMessage(exc.ToString());
                                    }
                                }
                            }
                        }
                    }

                }
            }
            catch (WrongArgumentException exc)
            {
                logger.AddMessage(exc.ToString());
            }
            
        }

        int LotFunction()
        {
            if (param["FixedVolume"]==0)
                return 10;
            if (_currentTicks > _maxTicks / 2)
                return (int)(10 * 1.5);
            return 10;
        }

        bool IsSystemON(Tick<int> currentTick)
        {
            int EndHour = param["StartHour"] + param["Duration"];
            int CurrentHour = currentTick.DateTime.Hour;

            if ((EndHour >= 24) && (CurrentHour < param["StartHour"]))
                CurrentHour += 24;

            return ((param["StartHour"] <= CurrentHour) && (CurrentHour <= EndHour));
        }

        public override void onEnd()
        {
            logger.AddMessage(this.Account.Account.Statistics.ToString());
        }

        internal void InitSystem(int AmountBars)
        {
            int PriceLow, PriceHigh;
            int i;

            PriceHigh = Math.Max(Meta.Open(AmountBars), Meta.High(AmountBars + 1));
            PriceLow = Math.Min(Meta.Open(AmountBars), Meta.Low(AmountBars + 1));

            bool FlagUP = true;
            Min = PriceHigh;
            Max = PriceLow;

            for (i = AmountBars; i >= 1; i--)
            {
                PriceHigh = Math.Max(Meta.Open(i), Meta.High(i + 1));
                PriceLow = Math.Min(Meta.Open(i), Meta.Low(i + 1));

                if (FlagUP)
                {
                    if (PriceHigh > Max)
                        Max = PriceHigh;
                    else if (Max - PriceLow >= param["TP"])
                    {
                        TralOPEN = Max - Min;

                        FlagUP = false;
                        Min = PriceLow;
                    }
                }
                else // (FlagUP[NSys] == FALSE)
                {
                    if (PriceLow < Min)
                        Min = PriceLow;
                    else if (PriceHigh - Min >= param["TP"])
                    {
                        TralOPEN = Max - Min;

                        FlagUP = true;
                        Max = PriceHigh;
                    }
                }
            }
        }
    }
}
