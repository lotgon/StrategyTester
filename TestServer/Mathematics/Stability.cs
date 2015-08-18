using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics
{
	public static class Stability
	{
		/// <summary>
		/// The method looks for a stability region.
		/// threshold value = limitValue + sigmaCoefficient * sigma;
		/// </summary>
		/// <param name="data">can't be null</param>
		/// <param name="limitValue"></param>
		/// <param name="sigmaCoefficient">defines filter amplitude and direction; see threshold value calculation</param>
		/// <returns>
		/// true cells defines stability region
		/// false cells defines instability region
		/// </returns>
		public static bool[,] FindStabilityRegion(double[,] data, double sigmaFactor, BinaryFunction predicate, double limitValue = 1)
		{
			int rows = data.GetLength(0);
			int columns = data.GetLength(1);

			List<double> deviations = new List<double>();

			for (int row = 0; row < rows; ++row)
			{
				for (int column = 0; column < columns; ++column)
				{
					double value = data[row, column];
					if (row - 1 >= 0)
					{
						double delta = data[row - 1, column] - value;
						deviations.Add(delta);
					}
					if (row < rows - 1)
					{
						double delta = data[row + 1, column] - value;
						deviations.Add(delta);
					}
					if (column - 1 >= 0)
					{
						double delta = data[row, column - 1] - value;
						deviations.Add(delta);
					}
					if (column < columns - 1)
					{
						double delta = data[row, column + 1] - value;
						deviations.Add(delta);
					}
				}
			}
			double sigma = Statistics.CalculateSampleVariance(deviations.ToArray());
			sigma = NumericalMethods.Sqrt(sigma);
			sigma *= sigmaFactor;
			double threshold = predicate(1, 0) ? (limitValue - sigma) : (limitValue + sigma);

			bool[,] result = new bool[rows, columns];

			for (int row = 0; row < rows; ++row)
			{
				for (int column = 0; column < columns; ++column)
				{
					result[row, column] = predicate(data[row, column], threshold);
				}
			}

			double ry = NumericalMethods.Sqrt(rows);
			double rx = NumericalMethods.Sqrt(columns);
			int r = (int)Math.Min(rx, ry) - 1;
			for (int index = 0; index < r; ++index)
			{
				result = Morphology.Erosion(result);
			}
			return result;
		}
		public static List<Pixel2> FindStabilityExtremums(double[,] data, double sigmaFactor, BinaryFunction predicate)
		{
			List<Pixel2> result = new List<Pixel2>();
			bool[,] region = FindStabilityRegion(data, sigmaFactor, predicate);

			for (; ; )
			{
				int row = -1;
				int column = -1;
				bool status = NumericalMethods.FindConditionalExtremum(data, region, predicate, out row, out column);
				if (!status)
				{
					break;
				}
				result.Add(new Pixel2(column, row));
				region = Morphology.Erosion(region, row, column);
			}
			return result;
		}
		public static List<Pixel2> FindStabilityMaximums(double[,] data, double sigmaFactor)
		{
			List<Pixel2> result = FindStabilityExtremums(data, sigmaFactor, Predicates.MoreOrEqual);
			return result;
		}
		public static List<Pixel2> FindStabilityMinimums(double[,] data, double sigmaFactor)
		{
			List<Pixel2> result = FindStabilityExtremums(data, sigmaFactor, Predicates.LessOrEqual);
			return result;
		}
	}
}
