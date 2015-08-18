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

namespace Average
{
    [RegisterAdvisor(Name = "AverageStrategy")]
    public class AverageStrategy : FxAdvisorCore.SimpleAdvisor
    {
        DateTime currTime = DateTime.MinValue;
        int MaxExtremum = 0;

        ExtremumPointStrategy ExtremumPoint;
        FluctuationCollection fluctuationList;
        News.NewsPeriods newsPeriod;
        SpreadAnalyzer spreadAnalyzer;
        StrategyTime strategyTime;
        
       // List<Fluctuation> FluctuationList = new List<Fluctuation>(numberFluctuation);

        public AverageStrategy() 
        {
        }

        public override bool onStart(int ticks)
        {
            ExtremumPoint = new ExtremumPointStrategy(param["TP"]);
            InitExtremum(param["NumberInitPeriods"]);
            if (ExtremumPoint.Count <= param["NumberEndFluctuation"]) 
            {
                logger.AddMessage("onStart error. Number of extremums in history is too low = " + ExtremumPoint.Count.ToString());
                return false;
            }

            fluctuationList = new FluctuationCollection(param["NumberStartFluctuation"], param["NumberEndFluctuation"]);

            for (int i = fluctuationList.StartIndex; i < fluctuationList.EndIndex; i++)
            {
                Meta.ObjectCreate(GetHLineName(i), MetaObjectType.OBJ_HLINE, 0, DateTime.Now, 148000, FxAdvisorCore.Convertor.startTime, 0, FxAdvisorCore.Convertor.startTime, 0).ToString();
                Meta.ObjectSetText(GetHLineName(i), i.ToString(), 10, "", 0);
            }
            newsPeriod = new NewsPeriods(param.NewsFilePath, 300, 600, base.Symbol, base.TestingMode, (param.Keys.Contains("GMT") ? param["GMT"] : 0), this.logger);
            spreadAnalyzer = new SpreadAnalyzer(100);
            strategyTime = new StrategyTime(param["ProhibitedStartHour"], param["ProhibitedStartMinute"], param["ProhibitedDuration"], (param.Keys.Contains("GMT") ? param["GMT"] : 0), 31);

            return true;
        }
        public string GetHLineName(int i)
        {
            return "HLine" + i;
        }
        public override void onTick(Tick<int> currTick)
        {
            try
            {
                spreadAnalyzer.EvaluateSpread(currTick);
                ShowComment(currTick);

                if (currTime.Ticks / TickHistory.tickInOneMinute == currTick.DateTime.Ticks / TickHistory.tickInOneMinute)
                    return;
                currTime = currTick.DateTime;

                ExtremumPoint.Process(Math.Max(currTick.Bid, Meta.High(this.Symbol, 1)), Math.Min(currTick.Bid, Meta.Low(this.Symbol, 1)), currTick.DateTime);
                fluctuationList.Update(ExtremumPoint);

                for (int i = fluctuationList.StartIndex; i < fluctuationList.EndIndex; i++)
                {
                    Meta.ObjectMove(GetHLineName(i), 0, DateTime.Now, fluctuationList[i].AveragePrice);
                }

                if (!strategyTime.IsSystemON(currTick) || newsPeriod.IsNewsTime(currTick.DateTime))
                {
                    OrderOperation.CloseAllOrders();
                    return;
                }

                //open/modify tp
                AddModifyLimitOrder(fluctuationList.GetHighest(param["OpenOrderPercent"]), param["SL"], fluctuationList.GetHighest(param["TPOrderPercent"]) + spreadAnalyzer.AverageSpread, OrderSide.Sell);
                AddModifyLimitOrder(fluctuationList.GetLowest(param["OpenOrderPercent"]) + spreadAnalyzer.AverageSpread, param["SL"], fluctuationList.GetLowest(param["TPOrderPercent"]), OrderSide.Buy);

                ModifyMarketOrders(OrderSide.Buy, fluctuationList.GetLowest(param["TPOrderPercent"]));
                ModifyMarketOrders(OrderSide.Sell, fluctuationList.GetHighest(param["TPOrderPercent"]) + spreadAnalyzer.AverageSpread);

                    //close limit orders
                    //move tp

                    //                    //check condition to close order
                    //int sign = fluctuationList.GetSignDistanceFromAverageInPercent(currTick.Bid);
                    //if (sign >= 0)
                    //    CloseOrders(OrderSide.Buy, marketOrders);
                    //if (sign <= 0)
                    //    CloseOrders(OrderSide.Sell, marketOrders);


                    //int peakDistanceFromAverageMinAbs = fluctuationList.GetMinDistanceFromAverageInPercent(currTick.Bid);
                    //if (Math.Abs(peakDistanceFromAverageMinAbs) > param["OpenOrderPercent"])
                    //    OpenNewMarketOrder(peakDistanceFromAverageMinAbs < 0 ? OrderSide.Buy : OrderSide.Sell, marketOrders, currTick);

       
            }
            catch (HistoryNotAvailableExceptions exc)
            {
                return;
            }
        }
        void AddModifyLimitOrder(int open, int slPIPS, int tp, OrderSide side)
        {
            open = MathPrice.RoundToDown(open);
            tp = MathPrice.RoundToDown(tp);
            int sl = side == OrderSide.Buy ? open - slPIPS : open + slPIPS;
            
            List<Order> limitOrders = SelectMulti(side, OrderOperation.GetLimitOrders());

            if (limitOrders.Count != 0)
            {
                foreach (Order currOrder in limitOrders)
                    Meta.OrderModify(currOrder.ID, open, sl, tp, currOrder.Type);
            }
            else if (SelectMulti(side, OrderOperation.GetMarketOrders()).Count == 0 )
            {
                try
                {
                    Meta.OrderSend(base.Symbol, OrderType.Limit, side, LotFunction(), open, sl, tp, "");
                }
                catch (Exception exc)
                {
                    logger.AddMessage(exc.ToString());
                }
            }
        }
        void ModifyMarketOrders(OrderSide side, int tp)
        {
            tp = MathPrice.RoundToDown(tp);

            List<Order> marketOrders = SelectMulti(side, OrderOperation.GetMarketOrders());
            if (marketOrders.Count != 0)
            {
                try
                {
                    foreach (Order currOrder in marketOrders)
                        Meta.OrderModify(currOrder.ID, currOrder.OpenPrice, currOrder.SL, tp, currOrder.Type);
                }
                catch (Exception exc)
                {
                    logger.AddMessage(exc.ToString());
                }
            }
        }
        List<Order> SelectMulti(OrderSide side, IEnumerable<Order> orders)
        {
            List<Order> retCollection = new List<Order>();
            foreach (Order order in orders)
            {
                if (order.Side == side)
                    retCollection.Add(order);
            }
            return retCollection;
        }
        int LotFunction()
        {
            return param["BasicVolume"];
        }
        //private void CloseOrders(OrderSide orderSide, IEnumerable<Order> marketOrders)
        //{
        //    foreach (Order currOrder in marketOrders)
        //    {
        //        if (orderSide == currOrder.Side)
        //            OrderOperation.CloseOrder(currOrder);
        //    }
        //}

