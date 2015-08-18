using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public enum MarketInfoType
    {
        MODE_LOW            = 1,  // Low day price. 
        MODE_HIGH           = 2,  // High day price. 
        MODE_TIME           = 5, // The last incoming tick time (last known server time). 
        MODE_BID            = 9, // Last incoming bid price. For the current symbol, it is stored in the predefined variable Bid 
        MODE_ASK            = 10, // Last incoming ask price. For the current symbol, it is stored in the predefined variable Ask 
        MODE_POINT          = 11, // Point size in the quote currency. For the current symbol, it is stored in the predefined variable Point 
        MODE_DIGITS         = 12, // Count of digits after decimal point in the symbol prices. For the current symbol, it is stored in the predefined variable Digits 
        MODE_SPREAD         = 13, // Spread value in points. 
        MODE_STOPLEVEL      = 14, // Stop level in points. 
        MODE_LOTSIZE        = 15, // Lot size in the base currency. 
        MODE_TICKVALUE      = 16, // Tick value in the deposit currency. 
        MODE_TICKSIZE       = 17, // Tick size in the quote currency. 
        MODE_SWAPLONG       = 18, // Swap of the long position. 
        MODE_SWAPSHORT      = 19, // Swap of the short position. 
        MODE_STARTING       = 20, // Market starting date (usually used for futures). 
        MODE_EXPIRATION     = 21, // Market expiration date (usually used for futures). 
        MODE_TRADEALLOWED   = 22, // Trade is allowed for the symbol. 
        MODE_MINLOT         = 23, // Minimum permitted amount of a lot. 
        MODE_LOTSTEP        = 24, // Step for changing lots. 
        MODE_MAXLOT         = 25, // Maximum permitted amount of a lot. 
        MODE_SWAPTYPE       = 26, // Swap calculation method. 0 - in points; 1 - in the symbol base currency; 2 - by interest; 3 - in the margin currency. 
        MODE_PROFITCALCMODE = 27, // Profit calculation mode. 0 - Forex; 1 - CFD; 2 - Futures. 
        MODE_MARGINCALCMODE = 28, // Margin calculation mode. 0 - Forex; 1 - CFD; 2 - Futures; 3 - CFD for indices. 
        MODE_MARGININIT     = 29, // Initial margin requirements for 1 lot. 
        MODE_MARGINMAINTENANCE = 30, // Margin to maintain open positions calculated for 1 lot. 
        MODE_MARGINHEDGED   = 31, // Hedged margin calculated for 1 lot. 
        MODE_MARGINREQUIRED = 32, // Free margin required to open 1 lot for buying. 
        MODE_FREEZELEVEL    = 33, // Order freeze level in points. If the execution price lies within the range defined by the freeze level, the order cannot be modified, cancelled or closed. 
    }
    public enum MetaLastErrors
    {
        ERR_NO_ERROR = 0,
        ERR_NO_RESULT = 1,
        ERR_COMMON_ERROR = 2,
        ERR_INVALID_TRADE_PARAMETERS = 3,
        ERR_SERVER_BUSY = 4,
        ERR_OLD_VERSION = 5,
        ERR_NO_CONNECTION = 6,
        ERR_NOT_ENOUGH_RIGHTS = 7,
        ERR_TOO_FREQUENT_REQUESTS = 8,
        ERR_MALFUNCTIONAL_TRADE = 9,
        ERR_ACCOUNT_DISABLED = 64,
        ERR_INVALID_ACCOUNT = 65,
        ERR_TRADE_TIMEOUT = 128,
        ERR_INVALID_PRICE = 129,
        ERR_INVALID_STOPS = 130,
        ERR_INVALID_TRADE_VOLUME = 131,
        ERR_MARKET_CLOSED = 132,
        ERR_TRADE_DISABLED = 133,
        ERR_NOT_ENOUGH_MONEY = 134,
        ERR_PRICE_CHANGED = 135,
        ERR_OFF_QUOTES = 136,
        ERR_BROKER_BUSY = 137,
        ERR_REQUOTE = 138,
        ERR_ORDER_LOCKED = 139,
        ERR_LONG_POSITIONS_ONLY_ALLOWED = 140,
        ERR_TOO_MANY_REQUESTS = 141,
        ERR_TRADE_MODIFY_DENIED = 145,
        ERR_TRADE_CONTEXT_BUSY = 146,
        ERR_TRADE_EXPIRATION_DENIED = 147,
        ERR_TRADE_TOO_MANY_ORDERS = 148
    }
}
