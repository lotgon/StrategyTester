using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EngineTest;
using Log4Smart;
using Common;
using System.IO;
using FxAdvisorCore.Extensibility;
using StrategyCommon;
using News;
using FxAdvisorCore;

namespace Pipsovik
{
    [RegisterAdvisor(Name = "PipsovikStrategy")]
    public class PipsovikStrategy : FxAdvisorCore.SimpleAdvisor
    {
        enum OpenMode
        {
            OnlyBuy=0, 
            OnlySell=1, 
            Both
        }
        class PipsovikParam : BasicParam
        {
            public int OpenOrderShift;
            public int HappyHour;
            public int HappyMinute;
            public int InitPeriodMinute;
            public int TradeDuration;
            public int ClosingDuration;
            public int WorkingDay;
        };

        class StrategyState
        {
            public PipsovikParam cparam;
            public SpreadAnalyzer spreadAnalyzer =  new SpreadAnalyzer(3);
            public StrategyTime tradeTime = new StrategyTime();
            public StrategyTime closeAllTime = new StrategyTime();
            public int happyPrice;

            public  DateTime happyPriceInitializationDateTime = DateTime.MinValue;
            //bool isDaylight = false;
            //public void UpdateDaylight(DateTime dateTime)
            //{
            //    bool current = StrategyTime.timeZoneInfo.IsDaylightSavingTime(dateTime);
            //    if (current != isDaylight)
            //    {
            //        if (current)
            //        {
            //            cparam.HappyHour -= 1;
            //            if (cparam.HappyHour < 0)
            //                cparam.HappyHour += 24;
            //        }
            //        else
            //        {
            //            cparam.HappyHour += 1;
            //            if (cparam.HappyHour > 23)
            //                cparam.HappyHour -= 24;

            //        }

            //        isDaylight = current;
            //    }
            //}
        }
        List<StrategyState> ssList = new List<StrategyState>();


        public PipsovikStrategy() 
        {
        }

        public override bool onStart(int ticks)
        {
            for (int i = 1; i < 100; i++)
            {
                string paramName = string.Format("P{0}String", i);
                if (this.param.ContainsString(paramName))
                {
                    StrategyState ssNew = new StrategyState();

                    ssNew.cparam = new PipsovikParam();
                    string[] paramStr = this.param.GetStringParameter(paramName).Split(new char[] { ',' });
                    ssNew.cparam.Symbol = paramStr[0];
                    ssNew.cparam.OpenOrderShift = Int32.Parse(paramStr[1]);
                    ssNew.cparam.BasicVolume = Int32.Parse(paramStr[2]);
                    ssNew.cparam.HappyHour = Int32.Parse(paramStr[3]);
                    ssNew.cparam.HappyMinute = Int32.Parse(paramStr[4]);
                    ssNew.cparam.InitPeriodMinute = Int32.Parse(paramStr[5]);
                    ssNew.cparam.TradeDuration = Int32.Parse(paramStr[6]);
                    ssNew.cparam.ClosingDuration = Int32.Parse(paramStr[7]);
                    ssNew.cparam.WorkingDay = Int32.Parse(paramStr[8]);
                    ssNew.cparam.ID = i;

                    int tradeMinuteTotal = (ssNew.cparam.HappyHour * 60 + ssNew.cparam.HappyMinute + ssNew.cparam.InitPeriodMinute);
                    int startHour =  tradeMinuteTotal / 60;
                    if (startHour >= 24)
                        startHour -= 24;
                    int startMinute = tradeMinuteTotal % 60;
                    ssNew.tradeTime.Init(startHour, startMinute, ssNew.cparam.TradeDuration, 0, ssNew.cparam.WorkingDay, 12);
                    ssNew.closeAllTime.Init(startHour, startMinute, ssNew.cparam.TradeDuration + ssNew.cparam.ClosingDuration, 0, ssNew.cparam.WorkingDay, 12);

                    ssList.Add(ssNew);
                }
            }

            if (!ssList.Any())
            {
                StrategyState ssNew = new StrategyState();

                ssNew.cparam = new PipsovikParam();
                ssNew.cparam.Symbol = base.Symbol;
                ssNew.cparam.OpenOrderShift = param["OpenOrderShift"];
                ssNew.cparam.BasicVolume = param["BasicVolume"];
                ssNew.cparam.HappyHour = param["HappyHour"];
                ssNew.cparam.HappyMinute = param["HappyMinute"];
                ssNew.cparam.InitPeriodMinute = param["InitPeriodMinute"];
                ssNew.cparam.TradeDuration = param["TradeDuration"];
                ssNew.cparam.ClosingDuration = param["ClosingDuration"];
                ssNew.cparam.WorkingDay = param["WorkingDay"];
                ssNew.cparam.ID = 0;

                int tradeMinuteTotal = (ssNew.cparam.HappyHour * 60 + ssNew.cparam.HappyMinute + ssNew.cparam.InitPeriodMinute);
                int startHour = tradeMinuteTotal / 60;
                if (startHour >= 24)
                    startHour -= 24;
                int startMinute = tradeMinuteTotal % 60;
                ssNew.tradeTime.Init(startHour, startMinute, ssNew.cparam.TradeDuration, 0, ssNew.cparam.WorkingDay, 12);
                ssNew.closeAllTime.Init(startHour, startMinute, ssNew.cparam.TradeDuration + ssNew.cparam.ClosingDuration, 0, ssNew.cparam.WorkingDay, 12);

                ssList.Add(ssNew);
            }


            return true;
        }

