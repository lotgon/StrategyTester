using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Mathematics
{
	public unsafe static class NumericalMethods
	{
		#region constants
		public const double Epsilon = 1.192092896e-07;
		#endregion
		#region min max methods
		public static bool FindConditionalExtremum(double[,] data, bool[,] condition, BinaryFunction predicate, out int row, out int column)
		{
			if (null == data)
			{
				throw new NullReferenceException("data cannot be null");
			}
			if (null == condition)
			{
				throw new NullReferenceException("condition cannot be null");
			}
			int rows = data.GetLength(0);
			int columns = data.GetLength(1);
			if ((rows != condition.GetLength(0)) || (columns != condition.GetLength(1)))
			{
				throw new ArgumentException("data and condition have different sizes");
			}
			
			row = -1;
			column = -1;

			bool result = false;

			double extremum = predicate(1, 0) ? double.NegativeInfinity : double.PositiveInfinity;
			

			for (int r = 0; r < rows; ++r)
			{
				for (int c = 0; c < columns; ++c)
				{
					if (!condition[r, c])
					{
						continue;
					}
					double value = data[r, c];
					if (predicate(value, extremum))
					{
						extremum = value;
						row = r;
						column = c;
						result = true;
					}
				}
			}
			return result;
		}
		public static bool FindConditionalMaximum(double[,] data, bool[,] condition, out int row, out int column)
		{
			bool result = FindConditionalExtremum(data, condition, Predicates.More, out row, out column);
			return result;
		}
		public static bool FindConditionalMinimum(double[,] data, bool[,] condition, out int row, out int column)
		{
			bool result = FindConditionalExtremum(data, condition, Predicates.Less, out row, out column);
			return result;
		}
		/// <summary>
		/// Calculates a minimal value
		/// </summary>
		/// <param name="data">
		/// 1) must be not null
		/// 2) data.Length must be more than zero
		/// </param>
		/// <returns></returns>
		public static double Minimum(this double[] data)
		{
			if (null == data)
			{
				throw new ArgumentException("data must be not null");
			}
			if (0 == data.Length)
			{
				string st = string.Format("data.Length = {0}, but must be more than 1", data.Length);
				throw new ArgumentException(st);
			}
			fixed (double* pointer = &data[0])
			{
				double result = Minimum(pointer, data.Length);
				return result;
			}
		}
		/// <summary>
		/// Calculates a minimal value
		/// </summary>
		/// <param name="data">
		/// 1) must be not null
		/// 2) data.Length must be more than zero
		/// </param>
		/// <returns></returns>
		public static double Minimum(this double[,] data)
		{
			if (null == data)
			{
				throw new ArgumentException("data must be not null");
			}
			if (0 == data.Length)
			{
				string st = string.Format("data.Length = {0}, but must be more than 1", data.Length);
				throw new ArgumentException(st);
			}
			fixed (double* pointer = &data[0, 0])
			{
				double result = Minimum(pointer, data.Length);
				return result;
			}
		}
		/// <summary>
		/// Calculates a maximal value
		/// </summary>
		/// <param name="data">
		/// 1) must be not null
		/// 2) data.Length must be more than zero
		/// </param>
		/// <returns></returns>
		public static double Maximum(this double[] data)
		{
			if (null == data)
			{
				throw new ArgumentException("data must be not null");
			}
			if (0 == data.Length)
			{
				string st = string.Format("data.Length = {0}, but must be more than 1", data.Length);
				throw new ArgumentException(st);
			}
			fixed (double* pointer = &data[0])
			{
				double result = Maximum(pointer, data.Length);
				return result;
			}
		}
		/// <summary>
		/// Calculates a maximal value
		/// </summary>
		/// <param name="data">
		/// 1) must be not null
		/// 2) data.Length must be more than zero
		/// </param>
		/// <returns></returns>
		public static double Maximum(this double[,] data)
		{
			if (null == data)
			{
				throw new ArgumentException("data must be not null");
			}
			if (0 == data.Length)
			{
				string st = string.Format("data.Length = {0}, but must be more than 1", data.Length);
				throw new ArgumentException(st);
			}
			fixed (double* pointer = &data[0, 0])
			{
				double result = Maximum(pointer, data.Length);
				return result;
			}
		}
		public static double[] MaximumByRow(this double[,] data)
		{
			if (null == data)
			{
				throw new ArgumentException("data must be not null");
			}

			if (0 == data.Length)
			{
				string st = string.Format("data.Length = {0}, but must be more than 1", data.Length);
				throw new ArgumentException(st);
			}
			int rows = data.GetLength(0);
			if (0 == rows)
			{
				string st = string.Format("data.GetLength(0) = {0}, but must be more than 0", rows);
				throw new ArgumentException(st);
			}
			int columns = data.GetLength(1);
			if (0 == columns)
			{
				string st = string.Format("data.GetLength(1) = {0}, but must be more than 0", columns);
				throw new ArgumentException(st);
			}
			double[] result = new double[rows];
			for (int i = 0; i < rows; ++i)
			{
				result[i] = data[i, 0];
			}
			for (int j = 1; j < columns; ++j)
			{
				for (int i = 0; i < rows; ++i)
				{
					if (result[i] < data[i, j])
					{
						result[i] = data[i, j];
					}
				}
			}
			return result;
		}
		private static double Minimum(double* data, int length)
		{
			double result = *data;
			double* current = data + 1;
			double* end = data + length;
			for (; current < end; ++current)
			{
				if (*current < result)
				{
					result = *current;
				}
			}
			return result;
		}
		private static double Maximum(double* data, int length)
		{
			double result = *data;
			double* current = data + 1;
			double* end = data + length;
			for (; current < end; ++current)
			{
				if (*current > result)
				{
					result = *current;
				}
			}
			return result;
		}
		#endregion
		#region normalization
		public static void Multiply(double[,] data, double factor)
		{
			fixed (double* pointer = &data[0, 0])
			{
				Multiply(factor, pointer, data.Length);
			}
		}
		public static void Multiply(double[] data, double factor)
		{
			fixed (double* pointer = &data[0])
			{
				Multiply(factor, pointer, data.Length);
			}
		}
		private static void Multiply(double factor, double* pointer, int count)
		{
			double* current = pointer;
			double* end = pointer + count;
			for (; current < end; ++current)
			{
				(*current) *= factor;
			}
		}
		#endregion
		#region standard functions
		public static double Sqrt(double value)
		{
			Debug.Assert(value >= -Epsilon);
			if (value <= 0)
			{
				return 0;
			}
			double result = Math.Sqrt(value);
			return result;
		}
		#endregion
		#region common functions
		/// <summary>
		/// The functions return true, if two values is approximately equal.
		/// </summary>
		/// <param name="first">must be finite value</param>
		/// <param name="second">must be finite value</param>
		/// <param name="epsilon">must be more or equal zero</param>
		/// <returns></returns>
		public static bool IsApproximatelyEqual(double first, double second, double epsilon = Epsilon)
		{
			if (epsilon <= 0)
			{
				string st = string.Format("epsilon must be non negative, but it is {0}", epsilon);
				throw new ArgumentException(st);
			}
			double a = Math.Abs(first);
			double b = Math.Abs(second);
			double maximum;
			double minimum;
			if (a < b)
			{
				minimum = a;
				maximum = b;
			}
			else
			{
				minimum = b;
				maximum = a;
			}
			double error = Math.Abs(second - first);
			double threshold = maximum * epsilon;
			bool result = (error <= threshold);
			return result;
		}
		/// <summary>
		/// Calculates result[i] = data[i + 1] - data[i]
		/// </summary>
		/// <param name="data">must be not null and data.Length > 1</param>
		/// <returns></returns>
		public static double[] CalculateAbsoluteVariation(double[] data)
		{
			if (null == data)
			{
				throw new ArgumentException("data can not be null");
			}
			if (data.Length <= 1)
			{
				string st = string.Format("data.Length = {0}, but must be more than 1", data.Length);
				throw new ArgumentException(st);
			}
			int count = data.Length - 1;
			double[] result = new double[count];

			fixed (double* input = &data[0])
			{
				fixed (double* output = &result[0])
				{
					for (int index = 0; index < count; ++index)
					{
						output[index] = input[index + 1] - input[index];
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Calculates result[, i] = data[, i + 1] - data[, i]
		/// </summary>
		/// <param name="data">
		/// 1) must be not null
		/// 2) data.GetLength(0) must be more than 0
		/// 3) data.GetLength(1) must be more than 1
		/// </param>
		/// <returns></returns>
		public static double[,] CalculateAbsoluteVariation(double[,] data)
		{
			if (null == data)
			{
				throw new ArgumentException("data can not be null");
			}
			int rows = data.GetLength(0);
			int columns = data.GetLength(1);
			if (rows < 1)
			{
				string st = string.Format("data.GetLength(0) = {0}, but must be more than 1", rows);
				throw new ArgumentException(st);
			}
			if (columns <= 1)
			{
				string st = string.Format("data.GetLength(1) = {0}, but must be more than 1", columns);
				throw new ArgumentException(st);
			}
			int count = columns - 1;
			double[,] result = new double[rows, count];
			fixed (double* input = &data[0, 0])
			{
				fixed (double* output = &result[0, 0])
				{
					for (int row = 0; row < rows; ++row)
					{
						double* source = input + row * columns;
						double* destination = output + row * (columns - 1);
						for (int index = 0; index < columns; ++index)
						{
							destination[index] = source[index + 1] - source[index];
						}
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Calculates result[i] = data[i + 1] / data[i] - 1
		/// </summary>
		/// <param name="data">
		/// must be not null and data.Length > 1
		/// all elements must be positive
		/// </param>
		/// <returns></returns>
		public static double[] CalculateRelativeVariation(double[] data)
		{
			if (null == data)
			{
				throw new ArgumentException("data can not be null");
			}
			if (data.Length <= 1)
			{
				string st = string.Format("data.Length = {0}, but must be more than 1", data.Length);
				throw new ArgumentException(st);
			}
			double minimum = data.Minimum();
			if (minimum <= 0)
			{
				throw new ArgumentException("data contains not positive number(s)");
			}
			int count = data.Length - 1;
			double[] result = new double[count];
			fixed (double* input = &data[0])
			{
				fixed (double* output = &result[0])
				{
					for (int index = 0; index < count; ++index)
					{
						output[index] = input[index + 1] / input[index] - 1;
					}
				}
			}
			return result;
		}
		/// <summary>
		/// Calculates result[, i] = data[, i + 1] - data[, i]
		/// </summary>
		/// <param name="data">
		/// 1) must be not null
		/// 2) data.GetLength(0) must be more than 0
		/// 3) data.GetLength(1) must be more than 1
		/// </param>
		/// <returns></returns>
		public static double[,] CalculateRelativeVariation(double[,] data)
		{
			if (null == data)
			{
				throw new ArgumentException("data can not be null");
			}
			int rows = data.GetLength(0);
			int columns = data.GetLength(1);
			if (rows < 1)
			{
				string st = string.Format("data.GetLength(0) = {0}, but must be more than 1", rows);
				throw new ArgumentException(st);
			}
			if (columns <= 1)
			{
				string st = string.Format("data.GetLength(1) = {0}, but must be more than 1", columns);
				throw new ArgumentException(st);
			}
			double minimum = data.Minimum();
			if (minimum <= 0)
			{
				throw new ArgumentException("data contains not positive number(s)");
			}
			int count = columns - 1;
			double[,] result = new double[rows, count];
			fixed (double* input = &data[0, 0])
			{
				fixed (double* output = &result[0, 0])
				{
					for (int row = 0; row < rows; ++row)
					{
						double* source = input + row * columns;
						double* destination = output + row * (columns - 1);
						for (int index = 0; index < columns; ++index)
						{
							destination[index] = source[index + 1] / source[index] - 1;
						}
					}
				}
			}
			return result;
		}
		#endregion
	}
}
