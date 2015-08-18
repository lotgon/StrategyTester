using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Engine, PublicKey=0024000004800000940000000602000000240000525341310004000001000100ed737e817080e31e498256a7f1397ddba85a2ca29442bb8506e3bcb83bf2c38ba8c0bfee70d9cf725dc8af7231a4ea682f35d79829f8b7ed9f20e884eea13979e38bfc2c5fd02c27b91b7240e8e84cf4e88c04620003ee6734446009034666e97245c408161f41d6bed90a30ddc6b37800177a841f958faaacaa902ab8ee9bda")]

namespace Common
{
    public class Order : ICloneable
    {
        const int PipsValue = 1;
        private int _ID;
        private OrderSide _Side;
        private OrderType _Type;
        private int _Volume;
        private int _SL;
        private int _TP;
        private int _OpenedPrice;
        private int _ClosedPrice;
        private int _OpenPrice;
        private string _Comment;
        private int _Swap = 0;
        internal Order(string symbol, OrderType type, OrderSide side, int volume)
        {
            if (type == OrderType.Limit)
                throw new ApplicationException("Wrong order type");
            this.Type = type;
            this.SL = 0;
            this.TP = 0;
            this.ID = 0;
            this.OpenedPrice = 0;
            this.Side = side;
            this.Volume = volume;
            this.ClosedPrice = 0;
            this.OpenPrice = 0;
            this.Symbol = symbol;
        }
        internal Order(string symbol, OrderType type, OrderSide side, int volume, int SL, int TP)
        {
            if (type == OrderType.Limit)
                throw new ApplicationException("Wrong order type");

            this.Type = type;
            this.SL = SL;
            this.TP = TP;
            this.ID = 0;
            this.OpenedPrice = 0;
            this.Side = side;
            this.Volume = volume;
            this.ClosedPrice = 0;
            this.OpenPrice = 0;
            this.Symbol = symbol;
        }
        internal Order(string symbol, OrderType type, int openprice, OrderSide side, int volume)
        {
            this.Type = type;
            this.ID = 0;
            this.SL = 0;
            this.TP = 0;
            this.Volume = volume;
            this.Side = side;
            this.OpenPrice = openprice;
            if (type == OrderType.Market)
            {
                this.OpenedPrice = 0;
            }
            this.ClosedPrice = 0;
            this.Symbol = symbol;
        }
        internal Order(string symbol, OrderType type, int openprice, OrderSide side, int volume, int SL, int TP)
            : this(symbol, type, openprice, side, volume)
        {
            this.SL = SL;
            this.TP = TP;
            this.Symbol = symbol;
        }
        internal Order(string symbol, OrderType type, int openprice, OrderSide side, int volume, int SL, int TP, string comment)
            : this(symbol, type, openprice, side, volume, SL, TP )
        {
            this.Comment = comment;
            this.SL = SL;
            this.TP = TP;
            this.Comment = comment;
        }
        internal Order(int orderID, string symbol, OrderType type, int openprice, OrderSide side, int volume, int SL, int TP)
            : this(symbol, type, openprice, side, volume, SL, TP)
        {
            this.ID = orderID;
        }
        internal Order(int orderID, string symbol, OrderType type, int openprice, OrderSide side, int volume, int SL, int TP, string comment)
            : this(symbol, type, openprice, side, volume, SL, TP)
        {
            this.ID = orderID;
            this.Comment = comment;
        }
        internal Order(int orderID, string symbol, MetaOperationType mot, int openprice, int volume, int SL, int TP, string comment)
            : this(orderID, symbol, MetaTypeConvertor.ToOrderType(mot), openprice, MetaTypeConvertor.ToOrderSide(mot), volume, SL, TP, comment)
        {
        }
        public static Order NewMarketOrder(string symbol, int currentAsk, int currentBid, OrderSide side, int volume, int SLpips, int TPpips)
        {
            if (side == OrderSide.Buy)
                return new Order(symbol, OrderType.Market, side, volume, currentAsk - SLpips, currentAsk + TPpips);
            else
                return new Order(symbol, OrderType.Market, side, volume, currentBid + SLpips, currentBid - TPpips);
        }
        public static Order NewLimitOrder(string symbol, int currentAsk, int currentBid, OrderSide side, int volume, int OpenPips, int SLpips, int TPpips)
        {
            if (side == OrderSide.Buy)
                return new Order(symbol, OrderType.Limit, currentAsk - OpenPips, side, volume, currentAsk - OpenPips - SLpips, currentAsk - OpenPips + TPpips);
            else
                return new Order(symbol, OrderType.Limit, currentBid + OpenPips, side, volume, currentBid + OpenPips + SLpips, currentBid + OpenPips - TPpips);
        }
        public static Order NewLimitOrder(string symbol, OrderSide side, int volume, int OpenPrice, int SLpips, int TPpips)
        {
            if (side == OrderSide.Buy)
                return new Order(symbol, OrderType.Limit, OpenPrice, side, volume, OpenPrice - SLpips, OpenPrice + TPpips);
            else
                return new Order(symbol, OrderType.Limit, OpenPrice, side, volume, OpenPrice + SLpips, OpenPrice - TPpips);
        }

