using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;

namespace CalculateSum
{
	public class PortfolioResult
	{
		#region members
		private string m_symbol;		
		private double m_coefficient;
		private StrategyParameter m_parameter;
		#endregion
		public PortfolioResult(string symbol, double coefficient, StrategyParameter parameter)
		{
			m_symbol = symbol;
			m_coefficient = coefficient;
			m_parameter = parameter;
		}
	}
}
