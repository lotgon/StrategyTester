using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public interface IOrderOperation
    {
        Order AddOrder(Order order);
        bool CloseOrder(Order order);
        bool CloseAllOrders();
        //void ModifyMarketOrderInPips(int orderID, int SLpips, int TPpips);
        //void ModifyLimitOrderInPips(int orderID, int OpenPrice, int SLpips, int TPpips);
        void ModifyMarketOrder(int orderID, int SL, int TP);
        void ModifyLimitOrder(int orderID, int OpenPrice, int SL, int TP);

        Order   GetOrderByID(int orderID);
        IEnumerable<Order> GetMarketOrders();
        IEnumerable<Order> GetMarketOrders(OrderSide side);
        IEnumerable<Order> GetLimitOrders();
        IEnumerable<Order> GetLimitOrders(OrderSide side);
        IEnumerable<Order> GetBuyMarketOrders();
        IEnumerable<Order> GetSellMarketOrders();
        IEnumerable<Order> GetBuyLimitOrders();
        IEnumerable<Order> GetSellLimitOrders();

        bool OrderWasActivated { get; set; }
    }

    public interface IAccount
    {
        Account Account
        {
            get;
        }

        int GetEquity();
    }

    public interface IHistory
    {
        IEnumerable<Order> ClosedOrders
        {
            get;
        }
        Order GetOrderByIDFromHistory(int orderID);
        bool Contains(int orderID);
        //GroupTick HistoryGroupTick(DateTime dateTime);
        GroupTick HistoryGroupTick(int lastMinute);

    }
    public interface IRegisterTickToHandle
    {
        void RegisterBuyLimit(int price);
        void RegisterBuyStop(int price);
        void RegisterSellLimit(int price);
        void RegisterSellStop(int price);
    }
    public interface IMeta
    {
        int High(string symbol, int lastMinute);
        int Low(string symbol, int lastMinute);
        int Open(string symbol, int lastMinute);

        bool OrderModify(int ticket, int price, int sl, int tp, OrderType type);
        int OrderSend(string symbol, OrderType type, OrderSide side, int volume, int open, int sl, int tp, string comment);
        bool OrderClose(int ticket, int lots, int price, int slippage);
        bool OrderCloseBy(int ticket, int ticket2, int Color);
        bool OrderDelete(int ticket);

        //not implemented by tester
        bool OrderSelect(int index, SelectType select, SelectModeType pool);
        int OrdersTotal();

        int GetOrderTicket();
        int GetOrderClosePrice(string symbol);
        DateTime GetOrderCloseTime();
        string GetOrderComment();
        int GetOrderCommission();
        DateTime GetOrderExpiration();
        int GetOrderLots();
        int GetOrderMagicNumber();
        int GetOrderOpenPrice(string symbol);
        DateTime GetOrderOpenTime();
        int GetOrderStopLoss(string symbol);
        int GetOrderSwap();
        string GetOrderSymbol();
        int GetOrderTakeProfit(string symbol);
        MetaOperationType GetOrderType();

        string Symbol();

        void Comment(string str);
        void Print(string str);

        bool IsTradeContextBusy();
        MetaLastErrors GetLastError();

        bool ObjectCreate(string name, MetaObjectType type, int window, DateTime time1, int price1, DateTime time2, int price2, DateTime time3, int price3);
        bool ObjectDelete(string name);
        string ObjectDescription(string name);
        int ObjectFind(string name);
        double ObjectGet(string name, int index);
        string ObjectGetFiboDescription(string name, int index);
        int ObjectGetShiftByValue(string name, int value);
        int ObjectGetValueByShift(string name, int shift);
        bool ObjectMove(string name, int point, DateTime time1, int price1);
        string ObjectName(int index);
        int ObjectsDeleteAll(int window, MetaObjectType type);
        bool ObjectSet(string name, int index, int value);
        bool ObjectSetFiboDescription(string name, int index, string text);
        bool ObjectSetText(string name, string text, int font_size, string font, int text_color);
        int ObjectsTotal(MetaObjectType type);
        MetaObjectType ObjectType(string name);

        double AccountBalance();
        double AccountCredit();
        string AccountCompany();
        string AccountCurrency();
        double AccountEquity();
        double AccountFreeMargin();
        double AccountFreeMarginCheck(string symbol, OrderType type, OrderSide side, int volume);
        double AccountFreeMarginMode();
        int AccountLeverage();
        double AccountMargin();
        string AccountName();
        int AccountNumber();
        double AccountProfit();
        string AccountServer();
        int AccountStopoutLevel();
        int AccountStopoutMode();

        int MarketInfo(string symbol, MarketInfoType type);
        void RefreshRates();

        int iBars(string symbol, GraphPeriod timeFrame);
        int iBarShift(string symbol, GraphPeriod timeFrame, DateTime dateTime, bool exact);
        int iClose(string symbol, GraphPeriod timeFrame, int shift);
        int iHigh(string symbol, GraphPeriod timeFrame, int shift);
        int iHighest(string symbol, GraphPeriod timeFrame, int type, int count, int start);
        int iLow(string symbol, GraphPeriod timeFrame, int shift);
        int iLowest(string symbol, GraphPeriod timeFrame, int type, int count, int start);
        int iOpen(string symbol, GraphPeriod timeFrame, int shift);
        DateTime iTime(string symbol, GraphPeriod timeFrame, int shift);
        double iVolume(string symbol, GraphPeriod timeFrame, int shift);

    }


}
