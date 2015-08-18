using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace ForexSuite
{
	public class QuotesManager
	{
		#region static functions
		public static double ConvertCurrency(string from, string to, DateTime when, double value)
		{
			from = from.ToUpper();
			to = to.ToUpper();
			double result = s_manager.DoConvertCurrency(from, to, when, value);
			return result;
		}
        public static double ConvertCurrencyEx(string from, string to, DateTime when, double value)
        {
            from = from.ToUpper();
            to = to.ToUpper();
            if (to == "JPY")
                value /= 100;
            if (from == "JPY")
                value *= 100;

            double result = s_manager.DoConvertCurrency(from, to, when, value);
            return result;
        }
        public static double ConvertCurrency(string instrument, DateTime when, double value)
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
            double result = ConvertCurrency(from, to, when, value);
            return result;
        }
		#endregion
		#region static members and constants
		private static QuotesManager s_manager = new QuotesManager();
		private readonly TimeSpan cAcceptableInterval = new TimeSpan(7, 0, 0, 0);
		#endregion
		#region methods
		private QuotesManager()
		{
			Initialize("AUDJPY", Resources.AUDJPY);
			Initialize("AUDNZD", Resources.AUDNZD);
			Initialize("AUDUSD", Resources.AUDUSD);
			Initialize("CADJPY", Resources.CADJPY);
			Initialize("CHFJPY", Resources.CHFJPY);
			Initialize("EURAUD", Resources.EURAUD);
			Initialize("EURCAD", Resources.EURCAD);
			Initialize("EURCHF", Resources.EURCHF);
			Initialize("EURGBP", Resources.EURGBP);
			Initialize("EURJPY", Resources.EURJPY);
			Initialize("EURNOK", Resources.EURNOK);
			Initialize("EURSEK", Resources.EURSEK);
			Initialize("EURUSD", Resources.EURUSD);
			Initialize("GBPCHF", Resources.GBPCHF);
			Initialize("GBPJPY", Resources.GBPJPY);
			Initialize("GBPUSD", Resources.GBPUSD);
			Initialize("NZDUSD", Resources.NZDUSD);
			Initialize("USDCAD", Resources.USDCAD);
			Initialize("USDCHF", Resources.USDCHF);
			Initialize("USDJPY", Resources.USDJPY);
			Initialize("USDNOK", Resources.USDNOK);
			Initialize("USDSEK", Resources.USDSEK);
		}
		private void Initialize(string symbol, string text)
		{
			string[] lines = text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			List<Quote> quotes = new List<Quote>();
			foreach (var element in lines)
			{
				Match match = m_pattern.Match(element);
				DateTime when = DateTime.Parse(match.Groups[1].Value);
				float value = float.Parse(match.Groups[2].Value);
				Quote quote = new Quote(when, value);
				quotes.Add(quote);
			}
			quotes.Sort();
			quotes.TrimExcess();
			m_quotes[symbol] = quotes;
		}
		private double DoConvertCurrency(string from, string to, DateTime when, double value)
		{
			if (from == to)
			{
				return value;
			}
			string key = from + to;
			List<Quote> quotes = null;
			bool status = m_quotes.TryGetValue(key, out quotes);

			if (status)
			{
				double factor = FindFactor(quotes, when);
				double result = value * factor;
				return result;
			}
			key = to + from;
			status = m_quotes.TryGetValue(key, out quotes);
			if (status)
			{
				double factor = FindFactor(quotes, when);
				double result = value / factor;
				return result;
			}
			string st = string.Format("Couldn't convert from = {0} to {1}", from, to);
			throw new ArgumentException(st);
		}
		private double FindFactor(List<Quote> quotes, DateTime when)
		{
			if (0 == quotes.Count)
			{
				throw new ArgumentException("No quotes");
			}

			Quote item = new Quote(when, 0F);
			int index = quotes.BinarySearch(item);
			
			if (index >= 0)
			{
				return quotes[index].Value;
			}
			index = ~index;
			if (quotes.Count <= index)
			{
				Quote last = quotes.Last();
				TimeSpan interval = when - last.When;
				if (interval < cAcceptableInterval)
				{
					return last.Value;
				}
				else
				{
					throw new ArgumentException("Extrapolation is failed");
				}
			}
			if (0 == index)
			{
				Quote first = quotes.First();
				TimeSpan interval = first.When - when;
				if (interval < cAcceptableInterval)
				{
					return first.Value;
				}
				else
				{
					throw new ArgumentException("Extrapolation is failed");
				}					
			}
			Quote left = quotes[index - 1];
			Quote right = quotes[index];
			Debug.Assert(left.When < when);
			Debug.Assert(when < right.When);
			double denominator = (right.When - left.When).TotalSeconds;
			double kLeft = (right.When - when).TotalSeconds;
			double kRight = (when - left.When).TotalSeconds;
			double result = (kLeft * left.Value + kRight * right.Value) / denominator;
			return result;
		}
		#endregion
		#region members
		readonly Regex m_pattern = new Regex("([^= ]+) = (.+)", RegexOptions.Compiled);
		readonly Dictionary<string, List<Quote>> m_quotes = new Dictionary<string, List<Quote>>();
		#endregion
	}
}
