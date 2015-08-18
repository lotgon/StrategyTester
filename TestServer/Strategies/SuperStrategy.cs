using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EngineTest;
using Log4Smart;
using Common;

namespace Strategies
{
    public class SuperParametersCollection
    {
        public SuperParameters[] items = new SuperParameters[SuperStrategy.countOrders];
        static public List<SuperParametersCollection> AllParameters
        {
            get
            {
                List<SuperParametersCollection> paramCollections = new List<SuperParametersCollection>();
                RecursiveInit(paramCollections, SuperParameters.AllParameters, SuperStrategy.countOrders-1);
                return paramCollections;
            }
        }

        public SuperParameters this[int key]
        {
            get
            {
                return items[key];
            }
            set
            {
                items[key] = value;
            }
        }
        static private  int[] indices = new int[10];
        static private void RecursiveInit(List<SuperParametersCollection> initList, List<SuperParameters> sourceList, int deep)
        {
            for( int i=0;i<sourceList.Count;i++)
            {
                indices[deep] = i;
                if( deep==0 )
                {
                    SuperParametersCollection collection = new SuperParametersCollection();
                    for( int j=0;j<SuperStrategy.countOrders;j++)
                        collection.items[j] = sourceList[ indices[j] ];
                    initList.Add(collection);                  
                }
                else
                    RecursiveInit(initList, sourceList, deep-1);
            }
        }
        public override string ToString()
        {
            string str = string.Empty;
            for (int j = 0; j < SuperStrategy.countOrders; j++)
                str += items[j].ToString() + Environment.NewLine;
            return str;            
        }

    }

    public class SuperParameters
    {
        public int LimitOpen;
        public int TralLimitOpen;
        public int TralTP;
        public int TP;
        public int SL;
        public int BasicVolume = 10;
        public int StartHour = 0;
        public int EndHour = 24;

        public override string ToString()
        {
            return string.Format("TP={0}_SL={1}_TralTP={2}_LimitOpen={3}_TralLimitOpen={4}", TP, SL, TralTP, LimitOpen, TralLimitOpen);
        }

        static public List<SuperParameters> AllParameters
        {
            get
            {
                List<SuperParameters> paramList = new List<SuperParameters>();


                for (int sl = 500; sl < 1000; sl += 150)
                    for (int tp = 100; tp < 500; tp += 30)
                         for (int limitOpen = tp; limitOpen < sl; limitOpen += 40)
                            //for (int tralLimitOpen = limitOpen; tralLimitOpen < limitOpen + 11; tralLimitOpen += 3)
                            for (int traltp = tp; traltp < limitOpen - 20 && traltp < tp + 200; traltp += 30)
                                //for (int startHour = 0; startHour <= 16; startHour += 8)
                                //    for (int endHour = startHour + 8; endHour <= 24; endHour += 8)
                                          {
                                SuperParameters p = new SuperParameters();
                                p.TP = tp;
                                p.SL = sl;
                                p.BasicVolume = 10;
                                p.TralLimitOpen = limitOpen;// tralLimitOpen;//limitOpen
                                p.TralTP = traltp;
                                p.LimitOpen = limitOpen;
                                //p.StartHour = startHour;
                                //p.EndHour = endHour;
                                paramList.Add(p);

                            }
            
                //SuperParameters p = new SuperParameters();
                //p.TP = 10;
                //p.SL = 49;
                //p.BasicVolume = 30;
                //p.TralLimitOpen = 14;
                //p.TralTP = 10;
                //p.LimitOpen = 14;
                //paramList.Add(p);

                return paramList;
            }
        }
    }
    public class SuperStrategy : Common.Strategy
    {
        public const int countOrders = 1;
        SuperParametersCollection param;
        IStrategyLogger logger;
        public SuperStrategy(SuperParametersCollection param, IStrategyLogger logger)
        {
            this.logger = logger;
            this.param = param;
            this.logger.AddMessage("Start strategy with parameter {0}", param.ToString());
        }

        Order[] marketArray = new Order[countOrders];
        Order[] limitArray = new Order[countOrders];
        Order[] lastMarketArray = new Order[countOrders];

        Order marketOrder = null;
        Order limitOrder = null;
        Order lastMarketOrder = null;

