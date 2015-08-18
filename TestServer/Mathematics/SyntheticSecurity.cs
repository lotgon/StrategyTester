using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mathematics.LinearAlgebra;

namespace Mathematics
{
	public class SyntheticSecurity
	{
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="Key">currently expected Datetime and int</typeparam>
		/// <param name="keys"></param>
		/// <param name="quotes"></param>
		/// <param name="coefficients"></param>
		/// <returns></returns>
		public static List<KeyValuePair<Key, double>> MakeSynthetic<Key>(IEnumerable<Key> keys,
																		IEnumerable<KeyValuePair<Key, double>>[] quotes,
																		double[] coefficients) where Key : IComparable<Key>
		{
			// check parameters
			int count = quotes.Length;
			if (coefficients.Length == count)
			{
				string st = string.Format("quotes and coefficients are incompatible");
				throw new ArgumentException(st);
			}
			// create enumerators
			double[] values = new double[count];
			IEnumerator<KeyValuePair<Key, double>>[] enumerators = new IEnumerator<KeyValuePair<Key, double>>[count];
			for (int index = 0; index < count; ++index)
			{
				enumerators[index] = quotes[index].GetEnumerator();
			}
			
			List<KeyValuePair<Key, double>> result = new List<KeyValuePair<Key, double>>();
			foreach (var element in keys)
			{
				for (int index = 0; index < count; ++index)
				{
					bool status = NextValue(element, enumerators[index], ref values[index]);
					if (!status)
					{
						return result;
					}
					double value = Vector.Product(values, coefficients);
					result.Add(new KeyValuePair<Key,double>(element, value));
				}
			}
			return result;
		}
		private static bool NextValue<Key>(Key key, IEnumerator<KeyValuePair<Key, double>> quotes, ref double value) where Key : IComparable<Key>
		{
			if (Predicates.Less(key, quotes.Current.Key))
			{
				return true;
			}
			for (; quotes.MoveNext(); )
			{
				if (Predicates.Less(key, quotes.Current.Key))
				{
					value = quotes.Current.Value;
					return true;
				}
			}
			return false;
		}
	}
}
