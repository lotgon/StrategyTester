using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics.Mathematica;
using Log4Smart;
using Mathematics.LinearAlgebra;

namespace Mathematics
{
	public unsafe class Portfolio
	{
		#region static data
		static readonly string s_portfolio = MathFormatProvider.StringFromData(Scripts.Portfolio);
		static readonly string s_portfolioDiscrete = MathFormatProvider.StringFromData(Scripts.PortfolioDiscrete);
		static readonly string s_portfolioMargin = MathFormatProvider.StringFromData(Scripts.PortfolioMargin);
		static readonly string s_portfolioMarginBalance = MathFormatProvider.StringFromData(Scripts.PortfolioMarginBalance);
		#endregion
		public double Profit {get; private set;}
		public double[] Coefficients {get; private set;}
		public double When {get; private set;}
		public static Portfolio CalculateLinear(double[,] equity, double reliability, double maximumLoss)
		{
			CalculateLinearValidation(reliability, maximumLoss);
			double[,] deltaEquity = NumericalMethods.CalculateAbsoluteVariation(equity);

			double[,] sigma = Statistics.CalculateCovarianceMatrix(deltaEquity);
			double factor = NumericalMethods.Sqrt(sigma.Maximum());
			if (factor <= 0)
			{
				string st = string.Format("Runtime error: invalid factor = {0}", factor);
				throw new InvalidOperationException(st);
			}
			NumericalMethods.Multiply(deltaEquity, 1 / factor);
			double[] profit = Statistics.CalculateAverage(deltaEquity);
			sigma = Statistics.CalculateCovarianceMatrix(deltaEquity);

			MathArgs args = new MathArgs();
			args.Add("$profit", profit);
			args.Add("$reliability", reliability);
			args.Add("$covariance", sigma);
			args.Add("$loss", maximumLoss / factor);
			Dictionary<string, object> output = MathematicaKernel.Execute(s_portfolio, args);
			Portfolio result = new Portfolio();
			result.Profit = (double)output["profit"] * factor;
			result.When = (double)output["when"];
			result.Coefficients = (double[])output["result"];
			return result;
		}
		public static double CalculateProfit(double[,] equity, double[] coefficients)
		{
			if (null == equity)
			{
				throw new NullReferenceException("Equity can not be null");
			}
			if (null == coefficients)
			{
				throw new NullReferenceException("Portfolio coefficients can not be null");
			}
			int rows = equity.GetLength(0);
			int columns = equity.GetLength(1);
			if (rows < 1)
			{
				string st = string.Format("Rows = {0}, but should be positive", rows);
				throw new ArgumentException(st);
			}
			if (columns < 2)
			{
				string st = string.Format("Rows = {0}, but should be more than 1", columns);
				throw new ArgumentException(st);
			}
			if (coefficients.Length != columns)
			{
				string st = string.Format("Equity and portfolio are incompatible");
				throw new ArgumentException(st);
			}
			double result = 0;
			for (int r = 0; r < rows; ++r)
			{
				result += (equity[r, columns - 1] - equity[r, 0]) * coefficients[r];
			}
			return result;
		}
		public static Portfolio CalculateLinear(double[,] equity, double reliability, double maximumLoss, double minimumCoefficientValue)
		{
			CalculateLinearValidation(reliability, maximumLoss);
			double[,] deltaEquity = NumericalMethods.CalculateAbsoluteVariation(equity);

			double[,] sigma = Statistics.CalculateCovarianceMatrix(deltaEquity);
			double factor = NumericalMethods.Sqrt(sigma.Maximum());
			if (factor <= 0)
			{
				string st = string.Format("Runtime error: invalid factor = {0}", factor);
				throw new InvalidOperationException(st);
			}
			NumericalMethods.Multiply(deltaEquity, 1 / factor);
			double[] profit = Statistics.CalculateAverage(deltaEquity);
			sigma = Statistics.CalculateCovarianceMatrix(deltaEquity);

			MathArgs args = new MathArgs();
			args.Add("$profit", profit);
			args.Add("$reliability", reliability);
			args.Add("$covariance", sigma);
			args.Add("$loss", maximumLoss / factor);
			args.Add("$coefficient", minimumCoefficientValue);
			Dictionary<string, object> output = MathematicaKernel.Execute(s_portfolioDiscrete, args);
			Portfolio result = new Portfolio();
			result.Profit = (double)output["profit"] * factor;
			result.When = (double)output["when"];
			result.Coefficients = (double[])output["result"];
			return result;
		}
		public static Portfolio CalculateLinearMarginBalance(PortfolioInput input, Vector[] balance)
		{
			CalculateLinearValidation(input.Reliability, input.MaximumLoss);

			if (input.InitialDeposit <= input.MaximumLoss)
			{
				string st = string.Format("Initial deposit = {0} should be more than maximum loss = {1}", input.InitialDeposit, input.MaximumLoss);
				throw new ArgumentException();
			}
			if (input.MarginLevelThreshold <= 0)
			{
				string st = string.Format("Margin level = {0}, but must be positive", input.MarginLevelThreshold);
				throw new ArgumentException();
			}

			double marginThreshold = (input.InitialDeposit - input.MaximumLoss) / input.MarginLevelThreshold;

			double[,] deltaEquity = NumericalMethods.CalculateAbsoluteVariation(input.Equity);
			double[,] sigma = Statistics.CalculateCovarianceMatrix(deltaEquity);

			int count = sigma.GetLength(0);
			for (int i = 0; i < count; ++i)
			{
				for (int j = 1 + i; j < count; ++j)
				{
					sigma[i, j] = 0;
					sigma[j, i] = 0;
				}
			}
			double[] profit = Statistics.CalculateAverage(deltaEquity);

			Log.Technical("sigma = {0}", MathFormatProvider.InputStringFromTable(sigma));
			Log.Technical("profit = {0}", MathFormatProvider.InputStringFromData(profit));
			int n = profit.Length;

			double[] m = input.Margin.MaximumByRow();

			Portfolio result = new Portfolio();
			result.Coefficients = new double[n];
			result.When = double.NaN;
			bool[] zeros = new bool[n];

			Vector[] _balance = new Vector[balance.Length - 1];
			for (int index = 1; index < balance.Length; ++index)
			{
				_balance[index - 1] = balance[index] - balance[0];
			}
			double[] margin = input.Margin.MaximumByRow();
			bool status = false;
			do
			{
				status = CalculateLinearMarginBalanceInternal(result, zeros, sigma, profit, margin, input, _balance);
			} while (!status);
			return result;
		}
		private static bool CalculateLinearMarginBalanceInternal(Portfolio portfolio, bool[] zeros, double[,] sigma, double[] profit, double[] margin, PortfolioInput input, Vector[] balance)
		{
			int count = 0;
			Dictionary<int, int> map = new Dictionary<int, int>();
			for (int index = 0; index < zeros.Length; ++index)
			{
				bool status = zeros[index];
				if (!status)
				{
					map[map.Count] = index;
					++count;
				}
			}
			if (0 == count)
			{
				return true;// nothing to do
			}
			// create sub-matrix
			double[,] _sigma = new double[count, count];
			double[] _profit = new double[count];
			double[] _margin = new double[count];

			for (int row = 0; row < count; ++row)
			{
				int r = map[row];
				_profit[row] = profit[r];
				_margin[row] = margin[r];
				for (int column = 0; column < count; ++column)
				{
					int c = map[column];
					_sigma[row, column] = sigma[r, c];
				}
			}


			double[,] _balance = new double[balance.Length, count];
			for (int row = 0; row < balance.Length; ++row)
			{
				int r = row;
				for (int column = 0; column < count; ++column)
				{
					int c = map[column];
					_balance[row, column] = balance[row][c];
				}
			}


			// calculate factor and check it
			double factor = NumericalMethods.Sqrt(_sigma.Maximum());
			if (factor <= 0)
			{
				string st = string.Format("Runtime error: invalid factor = {0}", factor);
				throw new InvalidOperationException(st);
			}
			// normalize data
			NumericalMethods.Multiply(_sigma, 1 / factor / factor);
			NumericalMethods.Multiply(_profit, 1 / factor);
			// execute mathematica script
			MathArgs args = new MathArgs();
			args.Add("$profit", _profit);
			args.Add("$reliability", input.Reliability);
			args.Add("$covariance", _sigma);
			args.Add("$loss", input.MaximumLoss / factor);
			args.Add("$profit", _profit);
			args.Add("$balance", _balance);
			args.Add("$margin", _margin);
			
			
			//args.Add("$margin", m);
			
			double marginThreshold = (input.InitialDeposit - input.MaximumLoss) / input.MarginLevelThreshold;
			args.Add("$marginThreshold", marginThreshold);

			MathResult output = MathematicaKernel.Execute(s_portfolioMarginBalance, args);
			double[] coefficients = (double[])output["result"];
			double p = (double)output["profit"];
			if (0 == p)
			{
				return true;
			}
			// check results
			bool result = FastPositiveMinimalThreshold(zeros, input.MinimumCoefficientValue, count, map, coefficients);
			if (!result)
			{
				return result;
			}
			// prepare results
			for (int index = 0; index < count; ++index)
			{
				portfolio.Coefficients[map[index]] = coefficients[index];
			}
			portfolio.Profit = (double)output["profit"] * factor;
			portfolio.When = (double)output["when"];
			return result;
		}


