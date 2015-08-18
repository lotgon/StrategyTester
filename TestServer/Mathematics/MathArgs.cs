using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mathematics
{
	public class MathArgs : SortedDictionary<string, string>, IComparable<MathArgs>
	{
		public void Add(string name, double value)
		{
			string st = MathFormatProvider.InputStringFromDouble(value);
			this[name] = st;
		}
		public void Add(string name, double[] values)
		{
			string st = MathFormatProvider.InputStringFromData(values);
			this[name] = st; 
		}
		public void Add(string name, double[,] values)
		{
			string st = MathFormatProvider.InputStringFromTable(values);
			this[name] = st; 
		}
		public int CompareTo(MathArgs other)
		{
			int result = Count.CompareTo(other.Count);
			if (0 != result)
			{
				return result;
			}
			Enumerator itThis = GetEnumerator();
			Enumerator itOther = other.GetEnumerator();
			for (; itThis.MoveNext() && itOther.MoveNext();)
			{
				result = itThis.Current.Key.CompareTo(itOther.Current.Key);
				if (0 != result)
				{
					return result;
				}
				result = itThis.Current.Value.CompareTo(itOther.Current.Value);
				if (0 != result)
				{
					return result;
				}
			}
			return result;
		}
	}
}
