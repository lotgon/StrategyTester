using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common;

namespace EngineTest
{
    public class EngineFast : IOrderOperation, IAccount, IHistory, IMeta, IRegisterTickToHandle
    {
        //Dictionary<int, Order> historyOrders = new Dictionary<int, Order>();
        List<int> historyOpenBuyPoints = new List<int>();
        List<int> historyOpenSellPoints = new List<int>();
        OrdersExecutor ordersCollection = new OrdersExecutor();

        int orderID = 1;
        GroupTick currentTick;
        int currentTickIndex;
        InputData inputData;
        FxAdvisorCore.SimpleAdvisor strategy;
        FuturePredictor fPredictor;
        bool IsBlankTick = true;
        int ReadOnlyPoint = Int32.MaxValue;

        public EngineFast()
        {
        }
        public void StartTest(InputData inputData, FxAdvisorCore.SimpleAdvisor strategy, FuturePredictor fPred, bool debugMode, int notReadOnlyTimePercent)
        {
            this.inputData = inputData;
            this.strategy = strategy;
            this.fPredictor = fPred;

            if (notReadOnlyTimePercent < 0 || notReadOnlyTimePercent > 100)
                throw new ArgumentException("notReadOnlyTimePercent should be from 0 to 100 instead of " + notReadOnlyTimePercent);
            this.ReadOnlyPoint = (notReadOnlyTimePercent * this.inputData.Data.Count) / 100;

            this.fPredictor.Orders = ordersCollection;
            Run(debugMode);
            this.fPredictor.Orders = null;
        }
        #region IOrderOperation Members
        public Order AddOrder(Order oldorder)
        {
            IsBlankTick = false;

            Order order = new Order(oldorder.Symbol, oldorder.Type, oldorder.OpenPrice, oldorder.Side, oldorder.Volume, oldorder.SL, oldorder.TP, oldorder.Comment);
            if (order.Type == OrderType.Market)
            {
                if (order.Side == OrderSide.Buy)
                    AddMarketOrder(order, currentTick.OpenAsk, true, true);
                else
                    AddMarketOrder(order, currentTick.OpenBid, true, true);

                return order;
            }
            else
            {
                order.ID = GetNextOrderID();
                ordersCollection.AddOrder(order);
                return order;
            }
        }
        public bool CloseOrder(Order order)
        {
            IsBlankTick = false;

            if (order.Type == OrderType.Market)
            {
                if (order.Side == OrderSide.Buy)
                    CloseMarketOrderByPrice(order, currentTick.OpenBid);
                else
                    CloseMarketOrderByPrice(order, currentTick.OpenAsk);
            }
            else
            {
                ordersCollection.RemoveLimitOrder(order);

            }
            return true;
        }
        public bool CloseAllOrders()
        {
            IsBlankTick = false;
            var all = GetBuyMarketOrders().Union(GetSellMarketOrders()).Union(GetBuyLimitOrders()).Union(GetSellLimitOrders());
            foreach (Order currOrder in all.ToArray())
                CloseOrder(currOrder);
            return true;
        }
        public void ModifyMarketOrder(int orderID, int SL, int TP)
        {
            IsBlankTick = false;

            Order order = GetOrderByID(orderID);
            if (order == null)
                throw new ApplicationException(string.Format("Can`t find market order with order id = {0} to modify.", orderID.ToString()));

             ordersCollection.ModifyMarketOrder(order, SL == order.SL ? 0 : SL, TP == order.TP ? 0 : TP);
        }
        public void ModifyLimitOrder(int orderID, int OpenPrice, int SL, int TP)
        {
            IsBlankTick = false;
            Order order = GetOrderByID(orderID);
            if (order == null)
                throw new ApplicationException(string.Format("Can`t find limit order with order id = {0} to modify.", orderID.ToString()));
            ordersCollection.ModifyLimitOrder(order, OpenPrice == order.OpenedPrice ? 0 : OpenPrice, SL == order.SL ? 0 : SL, TP == order.TP ? 0 : TP);
        }
        public Order GetOrderByID(int orderID)
        {
            return ordersCollection.GetOrderByID(orderID);
        }
        public IEnumerable<Order> GetMarketOrders(OrderSide side)
        {
            if (side == OrderSide.Buy)
                return ordersCollection.MarketBuyOrders;
            else
                return ordersCollection.MarketSellOrders;
        }
        public IEnumerable<Order> GetLimitOrders(OrderSide side)
        {
            if (side == OrderSide.Buy)
                return ordersCollection.LimitBuyOrders;
            else
                return ordersCollection.LimitSellOrders;
        }
        public IEnumerable<Order> GetBuyMarketOrders()
        {
            return ordersCollection.MarketBuyOrders;
        }
        public IEnumerable<Order> GetSellMarketOrders()
        {
            return ordersCollection.MarketSellOrders;
        }
        public IEnumerable<Order> GetBuyLimitOrders()
        {
            return ordersCollection.LimitBuyOrders;
        }
        public IEnumerable<Order> GetSellLimitOrders()
        {
            return ordersCollection.LimitSellOrders;
        }
        #endregion
        #region IMeta Members

