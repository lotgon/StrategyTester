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

namespace MartinGale
{
    [RegisterAdvisor(Name = "MartinGaleStrategy")]
    public class MartinGaleStrategy : FxAdvisorCore.SimpleAdvisor
    {
        DateTime currTime = DateTime.MinValue;

        SpreadAnalyzer spreadAnalyzer;
        List<ExtremumPointStrategy> ExtremumPoints = new List<ExtremumPointStrategy>();
        List<MartinGaleParam> customParams = new List<MartinGaleParam>();
        class MartinGaleParam : BasicParam
        {
            public int TP;
            public int MaxSteps;
            public int StartShift;
            public int CoefVolume;
        };
        
        public MartinGaleStrategy() 
        {
        }

        public override bool onStart(int ticks)
        {
            for (int i = 0; i < 1000; i++)
            {
                string paramName = string.Format("P{0}String", i);
                if (this.param.ContainsString(paramName))
                {
                    string[] paramStr = this.param.GetStringParameter(paramName).Split(new char[] { ',' });
                    MartinGaleParam cparam = new MartinGaleParam();
                    cparam.Symbol = paramStr[0];
                    cparam.TP = Int32.Parse(paramStr[1]);
                    cparam.MaxSteps = Int32.Parse(paramStr[2]);
                    cparam.StartShift = Int32.Parse(paramStr[3]) * cparam.TP/10;
                    cparam.CoefVolume = Int32.Parse(paramStr[4]);
                    cparam.BasicVolume = Int32.Parse(paramStr[5]);
                    cparam.ID = i;
                    if( this.param.Contains("InitHistoryMinutes"))
                        cparam.InitHistoryMinutes = this.param["InitHistoryMinutes"];
                    else
                        cparam.InitHistoryMinutes = 0;
                    if( this.param.Contains(ReadOnlyParamName))
                        cparam.ReadOnly = this.param[ReadOnlyParamName];
                    else
                        cparam.ReadOnly = 0;
                    customParams.Add(cparam);
                }
            }
            if (this.customParams.Count == 0)
            {
                MartinGaleParam cparam = new MartinGaleParam();
                cparam.Symbol = base.Symbol;
                cparam.TP = param["TP"];
                cparam.MaxSteps = param["MaxSteps"];
                cparam.StartShift = param["StartShift"] * cparam.TP / 10;
                cparam.CoefVolume = param["CoefVolume"];
                cparam.BasicVolume = param["BasicVolume"];
                cparam.ID = 0;
                if (this.param.Contains("InitHistoryMinutes"))
                    cparam.InitHistoryMinutes = this.param["InitHistoryMinutes"];
                else
                    cparam.InitHistoryMinutes = 0; 
                if (this.param.Contains(ReadOnlyParamName))
                    cparam.ReadOnly = this.param[ReadOnlyParamName];
                else
                    cparam.ReadOnly = 0;
                customParams.Add(cparam);
            }

            this.logger.AddMessage("Start strategy with parameter {0}", param.ToString());
            foreach (MartinGaleParam cParam in customParams)
                ExtremumPoints.Add( new ExtremumPointStrategy(cParam.TP));
            //this.logger.AddMessage("Init Extremums 1000");
            //InitExtremum(1000);
            spreadAnalyzer = new SpreadAnalyzer(100);
            return true;
        }
        //internal void InitExtremum(int AmountBars)
        //{
        //    ExtremumPoints.Min = Math.Max(Meta.Open(this.Symbol, AmountBars), Meta.High(this.Symbol, AmountBars + 1)); ;
        //    ExtremumPoints.Max = Math.Min(Meta.Open(this.Symbol, AmountBars), Meta.Low(this.Symbol, AmountBars + 1));

        //    for (int i = AmountBars; i >= 1; i--)
        //        ExtremumPoints.Process(Math.Max(Meta.Open(this.Symbol, i), Meta.High(this.Symbol, i + 1)), Math.Min(Meta.Open(this.Symbol, i), Meta.Low(this.Symbol, i + 1)));
        //}


