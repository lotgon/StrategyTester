using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Common;

namespace EngineTest
{
    public class Engine : IOrderOperation, IAccount, IHistory, IMeta, IRegisterTickToHandle   
    {
        Dictionary<int, Order> historyOrders = new Dictionary<int, Order>();
        List<Order> marketBuyOrders = new List<Order>();
        List<Order> marketSellOrders = new List<Order>();
        List<Order> limitBuyOrders = new List<Order>();
        List<Order> limitSellOrders = new List<Order>();
        const int FreezePips = 9;
        const int FREEZETPSL = 9;
        int orderID = 1;
        Account TestAccount;
        GroupTick currentTick;
        InputData inputData;
        FxAdvisorCore.SimpleAdvisor strategy;
        Log4Smart.IOrderLogger orderLogger;
        Log4Smart.IBalanceLogger balanceLogger;
        OrderLogHelper orderLogHelper;
        bool isTestSuccessfull = false;

        public Engine(Log4Smart.IOrderLogger logger, Log4Smart.IBalanceLogger balanceLogger)
        {
            this.orderLogger = logger;
            this.balanceLogger = balanceLogger;
            this.OrderWasActivated = true;

            orderLogHelper = new OrderLogHelper(logger);
        }

        public void StartTest(InputData inputData, FxAdvisorCore.SimpleAdvisor strategy, Account Account)
        {
            this.inputData = inputData;
            this.strategy = strategy;
            this.TestAccount = Account;

            //System.Threading.Thread t = new Thread(new ThreadStart(Run));
            //t.Start();
            StartTest(inputData, strategy, Account, true);
        }
        public void StartTest(InputData inputData, FxAdvisorCore.SimpleAdvisor strategy, Account Account, bool isCloseAtTheEnd)
        {
            this.inputData = inputData;
            this.strategy = strategy;
            this.TestAccount = Account;

            //System.Threading.Thread t = new Thread(new ThreadStart(Run));
            //t.Start();
            Run(isCloseAtTheEnd);
        }
        public bool IsTestSuccessfull
        {
            get { return isTestSuccessfull; }
            set { isTestSuccessfull = value; }
        }

        #region Events
        public event EventHandler<EventArgs> onEndStrategy;
        public class OrderEventArgs : EventArgs
        {
            public Order Order {get;private set;}
            public OrderEventArgs(Order order) { this.Order = order; }
        }
        public event EventHandler<OrderEventArgs> ActivatingOrder;

        protected void OnActivatingOrder(Order order)
        {
            if (ActivatingOrder != null)
                ActivatingOrder(this, new OrderEventArgs(order.Clone() as Order));
        }

        #endregion

        #region IOrderOperation Members
        public Order AddOrder(Order oldorder)
        {
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
                if (order.Side == OrderSide.Buy)
                {
                    if (currentTick.OpenAsk - order.OpenPrice < FreezePips)
                        throw new OpenPricetooCloseExceptions();
                }
                else
                {
                    if (order.OpenPrice - currentTick.OpenBid < FreezePips)
                        throw new OpenPricetooCloseExceptions();
                }
                CheckLimitOrderTPSL(order.OpenPrice, order.SL, order.TP, order.Side);
                if (!IsMarginAvailableForOrder(order))
                    throw new ApplicationException("Margin is not enough to add order.");

                order.ID = GetNextOrderID();
                InternalAddOrder(order);
                orderLogHelper.Orders(order, currentTick, OrderModificationType.Add);
                return order;

            }
        }
        public bool CloseOrder(Order order)
        {
            if (order.Type == OrderType.Market)
            {
                if (order.Side == OrderSide.Buy)
                    CloseMarketOrderByPrice(order, currentTick.OpenBid);
                else
                    CloseMarketOrderByPrice(order, currentTick.OpenAsk);
            }
            else
            {
                if (order.Side == OrderSide.Buy)
                {
                    if (!limitBuyOrders.Remove(order))
                        throw new NoOrdersinCollection();
                    else
                        OrderWasActivated = true;
                }
                else
                {
                    if (!limitSellOrders.Remove(order))
                        throw new NoOrdersinCollection();
                    else
                        OrderWasActivated = true;
                }
                orderLogHelper.Orders(order, currentTick, OrderModificationType.RemoveLimit);
            }
            return true;
        }
        public bool CloseAllOrders()
        {
            foreach( Order currOrder in GetMarketOrders().Union(GetLimitOrders()).ToArray() )
                CloseOrder( currOrder );
            return true;
        }

