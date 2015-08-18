using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class Precision
    {
        public static int GetPrecision(string Symbol)
        {
            switch (Symbol.ToUpper())
            {
                case "USDJPY":
                case "USD/JPY":
                case "CADJPY":
                case "CAD/JPY":
                case "CHFJPY":
                case "CHF/JPY":
                case "AUDJPY":
                case "AUD/JPY":
                case "EURJPY":
                case "EUR/JPY":
                case "GBPJPY":
                case "GBP/JPY":
                case "USDJPY.":
                case "CADJPY.":
                case "CHFJPY.":
                case "AUDJPY.":
                case "EURJPY.":
                case "GBPJPY.":
                case "XAGUSD":
                case "SILVER":
                case "NZDJPY":
                case "NZDJPY.":
                case "NZDJPY_AVG":
                case "NZD/JPY_AVG":
                case "USDJPY_AVG":
                case "USD/JPY_AVG":
                case "CADJPY_AVG":
                case "CAD/JPY_AVG":
                case "CHFJPY_AVG":
                case "CHF/JPY_AVG":
                case "AUDJPY_AVG":
                case "AUD/JPY_AVG":
                case "EURJPY_AVG":
                case "EUR/JPY_AVG":
                case "GBPJPY_AVG":
                case "XAGUSD_AVG":
                case "SILVER_AVG":

                    return 3;
                case "XAUUSD":
                case "GOLD":
                case "XAUUSD_AVG":
                case "GOLD_AVG":
                    return 2;
                default:
                    return 5;
            }
        }
    }
}
