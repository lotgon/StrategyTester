using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using FxAdvisorCore.Util;
using FxAdvisorCore.Logging;
using System.Diagnostics;
using Common;

namespace FxAdvisorCore 
{
    public class RemoteAdvisor : IMeta
    {
        #region private fields
        private static Logger logger = Logger.Get("RemoteAdvisor");

        private object _sync = new object();
        private object _advisorSync = new object();
        private int id;
        private AdvisorProxy advisor;
        private double ask;
        private double bid;
        private Command currentCommand;
        private BlockingQueue<Command> commandQueue = new BlockingQueue<Command>();
        private Dictionary<string, string> parameters = new Dictionary<string, string>();

        #endregion

        public RemoteAdvisor( )
        {
            Connected = true;
            IsInverted = false;
        }

        public int MagicNumber 
        {
            get
            {
                return Int32.Parse(parameters["MagicNumber"]);
            } 
        }
        public Dictionary<string, string> Parameters
        {
            get { return parameters; }
        }
        public int Init(AdvisorProxy advisor)
        {
            this.advisor = advisor;

            foreach( string currPair in MarketId.Split( new char[]{' '} ) )
            {
                string currString = currPair.Trim();
                if (currString.Length == 0)
                    continue;

                string[] keyValue = currString.Split(new char[] { '=' });
                parameters[keyValue[0]] = keyValue[1];
            }
            this.id = MagicNumber;
            return this.id;

        }
        public long Operations { get; private set; }
        public double Ask
        {
            get { return ask; }
            internal set { ask = value; }
        }
        public double Bid
        {
            get { return bid; }
            internal set { bid = value; }
        }
        public DateTime LastTickDateTime { get; internal set; }
        public string MarketId { get; internal set; }
        public bool Connected { get; internal set; }
        public void SetParameter(string key, string value)
        {
            this.parameters[key] = value;
        }
        public void SetParameters(string parametersString)
        {
            string[] parts = parametersString.Split(new char[] {' '},
                StringSplitOptions.RemoveEmptyEntries);

            foreach (string part in parts)
                ParseAndAddParameter(part);
        }
        public bool IsInverted { get; set; }

