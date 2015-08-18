using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics
{
	public class PortfolioInput
	{
		#region properties, which are required for all portfolio methods
		/// <summary>
		/// Required for all portfolio methods.
		/// </summary>
		public double[,] Equity
		{
			get
			{
				return m_equity;
			}
			set
			{
				if (null == value)
				{
					throw new NullReferenceException("Equity can not be null");
				}
				int rows = value.GetLength(0);
				if (rows < 1)
				{
					string st = string.Format("data.GetLength(0) = {0}, but must be more than 0", rows);
					throw new ArgumentException(st);
				}
				int columns = value.GetLength(1);
				if (columns < 2)
				{
					string st = string.Format("data.GetLength(1) = {0}, but must be more than 1", columns);
					throw new ArgumentException(st);
				}
				m_equity = value;
			}
		}
		/// <summary>
		/// Required for all portfolio methods.
		/// </summary>
		public double Reliability
		{
			get
			{
				return m_reliability;
			}
			set
			{
				if ((value <= 0.5) || (value >= 1))
				{
					string st = string.Format("Reliability = {0}, but should be more than 0.5 and less than 1", value);
					throw new ArgumentException(st);
				}
				m_reliability = value;
			}
		}
		/// <summary>
		/// Required for all portfolio methods.
		/// </summary>
		public double MaximumLoss
		{
			get
			{
				return m_maximumLoss;
			}
			set
			{
				if (value <= 0)
				{
					string st = string.Format("Maximum loss = {0}, but must be positive", value);
					throw new ArgumentException(st);
				}
				m_maximumLoss = value;
			}
		}
		#endregion
		#region optional properties
		public double InitialDeposit
		{
			get
			{
				return m_initialDeposit;
			}
			set
			{
				if (value <= 0)
				{
					string st = string.Format("Initial deposit = {0}, but should be more than 0", value);
					throw new ArgumentException(st);
				}
				m_initialDeposit = value;
			}
		}
		public double MarginLevelThreshold
		{
			get
			{
				return m_marginLevelThreshold;
			}
			set
			{
				if (value <= 1)
				{
					string st = string.Format("Margin level threshold = {0}, but should be more than 1", value);
					throw new ArgumentException(st);
				}
				m_marginLevelThreshold = value;
			}
		}
		public double[,] Margin
		{
			get
			{
				return m_margin;
			}
			set
			{
				if (null == value)
				{
					throw new NullReferenceException("Margin can not be null");
				}
				int rows = value.GetLength(0);
				if (rows < 1)
				{
					string st = string.Format("data.GetLength(0) = {0}, but must be more than 0", rows);
					throw new ArgumentException(st);
				}
				int columns = value.GetLength(1);
				if (columns < 2)
				{
					string st = string.Format("data.GetLength(1) = {0}, but must be more than 1", columns);
					throw new ArgumentException(st);
				}
				m_margin = value;
			}
		}
		public double MinimumCoefficientValue
		{
			get
			{
				return m_minimumCoefficientValue;
			}
			set
			{
				if (value <= 0)
				{
					string st = string.Format("Minimum coefficient value = {0}, but should be more than 0", value);
					throw new ArgumentException(st);
				}
				m_minimumCoefficientValue = value;
			}
		}
		#endregion
		#region members
		private double m_maximumLoss;
		private double[,] m_margin;
		private double m_marginLevelThreshold;
		private double m_initialDeposit;
		private double m_minimumCoefficientValue;
		private double m_reliability;
		private double[,] m_equity;
		#endregion
	}
}