        public override void onTick(Tick<int> currTickTemp)
        {
            DateTime latestDateTime = currTickTemp.DateTime;

            
            lock (ssList)
            {
                do
                {

                        InitComment();

                        foreach (StrategyState currSS in ssList)
                        {
                            try
                            {
                                Tick<int> currSymbolTick;

                                currSymbolTick = new Tick<int>();
                                currSymbolTick.Ask = Meta.MarketInfo(currSS.cparam.Symbol, MarketInfoType.MODE_ASK);
                                currSymbolTick.Bid = Meta.MarketInfo(currSS.cparam.Symbol, MarketInfoType.MODE_BID);
                                currSymbolTick.DateTime = Convertor.SecondsToDateTime(Meta.MarketInfo(currSS.cparam.Symbol, MarketInfoType.MODE_TIME));
                                currSymbolTick.volume = 1;
                                if (latestDateTime < currSymbolTick.DateTime)
                                    latestDateTime = currSymbolTick.DateTime;

                                //currSS.UpdateDaylight(currSymbolTick.DateTime);

                                if (!TestingMode && Math.Abs((currSymbolTick.DateTime - latestDateTime).TotalMinutes) > 3)
                                {
                                    AddComment(currSS.cparam.Symbol + ": time lag is too much.");
                                    continue;
                                }

                                int average = (currSymbolTick.Ask + currSymbolTick.Bid) / 2;

                                if (!UpdateHappyPrice(currSymbolTick, currSS))
                                    continue;

                                int diffPips = average - currSS.happyPrice;
                                AddComment(string.Format("Diff in Pips = {0}   ", diffPips));

                                currSS.spreadAnalyzer.EvaluateSpread(currSymbolTick);
                                AddComment(string.Format("Spread = {0}   ", currSS.spreadAnalyzer.AverageSpread));
                                if (currSS.spreadAnalyzer.AverageSpread * 1.7f > currSS.cparam.OpenOrderShift)
                                {
                                    AddComment("Spread Filter WARNING!\n");
                                    continue;
                                }

                                bool isTimeOn = currSS.tradeTime.IsSystemON(currSymbolTick);
                                bool closeTimeAll = currSS.closeAllTime.IsSystemON(currSymbolTick);
                                AddComment(string.Format("Open season = {0}\n", isTimeOn));


                                IEnumerable<Order> ordersBuy = OrderOperation.GetBuyMarketOrders().Where(p => p.Symbol == currSS.cparam.Symbol && p.Comment.StartsWith(currSS.cparam.IdentityComment));
                                IEnumerable<Order> ordersSell = OrderOperation.GetSellMarketOrders().Where(p => p.Symbol == currSS.cparam.Symbol && p.Comment.StartsWith(currSS.cparam.IdentityComment));
                                if (ordersBuy.Any() || ordersSell.Any())
                                {
                                    if (!closeTimeAll)
                                        foreach (Order currOrder in ordersBuy.Union(ordersSell).ToArray())
                                            OrderOperation.CloseOrder(currOrder);

                                    AddComment("Order has been opened\n");
                                    if (this.TestingMode)
                                    {
                                        if (ordersBuy.Any() && average >= currSS.happyPrice)
                                            foreach (Order currOrder in ordersBuy.ToArray())
                                                OrderOperation.CloseOrder(currOrder);
                                        if (ordersSell.Any() && average <= currSS.happyPrice)
                                            foreach (Order currOrder in ordersSell.ToArray())
                                                OrderOperation.CloseOrder(currOrder);
                                    }
                                    else
                                    {
                                        if (ordersBuy.Any() && GetLatestHigh(currSS.cparam.Symbol) >= currSS.happyPrice)
                                            foreach (Order currOrder in ordersBuy.ToArray())
                                                OrderOperation.CloseOrder(currOrder);
                                        if (ordersSell.Any() && GetLatestLow(currSS.cparam.Symbol) <= currSS.happyPrice)
                                            foreach (Order currOrder in ordersSell.ToArray())
                                                OrderOperation.CloseOrder(currOrder);
                                    }

                                    continue;
                                }

                                if (!isTimeOn)
                                    continue;

                                if (diffPips > currSS.cparam.OpenOrderShift)
                                {
                                    int orderId = Meta.OrderSend(currSS.cparam.Symbol, OrderType.Market, OrderSide.Sell, GetVolume(currSS), currSymbolTick.Bid, 0, 0, currSS.cparam.NewUniqueComment());
                                    if (orderId > 0)
                                        Meta.OrderModify(orderId, currSymbolTick.Bid, 0, currSS.happyPrice, OrderType.Market);

                                }
                                if (-diffPips > currSS.cparam.OpenOrderShift)
                                {
                                    int orderId = Meta.OrderSend(currSS.cparam.Symbol, OrderType.Market, OrderSide.Buy, GetVolume(currSS), currSymbolTick.Ask, 0, 0, currSS.cparam.NewUniqueComment());
                                    if (orderId > 0)
                                        Meta.OrderModify(orderId, currSymbolTick.Bid, 0, currSS.happyPrice, OrderType.Market);

                                }

                            }
                            catch (HistoryNotAvailableExceptions exc)
                            {
                                logger.AddMessage("tick = {0}\r\n {1}\n", currTickTemp.DateTime.ToLongTimeString(), exc);
                                throw;
                            }
                            catch (Exception exc)
                            {
                                AddComment(exc.ToString());
                            }
                        }

                   ShowComment();

                }
                while (!TestingMode);
            }
        }