		private static bool CalculateLinearFastInternal(Portfolio portfolio, bool[] zeros, double[,] sigma, double[] profit, double reliability, double maximumLoss,
														double minimumCoefficientValue, Func<bool[], double, int, Dictionary<int, int>, double[], bool> handler)
		{
			int count = 0;
			Dictionary<int, int> map = new Dictionary<int, int>();
			for (int index = 0; index < zeros.Length; ++index)
			{
				bool status = zeros[index];
				if (!status)
				{
					map[map.Count] = index;
					++count;
				}
			}
			if (0 == count)
			{
				return true;// nothing to do
			}
			// create sub-matrix
			double[,] _sigma = new double[count, count];
			double[] _profit = new double[count];

			for (int row = 0; row < _profit.Length; ++row)
			{
				int r = map[row];
				_profit[row] = profit[r];
				for (int column = 0; column < _profit.Length; ++column)
				{
					int c = map[column];
					_sigma[row, column] = sigma[r, c];

				}
			}
			// calculate factor and check it
			double factor = NumericalMethods.Sqrt(_sigma.Maximum());
			if (factor <= 0)
			{
				string st = string.Format("Runtime error: invalid factor = {0}", factor);
				throw new InvalidOperationException(st);
			}
			// normalize data
			NumericalMethods.Multiply(_sigma, 1 / factor / factor);
			NumericalMethods.Multiply(_profit, 1 / factor);
			// execute mathematica script
			MathArgs args = new MathArgs();
			args.Add("$profit", _profit);
			args.Add("$reliability", reliability);
			args.Add("$covariance", _sigma);
			args.Add("$loss", maximumLoss / factor);
			Dictionary<string, object> output = MathematicaKernel.Execute(s_portfolio, args);
			double[] coefficients = (double[])output["result"];
			// check results
			bool result = handler(zeros, minimumCoefficientValue, count, map, coefficients);
			if (!result)
			{
				return result;
			}
			// prepare results
			for (int index = 0; index < count; ++index)
			{
				portfolio.Coefficients[map[index]] = coefficients[index];
			}
			portfolio.Profit = (double)output["profit"] * factor;
			portfolio.When = (double)output["when"];
			return result;
		}