        //void IOrderOperation.ModifyMarketOrderInPips(int orderID, int SLpips, int TPpips)
        //{
        //    Order order = (this as IOrderOperation).GetOrderByID(orderID);
        //    if (order == null)
        //        throw new ApplicationException(string.Format("Can`t find market order with order id = {0} to modify.", orderID.ToString()));

        //    if (order.Side == OrderSide.Buy)
        //    {
        //        ModifyMarketOrder(order, SLpips > 0 ? currentTick.OpenAsk - SLpips : 0, TPpips > 0 ? currentTick.OpenAsk + TPpips : 0);
        //    }
        //    else
        //    {
        //        ModifyMarketOrder(order, SLpips > 0 ? currentTick.OpenBid + SLpips : 0, TPpips > 0 ? currentTick.OpenBid - TPpips : 0);
        //    }
        //}
        public void ModifyMarketOrder(int orderID, int SL, int TP)
        {
            Order order = GetOrderByID(orderID);
            if (order == null)
                throw new ApplicationException(string.Format("Can`t find market order with order id = {0} to modify.", orderID.ToString()));

            ModifyMarketOrderInternal(order, SL == order.SL ? 0 : SL, TP == order.TP ? 0:TP);

         }
        //void IOrderOperation.ModifyLimitOrderInPips(int orderID, int OpenPrice, int SLpips, int TPpips)
        //{
        //    Order order = (this as IOrderOperation).GetOrderByID(orderID);
        //    if (order == null)
        //        throw new ApplicationException(string.Format("Can`t find limit order with order id = {0} to modify.", orderID.ToString()));

        //    if (order.Side == OrderSide.Buy)
        //    {
        //        ModifyLimitOrder(order, OpenPrice, SLpips > 0 ? OpenPrice - SLpips : 0, TPpips > 0 ? OpenPrice + TPpips : 0);
        //    }
        //    else
        //    {
        //        ModifyLimitOrder(order, OpenPrice, SLpips > 0 ? OpenPrice + SLpips : 0, TPpips > 0 ? OpenPrice - TPpips : 0);
        //    }
        //}

        public void ModifyLimitOrder(int orderID, int OpenPrice, int SL, int TP)
        {
            Order order = GetOrderByID(orderID);
            if (order == null)
                throw new ApplicationException(string.Format("Can`t find limit order with order id = {0} to modify.", orderID.ToString()));
            ModifyLimitOrderInternal(order, OpenPrice == order.OpenedPrice ? 0: OpenPrice, SL == order.SL ? 0 : SL, TP == order.TP ? 0 : TP);
        }
        public Order GetOrderByID(int orderID)
        {
            foreach (Order currOrder in marketBuyOrders)
                if (currOrder.ID == orderID)
                    return currOrder;
            foreach (Order currOrder in marketSellOrders)
                if (currOrder.ID == orderID)
                    return currOrder;
            foreach (Order currOrder in limitBuyOrders)
                if (currOrder.ID == orderID)
                    return currOrder;
            foreach (Order currOrder in limitSellOrders)
                if (currOrder.ID == orderID)
                    return currOrder; 
            return null;
        }
        public IEnumerable<Order> GetMarketOrders()
        {
            return marketBuyOrders.Union(marketSellOrders).ToArray();
        }
        public IEnumerable<Order> GetMarketOrders(OrderSide side)
        {
            if (side == OrderSide.Buy)
                return marketBuyOrders.ToArray();
            else
                return marketSellOrders.ToArray();
        }
        public IEnumerable<Order> GetLimitOrders()
        {
            return limitBuyOrders.Union(limitSellOrders).ToArray();
        }
        public IEnumerable<Order> GetLimitOrders(OrderSide side)
        {
            if (side == OrderSide.Buy)
                return limitBuyOrders.ToArray();
            else
                return limitSellOrders.ToArray();
        }
        public IEnumerable<Order> GetBuyMarketOrders()
        {
            return marketBuyOrders;
        }
        public IEnumerable<Order> GetSellMarketOrders()
        {
            return marketSellOrders;
        }
        public IEnumerable<Order> GetBuyLimitOrders()
        {
            return limitBuyOrders;
        }
        public IEnumerable<Order> GetSellLimitOrders()
        {
            return limitSellOrders;
        }
        public bool OrderWasActivated { get; set; }
        #endregion
        #region IAccount Members
        public int  GetEquity()
        {
            int balance = TestAccount.Balance;
            foreach (Order order in marketBuyOrders)
                balance += order.EstimateProfit(currentTick.OpenAsk, currentTick.OpenBid);
            foreach (Order order in marketSellOrders)
                balance += order.EstimateProfit(currentTick.OpenAsk, currentTick.OpenBid);
            return balance;
        }
        public Account Account
        {
            get
            {
                return TestAccount;
            }
        }
        #endregion
        #region IHistory Members

