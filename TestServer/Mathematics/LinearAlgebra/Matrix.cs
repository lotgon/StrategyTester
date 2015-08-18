using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics.LinearAlgebra
{
	public unsafe class Matrix
	{
		public Matrix(int rows, int columns)
		{
			m_data = new double[rows, columns];
			m_rows = rows;
			m_colomns = columns;
		}
		#region properties
		public int Rows
		{
			get
			{
				return m_rows;
			}
		}
		public int Columns
		{
			get
			{
				return m_colomns;
			}
		}
		#endregion
		#region operators
		public static Vector operator * (Matrix first, Vector second)
		{
			int columns = first.Columns;
			if (columns != second.Length)
			{
				string st = string.Format("Matrix and vector have different sizes {0} and {1}", columns, second.Length);
				throw new ArgumentException(st);
			}

			int rows = first.Rows;
			double[,] left = first.Data;
			double[] right = second.Data;
			Vector result = new Vector(rows);
			double[] destination = result.Data;
			for (int row = 0; row < rows; ++row)
			{
				double sum = 0;
				for (int column = 0; column < columns; ++column)
				{
					sum += left[row, column] * right[column];
				}
				destination[row] = sum;
			}
			return result;
		}
		public static Vector operator *(Vector first, Matrix second)
		{
			int rows = second.Rows;
			if (rows != first.Length)
			{
				string st = string.Format("Matrix and vector have different sizes {0} and {1}", rows, first.Length);
				throw new ArgumentException(st);
			}

			int columns = second.Columns;
			double[] left = first.Data;
			double[,] right = second.Data;

			Vector result = new Vector(rows);
			double[] destination = result.Data;
			for (int column = 0; column < columns; ++column)
			{
				double sum = 0;
				for (int row = 0; row < rows; ++row)
				{
					sum += right[row, column] * left[row];
				}
				destination[column] = sum;
			}
			return result;
		}
		#endregion
		#region internal
		internal double[,] Data
		{
			get
			{
				return m_data;
			}
		}
		#endregion
		#region members
		private readonly int m_rows;
		private readonly int m_colomns;
		private readonly double[,] m_data;
		#endregion
	}
}
