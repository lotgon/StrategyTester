using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Mathematics
{
	public class MathFormatProvider : IFormatProvider
	{
		#region IFormatProvider interface supporting
		private readonly NumberFormatInfo m_info = new NumberFormatInfo();
		private static IFormatProvider s_provider = new MathFormatProvider();
		public MathFormatProvider()
		{
			m_info.NumberDecimalSeparator = ".";
		}
		public object GetFormat(Type type)
		{
			if (typeof(NumberFormatInfo) == type)
			{
				return m_info;
			}
			return null;
		}
		public static IFormatProvider Provider 
		{
			get
			{
				return s_provider;
			}
		}
		#endregion
		#region converting functions
		public unsafe static string StringFromData(byte[] data)
		{
			if ((null == data) || (0 == data.Length))
			{
				return string.Empty;
			}
			fixed (byte* begin = &data[0])
			{
				sbyte* st = (sbyte*)begin;
				return new string(st, 0, data.Length);
			}
		}
		public static string InputStringFromDouble(double value)
		{
			string result = value.ToString(s_provider);
			result = result.Replace("E", "10^");
			return result;
		}

		public static string InputStringFromData(double[] values)
		{
			if ((null == values) || (0 == values.Length))
			{
				return "{}";
			}
			StringBuilder builder = new StringBuilder();
			builder.Append('{');
			string st = InputStringFromDouble(values[0]);
			builder.Append(st);
			for (int index = 1; index < values.Length; ++index)
			{
				builder.Append(", ");
				st = InputStringFromDouble(values[index]);
				builder.Append(values[index]);
			}
			builder.Append('}');
			string result = builder.ToString();
			return result;
		}
		public static string ImportStringFromTable(double[] values)
		{
			if ((null == values) || (0 == values.Length))
			{
				return string.Empty;
			}
			StringBuilder builder = new StringBuilder();
			if (values.Length > 0)
			{
				string st = InputStringFromDouble(values[0]);
				builder.Append(st);
			}
			for (int index = 1; index < values.Length; ++index)
			{
				builder.Append('\t');
				string st = InputStringFromDouble(values[index]);
				builder.Append(st);
			}
			string result = builder.ToString();
			return result;

		}
		public static string ImportStringFromTable(double[,] values)
		{
			if ((null == values) || (0 == values.Length))
			{
				return string.Empty;
			}
			int rows = values.GetLength(0);
			int columns = values.GetLength(1);
			StringBuilder builder = new StringBuilder();
			for (int row = 0; row < rows; ++row)
			{
				if (columns > 0)
				{
					string st = InputStringFromDouble(values[row, 0]);
					builder.Append(st);
				}
				for (int column = 1; column < columns; ++column)
				{
					string st = InputStringFromDouble(values[row, column]);
					builder.Append('\t');
					builder.Append(st);
				}
				builder.AppendLine();
			}
			string result = builder.ToString();
			return result;
		}
		public static string InputStringFromTable(double[,] values)
		{
			if ((null == values) || (0 == values.Length))
			{
				return "{}";
			}
			int rows = values.GetLength(0);
			int columns = values.GetLength(1);
			StringBuilder builder = new StringBuilder();
			builder.Append('{');
			if (rows > 0)
			{
				InputStringFromTable(0, values, builder);
			}
			for (int index = 1; index < rows; ++index)
			{
				builder.Append(", ");
				InputStringFromTable(index, values, builder);
			}
			builder.Append('}');
			string result = builder.ToString();
			return result;
		}
		private static void InputStringFromTable(int row, double[,] values, StringBuilder builder)
		{
			int columns = values.GetLength(1);
			builder.Append('{');
			if (columns > 0)
			{
				string st = InputStringFromDouble(values[row, 0]);
				builder.Append(st);
			}
			for (int index = 1; index < columns; ++index)
			{
				builder.Append(", ");
				string st = InputStringFromDouble(values[row, index]);
				builder.Append(st);
			}
			builder.Append('}');
		}

		#endregion
	}
}
