using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common;
using Input = System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<Common.StrategyParameter, Common.StrategyResultStatistics>>;
using Mathematics;
using System.IO;

namespace MathCmd.StabilityTpShift
{
	internal class Adapter
	{
		private const string cTP = "TP";
		private const string cShift = "OpenOrderShift";
		private const int cStep = 100;
		private double[,] m_table;
		private int m_minTP = int.MaxValue;
		private int m_maxTP = int.MinValue;
		private int m_minShift = int.MaxValue;
		private int m_maxShift = int.MinValue;
		private Input m_data;
		internal Adapter(Input data)
		{
			m_data = data;
			foreach (var element in data)
			{
				int tp = element.Key[cTP];
				int shift = element.Key[cShift];

				m_maxTP = Math.Max(m_maxTP, tp);
				m_minTP = Math.Min(m_minTP, tp);

				m_maxShift = Math.Max(m_maxShift, shift);
				m_minShift = Math.Min(m_minShift, shift);
			}
			int rows = (m_maxTP - m_minTP) / cStep + 1;
			int columns = (m_maxShift - m_minShift) / cStep + 1;
			m_table = new double[rows, columns];
			foreach (var element in data)
			{
				int tp = element.Key[cTP];
				int shift = element.Key[cShift];
				int row = (tp - m_minTP) / cStep;
				int column = (shift - m_minShift) / cStep;
				m_table[row, column] = element.Value.CalculateConfidenceIntervalLow;
			}





		}
		internal void Run(string path)
		{
			List<Pixel2> pixels = Stability.FindStabilityMaximums(m_table, 1);
			if (0 == pixels.Count)
			{
				return;
			}


			int rows = m_table.GetLength(0);
			int columns = m_table.GetLength(1);

			double[,] list = new double[rows * columns, 3];
			int index = 0;
			for (int r = 0; r < rows; ++r)
			{
				for (int c = 0; c < columns; ++c, ++index)
				{
					list[index, 0] = CalcTp(r);
					list[index, 1] = CalcShift(c);
					list[index, 2] = m_table[r, c];
				}
			}
			string st = MathFormatProvider.ImportStringFromTable(list);

			
			if (pixels.Count > 3)
			{
				pixels.RemoveRange(3, pixels.Count - 3);
			}
			using (StreamWriter stream = new StreamWriter(path))
			{
				stream.WriteLine(st);
				foreach (var element in pixels)
				{
					int tp = CalcTp(element.Y);
					int shift = CalcShift(element.X);
					double value = m_table[element.Y, element.X];
					stream.WriteLine("{0}\t{1}\t{2}", tp, shift, value);
				}			
			}

		}

		private int CalcShift(int c)
		{
			return m_minShift + cStep * c;
		}
		private int CalcTp(int r)
		{
			return m_minTP + cStep * r;
		}
	}
}
