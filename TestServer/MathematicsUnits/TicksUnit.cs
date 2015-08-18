using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mathematics.Containers;
using Mathematics;

namespace MathematicsUnits
{
	/// <summary>
	/// Summary description for TicksUnit
	/// </summary>
	[TestClass]
	public class TicksUnit
	{
		private readonly List<KeyValuePair<DateTime, double>> m_items;
		private readonly UnilateralTickCollection<DateTime> m_ticks;
		private readonly DateTime m_from;
		private readonly DateTime m_to;
		private const int cCount = 1024;
		private readonly double m_minimum;
		private readonly double m_maximum;
		private const string cPattern = "(.........................).*EURUSD. Bid = ([^ ]+) Ask = ([^ ]+).*";
		public TicksUnit()
		{
			TicksFactory factory = new TicksFactory();
			factory.Parse(Resources.Ticks, cPattern, 1, 2, 3);
			m_items = factory.Bids;
			m_items.Sort(Compare);
			m_from = m_items[0].Key;
			m_to = m_items[m_items.Count - 1].Key;
			m_ticks = new UnilateralTickCollection<DateTime>(m_items);
			m_minimum = double.PositiveInfinity;
			m_maximum = double.NegativeInfinity;
			foreach (var element in m_items)
			{
				if (element.Value < m_minimum)
				{
					m_minimum = element.Value;
				}
				if (element.Value > m_maximum)
				{
					m_maximum = element.Value;
				}
			}
		}
		#region helper
		private static int Compare(KeyValuePair<DateTime, double> first, KeyValuePair<DateTime, double> second)
		{
			return first.Key.CompareTo(second.Key);
		}
		#endregion
		#region generic test
		private void RunTest(Action<DateTime, DateTime> test)
		{
			TimeSpan interval = m_to - m_from;

			test(m_from - interval, m_from);
			test(m_to, m_to + interval);
			test(m_from - interval - interval, m_from - interval);
			test(m_to + interval, m_to + interval + interval);


			Random random = new Random();
			for (int index = 0; index < 64 * 1024; ++index)
			{
				double first = (double)(2 * random.NextDouble() - 1);
				double second = (double)(2 * random.NextDouble() + 1);

				TimeSpan delta = new TimeSpan((long)(first * interval.Ticks));
				DateTime from = m_from + delta;

				delta = new TimeSpan((long)(second * interval.Ticks));
				DateTime to = m_to + delta;
				if (from < to)
				{
					test(from, to);
				}
				else if (from > to)
				{
					test(to, from);
				}
				else
				{
					--index;
				}
			}

		}
		private double NextThreshold(Random random)
		{ 
			double value = (double)(2 * random.NextDouble() - 1);
			double result = (m_minimum + m_maximum) /2 + (m_maximum - m_maximum) * value;
			return result;
		}
		private void RunTest(Action<DateTime, double> test)
		{
			TimeSpan interval = m_to - m_from;
			DateTime middle = m_from + new TimeSpan(interval.Ticks / 2);
			Random random = new Random();
			double threshold = NextThreshold(random);
			test(m_from - interval, threshold);

			threshold = NextThreshold(random);
			test(m_to + interval, threshold);


			for (int index = 0; index < 64 * 1024; ++index)
			{
				double value = (double)(2 * random.NextDouble() - 1);

				TimeSpan delta = new TimeSpan((long)(value * interval.Ticks));
				DateTime time = m_from + delta;

				threshold = NextThreshold(random);

				test(time, threshold);
			}

		}
		#endregion
		#region find minimum
		[TestMethod]
		public void FindMinimumTest()
		{			
			RunTest(FindMinimumTest);
		}
		private void FindMinimumTest(DateTime from, DateTime to)
		{
			double control = double.PositiveInfinity;
			foreach (var element in m_items)
			{
				if ((element.Key >= from) && (element.Key <= to))
				{
					if (control > element.Value)
					{
						control = element.Value;
					}
				}
			}
			double value = m_ticks.FindMinimum(from, to);
			if (value != control)
			{
				Assert.Fail("FindMinimum is failed");
			}
		}
		#endregion
		#region find maximum
		[TestMethod]
		public void FindMaximumTest()
		{
			RunTest(FindMaximumTest);
		}
		private void FindMaximumTest(DateTime from, DateTime to)
		{
			double control = double.NegativeInfinity;
			foreach (var element in m_items)
			{
				if ((element.Key >= from) && (element.Key <= to))
				{
					if (control < element.Value)
					{
						control = element.Value;
					}
				}
			}
			double value = m_ticks.FindMaximum(from, to);
			if (value != control)
			{
				Assert.Fail("FindMaximum is failed");
			}
		}
		#endregion