        public bool OrderModify(int ticket, int price, int sl, int tp, OrderType type)
        {
            IsBlankTick = false;
            if (type == OrderType.Limit)
                ModifyLimitOrder(ticket, price, sl, tp);
            else
                ModifyMarketOrder(ticket, sl, tp);
            return true;
        }

        public int OrderSend(string symbol, OrderType type, OrderSide side, int volume, int open, int sl, int tp)
        {
            IsBlankTick = false;
            return OrderSend(symbol, type, side, volume, open, sl, tp, "");
        }
        public int OrderSend(string symbol, OrderType type, OrderSide side, int volume, int open, int sl, int tp, string comment)
        {
            IsBlankTick = false;
            return AddOrder(new Order(symbol, type, open, side, volume, sl, tp, comment)).ID;
        }
        public bool OrderClose(int ticket, int lots, int price, int slippage)
        {
            IsBlankTick = false;
            Order order = GetOrderByID(ticket);
            if (order == null)
                throw new ApplicationException(string.Format("Can`t find market order with order id = {0} to close.", ticket.ToString()));

            return CloseOrder(order);
        }
        public string Symbol()
        {
            return inputData.Symbol;
        }
        public bool OrderDelete(int ticket)
        {
            IsBlankTick = false;

            Order order = GetOrderByID(ticket);
            if (order == null)
                throw new ApplicationException(string.Format("Can`t find limit order with order id = {0} to Delete.", ticket.ToString()));
            return CloseOrder(order);
        }
        #region IMeta Members


        public void Comment(string str)
        {
        }

        #endregion

        #endregion
        public IEnumerable<int> HistoryOpenBuyPoints
        {
            get
            {
                return historyOpenBuyPoints;
            }
        }
        public IEnumerable<int> HistoryOpenSellPoints
        {
            get
            {
                return historyOpenSellPoints;
            }
        }

        private void Run(bool debugMode)
        {
            if (inputData.Data.Count > 0)
                currentTick = inputData.Data[0];

            ordersCollection.ActivatingLimitOrder += new EventHandler<OrdersExecutor.OrderEventArgs>(ordersCollection_ActivatingLimitOrder);
            ordersCollection.ActivatingMarketSLOrder += new EventHandler<OrdersExecutor.OrderEventArgs>(ordersCollection_ActivatingMarketSLOrder);
            ordersCollection.ActivatingMarketTPOrder += new EventHandler<OrdersExecutor.OrderEventArgs>(ordersCollection_ActivatingMarketTPOrder);

            if (!strategy.TesterStart(inputData.Data.Count, this, this, this, this, this, inputData.Symbol))
                return;

            int nextInterrupt = 0;
            bool isReadOnlyActivated = false;
            for( currentTickIndex=0;currentTickIndex<inputData.Data.Count;)
            {
                if (!isReadOnlyActivated && currentTickIndex >= this.ReadOnlyPoint)
                {
                    isReadOnlyActivated = true;
                    strategy.SetReadOnly(true);
                }

                IsBlankTick = true;

                currentTick = inputData.Data[currentTickIndex];
                ordersCollection.Proccess(currentTick);
                strategy.TesterTick(currentTick.Tick);
                if (IsBlankTick == false && nextInterrupt != currentTickIndex)
                    throw new ApplicationException("Wrong fast logic");

                nextInterrupt = fPredictor.CalculateNextInterruption(currentTickIndex);
                if (debugMode)
                    currentTickIndex++;
                else
                    currentTickIndex = nextInterrupt;
            }
            CloseAllOrders();
            strategy.TesterEnd();
        }