        //private void OpenNewMarketOrder(OrderSide orderSide, IEnumerable<Order> marketOrders, Tick<int> currTick)
        //{
        //    int countOrdersWithSameSide = (from c in marketOrders
        //            where c.Side == orderSide
        //            select c).Count();
        //    if (countOrdersWithSameSide > 0)
        //        return;
        //    Meta.OrderSend(Symbol, OrderType.Market, orderSide, param["BasicVolume"], orderSide == OrderSide.Buy ? currTick.Ask : currTick.Bid, 0, 0);
        //}

        internal void InitExtremum(int AmountBars)
        {
            ExtremumPoint.Min = Math.Max(Meta.Open(this.Symbol, AmountBars), Meta.High(this.Symbol, AmountBars + 1)); ;
            ExtremumPoint.Max = Math.Min(Meta.Open(this.Symbol, AmountBars), Meta.Low(this.Symbol, AmountBars + 1));

            for (int i = AmountBars; i >= 1; i--)
            {
                ExtremumPoint.Process(Math.Max(Meta.Open(this.Symbol, i), Meta.High(this.Symbol, i + 1)), Math.Min(Meta.Open(this.Symbol, i), Meta.Low(this.Symbol, i + 1)), DateTime.MaxValue);
            }

        }
        private void ShowComment(Tick<int> currTick)
        {
            if (base.TestingMode)
                return;

            StringBuilder strBuilder = new StringBuilder();
            strBuilder.AppendLine(param.ToString());
            strBuilder.AppendLine("\nExtremum 1");
            strBuilder.Append("Min1 = " + ExtremumPoint.Min);
            strBuilder.Append("\n Max1 = " + ExtremumPoint.Max);
            strBuilder.Append("\n TralOPEN1 = " + ExtremumPoint.TralOPEN);
            strBuilder.Append("\n FlagUP1 = " + ExtremumPoint.FlagUP);

            for (int i = fluctuationList.StartIndex; i < fluctuationList.EndIndex; i++)
            {
                strBuilder.AppendFormat("\n Line{0} = {1}%", i, fluctuationList[i].GetDistanceFromAverageInPercent(currTick.Bid));
            }

            Meta.Comment(strBuilder.ToString());

        }
        public override void onEnd()
        {
            this.logger.AddMessage("MaxExtremum = {0}", MaxExtremum);
        }
    }
}
