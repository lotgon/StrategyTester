using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace EngineTest
{
    internal class OrdersExecutor
    {
        internal class IntReverseComparer : Comparer<int>
        {
            public override int Compare(int x, int y)
            {
                return y.CompareTo(x);
            }
        }
        List<Order> marketBuyOrders = new List<Order>();
        List<Order> marketSellOrders = new List<Order>();
        List<Order> limitBuyOrders = new List<Order>();
        List<Order> limitSellOrders = new List<Order>();

        internal SortedList<int, List<Order>> BuyLimits = new SortedList<int, List<Order>>(new IntReverseComparer());
        internal SortedList<int, List<Order>> BuyStops = new SortedList<int, List<Order>>();
        internal SortedList<int, List<Order>> SellLimits = new SortedList<int, List<Order>>();
        internal SortedList<int, List<Order>> SellStops = new SortedList<int, List<Order>>(new IntReverseComparer());
        Dictionary<int, Order> dictIdToOrder = new Dictionary<int, Order>();

        public IEnumerable<Order> MarketBuyOrders
        {
            get
            {
                return marketBuyOrders;
            }
        }
        public IEnumerable<Order> MarketSellOrders
        {
            get
            {
                return marketSellOrders;
            }
        }
        public IEnumerable<Order> LimitBuyOrders
        {
            get
            {
                return limitBuyOrders;
            }
        }
        public IEnumerable<Order> LimitSellOrders
        {
            get
            {
                return limitSellOrders;
            }
        }

        public void Proccess(GroupTick groupTick)
        {
            while (BuyLimits.Count > 0 && groupTick.MinAsk <= BuyLimits.Keys[0])
            {
                BuyLimits.Values[0].RemoveAll(p => p == null);
                if (BuyLimits.Values[0].Count == 0)
                {
                    BuyLimits.RemoveAt(0);
                    continue;
                }
                Order[] orders = BuyLimits.Values[0].ToArray();
                Array.ForEach(orders, p => Activate(p, groupTick));
            }
            while (SellLimits.Count > 0 && groupTick.MaxBid >= SellLimits.Keys[0])
            {
                SellLimits.Values[0].RemoveAll(p => p == null);
                if (SellLimits.Values[0].Count == 0)
                {
                    SellLimits.RemoveAt(0);
                    continue;
                }
                Order[] orders = SellLimits.Values[0].ToArray();
                Array.ForEach(orders, p => Activate(p, groupTick));
            }
            while (BuyStops.Count > 0 && groupTick.MaxAsk >= BuyStops.Keys[0])
            {
                BuyStops.Values[0].RemoveAll(p => p == null);
                if (BuyStops.Values[0].Count == 0)
                {
                    BuyStops.RemoveAt(0);
                    continue;
                }
                Order[] orders = BuyStops.Values[0].ToArray();
                Array.ForEach(orders, p => Activate(p, groupTick));
            }
            while (SellStops.Count > 0 && groupTick.MinBid >= SellStops.Keys[0])
            {
                SellStops.Values[0].RemoveAll(p => p == null);
                if (SellStops.Values[0].Count == 0)
                {
                    SellStops.RemoveAt(0);
                    continue;
                }
                Order[] orders = SellStops.Values[0].ToArray();
                Array.ForEach(orders, p => Activate(p, groupTick));
            }
        }

        public void AddOrder(Order order)
        {
            if (order.Type == OrderType.Limit)
            {
                if (order.Side == OrderSide.Buy)
                {
                    dictIdToOrder.Add(order.ID, order);
                    limitBuyOrders.Add(order);
                    
                    if( !BuyLimits.ContainsKey( order.OpenPrice ) )
                        BuyLimits[order.OpenPrice] = new List<Order>();
                    BuyLimits[order.OpenPrice].Add(order);
                }
                else
                {
                    dictIdToOrder.Add(order.ID, order);
                    limitSellOrders.Add(order);

                    if (!SellLimits.ContainsKey(order.OpenPrice))
                        SellLimits[order.OpenPrice] = new List<Order>();
                    SellLimits[order.OpenPrice].Add(order);
                }
            }
            else
            {
                if (order.Side == OrderSide.Buy)
                {
                    dictIdToOrder.Add(order.ID, order);
                    marketBuyOrders.Add(order);
                    if (order.TP > 0)
                    {
                        if (!SellLimits.ContainsKey(order.TP))
                            SellLimits[order.TP] = new List<Order>();
                        SellLimits[order.TP].Add(order);
                    }
                    if (order.SL > 0)
                    {
                        if (!SellStops.ContainsKey(order.SL))
                            SellStops[order.SL] = new List<Order>();
                        SellStops[order.SL].Add(order);
                    }
                }
                else
                {
                    dictIdToOrder.Add(order.ID, order);
                    marketSellOrders.Add(order);
                    if (order.TP > 0)
                    {
                        if (!BuyLimits.ContainsKey(order.TP))
                            BuyLimits[order.TP] = new List<Order>();
                        BuyLimits[order.TP].Add(order);
                    }
                    if (order.SL > 0)
                    {
                        if (!BuyStops.ContainsKey(order.SL))
                            BuyStops[order.SL] = new List<Order>();
                        BuyStops[order.SL].Add(order);
                    }
                }
            }
        }
        public void RemoveLimitOrder(Order order)
        {
            if (order.Side == OrderSide.Buy)
            {
                if (!limitBuyOrders.Remove(order))
                    throw new NoOrdersinCollection();
                BuyLimits[order.OpenPrice].Remove(order);
                if (BuyLimits[order.OpenPrice].Count == 0)
                    BuyLimits.Remove(order.OpenPrice);
            }
            else
            {
                if (!limitSellOrders.Remove(order))
                    throw new NoOrdersinCollection();
                SellLimits[order.OpenPrice].Remove(order);
                if (SellLimits[order.OpenPrice].Count == 0)
                    SellLimits.Remove(order.OpenPrice);
            }

            dictIdToOrder.Remove(order.ID);
        }
        public void RemoveMarketOrder(Order order)
        {
            if (order.Side == OrderSide.Buy)
            {
                if (!marketBuyOrders.Remove(order))
                    throw new NoOrdersinCollection();
                if (order.TP > 0)
                {
                    SellLimits[order.TP].Remove(order);
                    if (SellLimits[order.TP].Count == 0)
                        SellLimits.Remove(order.TP);
                }
                if (order.SL > 0)
                {
                    SellStops[order.SL].Remove(order);
                    if (SellStops[order.SL].Count == 0)
                        SellStops.Remove(order.SL);
                }
            }
            else
            {
                if (!marketSellOrders.Remove(order))
                    throw new NoOrdersinCollection();
                if (order.TP > 0)
                {
                    BuyLimits[order.TP].Remove(order);
                    if (BuyLimits[order.TP].Count == 0)
                        BuyLimits.Remove(order.TP);
                }
                if (order.SL > 0)
                {
                    BuyStops[order.SL].Remove(order);
                    if (BuyStops[order.SL].Count == 0)
                        BuyStops.Remove(order.SL);
                }
            }

            dictIdToOrder.Remove(order.ID);
        }
        public void ModifyMarketOrder(Order order, int SL, int TP)
        {
            RemoveMarketOrder(order);
            if (TP > 0)
                order.TP = TP;
            if (SL > 0)
                order.SL = 0;
            AddOrder(order);
        }
        public void ModifyLimitOrder(Order order, int OpenPrice, int SL, int TP)
        {
            RemoveLimitOrder(order);
            if (OpenPrice > 0)
                order.OpenPrice = OpenPrice;
            if (TP > 0)
                order.TP = TP;
            if (SL > 0)
                order.SL = SL;
            AddOrder(order);
            return;
        }
        public Order GetOrderByID(int orderID)
        {
            Order order = null;
            dictIdToOrder.TryGetValue(orderID, out order);
            return order;
        }

        public void RegisterBuyLimit(int price)
        {
            if (!BuyLimits.ContainsKey(price))
                BuyLimits[price] = new List<Order>();
            BuyLimits[price].Add(null);  
        }
        public void RegisterBuyStop(int price)
        {
            if (!BuyStops.ContainsKey(price))
                BuyStops[price] = new List<Order>();
            BuyStops[price].Add(null); 
        }
        public void RegisterSellLimit(int price)
        {
            if (!SellLimits.ContainsKey(price))
                SellLimits[price] = new List<Order>();
            SellLimits[price].Add(null); 
        }
        public void RegisterSellStop(int price)
        {
            if (!SellStops.ContainsKey(price))
                SellStops[price] = new List<Order>();
            SellStops[price].Add(null);
        }

        private void Activate(Order order, GroupTick groupTick)
        {
            if (order.Type == OrderType.Limit)
            {
                RemoveLimitOrder(order);
                OnActivatingLimitOrder(order);
                return;
            }
            if (order.Side == OrderSide.Buy && order.TP <= groupTick.MaxBid && order.TP != 0 ||
               order.Side == OrderSide.Sell && order.TP >= groupTick.MinAsk)
            {
                RemoveMarketOrder(order);
                onActivatingMarketTPOrder(order);
                return;
            }
            if (order.Side == OrderSide.Buy && order.SL >= groupTick.MinBid ||
                order.Side == OrderSide.Sell && order.SL <= groupTick.MaxAsk && order.SL != 0)
            {
                RemoveMarketOrder(order);
                onActivatingMarketSLOrder(order);
                return;
            }
        }
        #region events
        public class OrderEventArgs : EventArgs
        {
            public Order Order { get; private set; }
            public OrderEventArgs(Order order) { this.Order = order; }
        }
        public event EventHandler<OrderEventArgs> ActivatingLimitOrder;
        public event EventHandler<OrderEventArgs> ActivatingMarketTPOrder;
        public event EventHandler<OrderEventArgs> ActivatingMarketSLOrder;

        protected void OnActivatingLimitOrder(Order order)
        {
            if (ActivatingLimitOrder != null)
                ActivatingLimitOrder(this, new OrderEventArgs(order));
        }
        protected void onActivatingMarketTPOrder(Order order)
        {
            if (ActivatingLimitOrder != null)
                ActivatingMarketTPOrder(this, new OrderEventArgs(order));
        }
        protected void onActivatingMarketSLOrder(Order order)
        {
            if (ActivatingLimitOrder != null)
                ActivatingMarketSLOrder(this, new OrderEventArgs(order));
        }
        #endregion

    }
}
