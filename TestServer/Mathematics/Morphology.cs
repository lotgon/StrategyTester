using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics
{
	public class Morphology
	{
		public static bool[,] Erosion(bool[,] data)
		{
			int rows = data.GetLength(0);
			int columns = data.GetLength(1);
			bool[,] result = new bool[rows, columns];

			Copy(data, result);

			for (int row = 0; row < rows; ++row)
			{
				for (int column = 0; column < columns; ++column)
				{
					if (data[row, column])
					{
						continue;
					}
					if (row - 1 >= 0)
					{
						result[row - 1, column] = false;
					}
					if (row < rows - 1)
					{
						result[row + 1, column] = false;
					}
					if (column - 1 >= 0)
					{
						result[row, column - 1] = false;
					}
					if (column < columns - 1)
					{
						result[row, column + 1] = false;
					}
				}
			}
			return result;
		}
		public static bool[,] Erosion(bool[,] data, int row, int column, int radius)
		{
			int rows = data.GetLength(0);
			int columns = data.GetLength(1);
			bool[,] result = new bool[rows, columns];
			Copy(data, result);
			bool[,] marker = new bool[rows, columns];
			List<Pixel2> previous = new List<Pixel2>();
			List<Pixel2> next = new List<Pixel2>();
			// 0 iteration
			previous.Add(new Pixel2(column, row));
			result[row, column] = false;

			for (; radius > 0; --radius )
			{
				next.Clear();
				foreach (var pixel in previous)
				{
					int y = pixel.Y - 1;
					int x = pixel.X;
					if ((y >= 0) && !marker[y, x])
					{
						marker[y, x] = true;
						result[y, x] = false;
						next.Add(new Pixel2(x, y));
					}

					y += 2;
					if ((y < rows - 1) && !marker[y, x])
					{
						marker[y, x] = true;
						result[y, x] = false;
						next.Add(new Pixel2(x, y));
					}

					x--;
					y--;
					if ((x >= 0) && !marker[y, x])
					{
						marker[y, x] = true;
						result[y, x] = false;
						next.Add(new Pixel2(x, y));
					}

					x += 2;
					if ((x < columns - 1) && !marker[y, x])
					{
						marker[y, x] = true;
						result[y, x] = false;
						next.Add(new Pixel2(x, y));
					}
				}
				Algorithms.Swap(ref previous, ref next);
			}
			return result;
		}
		public static bool[,] Erosion(bool[,] data, int row, int column)
		{
			int rows = data.GetLength(0);
			int columns = data.GetLength(1);
			double ry = NumericalMethods.Sqrt(rows);
			double rx = NumericalMethods.Sqrt(columns);
			int radius = (int)Math.Min(rx, ry);
			bool[,] result = Erosion(data, row, column, radius);
			return result;
		}
		private static void Copy(bool[,] source, bool[,] destination)
		{
			int rows = source.GetLength(0);
			int columns = source.GetLength(1);
			for (int row = 0; row < rows; ++row)
			{
				for (int column = 0; column < columns; ++column)
				{
					destination[row, column] = source[row, column];
				}
			}
		}
	}
}
