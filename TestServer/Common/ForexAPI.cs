using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
public class ForexAPI : IOrderOperation, IAccount, IHistory
{
    protected IMeta MetaAPI { get; set; }
    protected int MagicNumber { get; private set; }
    public ForexAPI(IMeta metaAPI, int magic)
    {
        this.MetaAPI = metaAPI;
        this.MagicNumber = magic;
    }
    #region IOrderOperation Members

    public bool OrderWasActivated 
    { 
        get
        {
            return true;
        }
        set
        { }
    }

    public Order AddOrder(Order order)
    {
        int ticket = MetaAPI.OrderSend(order.Symbol, order.Type, order.Side, order.Volume, order.OpenPrice, order.SL, order.TP, "");
        return GetOrderByID(ticket);
    }

    public bool CloseOrder(Order order)
    {
        if (order.Type == OrderType.Market)
            return MetaAPI.OrderClose(order.ID, order.Volume, 0, 0);
        else
            return MetaAPI.OrderDelete(order.ID);
    }

    public bool CloseAllOrders()
    {
        bool retValue = true;
        foreach (Order currOrder in GetMarketOrders())
            retValue |= CloseOrder(currOrder);
        foreach (Order currOrder in GetLimitOrders())
            retValue |= CloseOrder(currOrder);
        return retValue;
    }

    public void ModifyMarketOrder(int orderID, int SL, int TP)
    {
        MetaAPI.OrderModify(orderID, 0, SL, TP, OrderType.Market);
    }

    public void ModifyLimitOrder(int orderID, int OpenPrice, int SL, int TP)
    {
        MetaAPI.OrderModify(orderID, OpenPrice, SL, TP, OrderType.Limit);
    }

    public Order GetOrderByID(int orderID)
    {
        if (!MetaAPI.OrderSelect(orderID, SelectType.SELECT_BY_TICKET, SelectModeType.MODE_TRADES))
            return null;
        return BuildOrderFromCurrent();
    }

    public IEnumerable<Order> GetMarketOrders()
    {
        List<Order> listOrders = new List<Order>();

        int total = MetaAPI.OrdersTotal();
        for (int i = total - 1; i >= 0; i--)
        {
            MetaAPI.OrderSelect(i, SelectType.SELECT_BY_POS, SelectModeType.MODE_TRADES);
            if (MetaTypeConvertor.ToOrderType(MetaAPI.GetOrderType()) == OrderType.Market && MagicNumber == MetaAPI.GetOrderMagicNumber())
                listOrders.Add(BuildOrderFromCurrent());
        }
        return listOrders;
    }

    public IEnumerable<Order> GetLimitOrders()
    {
        List<Order> listOrders = new List<Order>();
        
        int total = MetaAPI.OrdersTotal();
        for (int i = total - 1; i >= 0; i--)
        {
            MetaAPI.OrderSelect(i, SelectType.SELECT_BY_POS, SelectModeType.MODE_TRADES);
            if (MetaTypeConvertor.ToOrderType(MetaAPI.GetOrderType()) == OrderType.Limit && MagicNumber == MetaAPI.GetOrderMagicNumber())
                listOrders.Add( BuildOrderFromCurrent() );
        }
        return listOrders;
    }
    //public IEnumerable<Order> GetAllOrders()
    //{
    //    int total = MetaAPI.OrdersTotal();
    //    for (int i = total - 1; i >= 0; i++)
    //    {
    //        yield return BuildOrderFromCurrent();
    //    }
    //}

    public IEnumerable<Order> GetMarketOrders( OrderSide side )
    {
        List<Order> listOrders = new List<Order>();

        int total = MetaAPI.OrdersTotal();
        for (int i = total - 1; i >= 0; i--)
        {
            MetaAPI.OrderSelect(i, SelectType.SELECT_BY_POS, SelectModeType.MODE_TRADES);
            if (side == OrderSide.Buy)
            {
                if (MetaTypeConvertor.ToOrderType(MetaAPI.GetOrderType()) == OrderType.Market && MetaTypeConvertor.ToOrderSide(MetaAPI.GetOrderType()) == OrderSide.Buy && MagicNumber == MetaAPI.GetOrderMagicNumber())
                    listOrders.Add(BuildOrderFromCurrent());
            }
            else
            {
                if (MetaTypeConvertor.ToOrderType(MetaAPI.GetOrderType()) == OrderType.Market && MetaTypeConvertor.ToOrderSide(MetaAPI.GetOrderType()) == OrderSide.Sell && MagicNumber == MetaAPI.GetOrderMagicNumber())
                    listOrders.Add(BuildOrderFromCurrent());
            }
        }
        return listOrders;
    }
    public IEnumerable<Order> GetBuyMarketOrders()
    {
        return GetMarketOrders(OrderSide.Buy);
    }
    public IEnumerable<Order> GetSellMarketOrders()
    {
        return GetMarketOrders(OrderSide.Sell);
    }
    public IEnumerable<Order> GetBuyLimitOrders()
    {
        return GetLimitOrders(OrderSide.Buy);
    }
    public IEnumerable<Order> GetSellLimitOrders()
    {
        return GetLimitOrders(OrderSide.Sell);
    }
    public IEnumerable<Order> GetLimitOrders(OrderSide side)
    {
        List<Order> listOrders = new List<Order>();

        int total = MetaAPI.OrdersTotal();
        for (int i = total - 1; i >= 0; i--)
        {
            MetaAPI.OrderSelect(i, SelectType.SELECT_BY_POS, SelectModeType.MODE_TRADES);
            if (side == OrderSide.Buy)
            {
                if (MetaTypeConvertor.ToOrderType(MetaAPI.GetOrderType()) == OrderType.Limit && MetaTypeConvertor.ToOrderSide(MetaAPI.GetOrderType()) == OrderSide.Buy && MagicNumber == MetaAPI.GetOrderMagicNumber())
                    listOrders.Add(BuildOrderFromCurrent());
            }
            else
            {
                if (MetaTypeConvertor.ToOrderType(MetaAPI.GetOrderType()) == OrderType.Limit && MetaTypeConvertor.ToOrderSide(MetaAPI.GetOrderType()) == OrderSide.Sell && MagicNumber == MetaAPI.GetOrderMagicNumber())
                    listOrders.Add(BuildOrderFromCurrent());
            }
        }
        return listOrders;
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

    public bool Contains(int orderID)
    {
        throw new NotImplementedException();
    }

    public GroupTick HistoryGroupTick(int lastMinute)
    {
        throw new NotImplementedException();
    }

    public Order GetOrderByIDFromHistory(int orderID)
    {
        if (!MetaAPI.OrderSelect(orderID, SelectType.SELECT_BY_TICKET, SelectModeType.MODE_HISTORY))
            return null;
        return BuildOrderFromCurrent();
    }

    #endregion

    private Order BuildOrderFromCurrent()
    {
        string symbol = MetaAPI.GetOrderSymbol();
        Order newOrder = new Order(MetaAPI.GetOrderTicket(), symbol, MetaAPI.GetOrderType(), MetaAPI.GetOrderOpenPrice(symbol), MetaAPI.GetOrderLots(), MetaAPI.GetOrderStopLoss(symbol), MetaAPI.GetOrderTakeProfit(symbol), MetaAPI.GetOrderComment());
        newOrder.ClosedPrice = MetaAPI.GetOrderClosePrice(symbol);
        return newOrder;
    }
}
}