using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics.Containers;
using Mathematics;
using ForexSuite;

namespace MathCmd.ResetTime
{
	internal class Info
	{
		internal double Sum { get; private set; }
		internal double Sum2 { get; private set; }
		internal int Count { get; private set; }
		internal int AllCount { get; private set; }
		internal double Spread {get; private set;}
		internal double Spread2 { get; private set; }
		private string m_symbol;
		private int m_tpInPips;
		private double m_tp;
		internal Info(string symbol, int tp)
		{
			m_symbol = symbol;
			m_tpInPips = tp;
			m_tp = (double)SymbolsManager.ValueFromPips(symbol, tp);
		}
		internal void ProcessSell(DateTime time, double bid, double ask, UnilateralTickCollection<DateTime> ticks)
		{
			KeyValuePair<DateTime, double>? whenTP = ticks.FirstEventWhenLessOrEqual(time, bid - m_tp);
			AllCount++;
			if (null == whenTP)
			{
				return;
			}
			Count++;
			KeyValuePair<DateTime, double> when = (KeyValuePair<DateTime, double>)whenTP;
			TimeSpan interval = when.Key - time;
			double hours = interval.TotalHours;
			Sum += hours;
			Sum2 += hours * hours;
			if (time == when.Key)
			{
				return;
			}
			double maximum = ticks.FindMaximum(time, when.Key);
			maximum = maximum - bid;
			Spread += maximum;
			Spread2 += (maximum * maximum);
		}
		internal void ProcessBuy(DateTime time, double bid, double ask, UnilateralTickCollection<DateTime> ticks)
		{
			KeyValuePair<DateTime, double>? whenTP = ticks.FirstEventWhenMoreOrEqual(time, ask + m_tp);
			AllCount++;
			if (null == whenTP)
			{
				return;
			}
			Count++;
			KeyValuePair<DateTime, double> when = (KeyValuePair<DateTime, double>)whenTP;
			TimeSpan interval = when.Key - time;
			double hours = interval.TotalHours;
			Sum += hours;
			Sum2 += hours * hours;
			if (time == when.Key)
			{
				return;
			}
			double minimum = ticks.FindMinimum(time, when.Key);

			minimum = ask - minimum;
			Spread += minimum;
			Spread2 += (minimum * minimum);
		}
		public override string ToString()
		{
			double average = Sum / Count;
			double sigmaTime = NumericalMethods.Sqrt(Sum2 / Count - average * average);

			double spread = Spread / Count;
			double sigmaSpread = NumericalMethods.Sqrt(Spread2 / Count - spread * spread);
			double part = Count;
			part /= AllCount;

			int spreadInPips = SymbolsManager.PipsFromValue(m_symbol, spread);
			int sigmaSpreadInPips = SymbolsManager.PipsFromValue(m_symbol, sigmaSpread);

			string result = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", m_tpInPips, average, sigmaTime, spreadInPips, sigmaSpreadInPips, part);
			
			return result; 
		}
	}
}
