using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace FxAdvisorCore
{
    internal class Command
    {
        private int paramIndex = 0;
        private Commands commandName;
        private object[] cmdParams;
        private object result;
        private bool completed;

        public Command(Commands commandName)
        {
            this.commandName = commandName;
        }

        public Command(Commands commandName, object[] cmdParams)
        {
            this.cmdParams = cmdParams;
            this.commandName = commandName;
        }

        public int ParamCount
        {
            get { return cmdParams.Length; }
        }

        public Commands Name
        {
            get { return commandName; }
        }

        public virtual object getNextParam()
        {
            return cmdParams[paramIndex++];
        }

        public object Result
        {
            get { return result; }
        }

        public bool Completed
        {
            get { return completed; }
        }

        public virtual void Complete(object result)
        {
            this.result = result;
            this.completed = true;
        }
    }

    internal enum Commands
    {
        Exit            = 1,
        MarketInfo      = 2,
        SendOrder       = 3,
        RefreshRates    = 4,
        OrderSend       = 5,
        Comment         = 6,
        High = 7,
        Low = 8,
        Open = 9,
        Close = 10,
        OrderModify = 11,
        OrderClose = 12,
        OrderSelect = 13,
        OrdersTotal = 14,
        OrderTicket = 15,
        OrderClosePrice = 16,
        OrderCloseTime = 17,
        OrderComment = 18,
        OrderCommission = 19,
        OrderExpiration = 20,
        OrderLots = 21,
        OrderMagicNumber = 22,
        OrderOpenPrice = 23,
        OrderOpenTime = 24,
        OrderStopLoss = 25,
        OrderSwap = 26,
        OrderSymbol = 27,
        OrderTakeProfit = 28,
        OrderType = 29,
        Symbol = 30,
        Print = 31,
        OrderDelete = 32,
        IsTradeContextBusy = 33,
        GetLastError = 34,
        ObjectCreate = 35,
        ObjectDelete = 36,
        ObjectDescription =37,
        ObjectFind =38,
        ObjectGet = 39,
        ObjectGetFiboDescription = 40,
        ObjectGetShiftByValue = 41,
        ObjectGetValueByShift = 42,
        ObjectMove = 43,
        ObjectName = 44,
        ObjectsDeleteAll = 45,
        ObjectSet = 46,
        ObjectSetFiboDescription = 47,
        ObjectSetText = 48,
        ObjectsTotal = 49,
        ObjectType = 50,

        AccountBalance = 51,
        AccountCredit = 52,
        AccountCompany = 53,
        AccountCurrency = 54,
        AccountEquity = 55,
        AccountFreeMargin = 56,
        AccountFreeMarginCheck = 57,
        AccountFreeMarginMode = 58,
        AccountLeverage = 59,
        AccountMargin = 60,
        AccountName = 61,
        AccountNumber = 62,
        AccountProfit = 63,
        AccountServer = 64,
        AccountStopoutLevel = 65,
        AccountStopoutMode = 66
        ,IBars                   =67
        ,IBarShift               =68
        ,IClose                  =69
        ,IHigh                   =70
        ,IHighest                =71
        ,ILow                    =72
        ,ILowest                 =73
        ,IOpen                   =74
        ,ITime                   =75
        ,IVolume                 =76
        ,OrderCloseBy            =77
    }
}