        int testingLow = Int32.MaxValue;
        int testingHigh=0;

        private bool UpdateHappyPrice(Tick<int> currTick, StrategyState ss)
        {
            string id = ss.cparam.ID.ToString();

            if (TestingMode)
            {
                if (currTick.DateTime.Hour == ss.cparam.HappyHour && currTick.DateTime.Minute == ss.cparam.HappyMinute)
                {
                    int averagePrice = (currTick.Ask+currTick.Bid)/2;
                    if (testingLow > averagePrice)
                        testingLow = averagePrice;
                    if (testingHigh < averagePrice)
                        testingHigh = averagePrice;

                    ss.happyPrice = (testingLow + testingHigh) / 2;
                }
                else
                {
                    testingLow = Int32.MaxValue;
                    testingHigh = 0;
                }

                if (ss.happyPrice == 0)
                    return false;
                return true;
            }
            if ((currTick.DateTime - ss.happyPriceInitializationDateTime).TotalMinutes > 60 * 24)
                ss.happyPrice = 0;

            if (currTick.DateTime.Hour == ss.cparam.HappyHour && currTick.DateTime.Minute == ss.cparam.HappyMinute)
            {
                ss.happyPrice = 0;
                return false;
            }
            if (ss.happyPrice == 0)
            {
                AddComment(ss.cparam.ID, "Initializing happy price\n");

                DateTime happyDateTime = new DateTime(currTick.DateTime.Year, currTick.DateTime.Month, currTick.DateTime.Day,
                        ss.cparam.HappyHour, ss.cparam.HappyMinute, 0);

                int happyTotalMinute = ss.cparam.HappyHour * 60 + ss.cparam.HappyMinute;
                int currentTotalMinute = currTick.DateTime.Hour * 60 + currTick.DateTime.Minute;
                if (currentTotalMinute < happyTotalMinute)
                    happyDateTime = happyDateTime.AddDays(-1);
                int shift = Meta.iBarShift(GetNameAverageSymbol(ss.cparam.Symbol), GraphPeriod.PERIOD_M1, happyDateTime, true); 
                if( shift == -1)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        happyDateTime = happyDateTime.AddMinutes(-1);
                        shift = Meta.iBarShift(GetNameAverageSymbol(ss.cparam.Symbol), GraphPeriod.PERIOD_M1, happyDateTime, true); 
                    }

                    AddComment(GetNameAverageSymbol(ss.cparam.Symbol) + ": can`t get shift for this symbol.");
                    Meta.RefreshRates();
                    return false;
                }