        #region protected
        internal Command Command
        {
            get
            {
                lock (_sync)
                {
                    if (currentCommand == null)
                        currentCommand = commandQueue.Dequeue();
                    return currentCommand;
                }
            }
        }
        internal void SetCommandResult(object result)
        {
            lock (_sync)
            {
                if (currentCommand != null)
                {
                    currentCommand.Complete(result);
                    currentCommand = null;
                }
            }
        }
        internal void AsyncInit(Object stateInfo)
        {
            try
            {
                advisor.OnRemmoteAdvisorInit(this);
            }
            catch (Exception ex)
            {
                logger.Error.Log("OnRemmoteAdvisorInit", ex);
            }
            Return();
        }
        internal void AsyncDeinit(Object stateInfo)
        {
            try
            {
                advisor.OnRemmoteAdvisorDeinit(this);
            }
            catch (Exception ex)
            {
                logger.Error.Log("OnRemmoteAdvisorInit", ex);
            }
            Return();
        }
        internal void AsyncStart(Object stateInfo)
        {
            try
            {
                lock (_advisorSync)
                {
                    advisor.OnRemoteAdvisorStart(this);
                }
            }
            catch (Exception ex)
            {
                logger.Error.Log("OnRemoteAdvisorStart", ex);
            }
            Return();
        }
        protected void ParseAndAddParameter(string parameter)
        {
            string[] parts = parameter.Split('=');
            if (parts.Length == 2)
            {
                SetParameter(parts[0], parts[1]);
            }
        }
        private object ExecuteCommand(AsyncCommand cmd)
        {
            Operations++;
            commandQueue.Enqueue(cmd);
            cmd.WaitCompletion();
            return cmd.Result;
        }
        protected static string ValidateString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            return TruncateString(text);
        }
        protected static string TruncateString(string text)
        {
            return TruncateString(text, 255);
        }
        protected static string TruncateString(string text, int sizeLimit)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= sizeLimit)
                return text;

            return text.Substring(0, sizeLimit);
        }
        protected static string[] SplitString(string text, int chunkSize, int maxChunks)
        {
            if (string.IsNullOrEmpty(text))
                return new string[] { string.Empty };

            List<string> chunks = new List<string>(maxChunks);

            int i = 0;
            while (i < text.Length && chunks.Count < maxChunks)
            {
                int chunkLength = text.Length > i + chunkSize ? chunkSize : text.Length - i;
                chunks.Add(text.Substring(i, chunkLength));
                i += chunkSize;
            }

            return chunks.ToArray();
        }
        #endregion

        #region IMeta Members

        public int High(string symbol, int lastPeriod)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.High, new object[] { symbol, lastPeriod });
            ExecuteCommand(cmd);
            return Convertor.TranslateToPipPrice(symbol, (double)cmd.Result);
        }

        public int Low(string symbol, int lastPeriod)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.Low, new object[] { symbol, lastPeriod });
            ExecuteCommand(cmd);
            return Convertor.TranslateToPipPrice(symbol, (double)cmd.Result);
        }

        public int Open(string symbol, int lastPeriod)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.Open, new object[] { symbol, lastPeriod });
            ExecuteCommand(cmd);
            return Convertor.TranslateToPipPrice(symbol, (double)cmd.Result);
        }

        public bool OrderModify(int ticket, int price, int sl, int tp, OrderType type)
        {
            if( IsInverted )
                    return OrderModifyInternal(ticket, price, tp, sl, type);
            else
                return OrderModifyInternal(ticket, price, sl, tp, type);
        }
        protected bool OrderModifyInternal(int ticket, int price, int sl, int tp, OrderType type)
        {
            if (!OrderSelect(ticket, SelectType.SELECT_BY_TICKET, SelectModeType.MODE_TRADES))
                return false;
            string symbol = GetOrderSymbol();

            bool retResult;
            do
            {
                while (IsTradeContextBusy())
                {
                    Print("TradeContextBusy");
                    Thread.Sleep(100);
                }

                this.Print(string.Format("OrderModify ticket={0};price={1};sl={2};tp={3};",
                    ticket, Convertor.TranslateFromPipPrice(symbol, price), 
                    Convertor.TranslateFromPipPrice(symbol, sl), Convertor.TranslateFromPipPrice(symbol, tp)));

                AsyncCommand aCmd = new AsyncCommand(Commands.OrderModify,
                    new object[] { ticket, Convertor.TranslateFromPipPrice(symbol, price), 
                    Convertor.TranslateFromPipPrice(symbol, sl), Convertor.TranslateFromPipPrice(symbol, tp) });

                retResult = (bool)ExecuteCommand(aCmd);

                if (retResult)
                    return retResult;

            } while (GetLastError() == MetaLastErrors.ERR_SERVER_BUSY);
            return retResult;
        }

        public int OrderSend(string symbol, OrderType type, OrderSide side, int volume, int openPrice, int sl, int tp, string comment)
        {
            if (this.IsInverted)
            {
                OrderType newType = type;
                OrderSide newSide = side;
                int newSL = sl;
                int newTP = tp;

                switch (newType)
                {
                    case OrderType.Market:
                        newSide = side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
                        newSL = tp;
                        newTP = sl;
                        break;
                    case OrderType.Limit:
                        newType = OrderType.Stop;
                        newSide = side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
                        newSL = tp;
                        newTP = sl;
                        break;
                    case OrderType.Stop:
                        newType = OrderType.Limit;
                        newSide = side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
                        newSL = tp;
                        newTP = sl;
                        break;
                    default:
                        throw new ApplicationException("Unknown order type:" + newType.ToString());
                }
                return OrderSendInternal(symbol, newType, newSide, volume, openPrice, newSL, newTP, comment);
            }
            else
                return OrderSendInternal(symbol, type, side, volume, openPrice, sl, tp, comment);
        }
        protected int OrderSendInternal(string symbol, OrderType type, OrderSide side, int volume, int openPrice, int sl, int tp, string comment)
        {
            int retResult = 0;
            do
            {
                while (IsTradeContextBusy())
                {
                    Print("TradeContextBusy");
                    Thread.Sleep(100);
                }
                this.Print(string.Format("OrderSend symbol={0};type={1};volume={2};openPrice={3};slippage={4};sl={5};tp={6};comment={7}",
                    symbol, MetaTypeConvertor.ToMetaOperationType(side, type), (double)volume / 100, Convertor.TranslateFromPipPrice(symbol, openPrice), 15,
                    Convertor.TranslateFromPipPrice(symbol, sl), Convertor.TranslateFromPipPrice(symbol, tp), comment));

                AsyncCommand aCmd = new AsyncCommand(Commands.OrderSend,
                    new object[] { symbol, MetaTypeConvertor.ToMetaOperationType(side, type), (double)volume/100, Convertor.TranslateFromPipPrice(symbol, openPrice), 15,
                    Convertor.TranslateFromPipPrice(symbol, sl), Convertor.TranslateFromPipPrice(symbol, tp), comment, this.MagicNumber, 0, 0 });
                retResult = (int)ExecuteCommand(aCmd);
                if (retResult > 0)
                    return retResult;
            } while (GetLastError() == MetaLastErrors.ERR_SERVER_BUSY);
            return retResult;
        }
        public bool OrderClose(int ticket, int lots, int price, int slippage)
        {
            bool retResult;
            do
            {
                while (IsTradeContextBusy())
                {
                    Print("TradeContextBusy");
                    Thread.Sleep(100);
                }

                if (!OrderSelect(ticket, SelectType.SELECT_BY_TICKET, SelectModeType.MODE_TRADES))
                    return false;
                string symbol = GetOrderSymbol();

                AsyncCommand aCmd = new AsyncCommand(Commands.OrderClose,
                    new object[] { ticket, (double) lots/100, Convertor.TranslateFromPipPrice(symbol, price), slippage });
                retResult = (bool)ExecuteCommand(aCmd);

                if (retResult)
                    return retResult;

            } while (GetLastError() == MetaLastErrors.ERR_SERVER_BUSY);
            return retResult;
        }
        public bool OrderCloseBy(int ticket, int ticket2, int Color)
        {
            bool retResult;
            do
            {
                while (IsTradeContextBusy())
                {
                    Print("TradeContextBusy");
                    Thread.Sleep(100);
                }

                AsyncCommand aCmd = new AsyncCommand(Commands.OrderCloseBy,
                    new object[] { ticket, ticket2, Color });
                retResult = (bool)ExecuteCommand(aCmd);

                if (retResult)
                    return retResult;

            } while (GetLastError() == MetaLastErrors.ERR_SERVER_BUSY);
            return retResult;
        }
        public bool OrderDelete(int ticket)
        {
            bool retResult;
            do
            {
                while (IsTradeContextBusy())
                {
                    Print("TradeContextBusy");
                    Thread.Sleep(100);
                }

                AsyncCommand aCmd = new AsyncCommand(Commands.OrderDelete,
                    new object[] { ticket });
                retResult = (bool)ExecuteCommand(aCmd);

                if (retResult)
                    return retResult;

            } while (GetLastError() == MetaLastErrors.ERR_SERVER_BUSY);
            return retResult;
        }
        public bool OrderSelect(int index, SelectType select, SelectModeType pool)
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.OrderSelect,
               new object[] { index, select, pool });
            ExecuteCommand(aCmd);
            return (bool)aCmd.Result;
        }

        public int OrdersTotal()
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.OrdersTotal);
            ExecuteCommand(aCmd);
            return (int)aCmd.Result;
        }

        public int GetOrderTicket()
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.OrderTicket);
            ExecuteCommand(aCmd);
            return (int)aCmd.Result;
        }

        public int GetOrderClosePrice(string symbol)
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.OrderClosePrice);
            ExecuteCommand(aCmd);
            return Convertor.TranslateToPipPrice(symbol, (double)aCmd.Result);
        }

        public DateTime GetOrderCloseTime()
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.OrderCloseTime);
            ExecuteCommand(aCmd);
            return Convertor.SecondsToDateTime((int)aCmd.Result);
        }

        public string GetOrderComment()
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.OrderComment);
            ExecuteCommand(aCmd);
            return (string)aCmd.Result;
        }

        public int GetOrderCommission()
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.OrderCommission);
            ExecuteCommand(aCmd);
            return (int)(double)aCmd.Result;
        }

        public DateTime GetOrderExpiration()
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.OrderExpiration);
            ExecuteCommand(aCmd);
            return Convertor.SecondsToDateTime((int)aCmd.Result);
        }

        public int GetOrderLots()
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.OrderLots);
            ExecuteCommand(aCmd);
            return (int)(100*(double)aCmd.Result);
        }

        public int GetOrderMagicNumber()
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.OrderMagicNumber);
            ExecuteCommand(aCmd);
            return (int)aCmd.Result;
        }

        public int GetOrderOpenPrice(string symbol)
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.OrderOpenPrice);
            ExecuteCommand(aCmd);
            return Convertor.TranslateToPipPrice(symbol, (double)aCmd.Result);
        }

        public DateTime GetOrderOpenTime()
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.OrderOpenTime);
            ExecuteCommand(aCmd);
            return Convertor.SecondsToDateTime((int)aCmd.Result);
        }

        public int GetOrderStopLoss(string symbol)
        {
            Commands command = Commands.OrderStopLoss;

            if (IsInverted)
                command = Commands.OrderTakeProfit;

            AsyncCommand aCmd = new AsyncCommand(command);
            ExecuteCommand(aCmd);
            return Convertor.TranslateToPipPrice(symbol, (double)aCmd.Result);
        }

        public int GetOrderSwap()
        {
            throw new NotImplementedException();
        }

        public string GetOrderSymbol()
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.OrderSymbol);
            ExecuteCommand(aCmd);
            return (string)aCmd.Result;
        }

        public int GetOrderTakeProfit(string symbol)
        {
            Commands command = Commands.OrderTakeProfit;

            if (IsInverted)
                command = Commands.OrderStopLoss;

            AsyncCommand aCmd = new AsyncCommand(command);
            ExecuteCommand(aCmd);
            return Convertor.TranslateToPipPrice(symbol, (double)aCmd.Result);
        }

        virtual public MetaOperationType GetOrderType()
        {
            if (IsInverted)
            {
                AsyncCommand aCmd = new AsyncCommand(Commands.OrderType);
                ExecuteCommand(aCmd);
                MetaOperationType mtp = (MetaOperationType)aCmd.Result;
                switch (mtp)
                {
                    case MetaOperationType.OP_BUY:
                        return MetaOperationType.OP_SELL;
                    case MetaOperationType.OP_SELL:
                        return MetaOperationType.OP_BUY;
                    case MetaOperationType.OP_BUYLIMIT:
                        return MetaOperationType.OP_SELLSTOP;
                    case MetaOperationType.OP_SELLLIMIT:
                        return MetaOperationType.OP_BUYSTOP;
                    case MetaOperationType.OP_BUYSTOP:
                        return MetaOperationType.OP_SELLLIMIT;
                    case MetaOperationType.OP_SELLSTOP:
                        return MetaOperationType.OP_BUYLIMIT;
                    default:
                        throw new ApplicationException("Unknown order type:" + mtp.ToString());
                }
            }
            else
            {
                AsyncCommand aCmd = new AsyncCommand(Commands.OrderType);
                ExecuteCommand(aCmd);
                return (MetaOperationType)aCmd.Result;
            }
        }

        public string Symbol()
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.Symbol);
            ExecuteCommand(aCmd);
            return (string)aCmd.Result;
        }

        public void Comment(string text)
        {
            text += "\n MagicNumber = " + MagicNumber.ToString();
            // text size is limited in MetaTrader by 255 symbols
            string[] chunks = SplitString(text, 255, 50);

            ExecuteCommand(new AsyncCommand(Commands.Comment, chunks));
        }
        public void Print(string text)
        {
            text += " MagicNumber = " + MagicNumber.ToString();
            // text size is limited in MetaTrader by 255 symbols
            string[] chunks = SplitString(text, 255, 50);

            ExecuteCommand(new AsyncCommand(Commands.Print, chunks));
        }
        public bool IsTradeContextBusy()
        {
            return (bool)ExecuteCommand(new AsyncCommand(Commands.IsTradeContextBusy));
        }

        public MetaLastErrors GetLastError()
        {
            return (MetaLastErrors)(int)ExecuteCommand(new AsyncCommand(Commands.GetLastError));
        }
        #endregion
        #region IMeta Commands
        public int MarketInfo(string symbol, MarketInfoType type)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.MarketInfo, new object[] { symbol, type });
            ExecuteCommand(cmd);
            if (type == MarketInfoType.MODE_EXPIRATION || type == MarketInfoType.MODE_TIME)
                return (int)(double)cmd.Result;
            return Convertor.TranslateToPipPrice(symbol, (double)cmd.Result);
        }

        public void Return()
        {
            ExecuteCommand(new AsyncCommand(Commands.Exit));
        }

        public void RefreshRates()
        {
            ExecuteCommand(new AsyncCommand(Commands.RefreshRates));
        }

        #endregion Commands
        #region IMeta Graph Members


        public bool ObjectCreate(string name, MetaObjectType type, int window, DateTime time1, int price1, DateTime time2, int price2, DateTime time3, int price3)
        {
            string symbol = Symbol();

            AsyncCommand aCmd = new AsyncCommand(Commands.ObjectCreate,
                new object[] { name, (int)type, window, Convertor.DateTimeToSeconds(time1), Convertor.TranslateFromPipPrice(symbol, price1),
                    Convertor.DateTimeToSeconds(time2), Convertor.TranslateFromPipPrice(symbol, price2),
                    Convertor.DateTimeToSeconds(time3), Convertor.TranslateFromPipPrice(symbol, price3)
                });
            return (bool)ExecuteCommand(aCmd);
        }

        public bool ObjectDelete(string name)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectDelete, new object[] { name });
            ExecuteCommand(cmd);
            return (bool)cmd.Result;
            //return Convertor.TranslateToPipPrice(Symbol(), (double)cmd.Result);
        }

        public string ObjectDescription(string name)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectDescription, new object[] { name });
            ExecuteCommand(cmd);
            return (string)cmd.Result;
        }

        public int ObjectFind(string name)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectFind, new object[] { name });
            ExecuteCommand(cmd);
            return (int)cmd.Result;
        }

        public double ObjectGet(string name, int index)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectGet, new object[] { name, index });
            ExecuteCommand(cmd);
            return (double)cmd.Result;
        }

        public string ObjectGetFiboDescription(string name, int index)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectGetFiboDescription, new object[] { name, index });
            ExecuteCommand(cmd);
            return (string)cmd.Result;
        }

        public int ObjectGetShiftByValue(string name, int value)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectGetShiftByValue, new object[] { name, value });
            ExecuteCommand(cmd);
            return (int)cmd.Result;
        }

        public int ObjectGetValueByShift(string name, int shift)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectGetValueByShift, new object[] { name, shift });
            ExecuteCommand(cmd);
            return (Convertor.TranslateToPipPrice(Symbol(), (double)cmd.Result));
        }

        public bool ObjectMove(string name, int point, DateTime time1, int price1)
        {
            string symbol = Symbol();

            AsyncCommand aCmd = new AsyncCommand(Commands.ObjectMove,
                new object[] { name, point, Convertor.DateTimeToSeconds(time1), Convertor.TranslateFromPipPrice(symbol, price1)
                });
            return (bool)ExecuteCommand(aCmd);
        }

        public string ObjectName(int index)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectName, new object[] { index });
            ExecuteCommand(cmd);
            return (string)cmd.Result;
        }

        public int ObjectsDeleteAll(int window, MetaObjectType type)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectsDeleteAll, new object[] { window, (int)type });
            ExecuteCommand(cmd);
            return (int)cmd.Result;
        }

        public bool ObjectSet(string name, int index, int value)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectSet, new object[] { name, index, value });
            ExecuteCommand(cmd);
            return (bool)cmd.Result;
        }

        public bool ObjectSetFiboDescription(string name, int index, string text)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectSetFiboDescription, new object[] { name, index, text });
            ExecuteCommand(cmd);
            return (bool)cmd.Result;
        }

        public bool ObjectSetText(string name, string text, int font_size, string font, int text_color)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectSetText, new object[] { name, text, font_size, font, text_color });
            ExecuteCommand(cmd);
            return (bool)cmd.Result;
        }

        public int ObjectsTotal(MetaObjectType type)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectsTotal, new object[] { (int)type });
            ExecuteCommand(cmd);
            return (int)cmd.Result;
        }

        public MetaObjectType ObjectType(string name)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.ObjectType, new object[] { name });
            ExecuteCommand(cmd);
            return (MetaObjectType)(int)cmd.Result;
        }

        #endregion
        #region IMeta Members Account


        public double AccountBalance()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountBalance);
            ExecuteCommand(cmd);
            return (double)cmd.Result;
        }

        public double AccountCredit()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountCredit);
            ExecuteCommand(cmd);
            return (double)cmd.Result;
        }

        public string AccountCompany()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountCompany);
            ExecuteCommand(cmd);
            return (string)cmd.Result;
        }

        public string AccountCurrency()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountCurrency);
            ExecuteCommand(cmd);
            return (string)cmd.Result;
        }

        public double AccountEquity()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountEquity);
            ExecuteCommand(cmd);
            return (double)cmd.Result;
        }

        public double AccountFreeMargin()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountEquity);
            ExecuteCommand(cmd);
            return (double)cmd.Result;
        }

        public double AccountFreeMarginCheck(string symbol, OrderType type, OrderSide side, int volume)
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountFreeMarginCheck, new object[] { symbol, MetaTypeConvertor.ToMetaOperationType(side, type), ((double)volume)/100 });
            ExecuteCommand(cmd);
            return (double)cmd.Result;
        }

        public double AccountFreeMarginMode()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountFreeMarginMode);
            ExecuteCommand(cmd);
            return (double)cmd.Result;
        }

        public int AccountLeverage()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountLeverage);
            ExecuteCommand(cmd);
            return (int)cmd.Result;
        }

        public double AccountMargin()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountMargin);
            ExecuteCommand(cmd);
            return (double)cmd.Result;
        }

        public string AccountName()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountName);
            ExecuteCommand(cmd);
            return (string)cmd.Result;
        }

        public int AccountNumber()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountNumber);
            ExecuteCommand(cmd);
            return (int)cmd.Result;
        }

        public double AccountProfit()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountProfit);
            ExecuteCommand(cmd);
            return (double)cmd.Result;
        }

        public string AccountServer()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountServer);
            ExecuteCommand(cmd);
            return (string)cmd.Result;
        }

        public int AccountStopoutLevel()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountStopoutLevel);
            ExecuteCommand(cmd);
            return (int)cmd.Result;
        }

        public int AccountStopoutMode()
        {
            AsyncCommand cmd = new AsyncCommand(Commands.AccountStopoutMode);
            ExecuteCommand(cmd);
            return (int)cmd.Result;
        }

        #endregion
        #region Timeseries
        public int iBars(string symbol, GraphPeriod timeFrame)
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.IBars,
                new object[] { symbol, (int)timeFrame });
            return (int)ExecuteCommand(aCmd);
        }
        public int iBarShift(string symbol, GraphPeriod timeFrame, DateTime dateTime, bool exact)
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.IBarShift,
                new object[] { symbol, (int)timeFrame, Convertor.DateTimeToSeconds(dateTime), exact ? 1 : 0 });
            return (int)ExecuteCommand(aCmd);
        }
        public int iClose(string symbol, GraphPeriod timeFrame, int shift)
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.IClose,
                new object[] { symbol, (int)timeFrame, shift });
            int retValue = Convertor.TranslateToPipPrice(symbol, (double)ExecuteCommand(aCmd));
            if (retValue == 0)
                throw new ApplicationException("Local history is empty or meta is stupid f... thing");
            return retValue;
        }
        public int iHigh(string symbol, GraphPeriod timeFrame, int shift)
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.IHigh,
                new object[] { symbol, (int)timeFrame, shift });
            int retValue = Convertor.TranslateToPipPrice(symbol, (double)ExecuteCommand(aCmd));
            if (retValue == 0)
                throw new ApplicationException("Local history is empty or meta is stupid f... thing");
            return retValue;
        }
        public int iHighest(string symbol, GraphPeriod timeFrame, int type, int count, int start)
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.IHighest,
                new object[] { symbol, (int)timeFrame, type, count, start });
            return (int)ExecuteCommand(aCmd);
        }
        public int iLow(string symbol, GraphPeriod timeFrame, int shift)
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.ILow,
                new object[] { symbol, (int)timeFrame, shift });
            int retValue = Convertor.TranslateToPipPrice(symbol, (double)ExecuteCommand(aCmd));
            if (retValue == 0)
                throw new ApplicationException("Local history is empty or meta is stupid f... thing");
            return retValue;
        }
        public int iLowest(string symbol, GraphPeriod timeFrame, int type, int count, int start)
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.ILowest,
                new object[] { symbol, (int)timeFrame, type, count, start });
            return (int)ExecuteCommand(aCmd);
        }
        public int iOpen(string symbol, GraphPeriod timeFrame, int shift)
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.IOpen,
                new object[] { symbol, (int)timeFrame, shift });
            int retValue = Convertor.TranslateToPipPrice(symbol, (double)ExecuteCommand(aCmd));
            if (retValue == 0)
                throw new ApplicationException("Local history is empty or meta is stupid f... thing");
            return retValue;
        }
        public DateTime iTime(string symbol, GraphPeriod timeFrame, int shift)
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.ITime,
                new object[] { symbol, (int)timeFrame, shift });
            int retValue = (int)ExecuteCommand(aCmd);
            if (retValue == 0)
                throw new ApplicationException("Local history is empty or meta is stupid f... thing");
            return Convertor.SecondsToDateTime(retValue);
        }
        public double iVolume(string symbol, GraphPeriod timeFrame, int shift)
        {
            AsyncCommand aCmd = new AsyncCommand(Commands.IVolume,
                new object[] { symbol, (int)timeFrame, shift });
            double retValue = (double)ExecuteCommand(aCmd);
            if( Math.Abs( retValue) <= Double.Epsilon)
                throw new ApplicationException("Local history is empty or meta is stupid f... thing");
            return retValue;
        }        
        #endregion




    }
}
