using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace FxAdvisorCore
{
    public static class Convertor
    {
        static readonly public DateTime startTime = new DateTime(1970, 1, 1, 00, 00, 00);

        public static int TranslateToPipPrice(string symbol, double price)
        {
            return (int)(Math.Pow(10, Precision.GetPrecision(symbol)) * price);
        }
        public static double TranslateFromPipPrice(string symbol, int pipPrice)
        {
            return (double)(pipPrice / Math.Pow(10, Precision.GetPrecision(symbol)));
        }
        public static DateTime SecondsToDateTime(int seconds)
        {
            return startTime.AddSeconds(seconds);

        }
        public static int DateTimeToSeconds(DateTime dateTime)
        {
            return (int)((dateTime - startTime).TotalSeconds);
        }
    }
}