                int high = Meta.iHigh(GetNameAverageSymbol(ss.cparam.Symbol), GraphPeriod.PERIOD_M1, shift);
                int low = Meta.iLow(GetNameAverageSymbol(ss.cparam.Symbol), GraphPeriod.PERIOD_M1, shift);
                if (high == 0 || low == 0)
                {
                    AddComment(GetNameAverageSymbol(ss.cparam.Symbol) + ": can`t get iHigh or iLow from Meta\n");
                    Meta.RefreshRates();
                    return false;
                }
                ss.happyPrice = (high + low) / 2;
                ss.happyPriceInitializationDateTime = happyDateTime;
            }
            AddComment(ss.cparam.Symbol +  string.Format("  happyPrice={0}   ", ss.happyPrice));

            return true;
        }
        private int GetLatestHigh(string symbol)
        {
            return Meta.iHigh(GetNameAverageSymbol(symbol), GraphPeriod.PERIOD_M1, 0);
        }
        private int GetLatestLow(string symbol)
        {
            return Meta.iLow(GetNameAverageSymbol(symbol), GraphPeriod.PERIOD_M1, 0);
                
        }
        private int GetVolume(StrategyState ss)
        {
            if( ss.cparam.BasicVolume >  0 )
                return ss.cparam.BasicVolume;

            return (int)Math.Max(10, Meta.AccountEquity()*(-ss.cparam.BasicVolume)/1000);
        }
        private string GetNameAverageSymbol(string symbol)
        {
            return symbol + "_Avg";
        }

        public override void onEnd()
        {
        }
    }
}

public static class EnumerableExtensions
{
    public static T MaxElement<T, TCompare>(this IEnumerable<T> collection, Func<T, TCompare> func) where TCompare : IComparable<TCompare>
    {
        T maxItem = default(T);
        TCompare maxValue = default(TCompare);
        foreach (var item in collection)
        {
            TCompare temp = func(item);
            if (maxItem == null || temp.CompareTo(maxValue) > 0)
            {
                maxValue = temp;
                maxItem = item;
            }
        }
        return maxItem;
    }
    public static T MinElement<T, TCompare>(this IEnumerable<T> collection, Func<T, TCompare> func) where TCompare : IComparable<TCompare>
    {
        T minItem = default(T);
        TCompare minValue = default(TCompare);
        foreach (var item in collection)
        {
            TCompare temp = func(item);
            if (minItem == null || temp.CompareTo(minValue) < 0)
            {
                minValue = temp;
                minItem = item;
            }
        }
        return minItem;
    }
    public static bool IsEmpty<T>(this IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            return false;
        }
        return true;
    }
}