        public override void onTick(Tick<int> currTick)
        {
            try
            {
                spreadAnalyzer.EvaluateSpread(currTick);
                InitComment();


                if ((currTick.DateTime.DayOfWeek == DayOfWeek.Friday && currTick.DateTime.Hour > 23)
                    || (currTick.DateTime.DayOfWeek == DayOfWeek.Monday && currTick.DateTime.Hour < 1))
                {
                    this.OrderOperation.CloseAllOrders();
                    return;
                }

                for( int i =0; i<customParams.Count;i++)
                {
                    AddComment(() => string.Format("{0};   {1}\n", customParams[i].Symbol, ExtremumPoints[i].ToString()));
                    MartinGaleParam cparam = customParams[i];
                    Tick<int> currSymbolTick = new Tick<int>
                                                   {
                                                       Ask = Meta.MarketInfo(cparam.Symbol, MarketInfoType.MODE_ASK),
                                                       Bid = Meta.MarketInfo(cparam.Symbol, MarketInfoType.MODE_BID),
                                                       DateTime = currTick.DateTime,
                                                       volume = 1
                                                   };

                    IEnumerable<Order> allBuyOrders = OrderOperation.GetBuyMarketOrders()
                        .Where(p => p.Comment.StartsWith(cparam.IdentityComment));
                    IEnumerable<Order> allSellOrders = OrderOperation.GetSellMarketOrders()
                        .Where(p => p.Comment.StartsWith(cparam.IdentityComment));

                    if (allBuyOrders.Any() && ExtremumPoints[i].FlagUP)
                    {
                        Meta.Print("Closing all buy orders");
                        CloseOrders(allBuyOrders);
                    }
                    if (allSellOrders.Any() && !ExtremumPoints[i].FlagUP)
                    {
                        Meta.Print("Closing all sell orders");
                        CloseOrders(allSellOrders);
                    }

                    if (!ExtremumPoints[i].FlagUP)
                    {
                        int shift = (ExtremumPoints[i].Max - currSymbolTick.Bid);
                        AddComment(() => "Current Shift = " + shift);
                        int summOpenVolume = allBuyOrders.Sum(p => p.Volume);
                        int step = CalculateCurrentStep(cparam, shift, 0);
                        int reqVolume = GetVolumeByStep(cparam, step);
                        AddComment(() => "   Required Volume = " + reqVolume + "\n\n");

                        if (IsMaximumExceeded(cparam, shift))
                            CloseOrders(allBuyOrders);
                        else if (step == 1 || (step > 1 && allBuyOrders.Any()))
                        {
                                TryToOpenCorrectVolume(OrderSide.Buy, reqVolume - summOpenVolume, cparam);
                        }
                    }//FLagUP
                    else
                    {
                        int shift = (currSymbolTick.Bid - ExtremumPoints[i].Min);
                        AddComment(() => "Current Shift = " + shift.ToString());
                        int summOpenVolume = allSellOrders.Sum(p => p.Volume);
                        int step = CalculateCurrentStep(cparam, shift, 0);
                        int reqVolume = GetVolumeByStep(cparam, step);
                        AddComment(delegate { return "   Required Volume = " + reqVolume.ToString() + "\n\n"; });

                        if (IsMaximumExceeded(cparam, shift))
                            CloseOrders(allSellOrders);
                        else if (step == 1 || (step > 1 && allSellOrders.Any()))
                        {
                                TryToOpenCorrectVolume(OrderSide.Sell, reqVolume - summOpenVolume, cparam);
                        }

                    }
                }
            }
            catch (HistoryNotAvailableExceptions exc)
            {
                Meta.Print("HistoryNotAvailableExceptions exc");
                return;
            }
            finally
            {
                ShowComment();
            }
        }

        void CloseOrders(IEnumerable<Order> orders)
        {
            foreach (Order currOrder in orders.ToList())
                OrderOperation.CloseOrder(currOrder);
        }
        int CalculateCurrentStep(MartinGaleParam cparam, int currShift, int startShift)
        {
            if (startShift < cparam.StartShift)
                startShift = cparam.StartShift;

            if (currShift < startShift)
                return 0;
            return 1 + (currShift - startShift) * 2 / cparam.TP;
        }
        int GetVolumeByStep(MartinGaleParam cparam, int step)
        {
            return (int)(cparam.BasicVolume *
                (Math.Pow(cparam.CoefVolume, step) - 1));
        }

        bool IsMaximumExceeded(MartinGaleParam cparam, int currShift)
        {
            return currShift >= cparam.StartShift + cparam.MaxSteps * cparam.TP / 2 ;
        }
        void TryToOpenCorrectVolume(OrderSide side, int requiredVolume, MartinGaleParam cparam)
        {
            if (requiredVolume <= 0)
                return;

            this.Print(string.Format("TryToOpenCorrectVolume side={0}, requiredVolume={1}",
                side, requiredVolume));

            //OrderSide openingSide =  requiredVolume > 0 ? side : (side==OrderSide.Buy?OrderSide.Sell:OrderSide.Buy);
            //int openingVolume = Math.Abs( requiredVolume);

            Meta.OrderSend(cparam.Symbol, OrderType.Market, side, requiredVolume, 1, 0, 0, cparam.NewUniqueComment());
        }
        void Print(string message)
        {
            Meta.Print(message);
        }

 
        public override void onEnd()
        {
            this.logger.AddMessage("The end");
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
        return !collection.Any();
    }
}