		#region FirstEventWhenLess
		[TestMethod]
		public void FirstEventWhenLessTest()
		{
			RunTest(FirstEventWhenLessTest);
		}
		private void FirstEventWhenLessTest(DateTime start, double threshold)
		{
			KeyValuePair<DateTime, double>? entry = m_ticks.FirstEventWhenLess(start, threshold);
			foreach (var element in m_items)
			{
				if (element.Key < start)
				{
					continue;
				}
				if (element.Value >= threshold)
				{
					continue;
				}
				if (null == entry)
				{
					Assert.Fail("FirstEventWhenLess is failed - null return value");
				}
				KeyValuePair<DateTime, double> item = (KeyValuePair<DateTime, double>)entry;
				if (element.Key != item.Key)
				{
					Assert.Fail("FirstEventWhenLess is failed - invalid time");
				}
				if (element.Value != item.Value)
				{
					Assert.Fail("FirstEventWhenLess is failed - invalid value");
				}
				return;
			}
			if (null != entry)
			{
				Assert.Fail("FirstEventWhenLess is failed - not null return value");
			}
		}
		#endregion
		#region FirstEventWhenLessOrEqual
		[TestMethod]
		public void FirstEventWhenLessOrEqualTest()
		{
			RunTest(FirstEventWhenLessOrEqualTest);
		}
		private void FirstEventWhenLessOrEqualTest(DateTime start, double threshold)
		{
			KeyValuePair<DateTime, double>? entry = m_ticks.FirstEventWhenLessOrEqual(start, threshold);
			foreach (var element in m_items)
			{
				if (element.Key < start)
				{
					continue;
				}
				if (element.Value > threshold)
				{
					continue;
				}
				if (null == entry)
				{
					Assert.Fail("FirstEventWhenLessOrEqual is failed - null return value");
				}
				KeyValuePair<DateTime, double> item = (KeyValuePair<DateTime, double>)entry;
				if (element.Key != item.Key)
				{
					Assert.Fail("FirstEventWhenLessOrEqual is failed - invalid time");
				}
				if (element.Value != item.Value)
				{
					Assert.Fail("FirstEventWhenLessOrEqual is failed - invalid value");
				}
				return;
			}
			if (null != entry)
			{
				Assert.Fail("FirstEventWhenLessOrEqual is failed - not null return value");
			}
		}
		#endregion
		#region FirstEventWhenMore
		[TestMethod]
		public void FirstEventWhenMoreTest()
		{
			RunTest(FirstEventWhenMoreTest);
		}
		private void FirstEventWhenMoreTest(DateTime start, double threshold)
		{
			KeyValuePair<DateTime, double>? entry = m_ticks.FirstEventWhenMore(start, threshold);
			foreach (var element in m_items)
			{
				if (element.Key < start)
				{
					continue;
				}
				if (element.Value <= threshold)
				{
					continue;
				}
				if (null == entry)
				{
					Assert.Fail("FirstEventWhenMore is failed - null return value");
				}
				KeyValuePair<DateTime, double> item = (KeyValuePair<DateTime, double>)entry;
				if (element.Key != item.Key)
				{
					Assert.Fail("FirstEventWhenMore is failed - invalid time");
				}
				if (element.Value != item.Value)
				{
					Assert.Fail("FirstEventWhenMore is failed - invalid value");
				}
				return;
			}
			if (null != entry)
			{
				Assert.Fail("FirstEventWhenMore is failed - not null return value");
			}
		}
		#endregion
		#region FirstEventWhenMoreOrEqual
		[TestMethod]
		public void FirstEventWhenMoreOrEqualTest()
		{
			RunTest(FirstEventWhenMoreOrEqualTest);
		}
		private void FirstEventWhenMoreOrEqualTest(DateTime start, double threshold)
		{
			KeyValuePair<DateTime, double>? entry = m_ticks.FirstEventWhenMoreOrEqual(start, threshold);
			foreach (var element in m_items)
			{
				if (element.Key < start)
				{
					continue;
				}
				if (element.Value < threshold)
				{
					continue;
				}
				if (null == entry)
				{
					Assert.Fail("FirstEventWhenMoreOrEqual is failed - null return value");
				}
				KeyValuePair<DateTime, double> item = (KeyValuePair<DateTime, double>)entry;
				if (element.Key != item.Key)
				{
					Assert.Fail("FirstEventWhenMoreOrEqual is failed - invalid time");
				}
				if (element.Value != item.Value)
				{
					Assert.Fail("FirstEventWhenMoreOrEqual is failed - invalid value");
				}
				return;
			}
			if (null != entry)
			{
				Assert.Fail("FirstEventWhenMoreOrEqual is failed - not null return value");
			}
		}
		#endregion
		#region FirstEventWhenLess
		[TestMethod]
		public void LastEventWhenLessTest()
		{
			RunTest(LastEventWhenLessTest);
		}
		private void LastEventWhenLessTest(DateTime to, double threshold)
		{
			KeyValuePair<DateTime, double>? entry = m_ticks.LastEventWhenLess(to, threshold);
			for(int index = m_items.Count - 1; index > -1; --index)
			{
				var element = m_items[index];
				if (element.Key > to)
				{
					continue;
				}
				if (element.Value >= threshold)
				{
					continue;
				}
				if (null == entry)
				{
					Assert.Fail("LastEventWhenLess is failed - null return value");
				}
				KeyValuePair<DateTime, double> item = (KeyValuePair<DateTime, double>)entry;
				if (element.Key != item.Key)
				{
					Assert.Fail("LastEventWhenLess is failed - invalid time");
				}
				if (element.Value != item.Value)
				{
					Assert.Fail("LastEventWhenLess is failed - invalid value");
				}
				return;
			}
			if (null != entry)
			{
				Assert.Fail("LastEventWhenLess is failed - not null return value");
			}
		}
		#endregion
		#region LastEventWhenLessOrEqual
		[TestMethod]
		public void LastEventWhenLessOrEqualTest()
		{
			RunTest(LastEventWhenLessOrEqualTest);
		}
		private void LastEventWhenLessOrEqualTest(DateTime to, double threshold)
		{
			KeyValuePair<DateTime, double>? entry = m_ticks.LastEventWhenLessOrEqual(to, threshold);
			for (int index = m_items.Count - 1; index > -1; --index)
			{
				var element = m_items[index];
				if (element.Key > to)
				{
					continue;
				}
				if (element.Value > threshold)
				{
					continue;
				}
				if (null == entry)
				{
					Assert.Fail("LastEventWhenLessOrEqual is failed - null return value");
				}
				KeyValuePair<DateTime, double> item = (KeyValuePair<DateTime, double>)entry;
				if (element.Key != item.Key)
				{
					Assert.Fail("LastEventWhenLessOrEqual is failed - invalid time");
				}
				if (element.Value != item.Value)
				{
					Assert.Fail("LastEventWhenLessOrEqual is failed - invalid value");
				}
				return;
			}
			if (null != entry)
			{
				Assert.Fail("LastEventWhenLessOrEqual is failed - not null return value");
			}
		}
		#endregion
		#region LastEventWhenMore
		[TestMethod]
		public void LastEventWhenMoreTest()
		{
			RunTest(LastEventWhenMoreTest);
		}
		private void LastEventWhenMoreTest(DateTime to, double threshold)
		{
			KeyValuePair<DateTime, double>? entry = m_ticks.LastEventWhenMore(to, threshold);
			for (int index = m_items.Count - 1; index > -1; --index)
			{
				var element = m_items[index];
				if (element.Key > to)
				{
					continue;
				}
				if (element.Value <= threshold)
				{
					continue;
				}
				if (null == entry)
				{
					Assert.Fail("LastEventWhenMore is failed - null return value");
				}
				KeyValuePair<DateTime, double> item = (KeyValuePair<DateTime, double>)entry;
				if (element.Key != item.Key)
				{
					Assert.Fail("LastEventWhenMore is failed - invalid time");
				}
				if (element.Value != item.Value)
				{
					Assert.Fail("LastEventWhenMore is failed - invalid value");
				}
				return;
			}
			if (null != entry)
			{
				Assert.Fail("LastEventWhenMore is failed - not null return value");
			}
		}
		#endregion
		#region LastEventWhenMoreOrEqual
		[TestMethod]
		public void LastEventWhenMoreOrEqualTest()
		{
			RunTest(LastEventWhenMoreOrEqualTest);
		}
		private void LastEventWhenMoreOrEqualTest(DateTime to, double threshold)
		{
			KeyValuePair<DateTime, double>? entry = m_ticks.LastEventWhenMoreOrEqual(to, threshold);
			for (int index = m_items.Count - 1; index > -1; --index)
			{
				var element = m_items[index];
				if (element.Key > to)
				{
					continue;
				}
				if (element.Value < threshold)
				{
					continue;
				}
				if (null == entry)
				{
					Assert.Fail("LastEventWhenMoreOrEqual is failed - null return value");
				}
				KeyValuePair<DateTime, double> item = (KeyValuePair<DateTime, double>)entry;
				if (element.Key != item.Key)
				{
					Assert.Fail("LastEventWhenMoreOrEqual is failed - invalid time");
				}
				if (element.Value != item.Value)
				{
					Assert.Fail("LastEventWhenMoreOrEqual is failed - invalid value");
				}
				return;
			}
			if (null != entry)
			{
				Assert.Fail("LastEventWhenMoreOrEqual is failed - not null return value");
			}
		}
		#endregion
	}
}