        void ordersCollection_ActivatingMarketTPOrder(object sender, OrdersExecutor.OrderEventArgs e)
        {
            IsBlankTick = false;
            CloseMarketOrderByPrice(e.Order, e.Order.TP);
        }
        void ordersCollection_ActivatingMarketSLOrder(object sender, OrdersExecutor.OrderEventArgs e)
        {
            IsBlankTick = false;
            CloseMarketOrderByPrice(e.Order, e.Order.SL);
        }
        void ordersCollection_ActivatingLimitOrder(object sender, OrdersExecutor.OrderEventArgs e)
        {
            IsBlankTick = false;
            e.Order.ToMarketOrder();
            AddMarketOrder(e.Order, e.Order.OpenPrice, false, false);
        }

        private int AddMarketOrder(Order marketOrder, int openPrice, bool isCheckTPSL, bool isGeneratedID)
        {
            marketOrder.OpenedPrice = openPrice;
            if (isGeneratedID)
                marketOrder.ID = GetNextOrderID();
            ordersCollection.AddOrder(marketOrder);
            if( marketOrder.Side == OrderSide.Buy)
                historyOpenBuyPoints.Add(currentTickIndex);
            else
                historyOpenSellPoints.Add(currentTickIndex);
            return marketOrder.ID;

        }
        private void CloseMarketOrderByPrice(Order order, int closePrice)
        {
            order.ClosedPrice = closePrice;
            //historyOrders.Add(order.ID, order);
        }
        private int GetNextOrderID()
        {
            return ++orderID;
        }

    


        #region IMeta Members


        public bool OrderSelect(int index, SelectType select, SelectModeType pool)
        {
            throw new NotImplementedException();
        }

        public int OrdersTotal()
        {
            throw new NotImplementedException();
        }

        public int GetOrderTicket()
        {
            throw new NotImplementedException();
        }

        public int GetOrderClosePrice(string symbol)
        {
            throw new NotImplementedException();
        }

        public DateTime GetOrderCloseTime()
        {
            throw new NotImplementedException();
        }

        public string GetOrderComment()
        {
            throw new NotImplementedException();
        }

        public int GetOrderCommission()
        {
            throw new NotImplementedException();
        }

        public DateTime GetOrderExpiration()
        {
            throw new NotImplementedException();
        }

        public int GetOrderLots()
        {
            throw new NotImplementedException();
        }

        public int GetOrderMagicNumber()
        {
            throw new NotImplementedException();
        }

        public int GetOrderOpenPrice(string symbol)
        {
            throw new NotImplementedException();
        }

        public DateTime GetOrderOpenTime()
        {
            throw new NotImplementedException();
        }

        public int GetOrderStopLoss(string symbol)
        {
            throw new NotImplementedException();
        }

        public int GetOrderSwap()
        {
            throw new NotImplementedException();
        }

        public string GetOrderSymbol()
        {
            throw new NotImplementedException();
        }

        public int GetOrderTakeProfit(string symbol)
        {
            throw new NotImplementedException();
        }

        public MetaOperationType GetOrderType()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IMeta Members


        public void Print(string str)
        {
        }

        public bool IsTradeContextBusy()
        {
            return false;
        }

        public MetaLastErrors GetLastError()
        {
            return 0;
        }

        #endregion

        #region IMeta Graph Members


        public bool ObjectCreate(string name, MetaObjectType type, int window, DateTime time1, int price1, DateTime time2, int price2, DateTime time3, int price3)
        {
            return false;
        }

        public bool ObjectDelete(string name)
        {
            return false;
        }

        public string ObjectDescription(string name)
        {
            return "";
        }

        public int ObjectFind(string name)
        {
            return 0;
        }

        public double ObjectGet(string name, int index)
        {
            return 0;
        }

        public string ObjectGetFiboDescription(string name, int index)
        {
            return "";
        }

        public int ObjectGetShiftByValue(string name, int value)
        {
            return 0;
        }

        public int ObjectGetValueByShift(string name, int shift)
        {
            return 0;
        }

        public bool ObjectMove(string name, int point, DateTime time1, int price1)
        {
            return false;
        }

        public string ObjectName(int index)
        {
            return "";
        }

        public int ObjectsDeleteAll(int window, MetaObjectType type)
        {
            return 0;
        }

        public bool ObjectSet(string name, int index, int value)
        {
            return false;
        }

        public bool ObjectSetFiboDescription(string name, int index, string text)
        {
            return false;
        }

        public bool ObjectSetText(string name, string text, int font_size, string font, int text_color)
        {
            return false;
        }

        public int ObjectsTotal(MetaObjectType type)
        {
            return 0;
        }

        public MetaObjectType ObjectType(string name)
        {
            return MetaObjectType.EMPTY;
        }

