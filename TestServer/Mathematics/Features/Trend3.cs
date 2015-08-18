using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics.Features
{
	public class Trend3 : IFeature
	{
		#region properties
		public double Value 
		{
			get 
			{
				return m_result;	
			}
		}
		public string Name 
		{
			get
			{
				return "Trend3";
			}
		}
		public bool Ready 
		{ 
			get
			{
				return m_ticks.Count > cMaximumTicksNumber;
			}
		}
		#endregion
		public void Initialize()
		{
			Shutdown();			
		}
		public void Shutdown()
		{
			m_ticks.Clear();
			m_average = 0;
			m_count = 0;
			m_result = 0;
		}
		public void Tick(string symbol, DateTime time, double bid, double ask)
		{
			double average = (bid + ask);
			TimeSpan interval = time - m_lastDateTimeTick;
			if (interval.TotalMinutes < 0)
			{
				throw new ArgumentException("Runtime error: New tick time is more new than previous tick/feature initialization");
			}
			if (interval.TotalMinutes >= 2)
			{
				Shutdown();
			}
			m_count++;
			m_average += (bid + ask) / 2;
			m_lastDateTimeTick = time;													
		}
		#region constants
		private const int cMaximumTicksNumber = 1440; // minutes = 24 hours
		#endregion
		#region members
		private double m_average;
		private int m_count;
		private double m_result;
		private DateTime m_lastDateTimeTick;
		private readonly SortedDictionary<DateTime, double> m_ticks = new SortedDictionary<DateTime, double>();
		#endregion
	}
}