		private static bool FastAbsoluteMinimalThreshold(bool[] zeros, double minimumCoefficientValue, int count, Dictionary<int, int> map, double[] coefficients)
		{
			bool result = true;
			for (int index = 0; index < count; ++index)
			{
				if (Math.Abs(coefficients[index]) < minimumCoefficientValue)
				{
					result = false;
					zeros[map[index]] = true;
				}
			}
			return result;
		}
		private static bool FastPositiveMinimalThreshold(bool[] zeros, double minimumCoefficientValue, int count, Dictionary<int, int> map, double[] coefficients)
		{
			double minimum = double.PositiveInfinity;
			int position = -1;
			for (int index = 0; index < count; ++index)
			{
				double value = coefficients[index];
				if ((value < minimumCoefficientValue) && (value < minimum))
				{
					minimum = value;
					position = index;
				}
			}
			if (position >= 0)
			{
				zeros[map[position]] = true;
				return false;
			}
			return true;
		}
		public static Portfolio CalculateLinearFast(double[,] equity, double reliability, double maximumLoss, double minimumCoefficientValue)
		{
			CalculateLinearValidation(reliability, maximumLoss);

			double[,] deltaEquity = NumericalMethods.CalculateAbsoluteVariation(equity);
			double[,] sigma = Statistics.CalculateCovarianceMatrix(deltaEquity);
			Log.Technical("sigma = ", MathFormatProvider.InputStringFromTable(sigma));
			double[] profit = Statistics.CalculateAverage(deltaEquity);
			Log.Technical("sigma = ", MathFormatProvider.InputStringFromData(profit));
			int n = profit.Length;

			Portfolio result = new Portfolio();
			result.Coefficients = new double[n];
			result.When = double.NaN;
			bool[] zeros = new bool[n];

			bool status = false;
			do
			{
				status = CalculateLinearFastInternal(result, zeros, sigma, profit, reliability, maximumLoss, minimumCoefficientValue, FastAbsoluteMinimalThreshold);
			} while (!status);
			return result;
		}
		public static Portfolio CalculateLinearPositiveFast(double[,] equity, double reliability, double maximumLoss, double minimumCoefficientValue)
		{
			CalculateLinearValidation(reliability, maximumLoss);

			double[,] deltaEquity = NumericalMethods.CalculateAbsoluteVariation(equity);
			double[,] sigma = Statistics.CalculateCovarianceMatrix(deltaEquity);
			double[] profit = Statistics.CalculateAverage(deltaEquity);
			int n = profit.Length;

			Portfolio result = new Portfolio();
			result.Coefficients = new double[n];
			result.When = double.NaN;
			bool[] zeros = new bool[n];

			bool status = false;
			do
			{
				status = CalculateLinearFastInternal(result, zeros, sigma, profit, reliability, maximumLoss, minimumCoefficientValue, FastPositiveMinimalThreshold);
			} while (!status);
			return result;
		}
	
