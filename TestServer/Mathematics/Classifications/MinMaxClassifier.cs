using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics.Classifications
{
	class MinMaxClassifier
	{
		MinMaxClassifier()
		{
			Initialize();
		}
		public void Initialize()
		{
			m_ranges = new Dictionary<string, Interval>();
		}
		public void Teach(SortedDictionary<string, double> features)
		{
			foreach (var element in features)
			{
				Interval interval = GetOrCreate(element.Key);
				if (element.Value < interval.Minimum)
				{
					interval.Maximum = element.Value;
				}
				if (element.Value > interval.Maximum)
				{
					interval.Maximum = element.Value;
				}
				m_ranges[element.Key] = interval;
			}
		}
		public bool Classify(SortedDictionary<string, double> features)
		{
			foreach (var element in features)
			{
				Interval range = m_ranges[element.Key];
				if ((range.Minimum > element.Value) || (range.Maximum < element.Value))
				{
					return false;
				}
			}
			return true;
		}
		private Interval GetOrCreate(string name)
		{
			Interval result = new Interval(double.MaxValue, double.MinValue);
			bool status = m_ranges.TryGetValue(name, out result);
			if (!status)
			{
				m_ranges[name] = result;
			}
			return result;
		}
		Dictionary<string, Interval> m_ranges;
	}
}