        #endregion

        #region IMeta Account Members


        public double AccountBalance()
        {
            throw new NotImplementedException();
        }

        public double AccountCredit()
        {
            throw new NotImplementedException();
        }

        public string AccountCompany()
        {
            throw new NotImplementedException();
        }

        public string AccountCurrency()
        {
            throw new NotImplementedException();
        }

        public double AccountEquity()
        {
            throw new NotImplementedException();
        }

        public double AccountFreeMargin()
        {
            throw new NotImplementedException();
        }

        public double AccountFreeMarginCheck(string symbol, OrderType type, OrderSide side, int volume)
        {
            throw new NotImplementedException();
        }

        public double AccountFreeMarginMode()
        {
            throw new NotImplementedException();
        }

        public int AccountLeverage()
        {
            throw new NotImplementedException();
        }

        public double AccountMargin()
        {
            throw new NotImplementedException();
        }

        public string AccountName()
        {
            throw new NotImplementedException();
        }

        public int AccountNumber()
        {
            throw new NotImplementedException();
        }

        public double AccountProfit()
        {
            throw new NotImplementedException();
        }

        public string AccountServer()
        {
            throw new NotImplementedException();
        }

        public int AccountStopoutLevel()
        {
            throw new NotImplementedException();
        }

        public int AccountStopoutMode()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IMeta Members


        public int MarketInfo(string symbol, MarketInfoType type)
        {
            if (type == MarketInfoType.MODE_ASK)
                return this.currentTick.Tick.Ask;
            if (type == MarketInfoType.MODE_BID)
                return this.currentTick.Tick.Bid;
            throw new NotImplementedException();
        }

        public void RefreshRates()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IMeta Bars
        public int iBars(string symbol, GraphPeriod timeFrame)
        {
            throw new NotImplementedException();
        }
        public int iBarShift(string symbol, GraphPeriod timeFrame, DateTime dateTime, bool exact)
        {
            throw new NotImplementedException();
        }
        public int iClose(string symbol, GraphPeriod timeFrame, int shift)
        {
            throw new NotImplementedException();
        }
        public int iHigh(string symbol, GraphPeriod timeFrame, int shift)
        {
            throw new NotImplementedException();
        }
        public int iHighest(string symbol, GraphPeriod timeFrame, int type, int count, int start)
        {
            throw new NotImplementedException();
        }
        public int iLow(string symbol, GraphPeriod timeFrame, int shift)
        {
            throw new NotImplementedException();
        }
        public int iLowest(string symbol, GraphPeriod timeFrame, int type, int count, int start)
        {
            throw new NotImplementedException();
        }
        public int iOpen(string symbol, GraphPeriod timeFrame, int shift)
        {
            throw new NotImplementedException();
        }
        public DateTime iTime(string symbol, GraphPeriod timeFrame, int shift)
        {
            throw new NotImplementedException();
        }
        public double iVolume(string symbol, GraphPeriod timeFrame, int shift)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IAccount Members

        public Account Account
        {
            get { throw new NotImplementedException(); }
        }

        public int GetEquity()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IHistory Members

        public IEnumerable<Order> ClosedOrders
        {
            get { throw new NotImplementedException(); }
        }

        public Order GetOrderByIDFromHistory(int orderID)
        {
            throw new NotImplementedException();
        }

        public bool Contains(int orderID)
        {
            throw new NotImplementedException();
        }

        public GroupTick HistoryGroupTick(int lastMinute)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IMeta Members

        public int High(string symbol, int lastMinute)
        {
            throw new NotImplementedException();
        }

        public int Low(string symbol, int lastMinute)
        {
            throw new NotImplementedException();
        }

        public int Open(string symbol, int lastMinute)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IOrderOperation Members


        public IEnumerable<Order> GetMarketOrders()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Order> GetLimitOrders()
        {
            throw new NotImplementedException();
        }

        public bool OrderWasActivated
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        public void RegisterBuyLimit(int price)
        {
            ordersCollection.RegisterBuyLimit(price);
        }

        public void RegisterBuyStop(int price)
        {
            ordersCollection.RegisterBuyStop(price);
        }

        public void RegisterSellLimit(int price)
        {
            ordersCollection.RegisterSellLimit(price);
        }

        public void RegisterSellStop(int price)
        {
            ordersCollection.RegisterSellStop(price);
        }


        public bool OrderCloseBy(int ticket, int ticket2, int Color)
        {
            throw new NotImplementedException();
        }
    }
}