        public int ID
        {
            get { return _ID; }
            internal set { _ID = value; }
        }
        public OrderSide Side
        {
            get { return _Side; }
            internal set { _Side = value; }
        }
        public OrderSide RevertSide
        {
            get { return Side==OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy; }
        }
        public OrderType Type
        {
            get { return _Type; }
            internal set { _Type = value; }
        }
        public int Volume
        {
            get { return _Volume; }
            internal set { _Volume = value; }
        }
        public int SL
        {
            get { return _SL; }
            internal set { _SL = value; }
        }
        public int TP
        {
            get { return _TP; }
            internal set { _TP = value; }
        }
        public int OpenedPrice
        {
            get { return _OpenedPrice; }
            internal set { _OpenedPrice = value; }
        }
        public int ClosedPrice
        {
            get { return _ClosedPrice; }
            internal set { _ClosedPrice = value; }
        }
        public int OpenPrice
        {
            get { return _OpenPrice; }
            internal set { _OpenPrice = value; }
        }
        public string Symbol { get; internal set; }
        public string Comment { get; private set; }
        public int Swap
        {
            get
            {
                return this._Swap;
            }
        }

        public void AddSwap(int pipsSwap)
        {
            if (Type == OrderType.Market)
                this._Swap += pipsSwap;        
        }

        internal void ToMarketOrder()
        {
            if (this.Type == OrderType.Market)
                throw new ApplicationException("Wrong ToMarketOrder");
            this.OpenedPrice = this.OpenPrice;
            this.Type = OrderType.Market;
        }
        public bool IsClosed
        {
            get
            {
                return ClosedPrice != 0;
            }
        }
        public int Profit
        {
            get
            {
                if (Type == OrderType.Limit)
                    throw new ApplicationException("Can`t get profit for limit order");
                if( ClosedPrice == 0 )
                    throw new ApplicationException("Can`t get profit for not closed order");

                return EstimateProfit(ClosedPrice, ClosedPrice);
            }
        }
        public int EstimateProfit(int currentAsk, int currentBid)
        {
            if (Type == OrderType.Limit)
                throw new ApplicationException("Can`t get profit for limit order");

            if (Side == OrderSide.Buy)
            {
                return (currentBid - OpenedPrice + this.Swap) * Volume * PipsValue;
            }
            else
            {
                return (OpenedPrice - currentAsk + this.Swap) * Volume * PipsValue;
            }

        }
        public override string  ToString()
        {
            if (Type == OrderType.Market)
                return string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}", Side == OrderSide.Buy ? "market buy" : "market sell", ID,
                    Volume, OpenedPrice, SL, TP, IsClosed ? Profit.ToString() : "", this.Symbol);
            else
                return string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6} ", Side == OrderSide.Buy ? "limit buy" : "limit sell", ID,
                      Volume, OpenPrice, SL, TP, this.Symbol);

        }





        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