        public override void onTick(Tick<int> tick)
        {
            for (int i = 0; i < countOrders; i++)
            {
                try
                {
                    marketOrder = marketArray[i];
                    limitOrder = limitArray[i];
                    lastMarketOrder = lastMarketArray[i];

                    if (marketOrder != null && marketOrder.IsClosed)
                    {
                        lastMarketOrder = marketOrder;
                        marketOrder = null;
                    }
                    if (limitOrder != null && limitOrder.Type == OrderType.Market)
                    {
                        if (marketOrder != null)
                            throw new ApplicationException("Wroong logiic!");
                        marketOrder = limitOrder;
                        limitOrder = null;
                    }

                    if (marketOrder != null)
                    {
                        if (marketOrder.Side == OrderSide.Buy)
                        {
                            if (marketOrder.TP - tick.Ask > param[i].TralTP)
                                OrderOperation.ModifyMarketOrderInPips(marketOrder.ID, 0, param[i].TralTP);
                        }
                        else
                        {
                            if (tick.Bid - marketOrder.TP > param[i].TralTP)
                                OrderOperation.ModifyMarketOrderInPips(marketOrder.ID, 0, param[i].TralTP);
                        }
                    }
                    if (limitOrder != null)
                    {
                        if (!IsWorkingTime(param[i], tick))
                            OrderOperation.CloseOrder(limitOrder);

                        if (limitOrder.Side == OrderSide.Buy)
                        {
                            if (tick.Ask - limitOrder.OpenPrice > param[i].TralLimitOpen && (marketOrder== null || marketOrder.TP > tick.Ask - param[i].TralLimitOpen))
                                OrderOperation.ModifyLimitOrderInPips(limitOrder.ID, tick.Ask - param[i].TralLimitOpen, param[i].SL, param[i].TP);
                        }
                        else
                        {
                            if (limitOrder.OpenPrice - tick.Bid > param[i].TralLimitOpen && (marketOrder == null || marketOrder.TP < tick.Bid + param[i].TralLimitOpen))
                                OrderOperation.ModifyLimitOrderInPips(limitOrder.ID, tick.Bid + param[i].TralLimitOpen, param[i].SL, param[i].TP);
                        }
                    }
                    else if (IsWorkingTime(param[i], tick))
                    {
                        OrderSide side = OrderSide.Buy;
                        if (lastMarketOrder != null)
                            side = lastMarketOrder.RevertSide;
                        if (marketOrder != null)
                            side = marketOrder.RevertSide;
                        if (marketOrder == null
                            || (marketOrder.Side == OrderSide.Buy && tick.Bid + param[i].LimitOpen > marketOrder.TP)
                                || (marketOrder.Side == OrderSide.Sell && tick.Ask - param[i].LimitOpen < marketOrder.TP))
                        {
                            //if( tick.DateTime.Hour >= 14 && tick.DateTime.Hour <22 )
                                limitOrder = OrderOperation.AddOrder(Order.NewLimitOrder(tick.Ask, tick.Bid, side, GetVolume(param[i]), param[i].LimitOpen, param[i].SL, param[i].TP));
                        }
                    }
                }
                finally
                {
                    marketArray[i] = marketOrder;
                    limitArray[i] = limitOrder;
                    lastMarketArray[i] = lastMarketOrder;
                }
            }
        }
        public bool IsWorkingTime(SuperParameters sparam, Tick<int> tick)
        {
            return tick.DateTime.Hour >= sparam.StartHour && tick.DateTime.Hour < sparam.EndHour;
        }
        public int GetVolume(SuperParameters sparam)
        {
            if( _currentTicks > _maxTicks/2 )
                return (int) (sparam.BasicVolume*1.3);
            return sparam.BasicVolume;
            //if (lastMarketOrder == null)
            //    return sparam.BasicVolume;
            //if (lastMarketOrder.Profit > 0)
            //    return lastMarketOrder.Volume + sparam.BasicVolume;
            //else
            //    return Math.Max(sparam.BasicVolume, lastMarketOrder.Volume - 3 * sparam.BasicVolume);


        }
        public override void onEnd()
        {
            logger.AddMessage(string.Format("Balance = {0}", Account.GetEquity()));
        }
    }

}
