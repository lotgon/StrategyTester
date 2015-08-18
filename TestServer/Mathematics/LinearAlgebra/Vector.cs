using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics.LinearAlgebra
{
	public class Vector
	{
		public Vector(int count)
		{
			m_data = new double[count];
		}
		public Vector(double[] data)
		{
			m_data = new double[data.Length];
			data.CopyTo(m_data, 0);
		}
		#region properties and indexers
		public int Length
		{
			get
			{
				return m_data.Length;
			}
		}
		public double this[int index]
		{
			get
			{
				return m_data[index];
			}
			set
			{
				m_data[index] = value;
			}
		}
		#endregion
		#region operators
		public static double operator *(Vector first, Vector second)
		{
			return Product(first.m_data, second.m_data);
		}
		public static Vector operator -(Vector first, Vector second)
		{
			int count = first.Length;
			if (second.Length != count)
			{
				string st = string.Format("Two vectors have different lengths {0} and {1}", first.Length, second.Length);
				throw new ArgumentException(st);
			}
			Vector result = new Vector(count);
			for (int index = 0; index < count; ++index)
			{
				result.m_data[index] = first.m_data[index] - second.m_data[index];
			}
			return result;
		}
		public static Vector operator +(Vector first, Vector second)
		{
			int count = first.Length;
			if (second.Length != count)
			{
				string st = string.Format("Two vectors have different lengths {0} and {1}", first.Length, second.Length);
				throw new ArgumentException(st);
			}
			Vector result = new Vector(count);
			for (int index = 0; index < count; ++index)
			{
				result.m_data[index] = first.m_data[index] + second.m_data[index];
			}
			return result;
		}
		public static double Product(double[] left, double[] right)
		{
			if (left.Length != right.Length)
			{
				string st = string.Format("Two vectors have different lengths {0} and {1}", left.Length, right.Length);
				throw new ArgumentException(st);
			}
			int count = left.Length;
			double result = 0;
			for (int index = 0; index < count; ++index)
			{
				result += left[index] * right[index];
			}
			return result;
		}
		#endregion
		#region internal
		internal double[] Data
		{
			get
			{
				return m_data;
			}
		}
		#endregion
		#region members
		private readonly double[] m_data;
		#endregion
	}
}
