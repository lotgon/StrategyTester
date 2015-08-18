using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics.Containers;
using Mathematics;

namespace ForexSuite.Analyzers.ResetTime
{
	public class ResetTimeAnalyzer
	{
		#region members
		//private readonly string m_symbol;
		private readonly UnilateralTickCollection<int> m_bids;
		private readonly UnilateralTickCollection<int> m_asks;
		private int m_count;
		private int m_allCount;
		private double m_timeSum;
		private double m_timeSum2;
		private double m_lossSum;
		private double m_lossSum2;
        private double m_swap;
        private double m_coefToUSD;
		#endregion
		#region properties
		public double TakeProfit {get; private set;}
		public double AverageTime
		{
			get 
			{
				if (m_count <= 0)
				{
					return double.NaN;
				}
				return m_timeSum / m_count;
			}
		}
		public double SigmaTime
		{
			get
			{ 
				if (m_count <= 1)
				{
					return double.NaN;
				}
				double count = m_count;
				double average = AverageTime;
				double result = m_timeSum2 / count - average * average;
				result *= (count / (count - 1));
				result = NumericalMethods.Sqrt(result);
				return result;
			}
		}
		public double ResettingPercentage
		{
			get
			{
 				if (m_allCount <= 0)
 				{
					return double.NaN;
 				}
				double result = m_count;
				result /= m_allCount;
				return result;
			}
		}
		public double AverageLoss
		{
			get 
			{
				if (m_count <= 0)
				{
					return double.NaN;
				}
				return m_lossSum / m_count;
				
			}
		}
		public double SigmaLoss
		{
			get
			{
				if (m_count <= 1)
				{
					return double.NaN;
				}
				double count = m_count;
				double average = AverageLoss;
				double result = m_lossSum2 / count - average * average;
				result *= (count / (count - 1));
				result = NumericalMethods.Sqrt(result);
				return result;
			}
		}
        public double TimeFactor { get; private set; }
        public double RealProfit
        {
            get
            {
                return m_coefToUSD*(TakeProfit + m_swap * AverageTime / (TimeFactor * 60 * 24));//supposing that time in minutes
            }
        }
		#endregion
        public ResetTimeAnalyzer(double tp, double timeFactor, UnilateralTickCollection<int> bids, UnilateralTickCollection<int> asks,
            double coefToUSD, double swap = 0)
		{
			m_bids = bids;
			m_asks = asks;
			TakeProfit = tp;
            TimeFactor = timeFactor;
            this.m_swap = swap;
            this.m_coefToUSD = coefToUSD;
 		}
		public void Process(IEnumerable<int> timeBuyCollection, IEnumerable<int> timeSellCollection)
		{
            if (timeBuyCollection != null)
            {
                foreach (int currOpenPoint in timeBuyCollection)
                {
                    ProcessBuy(currOpenPoint, m_bids[currOpenPoint], m_asks[currOpenPoint]);
                }
            }
            if (timeSellCollection != null)
            {
                foreach (int currOpenPoint in timeSellCollection)
                {
                    ProcessSell(currOpenPoint, m_bids[currOpenPoint], m_asks[currOpenPoint]);
                }
            }
		}
        internal void ProcessSell(int time, double bid, double ask)
		{
			KeyValuePair<int, double>? whenTP = m_asks.FirstEventWhenLessOrEqual(time, bid - TakeProfit);
			m_allCount++;
			if (null == whenTP)
			{
                //AP it will be more fair
                whenTP = m_asks.LastTick;
				//return;
			}
			m_count++;
			KeyValuePair<int, double> when = (KeyValuePair<int, double>)whenTP;
            double interval = (when.Key - time) * this.TimeFactor;
			m_timeSum += interval;
			m_timeSum2 += interval * interval;

			if (time < when.Key)
			{
				double maximum = m_asks.FindMaximum(time, when.Key);
				maximum = maximum - bid;
				m_lossSum += maximum;
				m_lossSum2 += (maximum * maximum);
			}
		}
		internal void ProcessBuy(int time, double bid, double ask)
		{
			KeyValuePair<int, double>? whenTP = m_bids.FirstEventWhenMoreOrEqual(time, ask + TakeProfit);
			m_allCount++;
			if (null == whenTP)
			{
                //AP it will be more fair
                whenTP = m_bids.LastTick;
				//return;
			}
			m_count++;
			KeyValuePair<int, double> when = (KeyValuePair<int, double>)whenTP;
            double interval = (when.Key - time) * this.TimeFactor;
			m_timeSum += interval;
			m_timeSum2 += interval * interval;
			if (time < when.Key)
			{
				double minimum = m_bids.FindMinimum(time, when.Key);

				minimum = ask - minimum;
				m_lossSum += minimum;
				m_lossSum2 += (minimum * minimum);
			}
		}		
	}
}