		public static Portfolio CalculateLinear(double[,] equity, double[,] margin, double reliability, double maximumLoss,
												double maximumMargin)
		{
			CalculateLinearValidation(reliability, maximumLoss);
			
			if (maximumMargin <= 0)
			{
				string st = string.Format("maximumMargin = {0}, but must be positive", maximumMargin);
				throw new ArgumentException();
			}
			double[,] deltaEquity = NumericalMethods.CalculateAbsoluteVariation(equity);
			double[,] sigma = Statistics.CalculateCovarianceMatrix(deltaEquity);
			double factor = NumericalMethods.Sqrt(sigma.Maximum());
			if (factor <= 0)
			{
				string st = string.Format("Runtime error: invalid factor = {0}", factor);
				throw new InvalidOperationException(st);
			}
			double[] m = margin.MaximumByRow();
			MathArgs args = new MathArgs();

			NumericalMethods.Multiply(deltaEquity, 1 / factor);
			double[] profit = Statistics.CalculateAverage(deltaEquity);
			sigma = Statistics.CalculateCovarianceMatrix(deltaEquity);
			NumericalMethods.Multiply(m, 1 / factor);

			args.Add("$profit", profit);
			args.Add("$reliability", reliability);
			args.Add("$covariance", sigma);
			args.Add("$loss", maximumLoss / factor);
			args.Add("$threshold", maximumMargin / factor);
			args.Add("$margin", m);
			
			Dictionary<string, object> output = MathematicaKernel.Execute(s_portfolioMargin, args);
			Portfolio result = new Portfolio();
			result.Profit = (double)output["profit"] * factor;
			result.When = (double)output["when"];
			result.Coefficients = (double[])output["result"];
			return result;
		}
		private static bool CalculateLinearFastInternal(Portfolio portfolio, bool[] zeros, double[,] sigma, double[] profit, double[] margin, double reliability, double maximumLoss,
														double minimumCoefficientValue, double marginThreshold)
		{
			int count = 0;
			Dictionary<int, int> map = new Dictionary<int, int>();
			for (int index = 0; index < zeros.Length; ++index)
			{
				bool status = zeros[index];
				if (!status)
				{
					map[map.Count] = index;
					++count;
				}
			}
			if (0 == count)
			{
				return true;// nothing to do
			}
			// create sub-matrix
			double[,] _sigma = new double[count, count];
			double[] _profit = new double[count];
			double[] _margin = new double[count];

			for (int row = 0; row < _profit.Length; ++row)
			{
				int r = map[row];
				_profit[row] = profit[r];
				_margin[row] = margin[r];
				for (int column = 0; column < _profit.Length; ++column)
				{
					int c = map[column];
					_sigma[row, column] = sigma[r, c];

				}
			}
			// calculate factor and check it
			double factor = NumericalMethods.Sqrt(_sigma.Maximum());
			if (factor <= 0)
			{
				string st = string.Format("Runtime error: invalid factor = {0}", factor);
				throw new InvalidOperationException(st);
			}
			// normalize data
			NumericalMethods.Multiply(_sigma, 1 / factor / factor);
			NumericalMethods.Multiply(_profit, 1 / factor);
			NumericalMethods.Multiply(_margin, 1 / factor);

			MathArgs args = new MathArgs();

			args.Add("$profit", _profit);
			args.Add("$reliability", reliability);
			args.Add("$covariance", _sigma);
			args.Add("$loss", maximumLoss / factor);
			args.Add("$threshold", marginThreshold / factor);
			args.Add("$margin", _margin);

			Dictionary<string, object> output = MathematicaKernel.Execute(s_portfolioMargin, args);
			double[] coefficients = (double[])output["result"];
			// check results
			bool result = FastPositiveMinimalThreshold(zeros, minimumCoefficientValue, count, map, coefficients);
			if (!result)
			{
				return result;
			}
			// prepare results
			for (int index = 0; index < count; ++index)
			{
				portfolio.Coefficients[map[index]] = coefficients[index];
			}
			portfolio.Profit = (double)output["profit"] * factor;
			portfolio.When = (double)output["when"];
			return result;
		}
		public static Portfolio CalculateLinearFast(double[,] equity, double[,] margin, double initialDeposit, double reliability, double maximumLoss,
												double marginLevel, double minimumCoefficientValue)
		{
			CalculateLinearValidation(reliability, maximumLoss);

			if (maximumLoss <= 0)
			{
				string st = string.Format("maximumLoss = {0}, but must be positive", maximumLoss);
				throw new ArgumentException();
			}
			if (initialDeposit <= maximumLoss)
			{
				string st = string.Format("Initial deposit = {0} should be more than maximum loss = {1}", initialDeposit, maximumLoss);
				throw new ArgumentException();
			}
			if (marginLevel <= 0)
			{
				string st = string.Format("Margin level = {0}, but must be positive", marginLevel);
				throw new ArgumentException();
			}

			double marginThreshold = (initialDeposit - maximumLoss) / marginLevel;

			double[,] deltaEquity = NumericalMethods.CalculateAbsoluteVariation(equity);
			double[,] sigma = Statistics.CalculateCovarianceMatrix(deltaEquity);
			double[] profit = Statistics.CalculateAverage(deltaEquity);

			Log.Technical("sigma = {0}", MathFormatProvider.InputStringFromTable(sigma));
			Log.Technical("profit = {0}", MathFormatProvider.InputStringFromData(profit));
			int n = profit.Length;

			double[] m = margin.MaximumByRow();

			Portfolio result = new Portfolio();
			result.Coefficients = new double[n];
			result.When = double.NaN;
			bool[] zeros = new bool[n];


			bool status = false;
			do
			{
				status = CalculateLinearFastInternal(result, zeros, sigma, profit, m, reliability, maximumLoss, minimumCoefficientValue, marginThreshold);
			} while (!status);
			return result;
		}
		public static Portfolio CalculateLinearFastWithoutCorrelation(PortfolioInput input)
		{
			CalculateLinearValidation(input.Reliability, input.MaximumLoss);

			if (input.MaximumLoss <= 0)
			{
				string st = string.Format("maximumLoss = {0}, but must be positive", input.MaximumLoss);
				throw new ArgumentException();
			}
			if (input.InitialDeposit <= input.MaximumLoss)
			{
				string st = string.Format("Initial deposit = {0} should be more than maximum loss = {1}", input.InitialDeposit, input.MaximumLoss);
				throw new ArgumentException();
			}
			if (input.MarginLevelThreshold <= 0)
			{
				string st = string.Format("Margin level = {0}, but must be positive", input.MarginLevelThreshold);
				throw new ArgumentException();
			}

			double marginThreshold = (input.InitialDeposit - input.MaximumLoss) / input.MarginLevelThreshold;

			double[,] deltaEquity = NumericalMethods.CalculateAbsoluteVariation(input.Equity);
			double[,] sigma = Statistics.CalculateCovarianceMatrix(deltaEquity);
			double[] profit = Statistics.CalculateAverage(deltaEquity);
			int count = sigma.GetLength(0);
			for (int i = 0; i < count; ++i)
			{
				for (int j = 1 + i; j < count; ++j)
				{
					sigma[i, j] = 0;
					sigma[j, i] = 0; 
				}
			}

			Log.Technical("sigma = {0}", MathFormatProvider.InputStringFromTable(sigma));
			Log.Technical("profit = {0}", MathFormatProvider.InputStringFromData(profit));
			int n = profit.Length;

			double[] m = input.Margin.MaximumByRow();

			Portfolio result = new Portfolio();
			result.Coefficients = new double[n];
			result.When = double.NaN;
			bool[] zeros = new bool[n];


			bool status = false;
			do
			{
				status = CalculateLinearFastInternal(result, zeros, sigma, profit, m, input.Reliability, input.MaximumLoss, input.MinimumCoefficientValue, marginThreshold);
			} while (!status);
			return result;
		}
		private static void CalculateLinearValidation(double reliability, double maximumLoss)
		{
			if ((reliability <= 0.5) || (reliability >= 1))
			{
				string st = string.Format("Reliability = {0}, but should be more than 0.5 and less than 1", reliability);
				throw new ArgumentException(st);
			}
			if (maximumLoss <= 0)
			{
				string st = string.Format("Maximum loss = {0}, but must be positive", maximumLoss);
				throw new ArgumentException(st);
			}
		}
	}
}