        public IEnumerable<Order> ClosedOrders
        {
            get { return historyOrders.Values; }
        }
        public Order GetOrderByIDFromHistory(int orderID)
        {
            return historyOrders[orderID];
        }
        public bool Contains(int orderID)
        {
            return historyOrders.ContainsKey(orderID);
        }
        public GroupTick HistoryGroupTick(int stepBehind)
        {
            try
            {
                return inputData.TickHistory.GetRelativeGroupTick(currentTick.DateTime, stepBehind);
            }
            catch
            {
                throw;
            }
        }

        #endregion
        #region IMeta Members

        public int High(string symbol, int lastMinute)
        {
            return HistoryGroupTick(lastMinute).MaxBid;
        }

        public int Low(string symbol, int lastMinute)
        {
            return HistoryGroupTick(lastMinute).MinBid;
        }

        public int Open(string symbol, int lastMinute)
        {
            return HistoryGroupTick(lastMinute).OpenBid;
        }

        public bool OrderModify(int ticket, int price, int sl, int tp, OrderType type)
        {
            if (type == OrderType.Limit)
                ModifyLimitOrder(ticket, price, sl, tp);
            else
                ModifyMarketOrder(ticket, sl, tp);
            return true;
        }

        public int OrderSend(string symbol, OrderType type, OrderSide side, int volume, int open, int sl, int tp)
        {
            return OrderSend(symbol, type, side, volume, open, sl, tp, "");
        }
        public int OrderSend(string symbol, OrderType type, OrderSide side, int volume, int open, int sl, int tp, string comment)
        {
            return AddOrder(new Order(symbol, type, open, side, volume, sl, tp, comment)).ID;
        }
        public bool OrderClose(int ticket, int lots, int price, int slippage)
        {
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

        int dayForTickTack = -1;
        public void TickTack(GroupTick tick)
        {
            if (dayForTickTack != tick.DateTime.Day)
            {
                if (balanceLogger != null)
                    balanceLogger.AddMessage("{0}, {1}", tick.DateTime.ToLongDateString(), GetEquity());
                dayForTickTack = tick.DateTime.Day;

                this.Account.Statistics.AddEquityMarginStatistics(tick.DateTime, GetEquity(), Math.Max(TestAccount.MarginBuy, TestAccount.MarginSell));

            }

            //if (filterMinuteDateTime.Ticks / TickHistory.tickInOneMinute != tick.DateTime.Ticks / TickHistory.tickInOneMinute)
            //{
            //    this.Account.Statistics.AddEquityMarginStatistics(tick.DateTime, GetEquity(), Math.Max(TestAccount.MarginBuy, TestAccount.MarginSell));
            //    filterMinuteDateTime = tick.DateTime;
            //}
            if (tick.DateTime.Hour == 23 && currentTick.DateTime.Hour == 22)
                RunRollover();

            currentTick = tick;
            ActivateMarketOrders(marketBuyOrders);
            ActivateMarketOrders(marketSellOrders);
            ActivateLimitOrders(limitBuyOrders);
            ActivateLimitOrders(limitSellOrders);

            strategy.TesterTick(tick.Tick);
        }

        private void Run(bool isCloseAtTheEnd)
        {
            try
            {
                if (!strategy.TesterStart(inputData.Data.Count, this, this, this, this, this, inputData.Symbol))
                    return;

                if (inputData.Data.Count > 0)
                    currentTick = inputData.Data[0];
                else
                {
                    isTestSuccessfull = true;
                    return;
                }

                DateTime filterMinuteDateTime = DateTime.MinValue;

                foreach (GroupTick tick in inputData.Data)
                {
                    TickTack(tick);
                }
                if (inputData.Data.Count > 0)
                {
                    this.Account.Statistics.AddEquityMarginStatistics(inputData.Data.Last().DateTime, GetEquity(), Math.Max(TestAccount.MarginBuy, TestAccount.MarginSell));
                }
                if (isCloseAtTheEnd)
                {
                    CloseAllOrders();
                    strategy.TesterEnd();

                    if (onEndStrategy != null)
                        onEndStrategy(this, new EventArgs());
                }

                isTestSuccessfull = true;
            }
            catch (ApplicationException exc)
            {
                if( orderLogger != null )
                    orderLogger.AddMessage(exc.ToString());

            }
            catch (Exception exc)
            {
                if (orderLogger != null)
                    orderLogger.AddMessage(exc.ToString());
            }
        }
        private void ActivateLimitOrders(List<Order> limitOrders)
         {
             for (int i = limitOrders.Count - 1; i >= 0; i--)
             {
                 try
                 {
                     Order currOrder = limitOrders[i];
                     if (currOrder.Side == OrderSide.Buy && currOrder.OpenPrice >= currentTick.MinAsk
                         || currOrder.Side == OrderSide.Sell && currOrder.OpenPrice <= currentTick.MaxBid)
                     {
                         OnActivatingOrder(currOrder);
                         currOrder.ToMarketOrder();
                         AddMarketOrder(currOrder, currOrder.OpenPrice, false, false);
                         limitOrders.RemoveAt(i);
                         orderLogHelper.Orders(currOrder, currentTick, OrderModificationType.ActivateLimit);
                         return;
                     }
                 }
                 catch (ApplicationException exc)
                 {
                     //todo log
                     throw;
                 }
             }
         }
        private int AddMarketOrder(Order marketOrder, int openPrice, bool isCheckTPSL, bool isGeneratedID)
        {
            if (!IsMarginAvailableForOrder(marketOrder))
                throw new ApplicationException("Margin is not enough to add order.");

            if( isCheckTPSL )
                CheckMarketOrderTPSL(marketOrder.Side, marketOrder.SL, marketOrder.TP);

            marketOrder.OpenedPrice = openPrice;
            if (marketOrder.Side == OrderSide.Buy)
                TestAccount.MarginBuy += Account.VolumeMoneyCoef * marketOrder.Volume;
            else
                TestAccount.MarginSell += Account.VolumeMoneyCoef * marketOrder.Volume;

            if( isGeneratedID)
                marketOrder.ID = GetNextOrderID();
            InternalAddOrder(marketOrder);
            if (isGeneratedID)
                orderLogHelper.Orders(marketOrder, currentTick, OrderModificationType.Add);
            return marketOrder.ID;

        }
        private void ActivateMarketOrders(List<Order> marketOrders)
        {
            for (int i = marketOrders.Count - 1; i >= 0; i--)
            {
                try
                {
                    Order currOrder = marketOrders[i];
                    if (currOrder.Side == OrderSide.Buy && currOrder.SL >= currentTick.MinBid ||
                        currOrder.Side == OrderSide.Sell && currOrder.SL <= currentTick.MaxAsk && currOrder.SL!=0)
                    {
                        CloseMarketOrderByPrice(currOrder, currOrder.SL);
                        continue;
                    }

                    if (currOrder.Side == OrderSide.Buy && currOrder.TP <= currentTick.MaxBid && currOrder.TP!=0 ||
                        currOrder.Side == OrderSide.Sell && currOrder.TP >= currentTick.MinAsk)
                    {
                        CloseMarketOrderByPrice(currOrder, currOrder.TP);
                        continue;
                    }
                }
                catch (ApplicationException exc)
                {
                    //todo log
                    throw;
                }
            }
        }
        private void CloseMarketOrderByPrice(Order order, int closePrice)
        {
            if (order.Type != OrderType.Market)
                throw new ApplicationException("CloseMarketOrderByPrice was called for limit order");

            if (order.Side == OrderSide.Buy)
            {
                if (!marketBuyOrders.Remove(order))
                    throw new ApplicationException("Can`t find such order to close");
                else
                    OrderWasActivated = true;
            }
            else
            {
                if (!marketSellOrders.Remove(order))
                    throw new ApplicationException("Can`t find such order to close");
                else
                    OrderWasActivated = true;
            }

            order.ClosedPrice = closePrice;
            TestAccount.Balance += (int)(order.Profit);

            if (order.Side == OrderSide.Buy)
                TestAccount.MarginBuy -= Account.VolumeMoneyCoef * order.Volume;
            else
                TestAccount.MarginSell -= Account.VolumeMoneyCoef * order.Volume;
            
            historyOrders.Add(order.ID, order);
            orderLogHelper.Orders(order, currentTick, OrderModificationType.CloseMarket);
            TestAccount.Statistics.AddClosedOrder(order);
        }
        private void CheckMarketOrderTPSL(OrderSide orderSide, int SL, int TP)
        {
            if (orderSide == OrderSide.Buy)
            {
                if ((TP - FREEZETPSL < currentTick.OpenBid && TP > 0)
                    || (SL + FREEZETPSL > currentTick.OpenBid && SL > 0))
                    throw new SLTPtooCloseExceptions();
            }
            else
            {
                if ((TP + FREEZETPSL > currentTick.OpenAsk && TP > 0)
                    || (SL - FREEZETPSL < currentTick.OpenAsk && SL > 0))
                    throw new SLTPtooCloseExceptions();
            }
        }
        private void CheckLimitOrderTPSL(int openPrice, int SL, int TP, OrderSide side )
        {
            if (side == OrderSide.Buy)
            {
                if ((TP - FREEZETPSL < openPrice && TP > 0)
                    || (SL + FREEZETPSL > openPrice && SL > 0))
                    throw new SLTPtooCloseExceptions();
            }
            else
            {
                if ((SL - FREEZETPSL < openPrice && SL > 0)
                    || (TP + FREEZETPSL > openPrice && TP > 0))
                    throw new SLTPtooCloseExceptions();
            }
        }
        private bool IsMarginAvailableForOrder(Order order)
        {
            //if (order.Type == OrderType.Limit)
                
            int currentMargin = order.Side == OrderSide.Buy ? TestAccount.MarginBuy : TestAccount.MarginSell;
            return Account.VolumeMoneyCoef * order.Volume + currentMargin < GetEquity();
        }
        private int  GetNextOrderID()
        {
            return ++orderID;
        }
        private void ModifyMarketOrderInternal(Order order, int SL, int TP)
        {
            CheckMarketOrderTPSL(order.Side, SL, TP);
            if( TP > 0 )
            order.TP = TP;
            if( SL > 0)
            order.SL = SL;
            orderLogHelper.Orders(order, currentTick, OrderModificationType.Modify);
        }
        private void ModifyLimitOrderInternal(Order order, int OpenPrice, int SL, int TP)
        {
            CheckLimitOrderTPSL(OpenPrice, SL, TP, order.Side);
            if( OpenPrice > 0 )
            order.OpenPrice = OpenPrice;
            if( TP > 0 )
            order.TP = TP;
            if( SL > 0)
            order.SL = SL;

            orderLogHelper.Orders(order, currentTick, OrderModificationType.Modify);
            return;
        }
        private void RunRollover()
        {
            foreach (Order currOrder in marketBuyOrders)
            {
                currOrder.AddSwap(inputData.SwapBuy);
                if( orderLogger!= null )
                    orderLogger.AddMessage("New swap value for order {0} is {1}", currOrder.ID, currOrder.Swap);
            }
            foreach (Order currOrder in marketSellOrders)
            {
                currOrder.AddSwap(inputData.SwapSell);
                if (orderLogger != null)
                    orderLogger.AddMessage("New swap value for order {0} is {1}", currOrder.ID, currOrder.Swap);
            }
        }

        private void InternalAddOrder(Order order)
        {
            if (order.Type == OrderType.Limit)
            {
                if (order.Side == OrderSide.Buy)
                    limitBuyOrders.Add(order);
                else
                    limitSellOrders.Add(order);
            }
            else
            {
                if (order.Side == OrderSide.Buy)
                    marketBuyOrders.Add(order);
                else
                    marketSellOrders.Add(order);
            }
            OrderWasActivated = true;
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
            orderLogger.AddMessage(str);
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
            if (type == MarketInfoType.MODE_TIME)
                return FxAdvisorCore.Convertor.DateTimeToSeconds(this.currentTick.Tick.DateTime);
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
            return HistoryGroupTick(shift).CloseBid;
        }
        public int iHigh(string symbol, GraphPeriod timeFrame, int shift)
        {
            return HistoryGroupTick(shift).MaxBid;

        }
        public int iHighest(string symbol, GraphPeriod timeFrame, int type, int count, int start)
        {
            throw new NotImplementedException();
        }
        public int iLow(string symbol, GraphPeriod timeFrame, int shift)
        {
            return HistoryGroupTick(shift).MinBid;
        }
        public int iLowest(string symbol, GraphPeriod timeFrame, int type, int count, int start)
        {
            throw new NotImplementedException();
        }
        public int iOpen(string symbol, GraphPeriod timeFrame, int shift)
        {
            return HistoryGroupTick(shift).OpenBid;
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

        public void RegisterBuyLimit(int price)
        {
        }

        public void RegisterBuyStop(int price)
        {
        }

        public void RegisterSellLimit(int price)
        {
        }

        public void RegisterSellStop(int price)
        {
        }


        public bool OrderCloseBy(int ticket, int ticket2, int Color)
        {
            throw new NotImplementedException();
        }
    }
}
