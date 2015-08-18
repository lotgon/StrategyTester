using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ForexSuite
{
	public static class SymbolsManager
	{
		public static int DigitsFromSymbol(string symbol)
		{
			symbol = symbol.ToUpper();
			bool status = symbol.Contains("JPY");
			int result = status ? 3 : 5;
			return result;
		}
		public static int PipsFromValue(string symbol, double value)
		{
			int digits = DigitsFromSymbol(symbol);
			value *= Math.Pow(10, digits - 5);
			int result = (int)value;
			return result;
		}
		public static double ValueFromPips(string symbol, int pips)
		{
			int digits = DigitsFromSymbol(symbol);
			double result = pips;
			result *= Math.Pow(10, 5 - digits);
			return result;
		}
        public static string GetProfitSymbol(string instrument)
        {
            string from = string.Empty;
            string to = string.Empty;
            if (6 == instrument.Length)
            {
                from = instrument.Substring(0, 3);
                to = instrument.Substring(3, 3);
            }
            else if ((7 == instrument.Length) && ('/' == instrument[3]))
            {
                from = instrument.Substring(0, 3);
                to = instrument.Substring(4, 3);
            }
            return to;
        }
		/// <summary>
		/// Inverse a symbol.
		/// </summary>
		/// <param name="symbol">must have the following format XXXYYY or XXX/YYY</param>
		/// <returns>
		/// XXXYYY -> YYYXXX
		/// XXX/YYY -> YYY/XXX
		/// </returns>
		public static string Inverse(string symbol)
		{
			if (6 == symbol.Length)
			{
				string first = symbol.Substring(0, 3);
				string second = symbol.Substring(3, 3);
				string result = second + first;
				return result;
			}
			if ((7 == symbol.Length) && ('/' == symbol[3]))
			{
				string first = symbol.Substring(0, 3);
				string second = symbol.Substring(4, 3);
				string result = second + '/' + first;
				return result;
			}
			string st = string.Format("Unknonw symbol format = {0}", symbol);
			throw new ArgumentException(st);
		}
	}
}
