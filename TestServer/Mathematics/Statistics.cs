using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics.Mathematica;

namespace Mathematics
{
	public static unsafe class Statistics
	{
		#region static members
		private readonly static MathCache s_confidenceIntervalCache = new MathCache();
		/// <summary>
		/// contains script for confidence interval calculating
		/// </summary>
		private readonly static string s_confidenceInterval = MathFormatProvider.StringFromData(Scripts.ConfidenceInterval);		
		#endregion
		/// <summary>
		/// The function calculates covariance matrix.
		/// data[i,] - represents i vector data component
		/// </summary>
		/// <param name="data">
		/// 1) can not be null
		/// 2) data.GetLength(0) must be more than 0 (zero)
		/// 3) data.GetLength(1) must be more than 1 (one)
		/// </param>
		/// <returns>can not be null</returns>
		public static double[,] CalculateCovarianceMatrix(double[,] data)
		{
			if (null == data)
			{
				throw new ArgumentException("data parameter can not be null");
			}
			int rows = data.GetLength(0);
			if (rows < 1)
			{
				string st = string.Format("data.GetLength(0) = {0}, but must be more than 0", rows);
				throw new ArgumentException(st);
			}
			int columns = data.GetLength(1);
			if (columns < 2)
			{
				string st = string.Format("data.GetLength(1) = {0}, but must be more than 1", columns);
				throw new ArgumentException(st);
			}
			double length = columns - 1;
			double[] average = CalculateAverage(data);
			double[,] result = new double[rows, rows];

			fixed (double* pointer = &data[0, 0])
			{
				// calculate covariance matrix
				for (int i = 0; i < rows; ++i)
				{
					double xAverage = average[i];
					for (int j = i; j < rows; ++j)
					{
						double s = 0;
						double yAverage = average[j];

						double* px = pointer + i * columns;
						double* end = px + columns;
						double* py = pointer + j * columns;
						for (; px < end; ++px, ++py)
						{
							s += (*px - xAverage) * (*py - yAverage) / length;
						}
						result[i, j] = s;
						result[j, i] = s;
					}
				}
			}
			return result;
		}
		/// <summary>
		/// The function calculates average vector.
		/// data[i,] - represents i vector data component
		/// </summary>
		/// <param name="data">
		/// 1) can not be null
		/// 2) data.GetLength(0) must be more than 0 (zero)
		/// 3) data.GetLength(1) must be more than 1 (one)
		/// </param>
		/// <returns>can not be null</returns>
		public static double[] CalculateAverage(double[,] data)
		{
			if (null == data)
			{
				throw new ArgumentException("data parameter can not be null");
			}
			int rows = data.GetLength(0);
			if (rows < 1)
			{
				string st = string.Format("data.GetLength(0) = {0}, but must be more than 0", rows);
			}
			int columns = data.GetLength(1);
			if (columns < 2)
			{
				string st = string.Format("data.GetLength(1) = {0}, but must be more than 1", columns);
				throw new ArgumentException(st);
			}
			double length = columns;
			double[] result = new double[rows];

			fixed (double* pointer = &data[0, 0])
			{
				// calculate average
				for (int i = 0; i < rows; ++i)
				{
					double* current = pointer + i * columns;
					double* end = current + columns;
					double s = 0;
					for (; current < end; ++current )
					{
						s += *current / length;
					}
					result[i] = s;
				}
			}
			return result;
		}
		/// <summary>
		/// The function calculates	average by sample data.
		/// </summary>
		/// <param name="data">
		/// 1) can not be null
		/// 2) data.Length must be more than zero
		/// </param>
		/// <returns>a sample average value</returns>
		public static double CalculateAverage(double[] data)
		{
			if (null == data)
			{
				throw new ArgumentException("data parameter can not be null");
			}			
			if (data.Length < 1)
			{
				string st = string.Format("data.Length = {0}, but must be more than 0", data.Length);
			}			
			double length = data.Length;
			double result = 0;
			fixed (double* pointer = &data[0])
			{				
				double* current = pointer;
				double* end = current + data.Length;				
				// calculate average
				for (; current < end; ++current)
				{
					result += *current / length;
				}
			}
			return result;
		}
		public static double CalculateSampleVariance(double[] data)
		{
			double average = 0;
			double result = CalculateSampleVarianceAndAverage(data, out average);
			return result;
		}
		public static double CalculateSampleVarianceAndAverage(double[] data, out double average)
		{
			if (null == data)
			{
				throw new ArgumentException("data parameter can not be null");
			}
			if (data.Length < 1)
			{
				string st = string.Format("data.Length = {0}, but must be more than 1", data.Length);
				throw new ArgumentException(st);
			}
			average = CalculateAverage(data);
			double denominator = data.Length - 1;
			double result = 0;
			fixed (double* pointer = &data[0])
			{
				double* current = pointer;
				double* end = current + data.Length;
				// calculate average
				for (; current < end; ++current)
				{
					double value = *current - average;
					result += value * value / denominator;
				}
			}
			return result;
		}
		public static Interval CalculateConfidenceInterval(double[] data, double confidenceLevel)
		{
			if ((confidenceLevel <= 0) || (confidenceLevel >= 1))
			{
				string st = string.Format("Confidence level = {0}, but must be more than 0 and less than 1", confidenceLevel);
				throw new ArgumentException(st);
			}
			double average;
			double s2 = CalculateSampleVarianceAndAverage(data, out average);

			MathArgs args = new MathArgs();
			args.Add("$level", confidenceLevel);
			args.Add("$freedom", data.Length - 1);

			MathResult answer = s_confidenceIntervalCache.LookFor(args);
			if (null == answer)
			{
				answer = MathematicaKernel.Execute(s_confidenceInterval, args);
				s_confidenceIntervalCache.Add(args, answer);
			}
			double deviation = (double)answer["deviation"];
			double k = Math.Sqrt(s2 / data.Length);
			deviation *= k;
			double minimum = average - deviation;
			double maximum = average + deviation;
			return new Interval(minimum, maximum);
		}
	}
}
